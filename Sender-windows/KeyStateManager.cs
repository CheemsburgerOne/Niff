using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Input;
using Sender_windows.Connection;

namespace Sender_windows;

public class KeyStateManager
{
    private Action<byte[]> _dispatch;
    private Label? _debugLabel;

    public KeyStateManager(Action<byte[]> dispatch)
    {
        _dispatch = dispatch;
    }

    public void Debug(Label label) => _debugLabel = label;

    public void Event(KeyEventArgs keyEvent)
    {
        //_debugLabel.Content =$"Key: {keyEvent.Key.ToString()}\nToogle: {keyEvent.IsToggled.ToString().ToLower()}\nPressed: {keyEvent.IsRepeat.ToString()}";
        byte[]? data = SerializeKeyEvent(keyEvent);
        if (data == null) return;
        _dispatch(data);
    }

    private static byte[]? SerializeKeyEvent(KeyEventArgs keyEvent)
    {
        try
        {
            string json = JsonSerializer.Serialize<KeyEventArgsSerializedDto>(new KeyEventArgsSerializedDto(keyEvent));
            return Encoding.UTF8.GetBytes(json);
        }
        catch
        {
            return null;
        }
    }
}

