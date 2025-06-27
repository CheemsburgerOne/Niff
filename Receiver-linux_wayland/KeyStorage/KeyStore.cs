namespace Receiver_linux_wayland.KeyStorage;

public class KeyStore
{
    Dictionary<string, Key> _keys = new Dictionary<string, Key>();
//#define KEY_D         32
    public bool Load(string ydotooldKeyDefinitionsFile, string wpfToYdotooldTranslationFile)
    {
        List<string > ydotoolKeyDefinitions = File.ReadLines(ydotooldKeyDefinitionsFile).ToList();
        List<string> wpfKeyReference = File.ReadLines(wpfToYdotooldTranslationFile).ToList();

        foreach (var definition in ydotoolKeyDefinitions)
        {
            definition.
            
        }
        
        
        
        
    }

    private string NormalizeDefinition(string definition)
    {
        int secondSpaceStartIndex = 0;
        int textStartIndex;
        foreach (var c in definition[7..])
        {
            
            
        }
        
    }
}