using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sender_windows.Network.Payload;

public static partial class Payload
{
    public struct HelloDto : IPayload<HelloDto>
    {
        
        [JsonPropertyName("UN")]
        public string? Username { get; set; }
        [JsonPropertyName("PRK")]
        public string? PublicRsaKey { get; set; }

        public HelloDto(string username, string publicRsaKey)
        {
            Username = username;
            PublicRsaKey = publicRsaKey;
        }

        public HelloDto(){}
    }
}