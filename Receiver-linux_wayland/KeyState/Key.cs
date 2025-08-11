using System.Reflection;
using System.Runtime.Serialization;

namespace Receiver_linux_wayland.KeyState;

public static partial class KeyState
{
    public class Key
    {
        public string WpfName { get; private set; }
        public int WpfIdentifier { get; private set; }
        public int YdtIdentifier { get; private set; }
        
        public Key(string definition)
        {
            try
            {
                LoadFromDefinition(definition);
            }
            catch (SerializationException ex)
            {
                throw new SerializationException($"Failed to load key from \"{definition}\"", ex);
            }
        }

        private void LoadFromDefinition(string definition)
        {
            string[] parts = definition.Split(' ');
            
            Type myself = typeof(Key);
            if (parts.Length -1 != myself.GetProperties().Length) 
                throw new SerializationException($"Key definition has too few arguments: {myself.GetProperties().Length} ");

            try
            {
                WpfName = parts[1];
                WpfIdentifier = int.Parse(parts[2]);
                YdtIdentifier = int.Parse(parts[3]);
            }
            catch (Exception ex)
            {
                throw new SerializationException("Key definition could not be parsed", ex);
            }
        }
    }
    
    public enum KeyType
    {
        Single = 0,
        Pressed = 1,
        Toggled = 2
    }
}