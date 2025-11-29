// <copyright file="TestsSequentialMultiply.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace ParallelMatrixMultiplication.Test;

public class TestsSequentialMultiply
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
        { 1, 2 },
        { 3, 4 },
        });

        this.testMatrixB2x2 = Matrix.FillInFromArray(new int[,]
        {
        { 9, 8 },
        { 7, 6 },
        });

        this.testMatrixA2x3 = Matrix.FillInFromArray(new int[,]
        {
        { 2, -3, 1 },
        { 5, 4, -2 },
        });

        this.testMatrixB3x2 = Matrix.FillInFromArray(new int[,]
        {
        { -7, 5 },
        { 2, -1 },
        { 4, 3 },
        });

        this.firstExpectedResult = Matrix.FillInFromArray(new int[,]
        {
        { 23, 20 },
        { 55, 48 },
        });

        this.secondExpectedResult = Matrix.FillInFromArray(new int[,]
        {
        { -16, 16 },
        { -35, 15 },
        });
    }

    [Test]
    public void SequentialMultiplication_SquareMatrices_ShouldReturnExpectedResult()
    {
        var result = MatrixMultiplier.SequentialMultiplication(this.testMatrixA2x2, this.testMatrixB2x2);
        for (int i = 0; i < result.Rows; ++i)
        {
            for (int j = 0; j < result.Columns; ++j)
            {
                Assert.That(result[i, j], Is.EqualTo(this.firstExpectedResult[i, j]));
            }
        }
    }

    [Test]
    public void SequentialMultiplication_NotSquareMatrices_ShouldReturnExpectedResult()
    {
        var result = MatrixMultiplier.SequentialMultiplication(this.testMatrixA2x3, this.testMatrixB3x2);
        for (int i = 0; i < result.Rows; ++i)
        {
            for (int j = 0; j < result.Columns; ++j)
            {
                Assert.That(result[i, j], Is.EqualTo(this.secondExpectedResult[i, j]));
            }
        }
    }
}