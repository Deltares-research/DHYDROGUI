using System;
using System.Collections.Generic;
using System.Globalization;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter.RRBoundaryConditionsHelpers;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport.RRBoundaryConditionsHelpers
{
    [TestFixture]
    public class RRBoundaryConditionsConverterTest
    {
        private const string constantFunctionType = "constant";

        [Test]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException()
        {
            // Arrange & Act
            void Call() => new RRBoundaryConditionsConverter(null);

            // Assert
            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void Convert_ArgumentNull_ThrowsArgumentNullException()
        {
            // Arrange
            var dataParserProvider = new RRBoundaryConditionsDataParserProvider(Substitute.For<ILogHandler>(), Substitute.For<IBcCategoryParser>());
            var converter = new RRBoundaryConditionsConverter(dataParserProvider);

            // Act
            void Call() => converter.Convert(null);

            // Assert
            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(10)]
        public void GivenConstantDataXTimes_WhenConverting_RainfallRunoffBoundaryDataContainsConstantData(int amountOfBcBlockData)
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var bcCategoryParser = Substitute.For<IBcCategoryParser>();
            var parserProvider = new RRBoundaryConditionsDataParserProvider(logHandler, bcCategoryParser);
            var converter = new RRBoundaryConditionsConverter(parserProvider);

            const string baseSupportPoint = "name";
            const double baseValuePoint = 10;

            var bcBlockDatas = new List<BcBlockData>();
            for (var i = 0; i <= amountOfBcBlockData; i++)
            {
                bcBlockDatas.Add(CreateBcBlockData($"{baseSupportPoint}_{i}", baseValuePoint + i));
            }

            // Act
            IReadOnlyDictionary<string, RainfallRunoffBoundaryData> rainfallRunoffBoundaryDatas = converter.Convert(bcBlockDatas);

            // Assert
            for (var i = 0; i <= amountOfBcBlockData; i++)
            {
                Assert.That(rainfallRunoffBoundaryDatas.ContainsKey($"{baseSupportPoint}_{i}"));
                Assert.That(rainfallRunoffBoundaryDatas[$"{baseSupportPoint}_{i}"].Value, Is.EqualTo(baseValuePoint + i));
            }
        }

        [Test]
        public void GivenTimeseriesData_WhenConverting_RainfallRunoffBoundaryDataContainsTimeseriesData()
        {
            // Arrange
            var logHandler = Substitute.For<ILogHandler>();
            var bcCategoryParser = new BcCategoryParser(logHandler);
            var parserProvider = new RRBoundaryConditionsDataParserProvider(logHandler, bcCategoryParser);
            var converter = new RRBoundaryConditionsConverter(parserProvider);

            const string name = "boundary_location_name";
            BcBlockData timeSeriesBlockData = CreateTimeSeriesBcBlockData(name);
            var bcBlockDatas = new List<BcBlockData> { timeSeriesBlockData };

            // Act
            IReadOnlyDictionary<string, RainfallRunoffBoundaryData> rainfallRunoffBoundaryDatas = converter.Convert(bcBlockDatas);

            // Assert
            Assert.That(rainfallRunoffBoundaryDatas, Has.Count.EqualTo(1));
            RainfallRunoffBoundaryData convertedData = rainfallRunoffBoundaryDatas[name];
            Assert.That(convertedData.IsTimeSeries, Is.True);

            var referenceDate = new DateTime(2023, 2, 15);
            Assert.That(convertedData.Data[referenceDate.AddMinutes(0)], Is.EqualTo(1));
            Assert.That(convertedData.Data[referenceDate.AddMinutes(525600)], Is.EqualTo(2));
            Assert.That(convertedData.Data[referenceDate.AddMinutes(1052640)], Is.EqualTo(3));
        }

        private BcBlockData CreateBcBlockData(string name, double value)
        {
            var quantityData = new BcQuantityData();
            quantityData.Values.Add(value.ToString(CultureInfo.InvariantCulture));
            var bcBlockData = new BcBlockData()
            {
                SupportPoint = name,
                FunctionType = constantFunctionType,
                Quantities = new List<BcQuantityData>() { quantityData }
            };

            return bcBlockData;
        }

        private static BcBlockData CreateTimeSeriesBcBlockData(string name)
        {
            var bcQuantityTime = new BcQuantityData
            {
                Quantity = "time",
                Unit = "minutes since 2023-02-15 00:00:00",
                Values = new List<string>
                {
                    "0",
                    "525600",
                    "1052640"
                }
            };

            var bcQuantityData = new BcQuantityData
            {
                Quantity = "water_level",
                Unit = "m",
                Values = new List<string>
                {
                    "1",
                    "2",
                    "3"
                }
            };

            return new BcBlockData
            {
                FunctionType = "timeseries",
                SupportPoint = name,
                Quantities = new List<BcQuantityData>
                {
                    bcQuantityTime,
                    bcQuantityData
                }
            };
        }
    }
}