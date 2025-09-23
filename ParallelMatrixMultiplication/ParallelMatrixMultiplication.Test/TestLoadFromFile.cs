// <copyright file="TestLoadFromFile.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace ParallelMatrixMultiplication.Test
{
    public class TestLoadFromFile
    {
        private Matrix firstExpectedMatrix;
        private Matrix secondExpectedMatrix;

        [SetUp]
        public void Setup()
        {
            this.firstExpectedMatrix = new Matrix(3, 3);
            this.firstExpectedMatrix[0, 0] = 15;
            this.firstExpectedMatrix[0, 1] = 16;
            this.firstExpectedMatrix[0, 2] = 17;
            this.firstExpectedMatrix[1, 0] = 20;
            this.firstExpectedMatrix[1, 1] = 23;
            this.firstExpectedMatrix[1, 2] = 26;
            this.firstExpectedMatrix[2, 0] = 29;
            this.firstExpectedMatrix[2, 1] = 32;
            this.firstExpectedMatrix[2, 2] = 35;

            this.secondExpectedMatrix = new Matrix(2, 5);
            this.secondExpectedMatrix[0, 0] = 1;
            this.secondExpectedMatrix[0, 1] = 2;
            this.secondExpectedMatrix[0, 2] = 3;
            this.secondExpectedMatrix[0, 3] = 4;
            this.secondExpectedMatrix[0, 4] = 5;
            this.secondExpectedMatrix[1, 0] = 6;
            this.secondExpectedMatrix[1, 1] = 7;
            this.secondExpectedMatrix[1, 2] = 8;
            this.secondExpectedMatrix[1, 3] = 9;
            this.secondExpectedMatrix[1, 4] = 10;
        }

        [Test]
        public void LoadFromFile_SquareMatrix_ShouldReturnExpectedResult()
        {
            var result = Matrix.LoadFromFile("firstTestFileForRead.txt");
            for (int i = 0; i < result.Rows; ++i)
            {
                for (int j = 0; j < result.Columns; ++j)
                {
                    Assert.That(result[i, j], Is.EqualTo(this.firstExpectedMatrix[i, j]));
                }
            }
        }

        [Test]
        public void LoadFromFile_NotSquareMatrix_ShouldReturnExpectedResult()
        {
            var result = Matrix.LoadFromFile("secondTestFileForRead.txt");
            for (int i = 0; i < result.Rows; ++i)
            {
                for (int j = 0; j < result.Columns; ++j)
                {
                    Assert.That(result[i, j], Is.EqualTo(this.secondExpectedMatrix[i, j]));
                }
            }
        }
    }
}