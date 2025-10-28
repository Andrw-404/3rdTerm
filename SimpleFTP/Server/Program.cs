using Server;

string baseDirectory = Directory.GetCurrentDirectory();
ServerClass fileServer = new ServerClass(baseDirectory);
await fileServer.Start();