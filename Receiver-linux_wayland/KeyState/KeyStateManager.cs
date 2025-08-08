using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using System.Xml;
using CliWrap;
using Receiver_linux_wayland.Core;
using Receiver_linux_wayland.Network.Payload;
using Receiver_linux_wayland.Ydotool;

namespace Receiver_linux_wayland.KeyState;

public static partial class KeyState
{
    public class KeyStateManager
    {
        private Ydotool.YdotoolHelper _ydotooldHelper;
        private ILogger<CoreService> _logger;
        private static readonly int ModifiersCount = 4;
        
        private Dictionary<int, Key> Keys { get; } = new Dictionary<int, Key>(); 
        public KeyStateManager(string executable, string varPath, string etcPath, ILogger<CoreService> logger)
        {
            ArgumentException.ThrowIfNullOrEmpty(executable);
            ArgumentException.ThrowIfNullOrEmpty(varPath);
            ArgumentException.ThrowIfNullOrEmpty(etcPath);

            try
            {
                LoadFromTranslationFile(etcPath);
            }
            catch(Exception ex)
            {
                ExceptionDispatchInfo.Capture(ex).Throw();
            }
            
            _logger = logger;
            _ydotooldHelper = new YdotoolHelper(executable, varPath);
        } 
       public async Task ProcessEvent(Payload.KeyEventDto dto )
       {
           if (dto.Modifiers == null) throw new ArgumentNullException(nameof(dto));
           if (dto.Modifiers.Length != ModifiersCount) throw new ArgumentException($"Modifiers count != {ModifiersCount}");
           
           if ( !Keys.TryGetValue(dto.WpfId, out var key) ) throw new KeyNotFoundException($"{dto.WpfId}");
           
           List<string> keySequence =  new List<string>();
           BeginModifiers(keySequence, dto.Modifiers);
           keySequence.AddRange([$"{key.YdtIdentifier}:1",$"{key.YdtIdentifier}:0"]);
           EndModifiers(keySequence, dto.Modifiers);

           if (!await _ydotooldHelper.Key(keySequence)) throw new ApplicationException("Ydotool failed to execute");
       }
       
       private void EndModifiers(List<string> arguments, bool[] modifiers)
       {
           if (modifiers[0]) arguments.AddRange([$"{(int)Modifiers.Capslock}:1",$"{(int)Modifiers.Capslock}:0"]); 
           if (modifiers[1]) arguments.Add($"{(int)Modifiers.Shift}:0"); 
           if (modifiers[2]) arguments.Add($"{(int)Modifiers.Ctrl}:0"); 
           if (modifiers[3]) arguments.Add($"{(int)Modifiers.Alt}:0");
       }

       private void BeginModifiers(List<string> arguments, bool[] modifiers)
       {
           if (modifiers[0]) arguments.AddRange([$"{(int)Modifiers.Capslock}:1",$"{(int)Modifiers.Capslock}:0"]); 
           if (modifiers[1]) arguments.Add($"{(int)Modifiers.Shift}:1"); 
           if (modifiers[2]) arguments.Add($"{(int)Modifiers.Ctrl}:1"); 
           if (modifiers[3]) arguments.Add($"{(int)Modifiers.Alt}:1");
       }
       
       /// <summary>
       /// Load definitions from the config file into the dictionary
       /// </summary>
       /// <param name="etcPath">Path to directory structure /translation/translation.config</param>
       /// <exception cref="IOException">File could not be loaded</exception>
       /// <exception cref="ArgumentException">Definition is not valid</exception>
       private void LoadFromTranslationFile(string etcPath)
       {
           string translationFilePath = $"{etcPath}/translation/translation.config".Replace("//","/");
           
           List<string> definitions;
           
           try
           { 
               definitions = File.ReadLines(translationFilePath).ToList();
           }
           catch(Exception ex)
           {
               throw new IOException($"Failed to read translation file {translationFilePath}", ex);
           }
           
            //Ensure collections are empty
            Keys.Clear();
            
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
