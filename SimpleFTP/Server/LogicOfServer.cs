// <copyright file="LogicOfServer.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace SimpleFTP.Server;

using System.Text;

/// <summary>
/// A class with query processing logic.
/// </summary>
public class LogicOfServer
{
    private string baseDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogicOfServer"/> class.
    /// </summary>
    /// <param name="baseDirectory">The base directory for which requests will be processed.</param>
    public LogicOfServer(string baseDirectory)
    {
        this.baseDirectory = Path.GetFullPath(baseDirectory);
    }

    /// <summary>
    /// Checks whether the requested path is located inside the base directory.
    /// </summary>
    /// <param name="requestedPath">The path from the client's request.</param>
    /// <param name="fullPath">Full path.</param>
    /// <returns>True if the path is safe and located inside the base directory, otherwise False.</returns>
    public bool IsPathSafe(string requestedPath, out string? fullPath)
    {
        string combinedPath = Path.Combine(this.baseDirectory, requestedPath);
        fullPath = Path.GetFullPath(combinedPath);
        return fullPath.StartsWith(this.baseDirectory, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Processes the List command, compiles a list of files/folders and sends it to the client.
    /// </summary>
    /// <param name="streamwriter">StreamWriter for sending a text response.</param>
    /// <param name="directoryPath">The full path to the directory for listing.</param>
    /// <returns>Task representing the operation.</returns>
    public async Task List(StreamWriter streamwriter, string directoryPath)
    {
        DirectoryInfo infoAboutDirectory = new DirectoryInfo(directoryPath);

        if (!infoAboutDirectory.Exists)
        {
            await streamwriter.WriteLineAsync("-1");
            return;
        }

        try
        {
            FileSystemInfo[] filesAndFolders = infoAboutDirectory.GetFileSystemInfos();
            StringBuilder response = new StringBuilder();

            response.Append(filesAndFolders.Length);

            foreach (var data in filesAndFolders)
            {
                response.Append($" {data.Name} {(data is DirectoryInfo ? "true" : "false")}");
            }

            await streamwriter.WriteLineAsync(response.ToString());
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Error in 'List': {exception.Message}");
            await streamwriter.WriteLineAsync("-1");
        }
    }

    /// <summary>
    /// Processes the Get command, sends the file size and its contents to the client.
    /// </summary>
    /// <param name="stream">A network stream for sending data.</param>
    /// <param name="filePath">The full path to the download file.</param>
    /// <returns>Task representing the operation.</returns>
    public async Task Get(Stream stream, string filePath)
    {
        FileInfo file = new FileInfo(filePath);

        if (!file.Exists)
        {
            byte[] errorInBytes = BitConverter.GetBytes(-1L);
            await stream.WriteAsync(errorInBytes);
            await stream.FlushAsync();
            return;
        }

        try
        {
            long fileSize = file.Length;
            byte[] sizeBytes = BitConverter.GetBytes(fileSize);
            await stream.WriteAsync(sizeBytes);
            await stream.FlushAsync();

            await using (FileStream fileStream = file.OpenRead())
            {
                await fileStream.CopyToAsync(stream);
            }

            Console.WriteLine($"Sent file ({filePath}) ({fileSize} bytes)");
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Error in 'Get': {exception.Message}");
        }
    }
}