// <copyright file="Matrix.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace ParallelMatrixMultiplication
{
    /// <summary>
    /// A class representing a matrix and operations for working with it.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Matrix"/> class.
    /// </remarks>
    /// <param name="rows">The number of specified rows.</param>
    /// <param name="columns">The number of specified columns.</param>
    public class Matrix(int rows, int columns)
    {
        /// <summary>
        /// An array for storing matrix elements.
        /// </summary>
        private readonly int[,] numbers = new int[rows, columns];

        /// <summary>
        /// Gets number of rows in the matrix.
        /// </summary>
        public int Rows { get; private set; } = rows;

        /// <summary>
        /// Gets number of columns in the matrix.
        /// </summary>
        public int Columns { get; private set; } = columns;

        /// <summary>
        /// Indexer for accessing matrix elements.
        /// </summary>
        /// <param name="rows">Row index.</param>
        /// <param name="columns">Column index.</param>
        /// <returns>The value of the element.</returns>
        public int this[int rows, int columns]
        {
            get => this.numbers[rows, columns];
            set => this.numbers[rows, columns] = value;
        }

        /// <summary>
        /// A method for generating a matrix with random values.
        /// </summary>
        /// <param name="rows">Number of lines.</param>
        /// <param name="columns">Number of columns.</param>
        /// <returns>Matrix with random values.</returns>
        public static Matrix GenerateRandomMatrix(int rows, int columns)
        {
            Random random = new Random();
            Matrix matrix = new Matrix(rows, columns);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; ++j)
                {
                    matrix[i, j] = random.Next(1, 74);
                }
            }

            return matrix;
        }

        /// <summary>
        /// A method for loading a matrix from a file.
        /// </summary>
        /// <param name="filePath">The path to the matrix file.</param>
        /// <returns>The matrix from the file.</returns>
        /// <exception cref="FileNotFoundException">It is thrown if the file is not found.</exception>
        /// <exception cref="InvalidDataException">It is thrown if the file is empty or contains incorrect data.</exception>
        /// <exception cref="FormatException">It is thrown if the numbers in the file have an incorrect format.</exception>
        public static Matrix LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Файл не найден");
            }

            string[] lines = File.ReadAllLines(filePath);

            if (lines.Length == 0)
            {
                throw new InvalidDataException("Файл пуст");
            }

            int rows = lines.Length;

            string[] firstRowValues = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int columnsSize = firstRowValues.Length;

            Matrix matrix = new Matrix(rows, columnsSize);

            for (int i = 0; i < rows; ++i)
            {
                string[] values = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (values.Length != columnsSize)
                {
                    throw new InvalidDataException("Строка имеет некорректное значение стобцов");
                }

                for (int j = 0; j < columnsSize; ++j)
                {
                    if (!int.TryParse(values[j], out int parsedValue))
                    {
                        throw new FormatException("Неверный формат числа в строке");
                    }

                    matrix[i, j] = parsedValue;
                }
            }

            return matrix;
        }

        /// <summary>
        /// A method for saving a matrix to a file.
        /// </summary>
        /// <param name="filePath">The path to save the file.</param>
        public void SaveToFile(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                for (int i = 0; i < this.Rows; i++)
                {
                    for (int j = 0; j < this.Columns; j++)
                    {
                        writer.Write(this.numbers[i, j] + (j == this.Columns - 1 ? string.Empty : " "));
                    }

                    writer.WriteLine();
                }
            }
        }
    }
}