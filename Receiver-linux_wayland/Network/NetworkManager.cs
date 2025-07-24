using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
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

            _localKeyCryptoDevice  = rsaCryptoDevice;
        }

        private void InitializeTcpListener(int listenPort)
        {
            _listener = new TcpListener(listenPort);
            _listener.Start();
        }

        public async Task AwaitNewConnection() => _client = await _listener.AcceptTcpClientAsync();
        public void SendPacket<T>(byte operationId, PacketFlags flags, IPayload<T>? payload = null)
        {
            Packet packet = new Packet(operationId, flags);
            if (payload != null)
            {
                if (!packet.TryWithPayload(payload))
                {
                    // _logger.LogError();
                    //If loading the payload has failed, flag this operationId as no action, treat this as skipped operationId
                    packet = new Packet(operationId, PacketFlags.None);
                }
            }

            try
            {
                // _client.Send(packet.Serialize());
            }
            catch (Exception e)
            {
                
            }
            // if (_operationIdSuccessTable[lockedOperationId] == true)
            // {
            //     throw new Exception($"Operation has not been acknowledged");
            // }
            // _operationIdSuccessTable[lockedOperationId] = true;
        }
        
        public async Task<Packet?> ReceivePacketAsync(CancellationToken cancellationToken)
        {
            Memory<byte> receivedBYtes;
            int readBytes = await _client.GetStream().ReadAsync(receivedBYtes = _buffer.AsMemory(0, 256), cancellationToken);

            Packet receivedPacket = new Packet();

            return !receivedPacket.TryLoadFromBytes(receivedBYtes.Slice(0, readBytes).Span) ? null : receivedPacket;
        }
    }
}