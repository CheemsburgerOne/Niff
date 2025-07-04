namespace Receiver_linux_wayland.Core.Network;

public static partial class Network
{
    public struct Packet
    {
        public PacketFlags Flags { get; set; }
        public byte OperationId { get; set; }
        public byte[]? Data { get; set; }
    }
}