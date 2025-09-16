// <copyright file="TestsParallelMultiply.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace ParallelMatrixMultiplication.Test
{
    public class TestsParallelMultiply
    {
        private Matrix testMatrixA2x2;
        private Matrix testMatrixB2x2;

        private Matrix testMatrixA2x3;
        private Matrix testMatrixB3x2;

        private Matrix firstExpectedResult;
        private Matrix secondExpectedResult;

        [SetUp]
        public void Setup()
        {
            this.testMatrixA2x2 = new Matrix(2, 2);
            this.testMatrixA2x2[0, 0] = 9;
            this.testMatrixA2x2[0, 1] = 4;
            this.testMatrixA2x2[1, 0] = 5;
            this.testMatrixA2x2[1, 1] = 1;

            this.testMatrixB2x2 = new Matrix(2, 2);
            this.testMatrixB2x2[0, 0] = 5;
            this.testMatrixB2x2[0, 1] = 3;
            this.testMatrixB2x2[1, 0] = 8;
            this.testMatrixB2x2[1, 1] = 7;

            this.testMatrixA2x3 = new Matrix(2, 3);
            this.testMatrixA2x3[0, 0] = 2;
            this.testMatrixA2x3[0, 1] = 1;
            this.testMatrixA2x3[0, 2] = 0;
            this.testMatrixA2x3[1, 0] = 7;
            this.testMatrixA2x3[1, 1] = -5;
            this.testMatrixA2x3[1, 2] = 6;

            this.testMatrixB3x2 = new Matrix(3, 2);
            this.testMatrixB3x2[0, 0] = -2;
            this.testMatrixB3x2[0, 1] = 6;
            this.testMatrixB3x2[1, 0] = 4;
            this.testMatrixB3x2[1, 1] = 2;
            this.testMatrixB3x2[2, 0] = 3;
            this.testMatrixB3x2[2, 1] = 8;

            this.firstExpectedResult = new Matrix(2, 2);
            this.firstExpectedResult[0, 0] = 77;
            this.firstExpectedResult[0, 1] = 55;
            this.firstExpectedResult[1, 0] = 33;
            this.firstExpectedResult[1, 1] = 22;

            this.secondExpectedResult = new Matrix(2, 2);
            this.secondExpectedResult[0, 0] = 0;
            this.secondExpectedResult[0, 1] = 14;
            this.secondExpectedResult[1, 0] = -16;
            this.secondExpectedResult[1, 1] = 80;
        }

        [Test]
        public void ParallelMultiplication_SquareMatrices_ShouldReturnExpectedResult()
        {
            var result = MatrixMultiplier.ParallelMultiplication(this.testMatrixA2x2, this.testMatrixB2x2, 4);
            for (int i = 0; i < result.Rows; ++i)
            {
                for (int j = 0; j < result.Columns; ++j)
                {
                    Assert.That(result[i, j], Is.EqualTo(this.firstExpectedResult[i, j]));
                }
            }
        }

        [Test]
        public void ParallelMultiplication_NotSquareMatrices_ShouldReturnExpectedResult()
        {
            var result = MatrixMultiplier.ParallelMultiplication(this.testMatrixA2x3, this.testMatrixB3x2, 4);
            for (int i = 0; i < result.Rows; ++i)
            {
                for (int j = 0; j < result.Columns; ++j)
                {
                    Assert.That(result[i, j], Is.EqualTo(this.secondExpectedResult[i, j]));
                }
            }
        }
    }
}