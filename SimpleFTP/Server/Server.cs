// <copyright file="Server.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace SimpleFTP.Server;

using System.Net;
using System.Net.Sockets;
using System.Text;

/// <summary>
/// The main server class that manages listening for connections and client processing.
/// </summary>
public class Server
{
    private const int Port = 8888;
    private readonly TcpListener listener;
    private readonly LogicOfServer logicOfServer;
    private CancellationTokenSource cts;
    private bool isWorking;

    /// <summary>
    /// Initializes a new instance of the <see cref="Server"/> class.
    /// </summary>
    /// <param name="baseDirectory">The base directory for file operations.</param>
    public Server(string baseDirectory)
    {
        this.listener = new TcpListener(IPAddress.Any, Port);
        this.logicOfServer = new LogicOfServer(baseDirectory);
        this.cts = new CancellationTokenSource();
    }

    /// <summary>
    /// Starts listening for incoming connections and creates tasks for processing each client.
    /// </summary>
    /// <returns>A task representing an asynchronous server operation.</returns>
    public async Task Start()
    {
        this.listener.Start();
        this.isWorking = true;
        Console.WriteLine($"Listening on port {Port}...");

        while (this.isWorking)
        {
            try
            {
                TcpClient client = await this.listener.AcceptTcpClientAsync(this.cts.Token);
                Task clientTask = Task.Run(() => this.ClientHandler(client));
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                if (!this.isWorking)
                {
                    break;
                }

                Console.WriteLine($"Error accepting client {exception.Message}");
            }
        }
    }

    /// <summary>
    /// Initiates the server shutdown process by closing listener and canceling all pending operations.
    /// </summary>
    public void Stop()
    {
        this.isWorking = false;
        this.cts?.Cancel();
        this.listener?.Stop();
    }

    /// <summary>
    /// Frees up resources.
    /// </summary>
    public void Dispose()
    {
        this.Stop();
        this.listener?.Dispose();
        this.cts?.Dispose();
    }

    private async Task ClientHandler(TcpClient client)
    {
        Console.WriteLine("Client connected");
        try
        {
            await using NetworkStream stream = client.GetStream();
            {
                string? line = string.Empty;
                while (true)
                {
                    using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
                    await using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
                    line = await reader.ReadLineAsync();
                    if (line == null)
                    {
                        break;
                    }

                    Console.WriteLine($"Request: {line}");
                    string[] parts = line.Split(' ', 2);
                    if (parts.Length < 2)
                    {
                        continue;
                    }

                    string command = parts[0];
                    string path = parts[1];

                    if (!this.logicOfServer.IsPathSafe(path, out string? fullPath) || fullPath == null)
                    {
                        Console.WriteLine($"access denied {path}");
                        switch (command)
                        {
                            case "1":
                                await writer.WriteLineAsync("-1");
                                await writer.FlushAsync();
                                break;
                            case "2":
                                byte[] errorBytes = BitConverter.GetBytes(-1L);
                                await stream.WriteAsync(errorBytes);
                                await stream.FlushAsync();
                                break;
                        }

                        continue;
                    }

                    switch (command)
                    {
                        case "1":
                            await this.logicOfServer.List(writer, fullPath);
                            await writer.FlushAsync();
                            break;
                        case "2":
                            await this.logicOfServer.Get(stream, fullPath);
                            break;
                        default:
                            await writer.WriteLineAsync("-1");
                            await writer.FlushAsync();
                            break;
                    }
                }
            }
        }
        catch (Exception exception)
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