using System.Net.Sockets;
using System.Text.Json;

namespace Receiver_linux_wayland.Core.Network;

public static partial class Network
{
    public class NetworkManager
    {
        private UdpClient _client;

        private Func<Receiver_linux_wayland.Core.Core.KeyEventDto, string> _keyEventHandler;

        public NetworkManager()
        {
            _client = new UdpClient(listenPort);
            _keyEventHandler = keyEventHandler;
        }


        public async Task BeginReceive(CancellationTokenSource cts)
        {
            while (!cts.IsCancellationRequested)
            {
                await Handshake();
                while (!cts.IsCancellationRequested)
                {
                    UdpReceiveResult payload = await _client.ReceiveAsync();
                    

                }
            }
            
        }

        private async Task Handshake()
        {
            try
            {
                UdpReceiveResult result = await _client.ReceiveAsync();
                int port = int.Parse(result.Buffer);
                result = await _client.ReceiveAsync();
                

            }
           
            
            
        }

        private async Task ProcessPayload(UdpReceiveResult payload)
        {
            if (Receiver_linux_wayland.Core.Core.Constants.HbBytes.SequenceEqual(payload.Buffer))
            {
                await _client.SendAsync(Receiver_linux_wayland.Core.Core.Constants.HbAckBytes);
            }

            if (Receiver_linux_wayland.Core.Core.Constants.ByeBytes.SequenceEqual(payload.Buffer))
            {
                await _client.SendAsync(Receiver_linux_wayland.Core.Core.Constants.ByeAckBytes);
                _client.Close();
            }

            try
            {
                Receiver_linux_wayland.Core.Core.KeyEventDto dto = JsonSerializer.Deserialize<Receiver_linux_wayland.Core.Core.KeyEventDto>(payload.Buffer);
                _keyEventHandler?.Invoke(dto);
            }
            
            
        }
        

        public Task<object> Next(object data)
        {
            throw new NotImplementedException();
        }
    }

    public enum ReceiveMode
    {
        Single,
        Multi
    }

    public enum PacketType
    {
        Heartbeat,
        Keypress,
        Bye
    }
    

}