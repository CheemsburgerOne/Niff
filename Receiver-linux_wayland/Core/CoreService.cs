using CliWrap;
using Receiver_linux_wayland.Network;
using Receiver_linux_wayland.Network.Payload;

namespace Receiver_linux_wayland.Core;

public class CoreService : BackgroundService
{

    private readonly Receiver_linux_wayland.Network.Network.NetworkManager _networkManager;
    private readonly KeyState.KeyState.KeyStateManager _keyStateManager = new("ydotool");
    private readonly ILogger<CoreService> _systemdlogger;


    public CoreService(ILogger<CoreService> logger)
    {
        _systemdlogger = logger;
        _networkManager = new Receiver_linux_wayland.Network.Network.NetworkManager(logger);
        _keyStateManager.LoadFromFile("as");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CoreLoop(stoppingToken);
    }

    private async Task CoreLoop(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!_networkManager.Connected)
            {
                await _networkManager.AwaitNewConnection();
            }

            try
            {
                Receiver_linux_wayland.Network.Network.Packet? received =
                    await _networkManager.ReceivePacketAsync(stoppingToken);
                
                Payload.KeyEventDto? dto = received.GetPayloadAsType<Payload.KeyEventDto>();
                
                await _keyStateManager.ProcessEvent(dto.Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
    }
    
    
}