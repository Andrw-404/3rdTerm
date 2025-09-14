using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ParallelMatrixMultiplication
{
    public class Matrix
    {
        public int Rows { get; private set; }

        public int Columns { get; private set; }

        private readonly int[,] numbers;

        public Matrix(int rows, int columns)
        {
            this.Rows = rows;
            this.Columns = columns;
            this.numbers = new int[rows, columns];
        }

        public int this[int rows, int columns]
        {
            get => this.numbers[rows, columns];
            set => this.numbers[rows, columns] = value;
        }

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

            string[] firstRowValues = lines[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int columnsSize = firstRowValues.Length;

            Matrix matrix = new Matrix(rows, columnsSize);

            for (int i = 0; i < rows; ++i)
            {
                string[] values = lines[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

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
    }
}