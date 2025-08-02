using CliWrap;

namespace Receiver_linux_wayland.Ydotool;

public class YdotoolHelper
{
    private string _executable;
    private string _socketPath;

    private Command _keyCmdBase;
    
    public YdotoolHelper(string executable, string socketPath)
    {
        _executable = executable;
        _socketPath = socketPath;
    }
    
    private void Init(string socketPath)
    {
        _keyCmdBase = Cli.Wrap(_executable)
            .WithEnvironmentVariables(builder => builder.Set("YDOTOOL_SOCKET", $"{_socketPath}/.ydotoold_socket"));
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