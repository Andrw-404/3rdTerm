// <copyright file="TestLoadFromFile.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace ParallelMatrixMultiplication.Test;

public class TestLoadFromFile
{
    private Matrix firstExpectedMatrix;
    private Matrix secondExpectedMatrix;

    [SetUp]
    public void Setup()
    {
        this.firstExpectedMatrix = new Matrix(new int[,]
        {
            { 15, 16, 17 },
            { 20, 23, 26 },
            { 29, 32, 35 },
        });

        this.secondExpectedMatrix = new Matrix(new int[,]
        {
            { 1, 2, 3, 4, 5 },
            { 6, 7, 8, 9, 10 },
        });
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