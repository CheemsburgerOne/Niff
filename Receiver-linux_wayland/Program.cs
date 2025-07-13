namespace Receiver_linux_wayland;


public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddSystemd();
        builder.Services.AddHostedService<Core.CoreService>();
        var host = builder.Build();
        host.Run();
    }
}