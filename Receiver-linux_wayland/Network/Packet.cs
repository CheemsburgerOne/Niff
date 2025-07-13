using System.Text.Json;
using Receiver_linux_wayland.Payload;

namespace Receiver_linux_wayland.Network;

public static partial class Network
{
    public class Packet
    {
        public int OperationId { get; set; }
        public PacketFlags Flags { get; set; }
        public bool IsAckRequested { get; set; }
        public byte[]? Data { get; set; }

        public Packet(){}
        public Packet(int operationId, PacketFlags flags)
        {
            OperationId = operationId;
            Flags = flags;
        }
        
        public bool TryWithPayload<T>(IPayload<T> payload)
        {
            try
            {
                Data = payload.Serialize();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        
        public void RequestAck() => IsAckRequested = true;
        
        public byte[] Serialize()
        {
            try
            {
                return JsonSerializer.SerializeToUtf8Bytes(this, JsonSerializerOptions.Default);
            }
            catch (NotSupportedException ex)
            {
                throw new InvalidOperationException($"Cannot serialize data into {nameof(Packet)}.", ex);
            }
        }

        public bool TryLoadFromBytes(byte[] bytes)
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

        public T? GetPayloadAsType<T>()
        {
            try
            {
                return JsonSerializer.Deserialize<T>(Data, JsonSerializerOptions.Default);
            }
            catch (NotSupportedException ex)
            {
                throw new InvalidOperationException($"Cannot deserialize payload as {nameof(T)}", ex);
            }
            
        }
    }
}