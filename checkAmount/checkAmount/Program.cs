// <copyright file="Program.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

using System.Diagnostics;
using CheckAmount;

string testDir = @"..\..\..\TestDir";

Stopwatch sw = new Stopwatch();
CheckAmountСalculator calculator = new CheckAmountСalculator();

sw.Start();
string checkAmountOneThread = calculator.Calculate(testDir);
sw.Stop();
long timeOne = sw.ElapsedMilliseconds;
Console.WriteLine($"Хеш: {checkAmountOneThread}");
Console.WriteLine($"Время(однопоточный запуск): {timeOne} мс");

sw.Restart();
string checkAmountMulti = calculator.CalculateMultiThreaded(testDir);
sw.Stop();
long timeMulti = sw.ElapsedMilliseconds;
Console.WriteLine($"Хеш: {checkAmountMulti}");
Console.WriteLine($"Время(однопоточный запуск): {timeMulti} мс");

Console.WriteLine($"Хеши совпадают {checkAmountMulti == checkAmountOneThread}");