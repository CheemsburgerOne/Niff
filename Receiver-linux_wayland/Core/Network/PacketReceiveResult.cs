using System.Text.Json;

namespace Receiver_linux_wayland.Core.Network;

public static partial class Network
{
    public readonly struct PacketReceiveResult
    {
        public PacketFlags Flags { get; init; }
        public string? Data { get; init; }

        public T? DeserializePacketData<T>()
        {
            try
            {
                return JsonSerializer.Deserialize<T>(Data);
            }
            catch (Exception ex)

            {
                return default;
            }
            
        }
    }
}