// <copyright file="CheckAmountСalculator.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace CheckAmount;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Calculates the checksum (hash amount) for a given file or directory structure.
/// </summary>
public class CheckAmountСalculator
{
    private const int BufferSize = 4096;

    /// <summary>
    /// Initiates the single-threaded checksum calculation.
    /// </summary>
    /// <param name="path">The path to the file or directory.</param>
    /// <returns>The MD5 hash.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the path does not exist.</exception>
    public string Calculate(string path)
    {
        byte[] hashBytes;

        if (File.Exists(path))
        {
            hashBytes = this.CalculateFileCheckAmount(path);
        }
        else if (Directory.Exists(path))
        {
            hashBytes = this.CalculateDirectoryCheckAmount(path);
        }
        else
        {
            throw new FileNotFoundException("Путь не найден");
        }

        return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
    }

    /// <summary>
    /// Initiates the multi-threaded checksum calculation for directories.
    /// </summary>
    /// <param name="path">The path to the file or directory.</param>
    /// <returns>The MD5 hash.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the path does not exist.</exception>
    public string CalculateMultiThreaded(string path)
    {
        byte[] hashBytes;

        if (File.Exists(path))
        {
            hashBytes = this.CalculateFileCheckAmount(path);
        }
        else if (Directory.Exists(path))
        {
            hashBytes = this.CalculateDirectoryAmountThreaded(path);
        }
        else
        {
            throw new FileNotFoundException("Путь не найден");
        }

        return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
    }

    private byte[] CalculateFileCheckAmount(string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
        using (MD5 md = MD5.Create())
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            md.TransformBlock(fileNameBytes, 0, fileNameBytes.Length, fileNameBytes, 0);

            byte[] buffer = new byte[BufferSize];
            int bytesRead;

            while ((bytesRead = fileStream.Read(buffer, 0, BufferSize)) > 0)
            {
                md.TransformBlock(buffer, 0, bytesRead, buffer, 0);
            }

            md.TransformFinalBlock(new byte[0], 0, 0);
            return md.Hash!;
        }
    }

    private byte[] CalculateDirectoryCheckAmount(string dirPath)
    {
        string dirName = new DirectoryInfo(dirPath).Name;
        byte[] dirNameBytes = Encoding.UTF8.GetBytes(dirName);

        string[] subDirs = Directory.GetDirectories(dirPath);
        string[] files = Directory.GetFiles(dirPath);

        Array.Sort(subDirs);
        Array.Sort(files);

        using (MemoryStream stream = new MemoryStream())
        {
            stream.Write(dirNameBytes, 0, dirNameBytes.Length);

            foreach (string subDir in subDirs)
            {
                byte[] subDirHash = this.CalculateDirectoryCheckAmount(subDir);
                stream.Write(subDirHash, 0, subDirHash.Length);
            }

            foreach (string file in files)
            {
                byte[] fileHash = this.CalculateFileCheckAmount(file);
                stream.Write(fileHash, 0, fileHash.Length);
            }

            stream.Position = 0;
            using (MD5 md = MD5.Create())
            {
                return md.ComputeHash(stream);
            }
        }
    }

    private byte[] CalculateDirectoryAmountThreaded(string dirPath)
    {
        string dirName = new DirectoryInfo(dirPath).Name;
        byte[] dirNameBytes = Encoding.UTF8.GetBytes(dirName);

        string[] subDirs = Directory.GetDirectories(dirPath);
        string[] files = Directory.GetFiles(dirPath);

        Array.Sort(subDirs);
        Array.Sort(files);

        List<Task<byte[]>> tasks = new List<Task<byte[]>>();

        foreach (string subDir in subDirs)
        {
            tasks.Add(Task.Run(() => this.CalculateDirectoryAmountThreaded(subDir)));
        }

        foreach (string file in files)
        {
            tasks.Add(Task.Run(() => this.CalculateFileCheckAmount(file)));
        }

        Task.WaitAll(tasks.ToArray());

        using (MemoryStream stream = new MemoryStream())
        {
            stream.Write(dirNameBytes, 0, dirNameBytes.Length);

            foreach (Task<byte[]> task in tasks)
            {
                byte[] resultHash = task.Result;
                stream.Write(resultHash, 0, resultHash.Length);
            }

            stream.Position = 0;
            using (MD5 md = MD5.Create())
            {
                return md.ComputeHash(stream);
            }
        }
    }
}