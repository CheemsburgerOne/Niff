using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Sender_windows.Network.Payload;

namespace Sender_windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Network.Network.NetworkManager? _networkManager = null;
    private readonly KeyState.KeyState.KeyStateManager _keyStateManager = new KeyState.KeyState.KeyStateManager();

    private bool _isFullCaptureOn = false;
    
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void ConnectButton_OnClick(object sender, RoutedEventArgs e)
    {
        _networkManager = new Network.Network.NetworkManager();
        var connectTask = _networkManager.TryConnect( ConnectionStringTextbox.Text, 5000);
        
        SetControlsUserConnected();
        
        if (!await connectTask)
        {
            SetControlsUserNotConnected();
            await _networkManager!.Disconnect();
            MessageBox.Show("Remote host is not reachable.", "Connection failed");
            return;
        }

        if (await _networkManager.EstablishEncryptionWithRemotePeer()) return;
        await _networkManager!.Disconnect();
        SetControlsUserNotConnected();
        MessageBox.Show("Failed to establish encryption with remote peer.", "Connection failed");
    }
    private async void DisconnectButton_OnClick(object sender, RoutedEventArgs e)
    {
        await _networkManager!.Disconnect();
        SetControlsUserNotConnected();
    }

    private void SetControlsUserConnected()
    {
        DisconnectButton.IsEnabled = true;
        ConnectionStringTextbox.IsEnabled = false;
        PortTextbox.IsEnabled = false;
        ConnectButton.IsEnabled = false;
        
        _isFullCaptureOn = true;
    }

    private void SetControlsUserNotConnected()
    {
        ConnectionStringTextbox.IsEnabled = true;
        PortTextbox.IsEnabled = true;
        ConnectButton.IsEnabled = true;
        DisconnectButton.IsEnabled = false;
    }
    
    private void GlobalPreviewKeyDownHandler(object sender, KeyEventArgs e)
    {
        if (_isFullCaptureOn == false) return;
        if (_networkManager == null || !_networkManager.Connected) return;
        if (_keyStateManager.IsModifierKey(e.Key) || e.IsUp) return;
        e.Handled = true;
        Payload.KeyEventDto? dto = _keyStateManager.CreateKeyEventPayload(e);
        _networkManager.SendPacket<Payload.KeyEventDto>(Network.Network.PacketFlags.KeyEvent, dto);
    }
}