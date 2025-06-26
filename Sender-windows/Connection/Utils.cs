using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls;

namespace Sender_windows.Connection;

public partial class Connection
{ 
    private void ReportHeartbeatStatusNok(int n, int causeCode) => 
        _heartbeatLabelProgress.Report(
            causeCode == 0
            ? $"Heartbeat NOK ({n}). Last cause: Invalid response"
            : $"Heartbeat NOK ({n}). Last cause: Timeout");
    
    private void ReportHeartbeatStatusOk(int n) => _heartbeatLabelProgress.Report($"Heartbeat OK ({n})");
}

