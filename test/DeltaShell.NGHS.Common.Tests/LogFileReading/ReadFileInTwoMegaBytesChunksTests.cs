using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DeltaShell.NGHS.Common.IO.LogFileReading;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.LogFileReading
{
    [TestFixture]
    public class ReadFileInTwoMegaBytesChunksTests
    {
        [Test]
        public void IsInstanceOf_ILogFileReadingInterface()
        {
            var sut = new ReadFileInTwoMegaBytesChunks();
            Assert.IsInstanceOf<ILogFileReader>(sut);
        }

        [Test]
        [TestCaseSource(nameof(RetrieveTestData))]
        public void ReadingFromStream_ReturnsCorrectData(string data)
        {
            // Arrange
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
            var sut = new ReadFileInTwoMegaBytesChunks();

            // Act
            string result = sut.ReadCompleteStream(stream);
            
            // Assert
            StringAssert.AreEqualIgnoringCase(data, result);
        }

        private static IEnumerable<object> RetrieveTestData()
        {
            yield return new object[]
            {
                "SingleLine"
            };
            
            yield return new object[]
            {
                "first line"
                + Environment.NewLine
                + "Another line"
            };
        }
    }
}