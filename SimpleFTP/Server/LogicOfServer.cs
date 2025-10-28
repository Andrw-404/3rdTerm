using System.Text;

namespace Server
{
    public class LogicOfServer
    {
        private string baseDirectory;
        private string fullPath;

        public LogicOfServer(string baseDirectory)
        {
            this.baseDirectory = baseDirectory;
        }

        public bool IsPathSafe(string requestedPath, string fullPath)
        {
            this.fullPath = Path.GetFullPath(this.baseDirectory, requestedPath);
            return fullPath.StartsWith(this.baseDirectory, StringComparison.OrdinalIgnoreCase);
        }

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
                    response.Append($"{data.Name} {(data is FileInfo ? "true" : "false")}");
                }

                await streamwriter.WriteLineAsync(response.ToString());
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error in 'List': {exception.Message}");
                await streamwriter.WriteLineAsync("-1");
            }
        }

        public async Task Get(Stream stream, string filePath)
        {
            FileInfo file = new FileInfo(filePath);

            if (!file.Exists)
            {
                byte[] errorInBytes = BitConverter.GetBytes(-1L);
                await stream.WriteAsync(errorInBytes, 0, errorInBytes.Length);
                return;
            }

            try
            {
                long fileSize = file.Length;
                byte[] sizeBytes = BitConverter.GetBytes(fileSize);
                await stream.WriteAsync(sizeBytes, 0, sizeBytes.Length);

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
}