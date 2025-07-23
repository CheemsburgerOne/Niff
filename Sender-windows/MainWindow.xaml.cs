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
    private KeyState.KeyState.KeyStateManager _keyStateManager = new KeyState.KeyState.KeyStateManager();
    
    // private readonly Progress<(int, string?, bool?)> _radioButtonsProgress;
    // private readonly Progress<string> _heartbeatStatusLabelProgress;
    
    private readonly string _emptyString = "";
    private readonly List<RadioButton> _radioButtons;
    
    public MainWindow()
    {
        // this._keyPressedLabelProgress = keyPressedLabelProgress;
        InitializeComponent();
        // _radioButtonsProgress = new Progress<(int, string?, bool?)>(tuple => ModifyRadioButton(tuple.Item1, tuple.Item2, tuple.Item3));
        // _heartbeatStatusLabelProgress = new Progress<string>(content => HeartbeatStatusLabel.Content = content);
    }

    private async void ConnectButton_OnClick(object sender, RoutedEventArgs e)
    {
        _networkManager = new Network.Network.NetworkManager();
        var success = await _networkManager.TryConnect("1",2);
        if (success) SetControlsUserConnected();
    }
    private async void DisconnectButton_OnClick(object sender, RoutedEventArgs e)
    {
        await _networkManager!.Disconnect();
        SetControlsUserNotConnected();
    }

    private void SetControlsUserConnected()
    {
        DisconnectButton.IsEnabled = true;
        HostnameTextbox.IsEnabled = false;
        PortTextbox.IsEnabled = false;
        ConnectButton.IsEnabled = false;
    }


    private void SetControlsUserNotConnected()
    {
        HostnameTextbox.IsEnabled = true;
        PortTextbox.IsEnabled = true;
        ConnectButton.IsEnabled = true;
        DisconnectButton.IsEnabled = false;
    }
    private void DispatchKeyEvent(object sender, KeyEventArgs e)
    {
        if (_networkManager == null || !_networkManager.Connected) return;
        if (_keyStateManager.IsModifierKey(e.Key) || e.IsUp) return;
        Payload.KeyEventDto? dto = _keyStateManager.CreateKeyEventPayload(e);
        _networkManager.SendPacket<Payload.KeyEventDto>(Network.Network.PacketFlags.KeyEvent, dto);
    }

    private void OpenLoadRsaPublicKeyPemFileDialog(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
        if (openFileDialog.ShowDialog() == true)
        {
            string filePath = openFileDialog.FileName;
            MessageBox.Show($"Selected file: {filePath}");
        }
    }
}