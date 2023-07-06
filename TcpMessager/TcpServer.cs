using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TcpMessager
{
    public class TcpServer
    {
        private readonly IPAddress ipAddress;
        private readonly int port;
        private readonly int timeout;
        private TcpListener listener;
        private bool isRunning;
        private List<WeakReference<TcpServerClient>> clients;
        public TcpServer(IPAddress ipAddress, int port, int timeout = 1000)
        {
            this.ipAddress = ipAddress;
            this.port = port;
            this.timeout = timeout;
        }

        public void Start()
        {
            clients = new List<WeakReference<TcpServerClient>>();
            listener = new TcpListener(ipAddress, port);
            listener.Start();
            isRunning = true;

            Console.WriteLine("Server started. Listening for incoming connections...");

            Task.Run(async () =>
            {
                while (isRunning)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    TcpServerClient serverClient = new TcpServerClient(client);
                    serverClient.RequestReceived += ServerClientRequestReceived;
                    clients.Add(new WeakReference<TcpServerClient>(serverClient));
                    serverClient.Start();
                }
            });
        }

        public void Stop()
        {
            isRunning = false;
            listener.Stop();
            Console.WriteLine("Server stopped.");
        }
        public void ServerClientRequestReceived(object sender, EventArgs e)
        {
            var serverClient = sender as TcpServerClient;
            string request = serverClient.GetRequest();
            Console.WriteLine($"Received from {serverClient.RemoteEndPoint}: {request}");
            serverClient.SendResponse("ACK");
        }
    }
}