using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.FileWriters.TimeSeriesWriters;
using DeltaShell.NGHS.IO.Helpers;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters
{
    [TestFixture]
    public class BcTimeSeriesWriterTest
    {
        private IBcFileWriter writer;
        private IStructureBoundaryGenerator structureBoundaryFileWriter;
        private string filepath;
        private DateTime time;
        private List<DelftBcCategory> boundary;
        private const string structureName =  "structureName";
        
        [SetUp]
        public void Setup()
        {
            writer = Substitute.For<IBcFileWriter>();
            structureBoundaryFileWriter = Substitute.For<IStructureBoundaryGenerator>();
            
            boundary = CreateBoundary();

            time = new DateTime(10,10,10,10,10,10);
            
            filepath =  TestHelper.GetTestFilePath(@"BcFiles\Structure.bc");
        }

        private List<DelftBcCategory> CreateBoundary()
        {
            boundary = new List<DelftBcCategory>();
            var category = new DelftBcCategory("boundary");
            boundary.Add(category);
            return boundary;
        }

        [Test]
        public void WhenSingleBoundaryWritten_ThenWriteIsCalled()
        {
            //Arrange
            ITimeSeries structureData = Substitute.For<ITimeSeries>();
            structureBoundaryFileWriter.GenerateBoundary(structureName, structureData, time).Returns(boundary);
            BcTimeSeriesWriter bcTimeSeriesWriter = new BcTimeSeriesWriter(writer,structureBoundaryFileWriter);
            
            //Act & Assert
            Assert.That(writer.ReceivedCalls().Count(), Is.EqualTo(0));
            bcTimeSeriesWriter.Write(filepath, structureName,  structureData, time);
            Assert.That(writer.ReceivedCalls().Count(), Is.EqualTo(1));

        }
        
        [Test]
        [TestCaseSource(nameof(ArgNullSingleBoundary))]
        public void WhenSingleBoundaryWritten_WhenArgumentsAreNull_ThenThrowArgumentNullException(string givenFilePath, string givenStructureName, ITimeSeries givenStructureData)
        {
            //Arrange
            BcTimeSeriesWriter bcTimeSeriesWriter = new BcTimeSeriesWriter(Substitute.For<IBcFileWriter>(),
                                                                           Substitute.For<IStructureBoundaryGenerator>());
            
            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => bcTimeSeriesWriter.Write(givenFilePath, givenStructureName, givenStructureData , new DateTime()));
        }
        
        private static IEnumerable<TestCaseData> ArgNullSingleBoundary()
        {
            yield return new TestCaseData(null, string.Empty, Substitute.For<ITimeSeries>());
            yield return new TestCaseData(string.Empty, null, Substitute.For<ITimeSeries>());
            yield return new TestCaseData(string.Empty, string.Empty, null);
        }
        
        [Test]
        public void WhenMultipleBoundariesWritten_ThenWriteIsCalled()
        {
            //Arrange
            IEnumerable<IStructureTimeSeries> structureData = Substitute.For<IEnumerable<IStructureTimeSeries>>();
            structureBoundaryFileWriter.GenerateBoundaries(structureData, time).Returns(boundary);
            BcTimeSeriesWriter bcTimeSeriesWriter = new BcTimeSeriesWriter(writer,structureBoundaryFileWriter);
            
            //Act & Assert
            Assert.That(writer.ReceivedCalls().Count(), Is.EqualTo(0));
            bcTimeSeriesWriter.Write(filepath, structureData, time);
            Assert.That(writer.ReceivedCalls().Count(), Is.EqualTo(1));
        }

        [Test]
        [TestCaseSource(nameof(ArgNullMultipleBoundaries))]
        public void WhenMultipleBoundariesWritten_WhenArgumentsAreNull_ThenThrowArgumentNullException(string givenFilePath, IEnumerable<IStructureTimeSeries> givenStructureData)
        {
            //Arrange
            BcTimeSeriesWriter bcTimeSeriesWriter = new BcTimeSeriesWriter(Substitute.For<IBcFileWriter>(),
                                                                           Substitute.For<IStructureBoundaryGenerator>());
            
            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => bcTimeSeriesWriter.Write(givenFilePath, givenStructureData, new DateTime()));
        }

        private static IEnumerable<TestCaseData> ArgNullMultipleBoundaries()
        {
            yield return new TestCaseData(null, Substitute.For<IEnumerable<IStructureTimeSeries>>());
            yield return new TestCaseData(string.Empty, null);
        }
        

    }
}