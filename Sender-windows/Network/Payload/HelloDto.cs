using System.Text.Json;

namespace Sender_windows.Network.Payload;

public static partial class Payload
{
    public struct HelloDto : IPayload<HelloDto>
    {
        public int HeartbeatPort { get; set; }

        public HelloDto(int heartbeatPort)
        {
            HeartbeatPort = heartbeatPort;
        }

        public HelloDto() { }

    }
}