using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpMessager
{
    public class TcpServer
    {
        private readonly IPAddress ipAddress;
        private readonly int port;
        private TcpListener listener;
        private bool isRunning;

        public TcpServer(IPAddress ipAddress, int port)
        {
            this.ipAddress = ipAddress;
            this.port = port;
        }

        public void Start()
        {
            listener = new TcpListener(ipAddress, port);
            listener.Start();
            isRunning = true;

            Console.WriteLine("Server started. Listening for incoming connections...");

            Task.Run(async () =>
            {
                while (isRunning)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    _ = HandleClientAsync(client);
                }
            });
        }

        public void Stop()
        {
            isRunning = false;
            listener.Stop();
            Console.WriteLine("Server stopped.");
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");

            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[4096];

                while (isRunning)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received from {client.Client.RemoteEndPoint}: {request}");

                    string response = "Hello from the server!";
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred for client {client.Client.RemoteEndPoint}: {ex.Message}");
            }
            finally
            {
                Console.WriteLine($"Client disconnected: {client.Client.RemoteEndPoint}");
                client.Close();
            }
        }
    }
}