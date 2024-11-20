using System;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.TimeSeriesReaders
{
    [TestFixture]
    public class TimSpecificTimeSeriesReaderTest
    {
        [Test]
        public void WhenFileIsRead_ThenReaderUsesRead()
        {
            //Arrange
            var timFileReader = Substitute.For<ITimFileReader>();
            var reader = new TimSpecificTimeSeriesReader(timFileReader);
            timFileReader.ClearReceivedCalls();
            const string filePath = "someFilePath.tim";

            IFunction function = Substitute.For<ITimeSeries>();
            var structureTimeSeries = Substitute.For<IStructureTimeSeries>();
            structureTimeSeries.TimeSeries.Returns(function);

            DateTime refTime = DateTime.Now;

            //Act
            reader.Read(filePath, structureTimeSeries, refTime);

            // Assert
            Assert.That(timFileReader.ReceivedCalls().Count(), Is.EqualTo(1));
            timFileReader.Received(1).Read(filePath, function, refTime);
        }

        [Test]
        [TestCase("file.tim", true)]
        [TestCase("file.abc", false)]
        public void WhenCanReadProperty_GivenFileName_ReturnExpectedValue(string fileName, bool expectedReturnValue)
        {
            //Arrange
            ITimFileReader timFileReader = Substitute.For<ITimFileReader>();
            TimSpecificTimeSeriesReader reader = new TimSpecificTimeSeriesReader(timFileReader);
            
            //Act & Assert
            Assert.That(reader.CanReadProperty(fileName), Is.EqualTo(expectedReturnValue));
        }
    }
}