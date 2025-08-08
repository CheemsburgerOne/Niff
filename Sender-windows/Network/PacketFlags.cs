namespace Sender_windows.Network;

public static partial class Network
{
    [Flags]
    public enum PacketFlags : byte
    { 
        None = 0b_0000_0000,
        Hello = 0b_0000_0001,
        KeyEvent = 0b_0000_0010,
        Custom1 = 0b_0000_0100,
        Custom2 = 0b_0000_1000,
        Custom3 = 0b_0001_0000,
        Custom4 = 0b_0010_0000,
        Bye = 0b_0100_0000,
        Ack = 0b_1000_0000,
        All  = 0b_1111_1111
    }
}