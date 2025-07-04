using System.Data;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;

namespace Receiver_linux_wayland.Core;

public static partial class Core
{
    public class Key
    {
        public KeyType Type { get; private set; }
        public string WpfName { get; private set; } = null!;
        public int WpfIdentifier { get; private set; }
        //public string YdtName { get; private set; } 
        public int YdtIdentifier { get; private set; }
        public bool IsActive { get; set; }
        
        public Key(string definition)
        {
            try
            {
                LoadFromString(definition);
            }
            catch (SerializationException ex)
            {
                throw new SerializationException($"Failed to load key from \"{definition}\"", ex);
            }
        }

        private void LoadFromString(string definition)
        {
            string[] parts = definition.Split(' ');
            
            Type myself = typeof(Key);
            if (parts.Length != myself.GetFields(BindingFlags.Default).Length-1) 
                throw new SerializationException("Key definition does not have all necessary fields");

            try
            {
                Type = Enum.Parse<KeyType>(parts[0]);
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