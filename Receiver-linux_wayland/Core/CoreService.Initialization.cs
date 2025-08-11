using System.Security;
using Receiver_linux_wayland.Network;

namespace Receiver_linux_wayland.Core;

public partial class CoreService
{
    private void ValidateAccessRequiredDirectories()
    {
        try
        {
            _etcDirectory = new DirectoryInfo("/etc/niff");
            _varDirectory = new DirectoryInfo("/var/niff");
        }
        catch (SecurityException ex)
        {
            _systemdLogger.LogCritical(ex, "Insufficient permissions to access the required directories");
            throw new SecurityException(ex.Message);
        }
        catch (Exception ex)
        {
            _systemdLogger.LogCritical(ex, "Incorrect folder path");
            throw new Exception(ex.Message);
        }
    }
        
    void InitializeKeyStateManagerAndYdotoold()
    {
        CancellationTokenSource ydtdStartTokenSource = new CancellationTokenSource(5000);
        
        try
        {
            _keyStateManager = new("ydotool", _etcDirectory!, _varDirectory!, _systemdLogger);
            _ydotooldHelper = new Ydotool.YdotooldHelper(_etcDirectory!, _varDirectory!);
            _ydotooldHelper.Start(ydtdStartTokenSource.Token).Wait();
        }
        catch (Exception ex)
        {
            _systemdLogger.LogCritical(ex, "Unable to start key state utilities");
            throw new Exception(ex.Message);
        }
    }

    void InitializeNetworkManager(ILogger<CoreService> logger)
    {
        try
        {
            _networkManager = new NetworkManager(logger, _etcDirectory!, 12015);
        }
        catch (Exception ex)
        {
            _systemdLogger.LogCritical(ex, "Unable to start network manager");
            throw new Exception(ex.Message);
        }
    }
}