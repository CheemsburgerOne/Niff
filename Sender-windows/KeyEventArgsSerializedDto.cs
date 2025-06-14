using System.Windows.Input;

namespace Sender_windows;

public struct KeyEventArgsSerializedDto
{
    public string Key { get; set; }
    public bool IsToggled { get; set; }
    public bool IsRepeat { get; set; }

    public KeyEventArgsSerializedDto(KeyEventArgs keyEvent)
    {
        Key = keyEvent.Key.ToString();
        IsToggled = keyEvent.IsToggled;
        IsRepeat = keyEvent.IsRepeat;
    }
}
