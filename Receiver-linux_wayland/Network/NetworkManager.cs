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
    
    private Cryptography.Cryptography.Rsa.RsaCryptoDevice _localKeyCryptoDevice;
    private Cryptography.Cryptography.Rsa.RsaCryptoDevice _remoteKeyCryptoDevice;

    private bool _isEncryptionEstablished = false;
    public PeerState PeerState => _peerState;
    private PeerState _peerState = PeerState.Disconnected;
    private byte _operationId = 0;
    
    private TcpListener _listener;
    private TcpClient? _client;
    byte[] _buffer = new byte[1024];
    
    private ILogger<CoreService>? _logger = null;
    public bool Connected => _client is { Connected: true };
    public NetworkManager(ILogger<CoreService> logger, string localDataDirPath, int listenPort = 12015)
    {
        _logger = logger;
        InitializeRsaKeyStorageAndLocalKey(localDataDirPath);
        InitializeTcpListener(listenPort);
    }
    

    private void InitializeRsaKeyStorageAndLocalKey(string localDataDirPath)
    {
        _rsaKeyStorage = new Cryptography.Cryptography.RsaKeyStorage(localDataDirPath);
        if (!_rsaKeyStorage.LoadLocalKeyFromStorage()) _rsaKeyStorage.CreateLocalPublicKeyPemPermanent(true);
    }

    private void InitializeTcpListener(int listenPort)
    {
        _listener = new TcpListener(listenPort);
        _listener.Start();
    }

    public async Task WaitNewPeerThenEstablishConnection()
    {
        _client = await _listener.AcceptTcpClientAsync();
        _peerState = PeerState.Connected;
    }

    public void Disconnect()
    {
        if (Connected) _client!.Close();
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
        
        if (_isEncryptionEstablished) bytes = _localKeyCryptoDevice!.Encrypt(bytes);
        
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
    
    public async Task<Packet.Network.Packet?> ReceivePacket()
    {
        int readBytes = await _client.GetStream().ReadAsync(_buffer, 0,  _buffer.Length);
        System.Memory<byte> memorySlice = _buffer.AsMemory(0, readBytes);
        
        Packet.Network.Packet receivedPacket = new Packet.Network.Packet();

        if (!_isEncryptionEstablished)
            return !receivedPacket.TryLoadFromBytes(memorySlice.Span) ? null : receivedPacket;
        
        Span<byte> decryptedbytes = new System.Span<byte>(_localKeyCryptoDevice?.Decrypt(memorySlice.Span));
        return !receivedPacket.TryLoadFromBytes(decryptedbytes) ? null : receivedPacket;

    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte AcquireOperationId()
    {
        _operationId = _operationId++;
        return _operationId;
    }
}