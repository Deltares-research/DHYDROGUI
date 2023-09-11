using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;
using Arg = NSubstitute.Arg;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.TimeSeriesReaders
{
    [TestFixture]
    public class BcSpecificTimeSeriesReaderTest
    {
        private IDelftBcReader reader;
        private IBcCategoryParser parser;
        private ILogHandler logHandler;
        private IStructureTimeSeries structureTimeSeries;
        private const string filePath = "path";
        private const string quantity = "weir_crestLevel";
        private const string structureName = "Weir";
        private const string propertyName = "name";
        private const string tableQuantity = "quantity";
        private const string tableUnit = "unit";
        private const string unit = "m AD";
        private const string timeString = "time";
        private const string timeSince = "minutes since 2022-09-15 00:00:00";
        private const string timeSinceValue = "525600";
        private DateTime time;

        [SetUp]
        public void SetUp()
        {
            reader = Substitute.For<IDelftBcReader>();
            parser = Substitute.For<IBcCategoryParser>();
            logHandler = Substitute.For<ILogHandler>();
            structureTimeSeries = Substitute.For<IStructureTimeSeries>();
            structureTimeSeries.Structure.Name.Returns(structureName);
            structureTimeSeries.Structure.Returns(new Weir());
            structureTimeSeries.TimeSeries.Returns(new TimeSeries {Name = quantity});
            time = new DateTime(10, 10, 10, 10, 10, 10);
            IList<IDelftBcCategory> structuresFromFile = new List<IDelftBcCategory>();
            structuresFromFile.Add(GetCategory());
            reader.ReadDelftBcFile(filePath).Returns(structuresFromFile);
        }

        private IDelftBcCategory GetCategory()
        {
            IDelftBcCategory category = new DelftBcCategory("");
            category.Properties.AddRange(GetProperties());
            category.Table = GetTable();
            return category;
        }

        private List<DelftIniProperty> GetProperties()
        {
            List<DelftIniProperty> list = new List<DelftIniProperty> {new DelftIniProperty(propertyName,structureName," ")};
            return list;
        }
        
        private List<IDelftBcQuantityData> GetTable()
        {
            List<IDelftBcQuantityData> list = new List<IDelftBcQuantityData>
            {
                GetDelftBcQuantityData(timeString, timeSince, timeSinceValue),
                GetDelftBcQuantityData(quantity, unit, "100")
            };
            return list;
        }

        private static DelftBcQuantityData GetDelftBcQuantityData(string quantityString, string unitString, string valueString)
        {
            var delftBcQuantityDataTime = new DelftBcQuantityData(new DelftIniProperty(tableQuantity, quantityString, ""));
            delftBcQuantityDataTime.Unit = new DelftIniProperty(tableUnit, unitString, "");
            delftBcQuantityDataTime.Values = new List<string>() {valueString};
            return delftBcQuantityDataTime;
        }

        private static IEnumerable<TestCaseData> ArgumentNullCases()
        {
            yield return new TestCaseData(null,Substitute.For<IStructureTimeSeries>(),new DateTime(10, 10, 10, 10, 10, 10));
            yield return new TestCaseData("filePath",null,new DateTime(10, 10, 10, 10, 10, 10));
        }

        [Test]
        [TestCaseSource(nameof(ArgumentNullCases))]
        public void WhenReadingBcFile_ArgumentsAreNull_ThrowArgumentNullException(string filePath, IStructureTimeSeries structureTimeSeries, DateTime refDate)
        {
            // Setup
            BcSpecificTimeSeriesReader bcSpecificTimeSeriesReader = new BcSpecificTimeSeriesReader(reader, parser, logHandler);

            // Call
            void Call() => bcSpecificTimeSeriesReader.Read(filePath, structureTimeSeries, refDate);

            // Assert
            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void WhenReadingBcFile_WithCorrectData_TimeSeriesIsAdjustedWithCompleteFunctionOnce()
        {
            //Arrange
            parser.TryParseDateTimes(null, null, 0, out IEnumerable<DateTime> dateTimes).ReturnsForAnyArgs(true);
            parser.TryParseDoubles(null, 0, out IEnumerable<double> values).ReturnsForAnyArgs(true);
            BcSpecificTimeSeriesReader bcSpecificTimeSeriesReader = new BcSpecificTimeSeriesReader(reader, parser, logHandler);

            //Act & Assert
            bcSpecificTimeSeriesReader.Read(filePath, structureTimeSeries, time);
            parser.Received(1).CompleteFunction(structureTimeSeries.TimeSeries,
                                                dateTimes,
                                                values,
                                                Arg.Any<string>());
        }

        [Test]
        public void WhenReadingBcFile_WhenDateTimesIncorrect_TimeSeriesIsNotAdjustedWithCompleteFunction()
        {
            //Arrange
            parser.TryParseDateTimes(null, null, 0, out IEnumerable<DateTime> _).ReturnsForAnyArgs(false);
            BcSpecificTimeSeriesReader bcSpecificTimeSeriesReader = new BcSpecificTimeSeriesReader(reader, parser, logHandler);

            //Act & Assert
            bcSpecificTimeSeriesReader.Read(filePath, structureTimeSeries, time);
            parser.Received(0).CompleteFunction(Arg.Any<IFunction>(),
                                                Arg.Any<IEnumerable<DateTime>>(),
                                                Arg.Any<IEnumerable<double>>(),
                                                Arg.Any<string>());
        }

        [Test]
        public void WhenReadingBcFile_WhenDoublesIncorrect_TimeSeriesIsNotAdjustedWithCompleteFunction()
        {
            //Arrange
            parser.TryParseDateTimes(null, null, 0, out IEnumerable<DateTime> _).ReturnsForAnyArgs(true);
            parser.TryParseDoubles(null, 0, out IEnumerable<double> _).ReturnsForAnyArgs(false);
            BcSpecificTimeSeriesReader bcSpecificTimeSeriesReader = new BcSpecificTimeSeriesReader(reader, parser, logHandler);

            //Act & Assert
            bcSpecificTimeSeriesReader.Read(filePath, structureTimeSeries, time);
            parser.Received(0).CompleteFunction(Arg.Any<IFunction>(),
                                                Arg.Any<IEnumerable<DateTime>>(),
                                                Arg.Any<IEnumerable<double>>(),
                                                Arg.Any<string>());
        }
        
        [Test]
        public void WhenReadingBcFile_WhenDifferentName_TimeSeriesIsNotAdjustedWithCompleteFunction_AndLoggingIsMade()
        {
            //Arrange
            parser.TryParseDateTimes(null, null, 0, out IEnumerable<DateTime> dateTimes).ReturnsForAnyArgs(true);
            parser.TryParseDoubles(null, 0, out IEnumerable<double> functionValues).ReturnsForAnyArgs(false);
            BcSpecificTimeSeriesReader bcSpecificTimeSeriesReader = new BcSpecificTimeSeriesReader(reader, parser, logHandler);
            const string differentName = "DifferentName";
            structureTimeSeries.Structure.Name = differentName;

            //Act & Assert
            bcSpecificTimeSeriesReader.Read(filePath, structureTimeSeries, time);
            parser.Received(0).CompleteFunction(Arg.Any<IFunction>(),
                                                Arg.Any<IEnumerable<DateTime>>(),
                                                Arg.Any<IEnumerable<double>>(),
                                                Arg.Any<string>());
            string message = string.Format(Resources.BcSpecificTimeSeriesReader_Read_No_structure_found_with_name__0__quantity__1__in_file__2_,
                                           structureTimeSeries.Structure.Name,
                                           quantity,
                                           filePath);
            logHandler.Received(1).ReportWarning(message);
        }
        
        [Test]
        [TestCase("file.bc", true)]
        [TestCase("file.tim", false)]
        public void WhenCanReadProperty_GivenFileName_ReturnExpectedValue(string fileName, bool expectedReturnValue)
        {
            //Arrange
            parser.TryParseDateTimes(null, null, 0, out IEnumerable<DateTime> _).ReturnsForAnyArgs(true);
            parser.TryParseDoubles(null, 0, out IEnumerable<double> _).ReturnsForAnyArgs(true);
            BcSpecificTimeSeriesReader bcSpecificTimeSeriesReader = new BcSpecificTimeSeriesReader(reader, parser, logHandler);
            
            //Act & Assert
            Assert.That(bcSpecificTimeSeriesReader.CanReadProperty(fileName), Is.EqualTo(expectedReturnValue));
        }
    }
}