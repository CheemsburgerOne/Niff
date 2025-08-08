using System.Diagnostics.CodeAnalysis;
using CliWrap;

namespace Receiver_linux_wayland.Ydotool;

public class YdotoolHelper
{
    private readonly string _executable;
    private readonly string _varPath;

    private Command _keyCmdBase;
    
    public YdotoolHelper(string executable, string varPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(executable);
        ArgumentException.ThrowIfNullOrEmpty(varPath);
        
        _executable = executable;
        _varPath = varPath;
        
        _keyCmdBase = Cli.Wrap(_executable)
            .WithEnvironmentVariables(
                builder => builder.Set(
                    "YDOTOOL_SOCKET", 
                    $"{_varPath}/.ydotoold_socket".Replace("//","/")
                )
            )
            .WithValidation(CommandResultValidation.None);
    }
    
    public async Task<bool> Key(IEnumerable<string> keySequence)
    {
        List<string> commandArgs = ["key"];
        commandArgs.AddRange(keySequence);
        var fullCmd = _keyCmdBase.WithArguments(commandArgs);
        var result = await fullCmd.ExecuteAsync();
        return result.IsSuccess;
    }
}