namespace Receiver_linux_wayland.KeyStorage;

public static class KeyIdentifiersFileLoader
{
    public static void Load(string path)
    {
        IEnumerable<string> files = File.ReadLines(path);`

    }
    
}