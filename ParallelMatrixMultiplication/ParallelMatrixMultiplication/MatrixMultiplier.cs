using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelMatrixMultiplication
{
    public class MatrixMultiplier
    {
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

        public static Matrix ParallelMultiplication(Matrix a, Matrix b, int numThreads)
        {
            if (a.Columns != b.Rows)
            {
                throw new InvalidDataException("Умножение невозможно(количество столбцов первой матрицы не равно количеству строк второй)");
            }

            Matrix result = new Matrix(a.Rows, b.Columns);

            Thread[] threads = new Thread[numThreads];
            int rowsForThread = a.Rows / numThreads;

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
                threads[i].Start();
            }
            foreach (var thread in threads)
            {
                thread.Join();
            }

            return result;
        }
    }
}