using CliWrap;

namespace Receiver_linux_wayland.Ydotool;

public class YdotooldHelper
{
    private string _etcPath;
    private string _varPath;
    
    private Command _startCmd;
    private Command _stopCmd;
    private Command _restartCmd;
    
    public YdotooldHelper(string etcPath, string varPath)
    {
        _etcPath = etcPath;
        _varPath = varPath;
        Init();
    }
    
    private void Init()
    {
        var start = Cli.Wrap($"{_etcPath}/scripts/ydotoold-helper.sh");
        _startCmd = start.WithArguments(["start",$"{_varPath}/","7770" ]).WithValidation(CommandResultValidation.None);
        _stopCmd = start.WithArguments("stop").WithValidation(CommandResultValidation.None);
    }

    public async Task<bool> Start()
    {
        var result = await _startCmd.ExecuteAsync();
        return result.ExitCode is 0;
    }

    public async Task<bool> Stop()
    {
        var result = await _stopCmd.ExecuteAsync();
        return result.ExitCode is 0 or 1;
    }
}