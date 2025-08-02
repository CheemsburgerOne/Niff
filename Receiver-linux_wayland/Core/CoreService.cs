using System.Diagnostics;
using CliWrap;
using Receiver_linux_wayland.Network;
using Receiver_linux_wayland.Network.Packet;
using Receiver_linux_wayland.Network.Payload;

namespace Receiver_linux_wayland.Core;

public class CoreService : BackgroundService
{
    private NetworkManager _networkManager;
    
    private KeyState.KeyState.KeyStateManager _keyStateManager;
    
    private Ydotool.YdotooldHelper _ydotooldHelper;
    
    private readonly ILogger<CoreService> _systemdlogger;

    public CoreService(ILogger<CoreService> logger)
    {
        _systemdlogger = logger;
        InitializeKeyStateManagerAndYdotoold();
        InitializeNetworkManager();
        
        void InitializeKeyStateManagerAndYdotoold(string socketPath = "/var/niff", string translationPath = "/etc/niff/")
        {
            _keyStateManager = new("ydotool", socketPath);
            _keyStateManager.LoadFromFile(translationPath);
            
            _ydotooldHelper = new Ydotool.YdotooldHelper("ydotoold", socketPath);
            _ydotooldHelper.Start().Wait();
        }

        void InitializeNetworkManager()
        {
            _networkManager = new NetworkManager(logger, "/etc/niff");
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
            //This is neccessary because we cannot wait for a packet from a disconnected user
            if (_networkManager.PeerState == PeerState.Disconnected)
            {
                await _networkManager.WaitNewPeerThenEstablishConnection();
            }
            
            Network.Packet.Network.Packet? received = await _networkManager.ReceivePacket();
            if ( received == null ) continue;

            switch ( _networkManager.PeerState )
            {
                case PeerState.Disconnected:
                    break;
                case PeerState.Connected:
                    if (received.Flags == PacketFlags.Hello)
                    {
                        Payload.HelloDto helloDto = received.GetPayloadAsType<Payload.HelloDto>();
                        await _networkManager.ExchangePublicRsaKeysPemWithRemoteHost(helloDto);
                    }
                    break;
                case PeerState.ConnectedEncrypted:
                    if (received.Flags == PacketFlags.KeyEvent)
                    {
                        Payload.KeyEventDto keyEventDto = received.GetPayloadAsType<Payload.KeyEventDto>();
                        await _keyStateManager.ProcessEvent(keyEventDto);
                    }
                    
                    if (received.Flags == PacketFlags.Bye) _networkManager.Disconnect();
                    break;
                default:
                    break;
                
            }
        }
    }
}