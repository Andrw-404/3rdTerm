using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelMatrixMultiplication
{
    public class MatrixUserInterface
    {
        public static void LoadFromFileAndMultiply()
        {
            Console.WriteLine("\nВведите путь к файлу с первой матрицей: ");
            string pathA = Console.ReadLine();
            Console.WriteLine("\nВведите путь к файлу со второй матрицей: ");
            string pathB = Console.ReadLine();

            if (string.IsNullOrEmpty(pathA) || string.IsNullOrEmpty(pathB))
            {
                Console.WriteLine("Пути к файлам не могут быть пустыми");
                return;
            }

            Matrix first = Matrix.LoadFromFile(pathA);
            Matrix second = Matrix.LoadFromFile(pathB);

            Stopwatch stopwatch = Stopwatch.StartNew();
            Matrix sequentialResult = MatrixMultiplier.SequentialMultiplication(first, second);
            stopwatch.Stop();
            Console.WriteLine($"\nПоследовательное умножение завершено. Время: {stopwatch.ElapsedMilliseconds} мс");

            Console.WriteLine("\nВведите путь для сохранения результата последовательного умножения: ");
            string sequentialSavePath = Console.ReadLine();
            sequentialResult.SaveToFile(sequentialSavePath);

            int numOfThreads = Environment.ProcessorCount;
            Console.WriteLine("\nПараллельное умножение:");
            stopwatch.Restart();
            Matrix parallelResult = MatrixMultiplier.ParallelMultiplication(first, second, numOfThreads);
            stopwatch.Stop();
            Console.WriteLine($"\nПараллельное уммножение завершено. Время: {stopwatch.ElapsedMilliseconds} мс");

            Console.WriteLine("\nВведите путь для сохранения результата параллельного умножения: ");
            string parallelSavePath = Console.ReadLine();
            parallelResult.SaveToFile(parallelSavePath);
        }

        public static void UsersMatrixTests()
        {
            Console.WriteLine("Введите количество запусков для каждого теста: ");
            if (!int.TryParse(Console.ReadLine(), out int n) || n <= 0)
            {
                Console.WriteLine("Неверное количество запусков.");
                return;
            }

            var testCases = new List<(int RowsA, int ColumnsA, int RowsB, int ColumnsB)>()
            {
                (100, 100, 100, 100),
                (200, 200, 200, 200),
                (300,300,300,300),
                (400,400,400,400),
                (500,500,500,500),
            };

            Console.WriteLine($"Проведение тестирования с {n} запусками");
            Console.WriteLine("\nРазмеры матриц | Матожидание(последовательное) | СКО(последовательное) | Матожидание(параллельное) | СКО(параллельное) \n");

            foreach (var testCase in testCases)
            {
                int rowsA = testCase.RowsA;
                int columnsA = testCase.ColumnsA;
                int rowsB = testCase.RowsB;
                int columnsB = testCase.ColumnsB;

                Matrix matrixA = Matrix.GenerateRandomMatrix(rowsA, columnsA);
                Matrix matrixB = Matrix.GenerateRandomMatrix(rowsB, columnsB);

                var sequentialTimes = new List<double>();
                var parallelTimes = new List<double>();

                int numOfThreads = Environment.ProcessorCount;

                for (int i = 0; i < n; ++i)
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    MatrixMultiplier.SequentialMultiplication(matrixA, matrixB);
                    stopwatch.Stop();
                    sequentialTimes.Add(stopwatch.Elapsed.TotalMilliseconds);

                    stopwatch.Restart();
                    MatrixMultiplier.ParallelMultiplication(matrixA, matrixB, numOfThreads);
                    stopwatch.Stop();
                    parallelTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
                }

                double sequentialExpectation = CalculateMathematicalExpectation(sequentialTimes);
                double parallelExpectation = CalculateMathematicalExpectation(parallelTimes);

                double sequentialStandardDeviation = CalculateStandardDeviation(sequentialTimes, sequentialExpectation);
                double parallelStandardDeviation = CalculateStandardDeviation(parallelTimes, parallelExpectation);

                Console.WriteLine($"{rowsA}x{columnsA} * {rowsB}x{columnsB} | {sequentialExpectation:F2} мс | {sequentialStandardDeviation:F2} | {parallelExpectation:F2} | {parallelStandardDeviation:F2}\n");
            }
        }

        private static double CalculateMathematicalExpectation(List<double> values)
        {
            return values.Sum() / values.Count;
        }

        private static double CalculateStandardDeviation(List<double> values, double expectation)
        {
            double sumOfSquares = 0;
            foreach (double value in values)
            {
                sumOfSquares += (value - expectation) * (value - expectation);
            }

            return Math.Sqrt(sumOfSquares / values.Count);
        }
    }
}