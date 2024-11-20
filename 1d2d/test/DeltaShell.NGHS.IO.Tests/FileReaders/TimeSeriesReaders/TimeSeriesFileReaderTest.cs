using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.Properties;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.TimeSeriesReaders
{
    [TestFixture]
    public class TimeSeriesFileReaderTest
    {
        private string propertyName;
        private string filePath;
        private DateTime refDate;
        private ISpecificTimeSeriesFileReader substituteSpecificReader;
        private TimeSeriesFileReader reader;

        [SetUp]
        public void SetUp()
        {
            propertyName = "";
            filePath = "";
            refDate = new DateTime();
            substituteSpecificReader = Substitute.For<ISpecificTimeSeriesFileReader>();
        }
        
        [Test]
        public void Constructor_GivenNull_ThenReadThrowsArgumentNullException()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TimeSeriesFileReader(null));
        }
        
        [Test]
        public void Constructor_GivenNothing_ThenReadThrowsArgumentExceptionWithExpectedMessage()
        {
            //Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new TimeSeriesFileReader());
            Assert.That(exception.Message, Is.EqualTo(string.Format(Resources.TimeSeriesFileReader_TimeSeriesFileReader_No_readers_in__0_, nameof(TimeSeriesFileReader)))); 
        }
        
        [Test]
        [TestCaseSource(nameof(ArgNull))]
        public void WhenRead_GivenAnyParameterNull_ThenReadThrowsArgumentNullException(string givenPropertyName, string givenFilePath, IStructureTimeSeries givenStructureTimeSeries, DateTime givenRefData)
        {
            //Arrange
            substituteSpecificReader.CanReadProperty(Arg.Any<string>()).Returns(true);
            reader = new TimeSeriesFileReader(substituteSpecificReader);
            
            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => reader.Read(givenPropertyName, givenFilePath, givenStructureTimeSeries, givenRefData));
        }
        
        private static IEnumerable<TestCaseData> ArgNull()
        {
            yield return new TestCaseData(null, string.Empty, Substitute.For<IStructureTimeSeries>(), new DateTime());
            yield return new TestCaseData(string.Empty, null, Substitute.For<IStructureTimeSeries>(), new DateTime());
            yield return new TestCaseData(string.Empty, string.Empty, null, new DateTime());
        }
        
        [Test]
        public void WhenTimeSeriesFileReader_GivenReaders_ThenIsTimeSeriesPropertyReturnsTrue()
        {
            //Arrange
            substituteSpecificReader.CanReadProperty(Arg.Any<string>()).Returns(true);
            reader = new TimeSeriesFileReader(substituteSpecificReader);
            
            //Act & Assert
            Assert.That(reader.IsTimeSeriesProperty("fileName"), Is.True);
        }
        
        [Test]
        public void WhenTimeSeriesFileReader_GivenReaders_ThenReadThrowsNoException()
        {
            //Arrange
            substituteSpecificReader.CanReadProperty(Arg.Any<string>()).Returns(true);
            reader = new TimeSeriesFileReader(substituteSpecificReader);
            
            //Act & Assert
            Assert.DoesNotThrow(() => reader.Read(propertyName, filePath, Substitute.For<IStructureTimeSeries>(), refDate));
        }
        
        [Test]
        public void WhenIsTimeSeriesProperty_GivenUnknownPropertyName_ThenIsTimeSeriesPropertyReturnsFalse()
        {
            //Arrange
            reader = new TimeSeriesFileReader(Substitute.For<ISpecificTimeSeriesFileReader>());
            
            //Act & Assert
            Assert.That(reader.IsTimeSeriesProperty("fileName"), Is.False);
        }
    }
}