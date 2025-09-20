// <copyright file="MatrixMultiplier.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace ParallelMatrixMultiplication
{
    /// <summary>
    /// A class for performing matrix multiplication in sequential and parallel ways.
    /// </summary>
    public class MatrixMultiplier
    {
        /// <summary>
        /// Performs sequential multiplication of two matrices(AxB).
        /// </summary>
        /// <param name="a">The first matrix.</param>
        /// <param name="b">The second matrix.</param>
        /// <returns>The resulting matrix.</returns>
        /// <exception cref="InvalidDataException">It is thrown if the number of columns of the first
        /// matrix is not equal to the number of rows of the second.
        /// </exception>
        public static Matrix SequentialMultiplication(Matrix a, Matrix b)
        {
            if (a.Columns != b.Rows)
            {
                throw new InvalidDataException("Умножение невозможно(количество столбцов первой матрицы не равно количеству строк второй)");
            }

            Matrix result = new Matrix(a.Rows, b.Columns);
            for (int i = 0; i < a.Rows; ++i)
            {
                for (int j = 0; j < b.Columns; j++)
                {
                    int sum = 0;
                    for (int k = 0; k < a.Columns; k++)
                    {
                        sum += a[i, k] * b[k, j];
                    }

                    result[i, j] = sum;
                }
            }

            return result;
        }

        /// <summary>
        /// Performs parallel multiplication of two matrices(AxB).
        /// </summary>
        /// <param name="a">The first matrix.</param>
        /// <param name="b">The second matrix.</param>
        /// <param name="numThreads">Number of threads.</param>
        /// <returns>The resulting matrix.</returns>
        /// <exception cref="InvalidDataException">It is thrown if the number of columns of the first
        /// matrix is not equal to the number of rows of the second.
        /// </exception>
        public static Matrix ParallelMultiplication(Matrix a, Matrix b, int numThreads)
        {
            if (a.Columns != b.Rows)
            {
                throw new InvalidDataException("Умножение невозможно(количество столбцов первой матрицы не равно количеству строк второй)");
            }

            Matrix result = new Matrix(a.Rows, b.Columns);

            int actualNumOfThread = Math.Min(a.Rows, numThreads);
            Thread[] threads = new Thread[actualNumOfThread];
            int rowsForThread = a.Rows / actualNumOfThread;

            for (int i = 0; i < numThreads; ++i)
            {
                int startRow = i * rowsForThread;
                int endRow = (i == numThreads - 1) ? a.Rows : (i + 1) * rowsForThread;

                threads[i] = new Thread(() =>
                {
                    for (int row = startRow; row < endRow; row++)
                    {
                        for (int columns = 0; columns < b.Columns; columns++)
                        {
                            int sum = 0;
                            for (int k = 0; k < a.Columns; ++k)
                            {
                                sum += a[row, k] * b[k, columns];
                            }

                            result[row, columns] = sum;
                        }
                    }
                });
            }

            foreach (var thread in threads)
            {
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            return result;
        }
    }
}