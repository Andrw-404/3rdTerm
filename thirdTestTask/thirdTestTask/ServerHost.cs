using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace thirdTestTask;

public static class ServerHost
{
    public static async Task RunOnce(int port)
    {
        var listner = new TcpListener(IPAddress.Any, port);
        listner.Start();

        try
        {
            Console.WriteLine($"[Сервер] Ожидание подлюкчения на порту {port}...");

            using TcpClient client = await listner.AcceptTcpClientAsync();

            Console.WriteLine("[Сервер] Клиент подключился. Для выхода - exit");
            await ChatSession.RunAsync(client, Console.In, Console.Out);
        }

        finally
        {
            listner.Stop();
            Console.WriteLine("[Сервер] Завершение");
        }
    }
}
