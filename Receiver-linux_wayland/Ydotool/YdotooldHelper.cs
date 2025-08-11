using CliWrap;

namespace Receiver_linux_wayland.Ydotool;

public class YdotooldHelper
{
    private DirectoryInfo _etcDir;
    private DirectoryInfo _varDir;
    private DirectoryInfo _scriptDir;
    
    private Command _startCmd;
    private Command _stopCmd;
    private Command _restartCmd;
    
    public YdotooldHelper(DirectoryInfo etcDir, DirectoryInfo varDir)
    {
        _etcDir = etcDir;
        _varDir = varDir;
        _scriptDir = etcDir.GetDirectories("remote").Single();
        
        var baseCmd = Cli.Wrap($"{_etcDir.FullName}/scripts/ydotoold-helper.sh");
        _startCmd = baseCmd.WithArguments(["start",$"{_varDir.FullName}/","7770" ]).WithValidation(CommandResultValidation.None);
        _stopCmd = baseCmd.WithArguments("stop").WithValidation(CommandResultValidation.None);
    }

    public Task<bool> Start() => Start(CancellationToken.None);
    public async Task<bool> Start(CancellationToken ct)
    {
        CancellationTokenSource? cts = null;
        cts = ct.IsCancellationRequested ? new CancellationTokenSource(5000) : new CancellationTokenSource();
        
        var result = await _startCmd.ExecuteAsync(cts.Token);
        return result.ExitCode is 0;
    }

    public async Task<bool> Stop(CancellationToken ct)
    {
        CancellationTokenSource? cts = null;
        cts = ct.IsCancellationRequested ? new CancellationTokenSource(5000) : new CancellationTokenSource(); 
        
        var result = await _stopCmd.ExecuteAsync();
        return result.ExitCode is 0 or 1;
    }
}