using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace Sender_windows.KeyEvent;

public static partial class KeyEvent
{
    public struct KeyEventDto
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