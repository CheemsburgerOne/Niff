using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Receiver_linux_wayland.Core;
using Receiver_linux_wayland.Network.Payload;

namespace Receiver_linux_wayland.Network;

public static partial class Network
{
    public partial class NetworkManager
    {
        private Cryptography.Cryptography.RsaKeyStorage _rsaKeyStorage =
            new Cryptography.Cryptography.RsaKeyStorage("/tmp/NIff");
        
        private Cryptography.Cryptography.Rsa.RsaCryptoDevice _localKeyCryptoDevice;
        private Cryptography.Cryptography.Rsa.RsaCryptoDevice _remoteKeyCryptoDevice;

        private bool _isEncryptionEstablished = false;
        public PeerState PeerState => _peerState;
        private PeerState _peerState = PeerState.Disconnected;
        private byte _operationId = 0;
        
        private TcpListener _listener;
        private TcpClient? _client;
        byte[] _buffer = new byte[256];
        
        private ILogger<CoreService>? _logger = null;
        public bool Connected => _client is { Connected: true };
        public NetworkManager(ILogger<CoreService> logger, int listenPort = 12015)
        {
            _logger = logger;
            InitializeLocalRsaKey();
            InitializeTcpListener(listenPort);
        }
        

        private void InitializeLocalRsaKey()
        {
            Cryptography.Cryptography.Rsa.RsaCryptoDevice? rsaCryptoDevice = 
                _rsaKeyStorage.LoadLocalKeyFromStorage() ?? new Cryptography.Cryptography.Rsa.RsaCryptoDevice(2048);
            
            _rsaKeyStorage.RegisterLocalPublicKeyPemPermanent(rsaCryptoDevice);
            
            _localKeyCryptoDevice  = rsaCryptoDevice;
        }

        private void InitializeTcpListener(int listenPort)
        {
            _listener = new TcpListener(listenPort);
            _listener.Start();
        }

        public async Task EstablishNewConnection()
        {
            _client = await _listener.AcceptTcpClientAsync();
            _peerState = PeerState.Connected;
        }

        public void Disconnect()
        {
            if (Connected) _client.Close();
            _remoteKeyCryptoDevice = null;
        }

        public bool SendPacket<T>(PacketFlags flags, IPayload<T> payload)
        {
            byte operationId = AcquireOperationId();
            
            Packet packet = new Packet(AcquireOperationId(), flags);
            if (!packet.TryWithPayload(payload))
            {
                //If loading the payload has failed, flag this operationId as no action
                packet = new Packet(operationId, PacketFlags.None);
            }
            
            byte[]? bytes = packet.Serialize();
            
            if (_isEncryptionEstablished) bytes = _remoteKeyCryptoDevice!.Encrypt(bytes);
            
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
        
        public async Task<Packet?> ReceivePacket()
        {
            System.Memory<byte> memorySlice = _buffer.AsMemory(0, _client!.Available);
            int readBytes = await _client.GetStream().ReadAsync(memorySlice);
            
            Packet receivedPacket = new Packet();

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
}