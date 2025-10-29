using System.Net.Sockets;
using System.Text;

namespace SimpleFTP.Client
{
    public class Client
    {
        private const string ip = "127.0.0.1";
        private const int port = 8888;

        public static async Task StartClient()
        {
            using (TcpClient client = new TcpClient())
            {
                try
                {
                    await client.ConnectAsync(ip, port);
                    Console.WriteLine($"Connected at {ip}");
                    await using NetworkStream stream = client.GetStream();
                    using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
                    await using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
                    await CommandHandler(stream, reader, writer);
                }
                catch (Exception excetpion)
                {
                    Console.WriteLine($"Error. {excetpion.Message}");
                }
            }

            Console.WriteLine("Disconnected from the server");
        }

        private static async Task CommandHandler(NetworkStream stream, StreamReader reader, StreamWriter writer)
        {
            Console.WriteLine("Enter commands ('list <path>' or 'get <path> <local path>' or 'exit'");
            while (true)
            {
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                string[] parts = input.Split(' ');
                string command = parts[0].ToLower();

                if (command == "exit")
                {
                    break;
                }

                switch (command)
                {
                    case "list":
                        if (parts.Length == 2)
                        {
                            await HandleListRequest(reader, writer, parts[1]);
                        }
                        else
                        {
                            Console.WriteLine("Incorrect input. Enter 'list <path>'");
                        }

                        break;
                    case "get":
                        if (parts.Length == 3)
                        {
                            await HandleGetRequest(stream, writer, parts[1], parts[2]);
                        }
                        else
                        {
                            Console.WriteLine("Incorrect input. Enter 'get <path> <local path>'");
                        }

                        break;

                    default:
                        Console.WriteLine("Unknown command");
                        break;
                }
            }
        }

        private static async Task HandleListRequest(StreamReader reader, StreamWriter writer, string path)
        {
            await writer.WriteLineAsync($"1 {path}");
            await writer.FlushAsync();

            string? response = await reader.ReadLineAsync();
            if (response == "-1")
            {
                Console.WriteLine("Error. Directory was not found or access to it was denied");
                return;
            }

            string[] parts = response.Split(' ');
            if (!int.TryParse(parts[0], out int count) || count < 0)
            {
                Console.WriteLine("invalid response from the server");
                return;
            }

            if (count == 0)
            {
                Console.WriteLine("Directory is empty");
                return;
            }

            Console.WriteLine($"Directory contains {count} items:");
            for (int i = 0; i < count; ++i)
            {
                string name = parts[(i * 2) + 1];
                string isDirectory = parts[(i * 2) + 2] == "true" ? "[dir]" : "[file]";
                Console.WriteLine($" {isDirectory} {name}");
            }
        }

        private static async Task HandleGetRequest(NetworkStream stream, StreamWriter writer, string path, string localPath)
        {
            await writer.WriteLineAsync($"2 {path}");
            await writer.FlushAsync();
            byte[] sizeBuffer = new byte[8];
            await stream.ReadExactlyAsync(sizeBuffer, 0, 8);
            long fileSize = BitConverter.ToInt64(sizeBuffer, 0);

            if (fileSize == -1)
            {
                Console.WriteLine("File not found or access denied");
                return;
            }

            Console.WriteLine($"Downloading file {path} to {localPath}");
            long totalBytesRead = 0;

            try
            {
                await using (FileStream fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = new byte[81920];
                    while (totalBytesRead < fileSize)
                    {
                        int bytesToRead = (int)Math.Min(fileSize - totalBytesRead, buffer.Length);
                        int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, bytesToRead));

                        await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                        totalBytesRead += bytesRead;
                    }
                }

                Console.WriteLine("Download finish");
            }
            catch (Exception excetpion)
            {
                Console.WriteLine($"Error loading. {excetpion.Message}");
                if (File.Exists(localPath))
                {
                    File.Delete(localPath);
                }
            }
        }
    }
}