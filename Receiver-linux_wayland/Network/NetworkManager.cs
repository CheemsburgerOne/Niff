using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Receiver_linux_wayland.Core;
using Receiver_linux_wayland.Network.Packet;
using Receiver_linux_wayland.Network.Payload;

namespace Receiver_linux_wayland.Network;

public partial class NetworkManager
{
    private readonly Cryptography.Cryptography.RsaKeyStorage _rsaKeyStorage;
    
    public PeerState PeerState { get; private set; } = PeerState.Disconnected;

    private bool _isEncryptionEstablished = false;
    private byte _operationId;

    private string _serverName = "cheemstation";
    private TcpListener _listener;
    private TcpClient? _client;
    private NetworkStream? _stream;

    private readonly byte[] _buffer = new byte[1024];

    private static readonly int MaxPacketsPerRound = 3;
    private readonly List<Packet.Network.Packet> _packetsQueue = new List<Packet.Network.Packet>(MaxPacketsPerRound);
    
    CancellationToken _appShutdownToken;
    private ILogger<CoreService>? _logger = null;
    public bool Connected => _client is { Connected: true };

    public NetworkManager(
        ILogger<CoreService> logger, 
        DirectoryInfo etcDir, 
        int listenPort)
    {
        _logger = logger;
        _rsaKeyStorage = new Cryptography.Cryptography.RsaKeyStorage(etcDir);
        InitializeLocalRsaKey(etcDir);
        StartListening(listenPort);
    }

    public void WithCancellation(CancellationToken ct) => _appShutdownToken = ct;
    private void InitializeLocalRsaKey(DirectoryInfo etcDir)
    {
        if (_rsaKeyStorage.TryLoadLocalKeyFromStorage()) return;
        
        Cryptography.Cryptography.Rsa.RsaCryptoDevice newLocalRsaCryptoDevice =
            new Cryptography.Cryptography.Rsa.RsaCryptoDevice(2048);
        _rsaKeyStorage.RegisterNewLocalRsaKeyPermanent(newLocalRsaCryptoDevice, true);
    }

    private void StartListening(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
    }

    public async Task WaitNewPeerThenEstablishConnection(CancellationToken ct)
    {
        _client = await _listener.AcceptTcpClientAsync(ct);
        _stream = _client.GetStream();
        PeerState = PeerState.Connected;
    }

    public void Disconnect()
    {
        if (Connected) _client?.Close();
        _rsaKeyStorage.UnloadRemoteHostCryptoDevice();
        PeerState = PeerState.Disconnected;
    }
    
    public void StopListening() => _listener?.Stop();

    public bool SendPacket<T>(PacketFlags flags, IPayload<T> payload)
    {
        byte operationId = AcquireOperationId();

        Packet.Network.Packet packet = new Packet.Network.Packet(AcquireOperationId(), flags);

        packet.WithPayload(payload);
        byte[] bytes = packet.Serialize();

        return TrySendInner(bytes);
    }
    
    private bool TrySendInner(byte[] bytes)
    {
        try
        {
            if (_isEncryptionEstablished) bytes = _rsaKeyStorage.RemoteHostCryptoDevice!.Encrypt(bytes);
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
        int encryptedPacketSize = _rsaKeyStorage.LocalHostCryptoDevice.KeySize / 8;
        int bytesRead = 0;
        int bytesToRead;
        Memory<byte> memorySlice;
        
        while (true)
        {
            Packet.Network.Packet received = new Packet.Network.Packet();
            bytesToRead = _isEncryptionEstablished ? encryptedPacketSize : _client!.Available; 
            try
            {
                bytesRead = await _stream!.ReadAsync(_buffer, 0, bytesRead, stoppingToken);
            }
            catch
            {
                return _packetsQueue;
            }
            
            memorySlice = _buffer.AsMemory(0, bytesRead);
            
            Span<byte> bytesSpan = _isEncryptionEstablished
                ? new System.Span<byte>(_rsaKeyStorage.LocalHostCryptoDevice?.Decrypt(memorySlice.Span))
                : _buffer.AsMemory(0, bytesRead).Span;
            
            //If loading failed omit this payload
            if(!received.FromBytes(bytesSpan)) continue;
            _packetsQueue.Add(received);
            
            if (_client!.Available == 0) return _packetsQueue;
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