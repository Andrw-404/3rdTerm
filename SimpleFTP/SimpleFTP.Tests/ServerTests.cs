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