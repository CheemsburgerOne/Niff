using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Windows.Controls;

namespace Sender_windows;

public class ConnectionManager
{
    private UdpClient _client;
    private readonly ConcurrentQueue<byte[]> _sendQueue = new ConcurrentQueue<byte[]>();
    Label _debugLabel;
    
    private Task _heartbeatTask;
    private Task _sendTask;

    private readonly int _okHeartbeatResponseLength;

    public ConnectionManager()
    {
        var okHeartbeatResponse = Encoding.UTF8.GetBytes($"ok\n");
        _okHeartbeatResponseLength = okHeartbeatResponse.Length;
    }
    
    public void Send(byte[] data) => _sendQueue.Enqueue(data);
    
    public async Task Connect(string ip, int port, int hbPort)
    {
        _client = new UdpClient(hbPort);
        _client.MulticastLoopback = false;
        _client.Client.ReceiveTimeout = 3000;
        _client.Client.SendTimeout = 100;
        
        _client.Connect(ip, port);
        Send(Encoding.UTF8.GetBytes(hbPort.ToString()));
        _heartbeatTask = Heartbeat();
        _sendTask = Send();
        await Task.WhenAll(_heartbeatTask, _sendTask);
    }

    private Task Send()
    {
        return Task.Run(() =>
        {
            while (true)
            {
                if (_sendQueue.TryDequeue(out byte[] result))
                {
                    try
                    { 
                        _client.Send(result);
                    }
                    catch (SocketException ex)
                    {

                    }
                }
            }
        });
    }

    private async Task Heartbeat()
    {
        while (true)
        {
            await Task.Delay(3000);
            _sendQueue.Enqueue(Encoding.UTF8.GetBytes("hb\n"));
            var data = (await _client.ReceiveAsync()).Buffer;
            Console.WriteLine(Encoding.UTF8.GetString(data));
            if (data.Length != _okHeartbeatResponseLength) throw new Exception();
        }
    }
}