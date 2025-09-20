// <copyright file="MatrixUserInterface.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace ParallelMatrixMultiplication
{
    using System.Diagnostics;

    /// <summary>
    /// A user interface class for working with matrix multiplication.
    /// </summary>
    public class MatrixUserInterface
    {
        /// <summary>
        /// Loads matrices from files, performs sequential and parallel multiplication,
        /// and saves the results to files, as well as additionally measures execution time.
        /// </summary>
        /// <param name="pathA">The path to the first matrix.</param>
        /// <param name="pathB">The path to the second matrix.</param>
        /// <param name="sequentialSavePath">The path to the sequential result matrix.</param>
        /// <param name="parallelSavePath">The path to the parallel result matrix.</param>
        public static void LoadFromFileAndMultiply(string pathA, string pathB, string sequentialSavePath, string parallelSavePath)
        {
            Matrix first = Matrix.LoadFromFile(pathA);
            Matrix second = Matrix.LoadFromFile(pathB);

            Stopwatch stopwatch = Stopwatch.StartNew();
            Matrix sequentialResult = MatrixMultiplier.SequentialMultiplication(first, second);
            stopwatch.Stop();
            Console.WriteLine($"\nПоследовательное умножение завершено. Время: {stopwatch.ElapsedMilliseconds} мс");

            sequentialResult.SaveToFile(sequentialSavePath);

            int numOfThreads = Environment.ProcessorCount;
            Console.WriteLine($"\nПараллельное умножение c {numOfThreads} потоками:");
            stopwatch.Restart();
            Matrix parallelResult = MatrixMultiplier.ParallelMultiplication(first, second, numOfThreads);
            stopwatch.Stop();
            Console.WriteLine($"\nПараллельное уммножение завершено. Время: {stopwatch.ElapsedMilliseconds} мс");

            parallelResult.SaveToFile(parallelSavePath);
        }

        /// <summary>
        /// Performs performance testing of sequential and parallel matrix multiplication
        /// for various matrix sizes with a set number of runs.
        /// </summary>
        /// <param name="n">The number of runs for each test.</param>
        public static void UsersMatrixTests(int n)
        {
            var testCases = new List<(int RowsA, int ColumnsA, int RowsB, int ColumnsB)>()
            {
                (100, 100, 100, 100),
                (200, 200, 200, 200),
                (300, 300, 300, 300),
                (400, 400, 400, 400),
                (500, 500, 500, 500),
            };

            Console.WriteLine($"Проведение тестирования с {n} запусками");
            Console.WriteLine("\nРазмеры матриц | " +
                "Матожидание(последовательное) | " +
                "СКО(последовательное) | " +
                "Матожидание(параллельное) | " +
                "СКО(параллельное) \n");

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