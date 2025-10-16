// <copyright file="TestSaveToFile.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace ParallelMatrixMultiplication.Test
{
    public class TestSaveToFile
    {
        private Matrix matrixToSave;
        private string currentFilePath;

        [SetUp]
        public void Setup()
        {
            this.matrixToSave = new Matrix(3, 4);
            this.matrixToSave[0, 0] = 99;
            this.matrixToSave[0, 1] = 90;
            this.matrixToSave[0, 2] = 88;
            this.matrixToSave[0, 3] = 80;
            this.matrixToSave[1, 0] = 77;
            this.matrixToSave[1, 1] = 70;
            this.matrixToSave[1, 2] = 66;
            this.matrixToSave[1, 3] = 60;
            this.matrixToSave[2, 0] = 55;
            this.matrixToSave[2, 1] = 50;
            this.matrixToSave[2, 2] = 44;
            this.matrixToSave[2, 3] = 40;

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
}