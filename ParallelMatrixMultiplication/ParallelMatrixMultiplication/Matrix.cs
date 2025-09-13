using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelMatrixMultiplication
{
    class Matrix
    {
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        private readonly int[,] numbers;

        public Matrix(int rows, int columns)
        {
            this.numbers = new int[rows, columns];
        }

        public int this[int rows, int columns]
        {
            get => numbers[rows, columns];
            set => numbers[rows, columns] = value;
        }

        public static Matrix LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Файл не найден");
            }

            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line = string.Empty;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            if (lines.Count == 0)
            {
                throw new InvalidDataException("Файл пуст");
            }

            int rows = lines.Count;
            string[] firstRowValues = lines[0].Split(' ');
            int columnsSize = firstRowValues.Length;

            Matrix matrix = new Matrix(rows, columnsSize);

            for (int i = 0; i < rows; i++)
            {
                string[] values = lines[i].Split(' ');
                if (values.Length != columnsSize)
                {
                    throw new InvalidDataException("Срока имеет некорректное количество столбцов");
                }

                for (int j = 0; j < columnsSize; j++)
                {
                    matrix[i, j] = int.Parse(values[j]);
                }
            }

            return matrix;
        }
    }
}
