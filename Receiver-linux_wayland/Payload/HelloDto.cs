namespace Receiver_linux_wayland.Payload;

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