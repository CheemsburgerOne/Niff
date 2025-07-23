using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace Sender_windows.Network.Payload;

public static partial class Payload
{
    public struct KeyEventDto : IPayload<KeyEventDto>
    {
        [JsonPropertyName("pID")]
        public int WpfId { get; }
        [JsonPropertyName("pM")]
        public bool[] Modifiers { get; }

        public KeyEventDto(int wpfId, bool[] modifiers)
        {
            WpfId = wpfId;
            Modifiers = modifiers;
        }
        
        public KeyEventDto(){}
    }
}