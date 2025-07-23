using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Sender_windows.Network.Payload;

namespace Sender_windows.Network;

public static partial class Network
{
    
    /// <summary>
    /// Connection manager handles inbound and outbound UDP traffic.
    /// Class is responsible for sending keystroke events and periodic heartbeat packets on a separate port.
    /// Manager state is hinted via radio buttons.
    /// </summary>
    public partial class NetworkManager
    {
        private Cryptography.Cryptography.RsaCng.RsaCngCryptoDevice? _localCngKeyCryptoDevice = new Cryptography.Cryptography.RsaCng.RsaCngCryptoDevice("Niff");

        private Cryptography.Cryptography.Rsa.RsaCryptoDevice? _remoteKeyCryptoDevice;
        private bool _isEncryptionEstablished = false;
        private byte _operationId = 0;
        private TcpClient? _client;
        public bool Connected => _client?.Connected ?? false;
        private byte[] _buffer = new byte[256];
        
        
        public async Task<bool> TryConnect(string hostname, int port)
        {
            //Setup client parameters and heartbeat port
            hostname = "192.168.1.27";
            port = 12015;
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(hostname, port);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
        
        public async Task Disconnect()
        {
            if (_client!.Connected)
            {
                await _client.GetStream().FlushAsync();
                _client.Close();
            }
        }

        public bool SendPacket<T>(PacketFlags flags, IPayload<T> payload)
        {
            byte operationId = AcquireOperationId();
            
            Packet packet = new Packet(AcquireOperationId(), flags);
            if (!packet.WithPayload(payload))
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
            Memory<byte> memorySlice = _buffer.AsMemory(0, _client!.Available);
            int readBytes = await _client.GetStream().ReadAsync(memorySlice);
            
            Packet receivedPacket = new Packet();

            if (!_isEncryptionEstablished)
                return !receivedPacket.TryLoadFromBytes(memorySlice.Span) ? null : receivedPacket;
            
            Span<byte> decryptedbytes = new Span<byte>(_localCngKeyCryptoDevice?.Decrypt(memorySlice.Span));
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
