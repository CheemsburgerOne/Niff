using System.Text.Json.Serialization;

namespace Receiver_linux_wayland.Network.Payload;

public static partial class Payload
{
    public struct HelloDto : IPayload<HelloDto>
    {
        [JsonPropertyName("PRK")]
        public string? PublicRsaKey { get; set; }

        public HelloDto(string publicRsaKey)
        {
            PublicRsaKey = publicRsaKey;
        }

        public HelloDto(){}
    }
}