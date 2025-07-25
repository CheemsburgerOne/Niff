using System.Diagnostics;
using CliWrap;
using Receiver_linux_wayland.Network;
using Receiver_linux_wayland.Network.Payload;

namespace Receiver_linux_wayland.Core;

public class CoreService : BackgroundService
{

    private Receiver_linux_wayland.Network.Network.NetworkManager _networkManager;
    private KeyState.KeyState.KeyStateManager _keyStateManager;
    private readonly ILogger<CoreService> _systemdlogger;

    public CoreService(ILogger<CoreService> logger)
    {
        _systemdlogger = logger;
        InitializeKeyStateManager();
        InitializeNetworkManager();
        
        void InitializeKeyStateManager()
        {
            _keyStateManager = new("ydotool");
            _keyStateManager.LoadFromFile("as");
        }

        void InitializeNetworkManager()
        {
            _networkManager = new Receiver_linux_wayland.Network.Network.NetworkManager(logger);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CoreLoop(stoppingToken);
    }

    private async Task CoreLoop(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!_networkManager.Connected) await _networkManager.EstablishNewConnection();
            
            Receiver_linux_wayland.Network.Network.Packet? received =
                await _networkManager.ReceivePacket();

            if(received == null) continue;

            switch (_networkManager.PeerState)
            {
                case PeerState.Disconnected:
                    break;
                case PeerState.Connected:
                    if (received.Flags == PacketFlags.Hello)
                    {
                        Payload.HelloDto helloDto = received.GetPayloadAsType<Payload.HelloDto>();
                        await _networkManager.EstablishEncryptionWithRemotePeer(helloDto);
                        continue;
                    }
                    
                    if (received.Flags == PacketFlags.Bye)
                    {
                        
                        continue;
                    }
                    break;
                case PeerState.ConnectedEncrypted:
                    if (received.Flags == PacketFlags.KeyEvent)
                    {
                        Payload.KeyEventDto keyEventDto = received.GetPayloadAsType<Payload.KeyEventDto>();
                        await _keyStateManager.ProcessEvent(keyEventDto);
                    }
                    break;
                default:
                    break;
                
            }
        }
    }
}