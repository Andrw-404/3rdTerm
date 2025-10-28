using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class ServerClass
    {
        private const int Port = 8888;
        private readonly TcpListener listener;
        private readonly LogicOfServer logicOfServer;

        public ServerClass(string baseDirectory)
        {
            this.listener = new TcpListener(IPAddress.Any, Port);
            this.logicOfServer = new LogicOfServer(baseDirectory);
        }

        public async Task Start()
        {
            this.listener.Start();
            Console.WriteLine($"Listening on port {Port}...");

            while (true)
            {
                try
                {
                    TcpClient client = await this.listener.AcceptTcpClientAsync();
                    Task clientTask = Task.Run(() => this.ClientHandler(client));
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Error accepting client {exception.Message}");
                }
            }
        }

        private async Task ClientHandler(TcpClient client)
        {
            Console.WriteLine("Client connected");
            try
            {
                await using (NetworkStream stream = client.GetStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true))
                await using (var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true))
                {
                    string line = string.Empty;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        Console.WriteLine($"Request: {line}");
                        string[] parts = line.Split(' ', 2);
                        if (parts.Length < 2)
                        {
                            continue;
                        }

                        string command = parts[0];
                        string path = parts[1];

                        string fullPath = string.Empty;
                        if (!this.logicOfServer.IsPathSafe(path, fullPath))
                        {
                            Console.WriteLine($"Wrong path {fullPath}");
                            await writer.WriteLineAsync("-1");
                            continue;
                        }

                        switch (command)
                        {
                            case "1":
                                await this.logicOfServer.List(writer, fullPath);
                                break;
                            case "2":
                                await writer.FlushAsync();
                                await this.logicOfServer.Get(stream, fullPath);
                                break;
                            default:
                                await writer.WriteLineAsync("-1");
                                break;
                        }
                    }
                }
            }
            catch(Exception exception)
            {
                Console.WriteLine($"Error handling client {exception.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine("Client disconnected");
            }
        }
    }
}