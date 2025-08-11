using System.Text.Json;
using System.Text.Json.Serialization;
using Receiver_linux_wayland.Network.Payload;

namespace Receiver_linux_wayland.Network.Packet;

public static partial class Network
{
    public class Packet
    {
        [JsonPropertyName("OI")]
        public byte OperationId { get; set; }
        [JsonPropertyName("F")]
        public PacketFlags Flags { get; set; }
        [JsonPropertyName("D")]
        public byte[]? Data { get; set; }

        public Packet(){}
        public Packet(byte operationId, PacketFlags flags)
        {
            OperationId = operationId;
            Flags = flags;
        }
        
        public void WithPayload<T>(IPayload<T> payload) => Data = payload.Serialize();
        
        
        public byte[] Serialize() => JsonSerializer.SerializeToUtf8Bytes(this, JsonSerializerOptions.Default);

        public bool FromBytes(Span<byte> bytes)
        {
            try
            {
                Packet? packet = JsonSerializer.Deserialize<Packet>(bytes, JsonSerializerOptions.Default);
                OperationId = packet!.OperationId;
                Flags = packet.Flags;
                Data = packet.Data;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public T? GetPayloadAsType<T>() => JsonSerializer.Deserialize<T>(Data, JsonSerializerOptions.Default);
    }
}