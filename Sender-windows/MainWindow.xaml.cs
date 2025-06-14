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

namespace Sender_windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ConnectionManager _connectionManager;
    private readonly KeyStateManager _keyStateManager;
    private readonly string _emptyString = "";
    public MainWindow()
    {
        InitializeComponent();
        _connectionManager = new ConnectionManager();

        List<RadioButton> radioButtons = new List<RadioButton>();
        radioButtons.Add(S1Radio);
        radioButtons.Add(S2Radio);
        radioButtons.Add(S3Radio);
        radioButtons.Add(S4Radio);
        radioButtons.Add(S5Radio);
        
        _keyStateManager = new KeyStateManager(_connectionManager, DebugLabel);
        _keyStateManager.Debug(DebugLabel);
    }

    private async void InputKeyField_OnKeyDown(object sender, KeyEventArgs e) => await _keyStateManager.Event(e);
    //private async void MainWindow_OnKeyDown(object sender, KeyEventArgs e){}
    private async void InputKeyField_OnKeyUp(object sender, KeyEventArgs e) => await _keyStateManager.Event(e);
    //private async void MainWindow_OnKeyUp(object sender, KeyEventArgs e) {}

    private async void ConnectButton_OnClick(object sender, RoutedEventArgs e)
        => await _connectionManager.Connect(HostnameTextbox.Text, int.Parse(PortTextbox.Text), int.Parse(HbPortTextbox.Text));

    private void InputKeyField_OnTextChanged(object sender, TextChangedEventArgs e) => ((TextBox)sender).Text = _emptyString;
}