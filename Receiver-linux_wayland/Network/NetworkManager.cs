using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Receiver_linux_wayland.Core;
using Receiver_linux_wayland.Payload;

namespace Receiver_linux_wayland.Network;

public static partial class Network
{
    public class NetworkManager
    {
        private UdpClient _client;
        private ILogger<CoreService>? _logger = null;
        public NetworkManager(ILogger<CoreService> logger, int listenPort = 12015)
        {
            _logger = logger;
            _client = new UdpClient(listenPort);
        }
        public void SendPacket<T>(int operationId, PacketFlags flags, IPayload<T>? payload = null)
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
                _client.Send(packet.Serialize());
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
            IPEndPoint? ep = null;
            byte[] receivedBytes = (await _client.ReceiveAsync(cancellationToken)).Buffer;

            Packet receivedPacket = new Packet();

            return !receivedPacket.TryLoadFromBytes(receivedBytes) ? null : receivedPacket;
        }
    }
}