using System.Text.Json.Serialization;

namespace Receiver_linux_wayland.Network.Payload;

public static partial class Payload
{
    public struct KeyEventDto : IPayload<KeyEventDto>
    {
        [JsonPropertyName("pID")]
        public int WpfId { get; set; }
        [JsonPropertyName("pM")]
        public bool[]? Modifiers { get; set; }
        public KeyEventDto(){}

        public byte[] Serialize()
        {
            throw new NotSupportedException("This functionality is not supported on receiver side.");
        }
    }
}