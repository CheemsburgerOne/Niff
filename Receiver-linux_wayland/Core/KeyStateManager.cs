namespace Receiver_linux_wayland.Core;

public class KeyStateManager
{
    public Dictionary<int, Key> Keys { get; private set; } = new Dictionary<int, Key>();
    public LinkedList<Key> Toggled { get; private set; } = new LinkedList<Key>();
    public LinkedList<Key> Pressed { get; private set; } = new LinkedList<Key>();

    public string Event(int item)
    {
        
        
        
    }
    
    public void LoadFromFile(string translationFilepath)
    {
        List<string> lines = File.ReadLines(translationFilepath).ToList();
        foreach (var line in lines)
        {
            string[] parts = line.Split(' ');
            Key key = new Key()
            {
                WpfName = parts[0],
                WpfIdentifier = int.Parse(parts[1]),
                YdtIdentifier = int.Parse(parts[2]),
            };
            try
            {
                Keys.Add(key.WpfIdentifier, key);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Key {key.WpfName} has already been loaded.", e);
            }
        }
    }
}