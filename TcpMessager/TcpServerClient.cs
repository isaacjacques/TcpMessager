using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpMessager
{
    public class TcpServerClient
    {
        private NetworkStream stream;
        private Queue<string> requests;
        private Queue<string> responses;
        byte[] buffer;
        private TcpClient client;
        private bool isRunning;
        public TcpServerClient(TcpClient client)
        {
            this.client = client;
        }
        public void Start()
        {
            stream = client.GetStream();
            requests = new Queue<string>();
            responses = new Queue<string>();
            buffer = new byte[4096];
            isRunning = true;

            Console.WriteLine($"Server client {client.Client.RemoteEndPoint} started.");

            Task.Run(async () =>
            {
                try
                {
                    while (isRunning)
                    {
                        await HandleRequests();
                        await HandleResponses();
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
            });
        }
        private async Task HandleResponses()
        {
            if (responses.Count > 0)
            {
                string response = responses.Dequeue();
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                Console.WriteLine($"Responded to {client.Client.RemoteEndPoint}: {response}");
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
            }
        }
        private async Task HandleRequests()
        {
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            if (bytesRead > 0)
            {
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                requests.Enqueue(request);
                RequestReceived?.Invoke(this, EventArgs.Empty);
            }
        }
        public string GetRequest()
        {
            return requests.Dequeue();
        }
        public void SendResponse(string message)
        {
            responses.Enqueue(message);
        }
        public int RequestCount => requests.Count();
        public EndPoint? RemoteEndPoint => client.Client.RemoteEndPoint;
        public event EventHandler RequestReceived;
    }
}
