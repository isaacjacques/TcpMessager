using System.Net;
using TcpMessager;

IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
int port = 999;

TcpServer server = new TcpServer(ipAddress, port);
server.Start();

Console.WriteLine("Press Enter to stop the server...");
Console.ReadLine();

server.Stop();