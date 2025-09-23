// <copyright file="Program.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

using ParallelMatrixMultiplication;

args = Environment.GetCommandLineArgs().Skip(1).ToArray();
var (choice, parameters) = ArgumentParser.Parse(args);

try
{
    switch (choice)
    {
        case "load":
            if (parameters.Length < 4)
            {
                Console.WriteLine("Недостаточно аргументов. (load <pathA> <pathB> <resultSequential> <resultParallel>)");
                return;
            }

            string pathA = parameters[0];
            string pathB = parameters[1];
            string sequentialSavePath = parameters[2];
            string parallelSavePath = parameters[3];

            ArgumentNullException.ThrowIfNullOrEmpty(pathA);
            ArgumentNullException.ThrowIfNullOrEmpty(pathB);
            ArgumentNullException.ThrowIfNullOrEmpty(sequentialSavePath);
            ArgumentNullException.ThrowIfNullOrEmpty(parallelSavePath);
            MatrixUserInterface.LoadFromFileAndMultiply(pathA, pathB, sequentialSavePath, parallelSavePath);
            break;

        case "matrixtest":
            if (parameters.Length < 1)
            {
                Console.WriteLine("Недостаточно аргументов. (matrixtest <n>)");
                return;
            }

            if (!int.TryParse(parameters[0], out int n) || n <= 0)
            {
                Console.WriteLine("Неверное количество запусков. Ожидается целое положительное число");
                return;
            }

            MatrixUserInterface.UsersMatrixTests(n);
            break;

        default:
            Console.WriteLine("\nНеизвестный ввод. Ожидается load или matrixtest\n");
            break;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Произошла ошибка: {ex.Message}");
}