using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace thirdTestTask;

public static class App
{
    public static async Task<int> Run(string[] args)
    {
        if (TryParseServer(args, out int serverPort))
        {
            await ServerHost.RunOnce(serverPort);
            return 0;
        }

        if (TryParseClient(args, out IPAddress? ip, out int clientPort))
        {
            await ClientHost.Run(ip!, clientPort);
            return 0;
        }

        PrintUsage();
        return 2;
    }

    private static bool TryParseServer(string[] args, out int port)
    {
        port = 0;
        return args.Length == 1 && int.TryParse(args[0], out port);
    }

    private static bool TryParseClient(string[] args, out IPAddress? ip, out int port)
    {
        ip = null;
        port = 0;

        return args.Length == 2 && IPAddress.TryParse(args[0], out ip) && int.TryParse(args[1], out port);
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Использование:");
        Console.WriteLine("Сервер: <port>");
        Console.WriteLine("Клиент: <ip> <port>");
    }
}
