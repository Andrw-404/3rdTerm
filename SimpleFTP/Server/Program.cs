using Server;

string baseDirectory = Directory.GetCurrentDirectory();
Console.WriteLine($"Starting server. Base directory: {baseDirectory}");
ServerClass fileServer = new ServerClass(baseDirectory);
await fileServer.Start();