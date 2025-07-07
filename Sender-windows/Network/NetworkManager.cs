using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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
        private readonly Lock _operationIdLock = new Lock();
        private int _operationId = 0;
        private readonly List<PendingOperation> _pendingOperationsAwaitingAck = new List<PendingOperation>(10);
        
        //Udp client
        private readonly Lock _sendLock = new Lock();

        public UdpClient Client => _client;

        private UdpClient _client;
        
        public async Task<bool> TryConnect(string hostname, int port, int hbPort)
        {
            //Setup client parameters and heartbeat port
            _client = new UdpClient(hbPort);
            _client.MulticastLoopback = false;
            
            try
            {
                _client.Connect(hostname, port);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        
        public async Task Disconnect()
        {
            //Stop generating heartbeats and receiving OKs
            // await _heartbeatCancellationTokenSource.CancelAsync().ConfigureAwait(false);
            
            //Send bye to the receiver and wait 0,25 sec for response
            //
            // await Task.Delay(250).ConfigureAwait(false);
            
            //Run socket buffer to the end for and check for Bye acknowledge
            // while (true)
            // {
            //     //No ack received
            //     if (_client.Available == 0)
            //     {
            //         _heartbeatLabelProgress.Report("Disconnected without remote acknowledgement");
            //         break;
            //     }
            //     //Empty the buffer and look for BYEack
            //     var result = await _client.ReceiveAsync().ConfigureAwait(false);
            //     if (ByeAcknowledgeReferenceBytes.SequenceEqual(result.Buffer))
            //     {
            //         _heartbeatLabelProgress.Report("Disconnected");
            //         break;
            //     }
            // }
            //
            // _radioButtonsProgress.Report((4, null, true));
            //
            // //Mark sendQueue as done and terminate send task
            // Queue.CompleteAdding();
            // await _sendCancellationTokenSource.CancelAsync().ConfigureAwait(false);
            //
            // _heartbeatCancellationTokenSource?.Dispose();
            // _sendCancellationTokenSource?.Dispose();
            // _client.Close();
            
        }
        
        public void SendPacket<T>(PacketFlags flags, IPayload<T> payload)
        {
            int operationId = AcquireOperationId();
            Packet packet = new Packet(operationId, flags);
            if (!packet.WithPayload(payload))
            {
                //If loading the payload has failed, flag this operationId as no action
                packet = new Packet(operationId, PacketFlags.None);
            }
            byte[]? bytes = packet.Serialize();
            
            lock(_sendLock)
            {
                _client.Send(bytes);
            }

            //If Ack flag is set, sender requires acknowledgement 
            if ( (flags & PacketFlags.Ack) == PacketFlags.Ack)
            {
                _pendingOperationsAwaitingAck.Add(new PendingOperation(operationId, DateTime.Now.Add(TimeSpan.FromSeconds(4))));
            }

            // if (_operationIdSuccessTable[lockedOperationId] == true)
            // {
            //     throw new Exception($"Operation has not been acknowledged");
            // }
            // _operationIdSuccessTable[lockedOperationId] = true;
        }
        
        public Packet? ReceivePacket()
        {
            IPEndPoint? ep = null;
            byte[] receivedBytes = _client.Receive(ref ep);

            Packet receivedPacket = new Packet();

            if (!receivedPacket.TryLoadFromBytes(receivedBytes))
            { 
                return null;
            }

            if ((receivedPacket.Flags & PacketFlags.Ack) != PacketFlags.Ack) return receivedPacket;
            
            PendingOperation? pendingOperation =
                _pendingOperationsAwaitingAck.Find(e => e.OperationId == receivedPacket.OperationId);
                
            if (pendingOperation != null)
                _pendingOperationsAwaitingAck.Remove(pendingOperation);

            // _operationIdSuccessTable[deserialized.Value.OperationId] = false;
            return receivedPacket;
        }

        private int AcquireOperationId()
        {
            int lockedOperationId;
            lock(_operationIdLock)
            {
                _operationId = _operationId++;
                lockedOperationId = _operationId;
            }
            return lockedOperationId;
        }

        public class PendingOperation
        {
            public int OperationId { get; private set; }
            public DateTime ExpirationTime { get; private set; }

            public PendingOperation(int operationId, DateTime expirationTime)
            {
                OperationId = operationId;
                ExpirationTime = expirationTime;
            }
        }
    }
}
