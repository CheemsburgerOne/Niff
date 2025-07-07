namespace Sender_windows.Network;

public static partial class Network
{
    public struct PacketReceiveResult
    {
        public PacketFlags Flags { get; init; }
        public object? Data { get; init; }
    }
}