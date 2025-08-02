using CliWrap;

namespace Receiver_linux_wayland.Ydotool;

public class YdotooldHelper
{
    private string _executable;
    private string _socketPath;
    
    private Command _startCmd;
    private Command _stopCmd;
    private Command _restartCmd;
    
    public YdotooldHelper(string executable, string socketPath)
    {
        _executable = executable;
        _socketPath = socketPath;
        Init();
    }
    
    private void Init()
    {
        _startCmd = Cli.Wrap(_executable).WithArguments([$"--socket-path={_socketPath}/.ydotoold_socket","--socket-own=1000:1000"] );
        _stopCmd = Cli.Wrap("pkill").WithArguments(_executable);
    }

    public async Task<bool> Start()
    {
        var result = await _startCmd.ExecuteAsync();
        return result.IsSuccess;
    }

    public async Task<bool> Stop()
    {
        var result = await _stopCmd.ExecuteAsync();
        return result.ExitCode is 0 or 1;
    }

    public async Task<bool> Restart()
    {
        bool stopResult = await Stop();
        if (!stopResult) return false;

        bool startResult = await Start();
        return startResult;
    }
    
}