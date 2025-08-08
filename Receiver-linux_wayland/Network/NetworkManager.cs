using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Receiver_linux_wayland.Core;
using Receiver_linux_wayland.Network.Packet;
using Receiver_linux_wayland.Network.Payload;

namespace Receiver_linux_wayland.Network;

public partial class NetworkManager
{
    private Cryptography.Cryptography.RsaKeyStorage _rsaKeyStorage;
    
    public PeerState PeerState => _peerState;
    private PeerState _peerState = PeerState.Disconnected;
    
    private bool _isEncryptionEstablished = false;
    private byte _operationId = 0;
    
    private TcpListener _listener;
    private TcpClient? _client;
    private NetworkStream? _stream;
    
    byte[] _buffer = new byte[1024];

    private static readonly int MaxPacketsPerRound = 3;
    List<Packet.Network.Packet> _packetsQueue = new List<Packet.Network.Packet>(MaxPacketsPerRound);
    
    CancellationToken _appShutdownToken;
    private ILogger<CoreService>? _logger = null;
    public bool Connected => _client is { Connected: true };

    public NetworkManager(ILogger<CoreService> logger, string localDataDirPath, int listenPort,
        CancellationToken appShutdownToken)
    {
        _logger = logger;
        _appShutdownToken = appShutdownToken;
        InitializeRsaKeyStorageAndLocalKey(localDataDirPath);
        StartListening(listenPort);
    }
    

    private void InitializeRsaKeyStorageAndLocalKey(string localDataDirPath)
    {
        _rsaKeyStorage = new Cryptography.Cryptography.RsaKeyStorage(localDataDirPath);
        if (!_rsaKeyStorage.LoadLocalKeyFromStorage()) _rsaKeyStorage.CreateLocalPublicKeyPemPermanent(true);
    }

    private void StartListening(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
    }

    public async Task WaitNewPeerThenEstablishConnection(CancellationToken ct)
    {
        try
        {
            _client = await _listener.AcceptTcpClientAsync(ct);
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine("SAJONARA");
            throw new OperationCanceledException(ex.Message, ex);
        }
        _stream = _client.GetStream();
        _peerState = PeerState.Connected;
    }

    public void Disconnect()
    {
        if (Connected) _client?.Close();
        _peerState = PeerState.Disconnected;
    }

    public bool SendPacket<T>(PacketFlags flags, IPayload<T> payload)
    {
        byte operationId = AcquireOperationId();
        
        Packet.Network.Packet packet = new Packet.Network.Packet(AcquireOperationId(), flags);
        if (!packet.TryWithPayload(payload))
        {
            //If loading the payload has failed, flag this operationId as no action
            packet = new Packet.Network.Packet(operationId, PacketFlags.None);
        }
        
        byte[]? bytes = packet.Serialize();
        
        if (PeerState == PeerState.ConnectedEncrypted) bytes = _rsaKeyStorage.RemoteHostCryptoDevice!.Encrypt(bytes);
        
        try
        {
            _client!.GetStream().WriteAsync(bytes, 0, bytes.Length).Wait();
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    
    public async Task<List<Packet.Network.Packet>> ReceivePackets(CancellationToken stoppingToken)
    {
        int acceptedPacketSize = _rsaKeyStorage.LocalHostCryptoDevice.KeySize / 8;
        int bytesRead;
        System.Memory<byte> memorySlice;
        while (true)
        {
            if (_client.Available > acceptedPacketSize)
            {
                Console.Write("Hi");
            }
            Packet.Network.Packet received = new Packet.Network.Packet();
            if (_isEncryptionEstablished)
            {
                try
                {

                    bytesRead = await _stream.ReadAsync(_buffer, 0, acceptedPacketSize, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return _packetsQueue;
                }
                memorySlice = _buffer.AsMemory(0, bytesRead);
                Span<byte> decryptedbytes =
                    new System.Span<byte>(_rsaKeyStorage.LocalHostCryptoDevice?.Decrypt(memorySlice.Span));
                //If loading failed omit this payload
                if(!received.TryLoadFromBytes(decryptedbytes)) continue;
                _packetsQueue.Add(received);
                if (_client.Available == 0) return _packetsQueue;
                continue;
            }
            bytesRead = await _client.GetStream().ReadAsync(_buffer, 0, _client.Available);
            memorySlice = _buffer.AsMemory(0, bytesRead);
            //If loading failed omit this payload
            if(!received.TryLoadFromBytes(memorySlice.Span)) continue;
            _packetsQueue.Add(received);

            if (_client.Available == 0) return _packetsQueue;
        }
    }
    
    public void Clear() => _packetsQueue.Clear();
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte AcquireOperationId()
    {
        _operationId = _operationId++;
        return _operationId;
    }
}