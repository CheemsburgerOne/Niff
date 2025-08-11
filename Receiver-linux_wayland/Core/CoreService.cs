using System.Diagnostics;
using System.Security;
using CliWrap;
using Receiver_linux_wayland.Network;
using Receiver_linux_wayland.Network.Packet;
using Receiver_linux_wayland.Network.Payload;

namespace Receiver_linux_wayland.Core;

public partial class CoreService : BackgroundService
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
    
    private bool _initFailed = false;

    public CoreService(ILogger<CoreService> logger)
    {
        _systemdLogger = logger;
        
        try
        {
            ValidateAccessRequiredDirectories();
            InitializeKeyStateManagerAndYdotoold();
            InitializeNetworkManager(logger);
        }
        catch(Exception ex)
        {
            _initFailed = true;
            _systemdLogger.LogCritical(ex, "Program failed to initialize");
        }
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_initFailed) Environment.Exit(1);
        await CoreLoop(stoppingToken);
        Shutdown();
    }

    private async Task CoreLoop(CancellationToken stoppingToken)
    {
        _networkManager.WithCancellation(stoppingToken);
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
                        _networkManager.ExchangePublicRsaKeysPemWithRemoteHost(helloDto);
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
    private void Shutdown()
    {
        try
        {
            CancellationTokenSource cts = new CancellationTokenSource(4000);
            _ydotooldHelper.Stop(cts.Token).Wait();
            if ( _networkManager.PeerState != PeerState.Disconnected){ _networkManager.Disconnect();}
            
            
        }
        catch
        {
            _systemdLogger.LogWarning("Appllication failed to kill ydotoold process");
        }
    }
}