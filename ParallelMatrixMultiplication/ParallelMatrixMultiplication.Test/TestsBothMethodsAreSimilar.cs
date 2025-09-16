// <copyright file="TestsBothMethodsAreSimilar.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace ParallelMatrixMultiplication.Test
{
    public class TestsBothMethodsAreSimilar
    {
        private Matrix testRandomMatrixA;
        private Matrix testRandomMatrixB;

        [SetUp]
        public void Setup()
        {
            this.testRandomMatrixA = Matrix.GenerateRandomMatrix(77, 33);
            this.testRandomMatrixB = Matrix.GenerateRandomMatrix(33, 77);
        }

        [Test]
        public void SequentialAndParallelMultiplication_RandomMatrix_ShouldReturnSameResult()
        {
            var parallelResult = MatrixMultiplier.ParallelMultiplication(this.testRandomMatrixA, this.testRandomMatrixB, 4);
            var sequentialResult = MatrixMultiplier.SequentialMultiplication(this.testRandomMatrixA, this.testRandomMatrixB);
            for (int i = 0; i < parallelResult.Rows; ++i)
            {
                for (int j = 0; j < parallelResult.Columns; ++j)
                {
                    Assert.That(parallelResult[i, j], Is.EqualTo(sequentialResult[i, j]));
                }
            }
        }
    }
}