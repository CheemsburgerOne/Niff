using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Input;
using Sender_windows.Connection;

namespace Sender_windows;

public class KeyStateManager
{
    private struct Test
    {
        public Key Key { get; set; }
        public int Num { get; set; }
    }
    private Action<byte[]> _dispatch;
    private Label? _debugLabel;
    private FileStream _debugFile;
    private List<Test> _debugList = new List<Test>();

    public KeyStateManager(Action<byte[]> dispatch)
    {
        _dispatch = dispatch;
        _debugFile = File.Open("C://Users//dpasz//Desktop//Niff//Sender-windows//DebugKeys", FileMode.Create);
    }

    public void Debug(Label label) => _debugLabel = label;

    public void Event(KeyEventArgs keyEvent)
    {
        Test t1 = new Test()
        {
            Key = keyEvent.Key,
            Num = (int)keyEvent.Key
        };
        if (!_debugList.Exists(e => e.Key == t1.Key))
        {
            _debugList.Add(t1);
        }
        //_debugLabel.Content =$"Key: {keyEvent.Key.ToString()}\nToogle: {keyEvent.IsToggled.ToString().ToLower()}\nPressed: {keyEvent.IsRepeat.ToString()}";
        byte[]? data = SerializeKeyEvent(keyEvent);
        if (data == null) return;
        _dispatch(data);
    }

    public void Write()
    {
        foreach (Test item  in _debugList)
        {
            byte[] bytes = Encoding.UTF8.GetBytes($"{item.Key} {item.Num}\n");
            _debugFile.Write(bytes, 0, bytes.Length);
        }
        _debugFile.Flush();
        _debugFile.Close();
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

