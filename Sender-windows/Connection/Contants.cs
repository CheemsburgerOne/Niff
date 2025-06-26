using System.Text;

namespace Sender_windows.Connection;

public partial class Connection
{ 
    //Periodic heartbeat data
    private static readonly byte[] HeartbeatReferenceBytes = Encoding.UTF8.GetBytes($"HB");
    
    private static readonly byte[] HeartbeatAcknowledgeReferenceBytes = Encoding.UTF8.GetBytes($"HBack");
    private static int _heartbeatAcknowledgeReferenceLength;
    
    private static readonly byte[] ByeReferenceBytes = Encoding.UTF8.GetBytes($"BYE");
    private static int _byeReferenceLength;
    
    private static readonly byte[] ByeAcknowledgeReferenceBytes = Encoding.UTF8.GetBytes($"BYEack");
    private static int _byeAcknowledgeReferenceLength;
    
}