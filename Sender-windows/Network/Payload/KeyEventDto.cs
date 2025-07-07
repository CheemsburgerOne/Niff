using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Input;

namespace Sender_windows.Network.Payload;

public static partial class Payload
{
    public struct KeyEventDto : IPayload<KeyEventDto>
    {
        public string? Key { get; set; }
        public bool IsToggled { get; set; }
        public bool IsRepeat { get; set; }

        public KeyEventDto(KeyEventArgs keyEvent)
        {
            Key = keyEvent.Key.ToString();
            IsToggled = keyEvent.IsToggled;
            IsRepeat = keyEvent.IsRepeat;
        }
        
        public KeyEventDto(){}
    }
}