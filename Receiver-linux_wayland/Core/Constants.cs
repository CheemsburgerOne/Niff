using System.Text;

namespace Receiver_linux_wayland.Core;

public static partial class Core
{
    public static class Constants
    {
        public static byte[] Bytes = Encoding.UTF8.GetBytes("HB");
        public static byte[] HbAckBytes = Encoding.UTF8.GetBytes("HBack");
        public static byte[] ByeBytes = Encoding.UTF8.GetBytes("BYE");
        public static byte[] ByeAckBytes = Encoding.UTF8.GetBytes("BYEack");
    }
}
