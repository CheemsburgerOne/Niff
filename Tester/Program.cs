using System.Net;
using System.Net.Sockets;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;

namespace Tester;

class Program
{
    
    static void Main(string[] args)
    {
        Console.Clear();
        Console.WriteLine("------------ Tester-------------");
        Tester().Wait();
    }

    public static async Task Tester()
    {
        while (true)
        {
            Console.WriteLine($"Oczekuję na port");
            byte[] hbBytes = Encoding.UTF8.GetBytes("HB");
            byte[] hbAckBytes = Encoding.UTF8.GetBytes("HBack");
            byte[] byeBytes = Encoding.UTF8.GetBytes("BYE");
            byte[] byeAckBytes = Encoding.UTF8.GetBytes("BYEack");
            UdpClient client = new UdpClient(9998);;
            
            var data = client.ReceiveAsync().Result;
            int portnum = int.Parse(data.Buffer);
            Console.WriteLine($"Otrzymano port {portnum}");
            client.Connect(data.RemoteEndPoint.Address, portnum );
            client.Send(hbAckBytes);

            await Task.Run(async () =>
            {
                while (true)
                {
                    UdpReceiveResult receiveRes;
                    
                    //Receive packet
                    try
                    { 
                        receiveRes = await client.ReceiveAsync();
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("Socket Error");
                        break;
                    }

                    if (hbBytes.SequenceEqual(receiveRes.Buffer))
                    {
                        client.Send(hbAckBytes);
                        Console.WriteLine("Otrzymano HB, odesłano HBack.");
                        continue;
                    }
                    
                    if (byeBytes.SequenceEqual(receiveRes.Buffer))
                    {
                        client.Send(byeAckBytes);
                        Console.WriteLine("Otrzymano BYE, odesłano BYEack.");
                        client.Close();
                        Console.WriteLine("Socket zakmnięty.");
                        break;
                    }

                    KeyEventArgsSerializedDto? dto = DeserializeKeyEventDto(receiveRes.Buffer);
                    KeyEventArgsSerializedDto dto2 = dto!.Value;
                    Console.WriteLine($"Key: {dto2.Key}  Toggle: {dto2.IsToggled}  Repeat:{dto2.IsRepeat}");
                }
            });
        }
    }

    public static KeyEventArgsSerializedDto? DeserializeKeyEventDto(byte[] data)
    {
        try
        {
            return JsonSerializer.Deserialize<KeyEventArgsSerializedDto>(data);
        }
        catch (Exception e)
        {
            return null;
        }
    }
}