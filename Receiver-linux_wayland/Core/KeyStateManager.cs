using System.Runtime.Serialization;

namespace Receiver_linux_wayland.Core;

public static partial class Core
{
    public class KeyStateManager
    {
        private Dictionary<int, Key> Keys { get; } = new Dictionary<int, Key>();
        /// <summary>
        /// Refer to KeyType.Pressed definition
        /// </summary>
        private LinkedList<Key> Pressed { get; } = new LinkedList<Key>();
        /// <summary>
        /// Refer to KeyType.Toggled definition
        /// </summary>
        private LinkedList<Key> Toggled { get; } = new LinkedList<Key>();
        
        public string? Event(KeyEventDto dto)
        {
            //If WpfIdentifier does not exist return null string 
            if (!Keys.TryGetValue(dto.WpfIdentifier, out Key? key)) return null;
            
            switch (key.Type)
            {
                case KeyType.Single:
                    return SingleCommand(key.YdtIdentifier);
                
                case KeyType.Pressed:
                    switch (dto.IsActive)
                    {
                        //There might be inconsistencies with key events so we make sure we do not add duplicate
                        case true when Pressed.All(e => e.WpfIdentifier != dto.WpfIdentifier):
                            Pressed.AddLast(key);
                            return PressedDownCommand(key.YdtIdentifier);
                        //Make sure we do not delete key that was not present in the list
                        case false when Pressed.Any(e => e.WpfIdentifier == dto.WpfIdentifier):
                            Pressed.Remove(key);
                            return PressedUpCommand(key.YdtIdentifier);
                        default:
                            return null;
                    }
                //In case of toggles such as CapsLock and NumLock it is crucial to keep states synchronized
                case KeyType.Toggled:
                    switch (dto.IsActive)
                    {
                        //There might be inconsistencies with key events so we make sure we do not add duplicate
                        case true when Toggled.All(e => e.WpfIdentifier != dto.WpfIdentifier):
                            Toggled.AddLast(key);
                            return SingleCommand(key.YdtIdentifier);
                        //Make sure we do not delete key that was not present in the list
                        case false when Toggled.Any(e => e.WpfIdentifier == dto.WpfIdentifier):
                            Toggled.Remove(key);
                            return SingleCommand(key.YdtIdentifier);
                        default:
                            return null;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }   
        
        public void LoadFromFile(string translationFilepath)
        {
            //Ensure collections are empty
            Keys.Clear();
            Pressed.Clear();
            Toggled.Clear();
            
            List<string> definitions = File.ReadLines(translationFilepath).ToList();
            foreach (var definition in definitions)
            {
                try
                {
                    Key key = new Key(definition);
                    Keys.Add(key.WpfIdentifier, key);
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException($"Key has already been loaded.", e);
                }
                catch (SerializationException e)
                {
                    throw new ArgumentException($"Failed to load key from \"{definition}\"", e);
                }
            }
        }

        private string SingleCommand(int ydtIdentifier) => $"{ydtIdentifier}:1 {ydtIdentifier}:0";
        private string PressedDownCommand(int ydtIdentifier) => $"{ydtIdentifier}:1";
        private string PressedUpCommand(int ydtIdentifier) => $"{ydtIdentifier}:0";
    }
}
