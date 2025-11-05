// <copyright file="ServerTests.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace SimpleFTP.Tests;

using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SimpleFTP.Server;
using static System.Net.Mime.MediaTypeNames;

public class ServerTests
{
    private const string IP = "127.0.0.1";
    private const int PORT = 8888;
    private const string TestTextData = "Abcd,Efg";
    private readonly string baseDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestServerBase");
    private readonly int[] testIntData = new int[] { 59, -1, 10000, 52 };
    private Server? server;
    private Task? serverTask;
    private byte[] testBinaryData = Array.Empty<byte>();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        this.testBinaryData = this.IntToBytes(this.testIntData);

        if (Directory.Exists(this.baseDirectory))
        {
            Directory.Delete(this.baseDirectory, true);
        }

        Directory.CreateDirectory(this.baseDirectory);
        Directory.CreateDirectory(Path.Combine(this.baseDirectory, "Folder"));
        Directory.CreateDirectory(Path.Combine(this.baseDirectory, "EmptyFolder"));

        File.WriteAllText(Path.Combine(this.baseDirectory, "file.txt"), TestTextData);
        File.WriteAllBytes(Path.Combine(this.baseDirectory, "file.bin"), this.testBinaryData);
        File.WriteAllText(Path.Combine(this.baseDirectory, "Folder", "FileInFolder.txt"), "qwerty");

        this.server = new Server(this.baseDirectory);
        this.serverTask = this.server.Start();

        Thread.Sleep(300);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        this.server?.Stop();
        try
        {
            this.serverTask?.Wait(TimeSpan.FromSeconds(5));
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Error during server task teardown: {exception.Message}");
        }

        if (Directory.Exists(this.baseDirectory))
        {
            Directory.Delete(this.baseDirectory, true);
        }
    }

    [Test]
    public async Task List_ValidDirectory_ShouldReturnCorrectList()
    {
        var response = await this.PerformList(".");

        Assert.That(response, Is.Not.Null);
        Assert.That(response, Does.StartWith("4"));
        Assert.That(response, Contains.Substring(" file.txt false"));
        Assert.That(response, Contains.Substring(" file.bin false"));
        Assert.That(response, Contains.Substring(" Folder true"));
        Assert.That(response, Contains.Substring(" EmptyFolder true"));
    }

    [Test]
    public async Task List_EmptyDirectory_ShouldReturnCorrectList()
    {
        var response = await this.PerformList("EmptyFolder");
        Assert.That(response, Is.EqualTo("0"));
    }

    [Test]
    public async Task List_NonExistentDirectory_ShouldReturnException()
    {
        var response = await this.PerformList("kmnds");
        Assert.That(response, Is.EqualTo("-1"));
    }

    [Test]
    public async Task Get_ValidTextFile_ShouldReturnCorrectContent()
    {
        var (size, data) = await this.PerformGet("file.txt");
        string received = Encoding.UTF8.GetString(data);
        Assert.That(received, Is.EqualTo(TestTextData));
    }

    [Test]
    public async Task Get_ValidBinaryFile_ShouldReturnCorrectContent()
    {
        var (size, data) = await this.PerformGet("file.bin");
        Assert.That(data, Is.EqualTo(this.IntToBytes(this.testIntData)));
    }

    [Test]
    public async Task Get_FileInSubFolder_ShouldReturnCorrectContent()
    {
        var (size, data) = await this.PerformGet("Folder/FileInFolder.txt");
        Assert.That(data, Is.EqualTo("qwerty"));
    }

    private async Task<string?> PerformList(string path)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(IP, PORT);
        await using var stream = client.GetStream();
        await using var writer = new StreamWriter(stream) { AutoFlush = true };
        using var reader = new StreamReader(stream);

        await writer.WriteLineAsync($"1 {path}");
        return await reader.ReadLineAsync();
    }

    private async Task<(long Size, byte[] Data)> PerformGet(string path)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(IP, PORT);
        await using var stream = client.GetStream();

        string command = $"2 {path}\n";
        byte[] commandBytes = Encoding.UTF8.GetBytes(command);
        await stream.WriteAsync(commandBytes);
        await stream.FlushAsync();

        await Task.Delay(100);

        byte[] sizeBuffer = new byte[8];

        await stream.ReadExactlyAsync(sizeBuffer, 0, 8);
        long fileSize = BitConverter.ToInt64(sizeBuffer, 0);

        if (fileSize == -1)
        {
            return (-1L, Array.Empty<byte>());
        }

        using var memoryStream = new MemoryStream();
        byte[] buffer = new byte[81920];
        long totalBytesRead = 0;
        while (totalBytesRead < fileSize)
        {
            int bytesToRead = (int)Math.Min(fileSize - totalBytesRead, buffer.Length);
            int bytesRead = await stream.ReadAsync(buffer, 0, bytesToRead);
            if (bytesRead == 0)
            {
                throw new EndOfStreamException("Connection closed");
            }

            await memoryStream.WriteAsync(buffer, 0, bytesRead);
            totalBytesRead += bytesRead;
        }

        return (fileSize, memoryStream.ToArray());
    }

    private byte[] IntToBytes(int[] data)
    {
        using (var memoryStream = new MemoryStream())
        {
            foreach (int num in this.testIntData)
            {
                byte[] bytes = BitConverter.GetBytes(num);
                memoryStream.Write(bytes, 0, bytes.Length);
            }

            return memoryStream.ToArray();
        }
    }
}