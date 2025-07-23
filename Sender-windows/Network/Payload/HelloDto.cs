using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sender_windows.Network.Payload;

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