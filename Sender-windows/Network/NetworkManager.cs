using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
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
        private string _username;
        public bool Connected => _client?.Connected ?? false;
        private byte[] _buffer = new byte[1024];
        
        
        public async Task<bool> TryConnect(string conectionString, int timeoutMiliseconds)
        {
            //conectionString = "cheemsburger-personal@192.168.1.27:12015";
            if (string.IsNullOrEmpty(conectionString)) return false;
            //Setup client parameters and heartbeat port
            try
            {
                var split1 = conectionString.Split('@');
                var username = split1[0];
                _username = username;
                if (split1.Length != 2) throw new Exception("Bad connection string");
                
                var split2 = split1[1].Split(':');
                if (split2.Length != 2) throw new Exception("Bad connection string");
                var hostIp =  split2[0];
                var port = int.Parse(split2[1]);

                _client = new TcpClient();
                CancellationTokenSource cts = new CancellationTokenSource(timeoutMiliseconds);
                await _client.ConnectAsync(hostIp, port, cts.Token);

                return true;
            }
            catch (OperationCanceledException ex)
            {
                MessageBox.Show("Remote host is not responding.", "Timeout");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection string is in incorrect format", "Bad connection string");
                return false;
            }
        }
        
        public async Task Disconnect()
        {
            if (_client!.Connected) await _client.GetStream().FlushAsync();
            _client.Close();
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
            
            byte[] bytes = packet.Serialize();

            return TrySendInner(bytes);
        }

        private bool TrySendInner(byte[] bytes)
        {
            try
            {
                if (_isEncryptionEstablished) bytes = _remoteKeyCryptoDevice!.Encrypt(bytes);
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
            int readBytes = await _client!.GetStream().ReadAsync(_buffer, 0, _buffer.Length);
            Memory<byte> memorySlice = _buffer.AsMemory(0, readBytes);
            
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
