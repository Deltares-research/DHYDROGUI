using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Deserialization;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Data;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Deserialization
{
    [TestFixture]
    public class LateralParserTest
    {
        [Test]
        public void Constructor_ArgNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new LateralParser(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Parse_ArgNull_ThrowsArgumentNullException()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralParser = new LateralParser(logHandler);

            // Call
            void Call() => lateralParser.Parse(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        [TestCase("2d", LateralLocationType.TwoD)]
        [TestCase("2D", LateralLocationType.TwoD)]
        public void Parse_ParsesLateralCategoryWithValues(string locationTypeValue, LateralLocationType expLateralLocationType)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralParser = new LateralParser(logHandler);

            var delftIniCategory = new DelftIniCategory("lateral");
            delftIniCategory.AddProperty("id", "some_id");
            delftIniCategory.AddProperty("name", "some_name");
            delftIniCategory.AddProperty("type", "discharge");
            delftIniCategory.AddProperty("locationType", locationTypeValue);
            delftIniCategory.AddProperty("numCoordinates", "3");
            delftIniCategory.AddProperty("xCoordinates", "1.23 2.34 3.45");
            delftIniCategory.AddProperty("yCoordinates", "4.56 5.67 6.78");
            delftIniCategory.AddProperty("discharge", "7.89");

            // Call
            LateralDTO lateralDTO = lateralParser.Parse(delftIniCategory);

            // Assert
            var expXCoordinates = new[] { 1.23, 2.34, 3.45 };
            var expYCoordinates = new[] { 4.56, 5.67, 6.78 };
            var expDischarge = new Steerable
            {
                Mode = SteerableMode.ConstantValue,
                ConstantValue = 7.89
            };

            var expLateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, expLateralLocationType,
                                               3, expXCoordinates, expYCoordinates, expDischarge);
            Assert.That(lateralDTO, Is.EqualTo(expLateralDTO).Using(new LateralDTOEqualityComparer()));
        }

        [Test]
        public void Parse_ParsesLateralCategoryWithUnknownValues()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralParser = new LateralParser(logHandler);

            var delftIniCategory = new DelftIniCategory("lateral");
            delftIniCategory.AddProperty("id", "some_id");
            delftIniCategory.AddProperty("name", "some_name");
            delftIniCategory.AddProperty("type", "unsupported_type");
            delftIniCategory.AddProperty("locationType", "unsupported_location_type");
            delftIniCategory.AddProperty("numCoordinates", "3");
            delftIniCategory.AddProperty("xCoordinates", "1.23 2.34 3.45");
            delftIniCategory.AddProperty("yCoordinates", "4.56 5.67 6.78");
            delftIniCategory.AddProperty("discharge", "7.89");

            // Call
            LateralDTO lateralDTO = lateralParser.Parse(delftIniCategory);

            // Assert
            var expXCoordinates = new[] { 1.23, 2.34, 3.45 };
            var expYCoordinates = new[] { 4.56, 5.67, 6.78 };
            var expDischarge = new Steerable
            {
                Mode = SteerableMode.ConstantValue,
                ConstantValue = 7.89
            };

            var expLateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Unsupported, LateralLocationType.Unsupported,
                                               3, expXCoordinates, expYCoordinates, expDischarge);
            Assert.That(lateralDTO, Is.EqualTo(expLateralDTO).Using(new LateralDTOEqualityComparer()));
        }

        [Test]
        public void Parse_ParsesLateralCategoryWithConstantDischarge()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralParser = new LateralParser(logHandler);

            var delftIniCategory = new DelftIniCategory("lateral");
            delftIniCategory.AddProperty("id", "some_id");
            delftIniCategory.AddProperty("name", "some_name");
            delftIniCategory.AddProperty("type", "discharge");
            delftIniCategory.AddProperty("locationType", "2d");
            delftIniCategory.AddProperty("numCoordinates", "3");
            delftIniCategory.AddProperty("xCoordinates", "1.23 2.34 3.45");
            delftIniCategory.AddProperty("yCoordinates", "4.56 5.67 6.78");
            delftIniCategory.AddProperty("discharge", "7.89");

            // Call
            LateralDTO lateralDTO = lateralParser.Parse(delftIniCategory);

            // Assert
            var expXCoordinates = new[] { 1.23, 2.34, 3.45 };
            var expYCoordinates = new[] { 4.56, 5.67, 6.78 };
            var expDischarge = new Steerable
            {
                Mode = SteerableMode.ConstantValue,
                ConstantValue = 7.89
            };
            var expLateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                               3, expXCoordinates, expYCoordinates, expDischarge);
            Assert.That(lateralDTO, Is.EqualTo(expLateralDTO).Using(new LateralDTOEqualityComparer()));
        }

        [Test]
        public void Parse_ParsesLateralCategoryWithInvalidCoordinates_ReportsError()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralParser = new LateralParser(logHandler);

            var delftIniCategory = new DelftIniCategory("lateral");
            delftIniCategory.AddProperty("id", "some_id");
            delftIniCategory.AddProperty("name", "some_name");
            delftIniCategory.AddProperty("type", "discharge");
            delftIniCategory.AddProperty("locationType", "2d");
            delftIniCategory.AddProperty("numCoordinates", "3");
            delftIniCategory.AddProperty("xCoordinates", "1.oeps23 2.34 3.45");
            delftIniCategory.AddProperty("yCoordinates", "4.56 5.67 6./78");
            delftIniCategory.AddProperty("discharge", "7.89");

            // Call
            LateralDTO lateralDTO = lateralParser.Parse(delftIniCategory);

            // Assert
            var expXCoordinates = new[] { 2.34, 3.45 };
            var expYCoordinates = new[] { 4.56, 5.67, };
            var expDischarge = new Steerable
            {
                Mode = SteerableMode.ConstantValue,
                ConstantValue = 7.89
            };
            var expLateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                               3, expXCoordinates, expYCoordinates, expDischarge);
            Assert.That(lateralDTO, Is.EqualTo(expLateralDTO).Using(new LateralDTOEqualityComparer()));

            logHandler.Received(1).ReportError($"'1.oeps23' could not be parsed to a double for property 'xCoordinates'. Line: 0");
            logHandler.Received(1).ReportError($"'6./78' could not be parsed to a double for property 'yCoordinates'. Line: 0");
        }

        [Test]
        [TestCase("timeseries.bc")]
        [TestCase("TIMSERIES.BC")]
        public void Parse_ParsesLateralCategoryWithTimeSeriesDischarge(string dischargeValue)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralParser = new LateralParser(logHandler);

            var delftIniCategory = new DelftIniCategory("lateral");
            delftIniCategory.AddProperty("id", "some_id");
            delftIniCategory.AddProperty("name", "some_name");
            delftIniCategory.AddProperty("type", "discharge");
            delftIniCategory.AddProperty("locationType", "2d");
            delftIniCategory.AddProperty("numCoordinates", "3");
            delftIniCategory.AddProperty("xCoordinates", "1.23 2.34 3.45");
            delftIniCategory.AddProperty("yCoordinates", "4.56 5.67 6.78");
            delftIniCategory.AddProperty("discharge", dischargeValue);

            // Call
            LateralDTO lateralDTO = lateralParser.Parse(delftIniCategory);

            // Assert
            var expXCoordinates = new[] { 1.23, 2.34, 3.45 };
            var expYCoordinates = new[] { 4.56, 5.67, 6.78 };
            var expDischarge = new Steerable
            {
                Mode = SteerableMode.TimeSeries,
                TimeSeriesFilename = dischargeValue
            };
            var expLateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                               3, expXCoordinates, expYCoordinates, expDischarge);
            Assert.That(lateralDTO, Is.EqualTo(expLateralDTO).Using(new LateralDTOEqualityComparer()));
        }

        [Test]
        [TestCase("realtime")]
        [TestCase("REALTIME")]
        public void Parse_ParsesLateralCategoryWithRealTimeDischarge(string dischargeValue)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralParser = new LateralParser(logHandler);

            var delftIniCategory = new DelftIniCategory("lateral");
            delftIniCategory.AddProperty("id", "some_id");
            delftIniCategory.AddProperty("name", "some_name");
            delftIniCategory.AddProperty("type", "discharge");
            delftIniCategory.AddProperty("locationType", "2d");
            delftIniCategory.AddProperty("numCoordinates", "3");
            delftIniCategory.AddProperty("xCoordinates", "1.23 2.34 3.45");
            delftIniCategory.AddProperty("yCoordinates", "4.56 5.67 6.78");
            delftIniCategory.AddProperty("discharge", dischargeValue);

            // Call
            LateralDTO lateralDTO = lateralParser.Parse(delftIniCategory);

            // Assert
            var expXCoordinates = new[] { 1.23, 2.34, 3.45 };
            var expYCoordinates = new[] { 4.56, 5.67, 6.78 };
            var expDischarge = new Steerable { Mode = SteerableMode.External };
            var expLateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                               3, expXCoordinates, expYCoordinates, expDischarge);
            Assert.That(lateralDTO, Is.EqualTo(expLateralDTO).Using(new LateralDTOEqualityComparer()));
        }

        [Test]
        public void Parse_ParsesLateralCategoryWithUnsupportedDischarge_ReportsError()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralParser = new LateralParser(logHandler);

            var delftIniCategory = new DelftIniCategory("lateral");
            delftIniCategory.AddProperty("id", "some_id");
            delftIniCategory.AddProperty("name", "some_name");
            delftIniCategory.AddProperty("type", "discharge");
            delftIniCategory.AddProperty("locationType", "2d");
            delftIniCategory.AddProperty("numCoordinates", "3");
            delftIniCategory.AddProperty("xCoordinates", "1.23 2.34 3.45");
            delftIniCategory.AddProperty("yCoordinates", "4.56 5.67 6.78");
            delftIniCategory.AddProperty("discharge", "unsupported");

            // Call
            LateralDTO lateralDTO = lateralParser.Parse(delftIniCategory);

            // Assert
            Assert.That(lateralDTO.Discharge, Is.Null);
            logHandler.Received(1).ReportError($"Discharge value 'unsupported' could not be parsed into either a scalar, time series file or \"realtime\". Line: 0");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Parse_ParsesLateralCategoryWithEmptyValues_SetsPropertiesToNull(string emptyValue)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralParser = new LateralParser(logHandler);

            var delftIniCategory = new DelftIniCategory("lateral");
            delftIniCategory.AddProperty("id", "");
            delftIniCategory.AddProperty("name", "");
            delftIniCategory.AddProperty("type", "");
            delftIniCategory.AddProperty("locationType", "");
            delftIniCategory.AddProperty("numCoordinates", "");
            delftIniCategory.AddProperty("xCoordinates", "");
            delftIniCategory.AddProperty("yCoordinates", "");
            delftIniCategory.AddProperty("discharge", "");

            // Call
            LateralDTO lateralDTO = lateralParser.Parse(delftIniCategory);

            // Assert
            var expLateralDTO = new LateralDTO(null, null, LateralForcingType.None, LateralLocationType.None,
                                               null, null, null, null);
            Assert.That(lateralDTO, Is.EqualTo(expLateralDTO).Using(new LateralDTOEqualityComparer()));
        }

        [Test]
        public void Parse_ParsesLateralCategoryWithoutValues()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralParser = new LateralParser(logHandler);

            var delftIniCategory = new DelftIniCategory("lateral");

            // Call
            LateralDTO lateralDTO = lateralParser.Parse(delftIniCategory);

            // Assert
            var expLateralDTO = new LateralDTO(null, null, LateralForcingType.None, LateralLocationType.None,
                                               null, null, null, null);
            Assert.That(lateralDTO, Is.EqualTo(expLateralDTO).Using(new LateralDTOEqualityComparer()));
        }
    }
}