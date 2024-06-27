using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter.RRBoundaryConditionsHelpers;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport.RRBoundaryConditionsHelpers
{
    [TestFixture]
    public class RRBoundaryConditionsTimeSeriesParserTest
    {
        private const string functionType = "timeseries";
        private const string nameOfSupportPoint = "NameOfSupportPoint";

        private const string waterLevel = "water_level";
        private const string unit = "m";

        [Test]
        [TestCaseSource(nameof(ConstructorArgumentNullCases))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(ILogHandler logHandler, IBcSectionParser parser)
        {
            // Arrange & Act
            void Call() => new RRBoundaryConditionsTimeSeriesParser(logHandler, parser);

            // Assert
            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void Parse_ArgumentNull_ThrowsArgumentNullException()
        {
            // Arrange
            var parser = new RRBoundaryConditionsTimeSeriesParser(Substitute.For<ILogHandler>(), Substitute.For<IBcSectionParser>());

            // Act
            void Call() => parser.Parse(null);

            // Assert
            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void Parse_ArgumentNotTimeseries_ThrowsArgumentException()
        {
            // Arrange
            var parser = new RRBoundaryConditionsTimeSeriesParser(Substitute.For<ILogHandler>(), Substitute.For<IBcSectionParser>());
            const string expectedArgumentExceptionMessage = "The provided 'BcBlockData' is not timeseries.";
            var bcBlockData = new BcBlockData { FunctionType = "NotTimeseries" };

            // Act
            void Call() => parser.Parse(bcBlockData);

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.Message, Is.EqualTo(expectedArgumentExceptionMessage));
        }

        [Test]
        public void GivenValidBcDataBlock_WhenParsing_ThenExpectCorrectlyParsed()
        {
            // Arrange
            var parser = new RRBoundaryConditionsTimeSeriesParser(Substitute.For<ILogHandler>(), new BcSectionParser(Substitute.For<ILogHandler>()));

            // Act
            RainfallRunoffBoundaryData rainfallRunoffBoundaryData = parser.Parse(GetTimeSeriesToBcBlockData());

            // Assert
            IMultiDimensionalArray dateTimes = rainfallRunoffBoundaryData.Data.Arguments[0].Values;
            IMultiDimensionalArray values = rainfallRunoffBoundaryData.Data.Components[0].Values;

            var expectedFirstDate = new DateTime(2023, 2, 15, 0, 0, 0);
            var expectedSecondDate = new DateTime(2024, 2, 15, 0, 0, 0);
            var expectedThirdDate = new DateTime(2025, 2, 15, 0, 0, 0);

            Assert.That(rainfallRunoffBoundaryData.IsTimeSeries, Is.True);

            Assert.That(rainfallRunoffBoundaryData.Data.Arguments[0].ExtrapolationType, Is.EqualTo(ExtrapolationType.Periodic));

            Assert.That(dateTimes[0], Is.EqualTo(expectedFirstDate));
            Assert.That(dateTimes[1], Is.EqualTo(expectedSecondDate));
            Assert.That(dateTimes[2], Is.EqualTo(expectedThirdDate));

            Assert.That(values[0], Is.EqualTo(1));
            Assert.That(values[1], Is.EqualTo(2));
            Assert.That(values[2], Is.EqualTo(3));
        }

        [Test]
        public void GivenNoDataInBcDataBlock_WhenParsing_ThenExpectWarningAndValueOf0()
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var bcCategoryParser = Substitute.For<IBcSectionParser>();
            var parser = new RRBoundaryConditionsTimeSeriesParser(logHandler, bcCategoryParser);

            var bcBlockData = new BcBlockData()
            {
                SupportPoint = nameOfSupportPoint,
                FunctionType = functionType,
                Quantities = new List<BcQuantityData>()
                {
                    new BcQuantityData(),
                    new BcQuantityData()
                }
            };

            // Act
            RainfallRunoffBoundaryData rainfallRunoffBoundaryData = parser.Parse(bcBlockData);

            // Assert
            var expectedMessage = $"No boundary data available for boundary \"{bcBlockData.SupportPoint}\"";
            logHandler.Received(1).ReportWarning(expectedMessage);

            IMultiDimensionalArray dateTimes = rainfallRunoffBoundaryData.Data.Arguments[0].Values;
            IMultiDimensionalArray values = rainfallRunoffBoundaryData.Data.Components[0].Values;
            Assert.That(rainfallRunoffBoundaryData.IsTimeSeries, Is.True);
            Assert.That(dateTimes.Count, Is.EqualTo(0));
            Assert.That(values, Is.Empty);
        }

        [Test]
        public void GivenInvalidDataInBcDataBlock_WhenParsing_ThenExpectWarningAndValueOf0()
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var bcCategoryParser = Substitute.For<IBcSectionParser>();
            var parser = new RRBoundaryConditionsTimeSeriesParser(logHandler, bcCategoryParser);

            var quantityData = new BcQuantityData();
            quantityData.Values.Add("InvalidData");
            var bcBlockData = new BcBlockData()
            {
                SupportPoint = nameOfSupportPoint,
                FunctionType = functionType,
                Quantities = new List<BcQuantityData>()
                {
                    quantityData,
                    quantityData
                }
            };

            // Act
            RainfallRunoffBoundaryData rainfallRunoffBoundaryData = parser.Parse(bcBlockData);

            // Assert
            var expectedMessage = $"No valid data available for boundary \"{bcBlockData.SupportPoint}\"";
            logHandler.Received(1).ReportError(expectedMessage);

            IMultiDimensionalArray dateTimes = rainfallRunoffBoundaryData.Data.Arguments[0].Values;
            IMultiDimensionalArray values = rainfallRunoffBoundaryData.Data.Components[0].Values;
            Assert.That(rainfallRunoffBoundaryData.IsTimeSeries, Is.True);
            Assert.That(dateTimes.Count, Is.EqualTo(0));
            Assert.That(values.Count, Is.EqualTo(0));
        }

        private static IEnumerable<TestCaseData> ConstructorArgumentNullCases()
        {
            yield return new TestCaseData(null, Substitute.For<IBcSectionParser>());
            yield return new TestCaseData(Substitute.For<ILogHandler>(), null);
        }

        private BcBlockData GetTimeSeriesToBcBlockData()
        {
            var data = new BcBlockData();
            data.FunctionType = functionType;

            var bcQuantityTime = new BcQuantityData()
            {
                Quantity = "time",
                Unit = "minutes since 2023-02-15 00:00:00",
                Values = new List<string>()
                {
                    "0",
                    "525600",
                    "1052640"
                }
            };

            var bcQuantityData = new BcQuantityData()
            {
                Quantity = waterLevel,
                Unit = unit,
                Values = new List<string>()
                {
                    "1",
                    "2",
                    "3"
                }
            };

            data.Quantities = new List<BcQuantityData>()
            {
                bcQuantityTime,
                bcQuantityData
            };
            return data;
        }
    }
}