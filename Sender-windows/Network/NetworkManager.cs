using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

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
        private byte _operationId = 0;
        
        //Udp client
        private readonly Lock _sendLock = new Lock();

        public UdpClient Client => _client;

        private UdpClient _client;
        
        //Sending tasks
        // private ConfiguredTaskAwaitable _heartbeatTask;
        // private readonly CancellationTokenSource _heartbeatCancellationTokenSource = new CancellationTokenSource();
        // private Task _sendTask;
        // private readonly CancellationTokenSource _sendCancellationTokenSource = new CancellationTokenSource();
        
        public async Task<bool> TryConnect(string hostname, int port, int hbPort)
        {
            //Set radio to Connecting
            // _radioButtonsProgress.Report((1, null, true));
            //Setup client parameters and heartbeat port
            _client = new UdpClient(hbPort);
            _client.MulticastLoopback = false;
             
            //Begin handshake, throw error after timeout
            try
            {
                _client.Connect(hostname, port);
                // await PerformHandshake(
                //     hbPort,
                //     10,
                //     i => _radioButtonsProgress.Report( (1, $"Connecting ({i}s)", null) ) );
            }
            catch (Exception ex)
            {
                //Set radio_1 to basic label
                // _radioButtonsProgress.Report((1, "Connecting", null));
                //Set radio_2 Timeout / refused as active, then abort
                // _radioButtonsProgress.Report((2, null, true));
                return false;
            }
            
            //Return radio 1 to default 
            // _radioButtonsProgress.Report((1, "Connecting", null));
            //Set radio to connected, run and await sending tasks
            // _radioButtonsProgress.Report((3, null, true));

            // _sendTask = Task.Run(async () => await BeginSend(_sendCancellationTokenSource) );
            // _heartbeatTask = BeginHeartbeat(15000, 30000, 3, _heartbeatCancellationTokenSource).ConfigureAwait(false);
            
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
        
        public void SendPacket<T>(PacketFlags flags, T data)
        {
            byte lockedOperationId;
            lock(_operationIdLock)
            {
                _operationId = _operationId++;
                lockedOperationId = _operationId;
            }
            
            Packet<T> packet = new Packet<T>(lockedOperationId, flags, data);
            byte[]? bytes = packet.Serialize();
            lock(_sendLock)
            {
                _client.Send(bytes);
            }

            // if (_operationIdSuccessTable[lockedOperationId] == true)
            // {
            //     throw new Exception($"Operation has not been acknowledged");
            // }
            // _operationIdSuccessTable[lockedOperationId] = true;
        }
        
        public PacketReceiveResult ReceivePacket()
        {
            IPEndPoint? ep = null;
            byte[] receivedBytes = _client.Receive(ref ep);

            Packet<string>? deserialized = TryDeserializePacket<string>(receivedBytes);
            
            // _operationIdSuccessTable[deserialized.Value.OperationId] = false;

            return new PacketReceiveResult()
            {
                Data = deserialized.Value.Data ?? null,
                Flags = deserialized.Value.Flags
            };
        }
    }
}
