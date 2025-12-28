using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace thirdTestTask;

public static class ClientHost
{
    public static async Task Run(IPAddress ip, int port)
    {
        using var client = new TcpClient();

        Console.WriteLine($"[Клиент] подключение к {ip}: {port} ");
        await client.ConnectAsync(ip, port);

        Console.WriteLine("[Клиент] подключено. Для выхода - exit");
        await ChatSession.RunAsync(client, Console.In, Console.Out);

        Console.WriteLine("[Клиент] завершение");
    }
}
