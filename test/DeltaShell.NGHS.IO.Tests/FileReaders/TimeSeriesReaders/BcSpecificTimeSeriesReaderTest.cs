using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;
using Arg = NSubstitute.Arg;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.TimeSeriesReaders
{
    [TestFixture]
    public class BcSpecificTimeSeriesReaderTest
    {
        private IBcReader reader;
        private IBcSectionParser parser;
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
            reader = Substitute.For<IBcReader>();
            parser = Substitute.For<IBcSectionParser>();
            logHandler = Substitute.For<ILogHandler>();
            structureTimeSeries = Substitute.For<IStructureTimeSeries>();
            structureTimeSeries.Structure.Name.Returns(structureName);
            structureTimeSeries.Structure.Returns(new Weir());
            structureTimeSeries.TimeSeries.Returns(new TimeSeries {Name = quantity});
            time = new DateTime(10, 10, 10, 10, 10, 10);
            IList<BcIniSection> structuresFromFile = new List<BcIniSection>();
            structuresFromFile.Add(GetSection());
            reader.ReadBcFile(filePath).Returns(structuresFromFile);
        }

        private BcIniSection GetSection()
        {
            BcIniSection iniSection = new BcIniSection("boundary");
            iniSection.Section.AddMultipleProperties(GetProperties());
            iniSection.Table = GetTable();
            return iniSection;
        }

        private List<IniProperty> GetProperties()
        {
            List<IniProperty> list = new List<IniProperty> {new IniProperty(propertyName,structureName," ")};
            return list;
        }
        
        private List<IBcQuantityData> GetTable()
        {
            List<IBcQuantityData> list = new List<IBcQuantityData>
            {
                GetBcQuantityData(timeString, timeSince, timeSinceValue),
                GetBcQuantityData(quantity, unit, "100")
            };
            return list;
        }

        private static BcQuantityData GetBcQuantityData(string quantityString, string unitString, string valueString)
        {
            var bcQuantityDataTime = new BcQuantityData(new IniProperty(tableQuantity, quantityString, ""));
            bcQuantityDataTime.Unit = new IniProperty(tableUnit, unitString, "");
            bcQuantityDataTime.Values = new List<string>() {valueString};
            return bcQuantityDataTime;
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