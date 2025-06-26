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
using Sender_windows.Connection;

namespace Sender_windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Connection.Connection _connection;
    private KeyStateManager _keyStateManager;
    
    private Progress<(int, string?, bool?)> radioButtonsProgress;
    private Progress<string> heartbeatStatusLabelProgress;
    private Progress<string> keyPressedLabelProgress;
    
    private readonly string _emptyString = "";
    private readonly List<RadioButton> _radioButtons;
    
    public MainWindow()
    {
        InitializeComponent();
        
        _radioButtons =
        [
            S1Radio,
            S2Radio,
            S3Radio,
            S4Radio,
            S5Radio
        ];

        radioButtonsProgress = new Progress<(int, string?, bool?)>(tuple => ModifyRadioButton(tuple.Item1, tuple.Item2, tuple.Item3));
        heartbeatStatusLabelProgress = new Progress<string>(content => HeartbeatStatusLabel.Content = content);
    }

    private async void ConnectButton_OnClick(object sender, RoutedEventArgs e)
    {
        _connection = new Connection.Connection(radioButtonsProgress, heartbeatStatusLabelProgress);
        // await _connectionManager.Connect(
        //     HostnameTextbox.Text, 
        //     int.Parse(PortTextbox.Text),
        //     int.Parse(HbPortTextbox.Text));
        HostnameTextbox.IsEnabled = false;
        PortTextbox.IsEnabled = false;
        HbPortTextbox.IsEnabled = false;
        ConnectButton.IsEnabled = false;

        bool success = await _connection.Connect(HostnameTextbox.Text, 0, 0);
        _keyStateManager = new KeyStateManager(_connection.Send);
        //If connecting failed revert states
        if (!success)
        { 
            HostnameTextbox.IsEnabled = true;
            PortTextbox.IsEnabled = true;
            HbPortTextbox.IsEnabled = true;
            ConnectButton.IsEnabled = true;
        }
        
        DisconnectButton.IsEnabled = true;
    }
    private async void DisconnectButton_OnClick(object sender, RoutedEventArgs e)
    {
        await _connection.Disconnect();
        HostnameTextbox.IsEnabled = true;
        PortTextbox.IsEnabled = true;
        HbPortTextbox.IsEnabled = true;
        ConnectButton.IsEnabled = true;
        DisconnectButton.IsEnabled = false;
    }

    private void InputKeyField_OnTextChanged(object sender, TextChangedEventArgs e) => ((TextBox)sender).Text = _emptyString;

    /// <summary>
    /// Modify the desired radio button;
    /// </summary>
    /// <param name="index">Index / column of the radio starting from 0</param>
    /// <param name="content"></param>
    /// <param name="active"></param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when accessed index does not exist</exception>
    private void ModifyRadioButton(int index, string? content = null, bool? active = null)
    {
        if (index < 0 || index > _radioButtons.Count -1 ) throw new ArgumentOutOfRangeException(nameof(index));
        if (active == false) throw new ArgumentException("Active cannot be set to false");

        if (active.HasValue) _radioButtons[index].IsChecked = active.Value;
        if (content != null) _radioButtons[index].Content = content;
    }
    
    private void DispatchKeyEvent(object sender, KeyEventArgs e)
    {
        _keyStateManager.Event(e);
    }
}