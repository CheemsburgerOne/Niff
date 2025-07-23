using System.IO;
using System.Runtime.Serialization;
using System.Windows.Input;
using Sender_windows.Network.Payload;

namespace Sender_windows.KeyState;

public static partial class KeyState
{
    public class KeyStateManager
    {
        private readonly bool[] _modifiersStateBuffer = new bool[4];

        public Payload.KeyEventDto? CreateKeyEventPayload(KeyEventArgs key) => new Payload.KeyEventDto((int) key.Key, GetModifiers());
        
        public bool IsModifierKey(Key key) =>
            key is 
                Key.CapsLock or
                Key.LeftCtrl or
                Key.RightCtrl or
                Key.LeftShift or
                Key.RightShift or
                Key.LeftAlt or
                Key.RightAlt;
        
        private bool[] GetModifiers()
        {
            _modifiersStateBuffer[0] = Keyboard.IsKeyToggled(Key.CapsLock);
            _modifiersStateBuffer[1] = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            _modifiersStateBuffer[2] = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            _modifiersStateBuffer[3] = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
            return _modifiersStateBuffer;
        }
        
        // public void LoadFromFile(string translationFilepath)
        // {
        //     //Ensure collections are empty
        //     Keys.Clear();
        //     
        //     List<string> definitions = File.ReadLines(translationFilepath).ToList();
        //     foreach (var definition in definitions)
        //     {
        //         try
        //         {
        //             Key key = new Key(definition);
        //             Keys.Add(key.WpfIdentifier, key);
        //         }
        //         catch (ArgumentException e)
        //         {
        //             throw new ArgumentException($"Key has already been loaded.");
        //         }
        //         catch (SerializationException e)
        //         {
        //             throw new ArgumentException($"Failed to load key from \"{definition}\"", e);
        //         }
        //     }
        // }
    }
}