// <copyright file="Program.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

using ParallelMatrixMultiplication;

string? choice;

while (true)
{
    if (args.Length == 0)
    {
        Console.WriteLine("Выберите способ получения матриц:");
        Console.WriteLine("1. Загрузить из файлов");
        Console.WriteLine("2. Рандомная генерация + тесты");
        Console.WriteLine("3. Выйти");
        Console.WriteLine("Выбор: ");
        choice = Console.ReadLine();
    }
    else
    {
        choice = args[0];
    }

    if (string.IsNullOrWhiteSpace(choice))
    {
        Console.WriteLine("Выбор не указан");
        return;
    }

    try
    {
        switch (choice)
        {
            case "1":
                MatrixUserInterface.LoadFromFileAndMultiply();
                break;

            case "2":
                MatrixUserInterface.UsersMatrixTests();
                break;

            case "3":
                Console.WriteLine("Выход...");
                return;

            default:
                Console.WriteLine("\nНеизвестный ввод\n");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Произошла ошибка: {ex.Message}");
    }
}