// using System.Net.Sockets;
// using System.Text;
//
// namespace Sender_windows.Network;
//
// public static partial class Network
// {
//     public partial class ConnectionManager
//     {
//         
//         /// <summary>
//         /// 
//         /// </summary>
//         /// <param name="cts"></param>
//         private async Task BeginSend(CancellationTokenSource cts)
//         {
//             while (!cts.IsCancellationRequested)
//             {
//                 try
//                 {
//                     var received = PacketQ.Take();
//                     await _client.SendAsync(received).ConfigureAwait(false);
//                 }
//                 catch (SocketException ex)
//                 {
//                 }
//                 catch (InvalidOperationException ex) //Graceful shutdown on disconnect
//                 {
//                     return;
//                 }
//             }
//         }
//         
//         /// <summary>
//         /// Periodically sends "hb\n" to the receiver as a heartbeat mechanism and awaits an "ok\n" response.
//         /// </summary>
//         /// <param name="millisecondsPeriod">How often heartbeat is sent in milliseconds</param>
//         /// <param name="millisecondsTimeout">How long each response is awaited in milliseconds</param>
//         /// <param name="heartbeatsFailedThreshold">How many unsuccessful responses need to be received to throw an exception</param>
//         /// <param name="cst">Token to monitor the cancel request</param>
//         /// <exception cref="SocketException">Thrown when receiver fails to respond consecutively 'threshold' times </exception>
//         private async Task BeginHeartbeat(int millisecondsPeriod, int millisecondsTimeout, int heartbeatsFailedThreshold, CancellationTokenSource cst)
//         {
//             ArgumentOutOfRangeException.ThrowIfNegative(millisecondsPeriod);
//             ArgumentOutOfRangeException.ThrowIfNegative(millisecondsTimeout);
//             ArgumentOutOfRangeException.ThrowIfLessThan(heartbeatsFailedThreshold, 1);
//
//             ReportHeartbeatStatusOk(0);
//             
//             int lastCause = 0;
//             int consecutiveHeartbeatsOk = 0;
//             int consecutiveHeartbeatsFailed = 0;
//             
//             while (!cst.IsCancellationRequested)
//             {
//                 if (consecutiveHeartbeatsFailed >= heartbeatsFailedThreshold) throw new SocketException(lastCause);
//                 //Every period try sending a heartbeat, wait timeout for response
//                 await Task.Delay(millisecondsPeriod).ConfigureAwait(false);
//                 Queue.Add(HeartbeatReferenceBytes);
//                 CancellationTokenSource innerCts = new CancellationTokenSource(millisecondsTimeout);
//
//                 try
//                 {
//                     //If value is received check if it is correct, otherwise count as failed
//                     var result = await _client.ReceiveAsync(innerCts.Token).ConfigureAwait(false);
//                     Console.WriteLine("Received O");
//                     if (!HeartbeatAcknowledgeReferenceBytes.SequenceEqual(result.Buffer))
//                     {
//                         lastCause = 0;
//                         consecutiveHeartbeatsOk = 0;
//                         consecutiveHeartbeatsFailed += 1;
//                         ReportHeartbeatStatusNok(consecutiveHeartbeatsFailed, lastCause);
//                     }
//                     else
//                     {
//                         consecutiveHeartbeatsFailed = 0;
//                         consecutiveHeartbeatsOk += 1;
//                         ReportHeartbeatStatusOk(consecutiveHeartbeatsOk);
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     lastCause = 1;
//                     consecutiveHeartbeatsFailed+=1;
//                     ReportHeartbeatStatusNok(consecutiveHeartbeatsFailed, lastCause);
//                 }
//             }
//         }
//         
//         /// <summary>
//         /// Performs a handshake with remote receiver by sending a heartbeat port and receiving OK response.
//         /// </summary>
//         /// <param name="hbPort">Heartbeat port to send to remote receiver</param>
//         /// <param name="receiveTimeout">Timeout in seconds after which exception is thrown</param>
//         /// <param name="onCountdownTick">Delegate to be invoked every second. Passes loop number</param>
//         /// <param name="onCountdownEnd">Delegate to be invoked after the countdown. Passes total seconds elapsed</param>
//         /// <exception cref="SocketException">Thrown when I/O operation on socket failed with timeout or remote unreachable</exception>
//         private async Task PerformHandshake(
//             int hbPort, 
//             int receiveTimeout, 
//             Action<int>? onCountdownTick = null, 
//             Action<int>? onCountdownEnd = null
//             )
//         {
//             //Send test heartbeat and wait given time for answer, otherwise timeout
//             try
//             {
//                 SendPacket(PacketFlags.Hello, hbPort);
//                 
//             }
//             catch (Exception ex)
//             {
//                 throw new SocketException(0, ex.Message);
//             }
//             
//             CancellationTokenSource cst = new CancellationTokenSource();
//             
//             //Start countdown that cancels the token
//             _ = CountdownAsync(
//                 receiveTimeout, 
//                 cst, 
//                 onCountdownTick,
//                 onCountdownEnd
//             ).ConfigureAwait(false);
//
//             try
//             {
//                 UdpReceiveResult result = await _client.ReceiveAsync(cst.Token).ConfigureAwait(false);
//                 await cst.CancelAsync().ConfigureAwait(false);
//                 if (!result.Buffer.SequenceEqual(HeartbeatAcknowledgeReferenceBytes) ) throw new SocketException();
//             }
//             catch(Exception ex) when (ex is OperationCanceledException or SocketException)
//             {
//                 throw new SocketException(0, ex.Message);
//             } 
//         }
//
//         /// <summary>
//         /// Starts an asynchronous countdown that cancels the token at the end.
//         /// If token is canceled from another scope operation immediately returns without exception.
//         /// </summary>
//         /// <param name="seconds">Countdown timespan. If 0, runs onEnd immediately, negative values exit immediately</param>
//         /// <param name="cts">Token to be canceled. Must be valid for non-negative seconds</param>
//         /// <param name="onTic">Delegate to be invoked every second. Passes loop number</param>
//         /// <param name="onEnd">Delegate to be invoked after the countdown. Passes total seconds elapsed</param>
//         /// <exception cref="ArgumentException">Thrown when countdown proceeds but token is invalid</exception>
//         private static async Task CountdownAsync(
//             int seconds, 
//             CancellationTokenSource? cts = null,
//             Action<int>? onTic = null,
//             Action<int>? onEnd = null )
//         {
//             if (seconds < 0) return;
//             if (cts == null) throw new ArgumentException("Token must be valid!");
//             
//             for (int i = 0; i < seconds; i++)
//             {
//                 if (cts.IsCancellationRequested) return;
//                 onTic?.Invoke(i);
//                 await Task.Delay(1000).ConfigureAwait(false);
//             }
//             
//             if (cts.IsCancellationRequested) return;
//             onEnd?.Invoke(seconds);
//             await cts.CancelAsync().ConfigureAwait(false);
//         }
//     }
// }
