using System.Diagnostics;
using System.Security;
using CliWrap;
using Receiver_linux_wayland.Network;
using Receiver_linux_wayland.Network.Packet;
using Receiver_linux_wayland.Network.Payload;

namespace Receiver_linux_wayland.Core;

public class CoreService : BackgroundService
{
    //Directories
    private DirectoryInfo _etcDirectory;
    private DirectoryInfo _varDirectory;
    
    //Modules
    private NetworkManager _networkManager;
    private KeyState.KeyState.KeyStateManager _keyStateManager;
    
    //Ydotoold helper
    private Ydotool.YdotooldHelper _ydotooldHelper;
    
    //Logger
    private readonly ILogger<CoreService> _systemdLogger;

    public CoreService(ILogger<CoreService> logger)
    {
        _systemdLogger = logger;
        ValidateAccessRequiredDirectories();
        InitializeKeyStateManagerAndYdotoold();
        InitializeNetworkManager();

        void ValidateAccessRequiredDirectories()
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
            _keyStateManager = new("ydotool", _varDirectory.FullName, _etcDirectory.FullName);
            _ydotooldHelper = new Ydotool.YdotooldHelper(_etcDirectory.FullName, _varDirectory.FullName);
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
        List<Network.Packet.Network.Packet> packets;
        while (!stoppingToken.IsCancellationRequested)
        {
            //This is neccessary because we cannot wait for a packet from a disconnected user
            if (_networkManager.PeerState == PeerState.Disconnected)
            {
                try
                {
                    await _networkManager.WaitNewPeerThenEstablishConnection(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
            
            packets = await _networkManager.ReceivePackets(stoppingToken);

            foreach (var packet in packets) { await ProcessPacket(packet); }
            _networkManager.Clear();
        }
        
        return;

        async Task ProcessPacket(Network.Packet.Network.Packet packet)
        {
            switch ( _networkManager.PeerState )
            {
                case PeerState.Disconnected:
                    break;
                case PeerState.Connected:
                    if (packet.Flags == PacketFlags.Hello)
                    {
                        Payload.HelloDto helloDto = packet.GetPayloadAsType<Payload.HelloDto>();
                        await _networkManager.ExchangePublicRsaKeysPemWithRemoteHost(helloDto);
                    }
                    break;
                case PeerState.ConnectedEncrypted:
                    if (packet.Flags == PacketFlags.KeyEvent)
                    {
                        Payload.KeyEventDto keyEventDto = packet.GetPayloadAsType<Payload.KeyEventDto>();
                        await _keyStateManager.ProcessEvent(keyEventDto);
                    }
                    
                    if (packet.Flags == PacketFlags.Bye) _networkManager.Disconnect();
                    break;
                default:
                    break;
            }
            return;
        }
    }
}