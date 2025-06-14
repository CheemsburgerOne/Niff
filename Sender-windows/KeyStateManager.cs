using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sender_windows;

public class KeyStateManager
{
    private readonly ConcurrentQueue<KeyEventArgs> _eventQueue = new ConcurrentQueue<KeyEventArgs>();
    private readonly ConnectionManager _connectionManager;

    private Label? _debugLabel;

    public KeyStateManager(ConnectionManager connectionManager, Label debugLabel)
    {
        _connectionManager = connectionManager;
        _debugLabel = debugLabel;
    }

    public void Debug(Label label) => _debugLabel = label;

    public async Task Event(KeyEventArgs keyEvent)
    {
        _eventQueue.Enqueue(keyEvent);
        await Process();
    }

    private async Task Process()
    {
        if (_eventQueue.TryDequeue(out KeyEventArgs keyEvent))
        {
            _debugLabel.Content =$"Key: {keyEvent.Key.ToString()}\nToogle: {keyEvent.IsToggled.ToString().ToLower()}\nPressed: {keyEvent.IsRepeat.ToString()}";
            byte[]? data = SerializeKeyEvent(keyEvent);
            if (data == null) return;
            _connectionManager.Send(data);
        }
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

