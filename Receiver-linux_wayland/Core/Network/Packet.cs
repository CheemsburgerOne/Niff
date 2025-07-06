using System.Text.Json;

namespace Receiver_linux_wayland.Core.Network;

public static partial class Network
{
    public struct Packet<T>(byte operationId, PacketFlags flags, T? data)
    {
        public byte OperationId { get; set; } = operationId;
        public PacketFlags Flags { get; set; } = flags;
        public T? Data { get; set; } = data;

        public string? Serialize() => JsonSerializer.Serialize<Packet<T>>(this);

    }
    
    public static Packet<T>? TryDeserializePacket<T>(byte[] data)
    {
        try
        {
            return JsonSerializer.Deserialize<Packet<T>>(data);
        }
        catch (JsonException ex)
        {
            return null;
        }
    }
}