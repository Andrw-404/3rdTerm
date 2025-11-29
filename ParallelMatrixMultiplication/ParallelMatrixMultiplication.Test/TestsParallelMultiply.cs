// <copyright file="TestsParallelMultiply.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace ParallelMatrixMultiplication.Test;

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
        this.testMatrixA2x2 = Matrix.FillInFromArray(new int[,]
        {
        { 9, 4 },
        { 5, 1 },
        });

        this.testMatrixB2x2 = Matrix.FillInFromArray(new int[,]
        {
        { 5, 3 },
        { 8, 7 },
        });

        this.testMatrixA2x3 = Matrix.FillInFromArray(new int[,]
        {
        { 2, 1, 0 },
        { 7, -5, 6 },
        });

        this.testMatrixB3x2 = Matrix.FillInFromArray(new int[,]
        {
        { -2, 6 },
        { 4, 2 },
        { 3, 8 },
        });

        this.firstExpectedResult = Matrix.FillInFromArray(new int[,]
        {
        { 77, 55 },
        { 33, 22 },
        });

        this.secondExpectedResult = Matrix.FillInFromArray(new int[,]
        {
        { 0, 14 },
        { -16, 80 },
        });
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