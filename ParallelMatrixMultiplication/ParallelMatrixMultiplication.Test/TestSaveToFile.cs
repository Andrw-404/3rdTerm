// <copyright file="TestSaveToFile.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace ParallelMatrixMultiplication.Test;

public class TestSaveToFile
{
    private Matrix matrixToSave;
    private string currentFilePath;

    [SetUp]
    public void Setup()
    {
        this.matrixToSave = new Matrix(new int[,]
        {
            { 99, 90, 88, 80 },
            { 77, 70, 66, 60 },
            { 55, 50, 44, 40 },
        });

        this.currentFilePath = "Test_STF.txt";
    }

    [TearDown]
    public void Teardown()
    {
        if (File.Exists(this.currentFilePath))
        {
            File.Delete(this.currentFilePath);
        }
    }

    [Test]
    public void SaveToFile_ValidData_ShouldCreateExpectedFile()
    {
        this.matrixToSave.SaveToFile(this.currentFilePath);

        Assert.That(File.Exists(this.currentFilePath), Is.True);

        string expectedData = File.ReadAllText("expectedForSaveToFileTest.txt");
        string currentData = File.ReadAllText(this.currentFilePath);

        Assert.That(currentData, Is.EqualTo(expectedData));
    }
}