using System.Text.Json;

namespace Sender_windows.Network;

public static partial class Network
{
    public struct Packet<T>(byte operationId, PacketFlags flags, T? data)
    {
        public byte OperationId { get; private init; } = operationId;
        public PacketFlags Flags { get; private init; } = flags;
        public T? Data { get; private init; } = data;

        public byte[]? Serialize()
        {
            try
            {
                return JsonSerializer.SerializeToUtf8Bytes(this, JsonSerializerOptions.Default);
            }
            catch (NotSupportedException ex)
            {
                throw new InvalidOperationException($"Cannot serialize data into {nameof(Packet<T>)}.", ex);
            }
        }
    }
    
    public static Packet<T>? TryDeserializePacket<T>(byte[] data)
    {
        try
        {
            Packet<object> rawPacket = JsonSerializer.Deserialize<Packet<object>>(data);
            switch (rawPacket.Flags)
            {
                case PacketFlags.Hello:
                {
                    return null;
                }
                case PacketFlags.Event:
                {
                    return null;
                }
                default:
                {
                    return null;
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Cannot deserialize data into {nameof(Packet<T>)}.", ex);
        }
    }
}