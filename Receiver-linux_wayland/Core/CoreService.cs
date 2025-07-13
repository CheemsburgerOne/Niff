using CliWrap;
using Receiver_linux_wayland.Network;

namespace Receiver_linux_wayland.Core;

public class CoreService : BackgroundService
{

    private readonly Receiver_linux_wayland.Network.Network.NetworkManager _networkManager;
    private readonly KeyState.KeyState.KeyStateManager _keyStateManager = new();
    private readonly ILogger<CoreService> _systemdlogger;
    private Command _defaultCommand;


    public CoreService(ILogger<CoreService> logger)
    {
        _systemdlogger = logger;
        _networkManager = new Receiver_linux_wayland.Network.Network.NetworkManager(logger);

        _defaultCommand = CliWrap.Cli.Wrap("ydotoold");


    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CoreLoop(stoppingToken);
    }

    private async Task CoreLoop(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Receiver_linux_wayland.Network.Network.Packet? received = await _networkManager.ReceivePacketAsync(stoppingToken);
            received.Flags &= ~PacketFlags.Ack;
            if (received.Flags == PacketFlags.KeyEvent)
            {
                Payload.Payload.KeyEventDto? keyEventDto = received.GetPayloadAsType<Payload.Payload.KeyEventDto>();
                string keyboardCommand = _keyStateManager.Event(keyEventDto.Value);
                var result = _defaultCommand.WithArguments(c =>c.Add(keyboardCommand));
                await result.ExecuteAsync(stoppingToken);
            }
        }

    }   
    
    
}