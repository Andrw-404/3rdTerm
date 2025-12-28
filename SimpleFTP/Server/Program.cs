// <copyright file="Program.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

using SimpleFTP.Server;

string baseDirectory = Directory.GetCurrentDirectory();
Console.WriteLine($"Starting server. Base directory: {baseDirectory}");
Server fileServer = new Server(baseDirectory);
await fileServer.Start();