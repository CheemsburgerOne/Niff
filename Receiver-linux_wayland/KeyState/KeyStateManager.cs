using System.Runtime.Serialization;
using CliWrap;
using Receiver_linux_wayland.Network.Payload;

namespace Receiver_linux_wayland.KeyState;

public static partial class KeyState
{
    public class KeyStateManager
    {
        private Command _keySequenceCommand;
        private readonly int _modifierKeyYdtReferenceCount = 4;
        
        private Dictionary<int, Key> Keys { get; } = new Dictionary<int, Key>();

       public KeyStateManager(string executable)
       {
           _keySequenceCommand = CliWrap.Cli.Wrap(executable);
       } 
       public async Task<bool> ProcessEvent(Payload.KeyEventDto dto) 
       {
           if (dto.Modifiers == null) return false;
           List<string> command =  new List<string>();
           command.Add("key");
           BeginModifiers(command, dto.Modifiers);
           
           if (Keys.TryGetValue(dto.WpfId, out var key)) command.AddRange([$"{key.YdtIdentifier}:1",$"{key.YdtIdentifier}:0"]);
           
           EndModifiers(command, dto.Modifiers);

           foreach (var item in command)
           {
               Console.Write(item);
               Console.Write(' ');
           }
           Console.Write(Environment.NewLine);
           
           await _keySequenceCommand.WithArguments(a => a.Add(command)).ExecuteAsync();
           
           return true;
           
           void BeginModifiers(List<string> arguments, bool[] modifiers ) 
           { 
               if (modifiers[0]) arguments.AddRange([$"{(int)Modifiers.Capslock}:1",$"{(int)Modifiers.Capslock}:0"]); 
               if (modifiers[1]) arguments.Add($"{(int)Modifiers.Shift}:1"); 
               if (modifiers[2]) arguments.Add($"{(int)Modifiers.Ctrl}:1"); 
               if (modifiers[3]) arguments.Add($"{(int)Modifiers.Alt}:1"); 
           }
           
           void EndModifiers(List<string> arguments, bool[] modifiers ) 
           { 
               if (modifiers[0]) arguments.AddRange([$"{(int)Modifiers.Capslock}:1",$"{(int)Modifiers.Capslock}:0"]); 
               if (modifiers[1]) arguments.Add($"{(int)Modifiers.Shift}:0"); 
               if (modifiers[2]) arguments.Add($"{(int)Modifiers.Ctrl}:0"); 
               if (modifiers[3]) arguments.Add($"{(int)Modifiers.Alt}:0"); 
           }
       }   
        
        public void LoadFromFile(string translationFilepath)
        {
            translationFilepath = "/home/cheemsburger/RiderProjects/Niff/Receiver-linux_wayland/KeyState/key_definitions";
            //Ensure collections are empty
            Keys.Clear();
            
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
                    throw new ArgumentException($"Key has already been loaded.");
                }
                catch (SerializationException e)
                {
                    throw new ArgumentException($"Failed to load key from \"{definition}\"", e);
                }
            }
        }
        
        private enum Modifiers
        {
            Capslock = 58,
            Shift = 42,
            Ctrl = 29,
            Alt = 100
        }
    }
}
