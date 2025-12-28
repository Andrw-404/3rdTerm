using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace thirdTestTask;

public static class ChatSession
{
    public static async Task RunAsync(TcpClient client, TextReader input, TextWriter output, CancellationToken token = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        CancellationToken token2 = cts.Token;

        NetworkStream stream = client.GetStream();

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
        using var writer = new StreamWriter(stream, Encoding.UTF8, bufferSize: 1024, leaveOpen: true){ AutoFlush = true };
        Task receiveTask = Task.Run(async () =>
        {
            try
            {
                while (!token2.IsCancellationRequested)
                {
                    string? msg = await reader.ReadLineAsync();

                    if (msg == null)
                    {
                        await SafeWriteLineAsync(output, "\n[СИСТЕМА] Соединение разорвано.");
                        cts.Cancel();
                        break;
                    }

                    if (IsExit(msg))
                    {
                        await SafeWriteLineAsync(output, "\n[СИСТЕМА] Собеседник завершил чат.");
                        cts.Cancel();
                        break;
                    }

                    await SafeWriteLineAsync(output, $"[Собеседник]: {msg}");
                }
            }
            catch
            {
                cts.Cancel();
            }
        }, token2);

        Task sendTask = Task.Run(async () =>
        {
            try
            {
                while (!token2.IsCancellationRequested)
                {
                    string? line = await ReadLineWithCancelAsync(input, token2);

                    if (line == null)
                    {
                        cts.Cancel();
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    await writer.WriteLineAsync(line);

                    if (IsExit(line))
                    {
                        cts.Cancel();
                        break;
                    }
                }
            }
            catch
            {
                cts.Cancel();
            }
        }, token2);

        await Task.WhenAny(receiveTask, sendTask);

        try { client.Close(); } 
        catch { }

        cts.Cancel();
        try { await Task.WhenAll(receiveTask, sendTask); }
        catch { }
    }

    private static bool IsExit(string s)
        => s.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase);

    private static Task SafeWriteLineAsync(TextWriter output, string text)
    {
        output.WriteLine(text);
        output.Flush();
        return Task.CompletedTask;
    }

    private static async Task<string?> ReadLineWithCancelAsync(TextReader input, CancellationToken token)
    {
        Task<string?> readTask = input.ReadLineAsync();
        Task done = await Task.WhenAny(readTask, Task.Delay(Timeout.Infinite, token));

        if (done != readTask) return null;
        return await readTask;
    }
}

