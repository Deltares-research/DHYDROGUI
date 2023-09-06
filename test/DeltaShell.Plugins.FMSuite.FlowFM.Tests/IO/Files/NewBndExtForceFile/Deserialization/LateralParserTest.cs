using DeltaShell.NGHS.IO.Ini;
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
        public void Parse_ParsesLateralSectionWithValues(string locationTypeValue, LateralLocationType expLateralLocationType)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralParser = new LateralParser(logHandler);

            var section = new IniSection("lateral");
            section.AddProperty("id", "some_id");
            section.AddProperty("name", "some_name");
            section.AddProperty("type", "discharge");
            section.AddProperty("locationType", locationTypeValue);
            section.AddProperty("numCoordinates", "3");
            section.AddProperty("xCoordinates", "1.23 2.34 3.45");
            section.AddProperty("yCoordinates", "4.56 5.67 6.78");
            section.AddProperty("discharge", "7.89");

            // Call
            LateralDTO lateralDTO = lateralParser.Parse(section);

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
        public void Parse_ParsesLateralSectionWithUnknownValues()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralParser = new LateralParser(logHandler);

            var section = new IniSection("lateral");
            section.AddProperty("id", "some_id");
            section.AddProperty("name", "some_name");
            section.AddProperty("type", "unsupported_type");
            section.AddProperty("locationType", "unsupported_location_type");
            section.AddProperty("numCoordinates", "3");
            section.AddProperty("xCoordinates", "1.23 2.34 3.45");
            section.AddProperty("yCoordinates", "4.56 5.67 6.78");
            section.AddProperty("discharge", "7.89");

            // Call
            LateralDTO lateralDTO = lateralParser.Parse(section);

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
        public void Parse_ParsesLateralSectionWithConstantDischarge()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralParser = new LateralParser(logHandler);

            var section = new IniSection("lateral");
            section.AddProperty("id", "some_id");
            section.AddProperty("name", "some_name");
            section.AddProperty("type", "discharge");
            section.AddProperty("locationType", "2d");
            section.AddProperty("numCoordinates", "3");
            section.AddProperty("xCoordinates", "1.23 2.34 3.45");
            section.AddProperty("yCoordinates", "4.56 5.67 6.78");
            section.AddProperty("discharge", "7.89");

            // Call
            LateralDTO lateralDTO = lateralParser.Parse(section);

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
        public void Parse_ParsesLateralSectionWithInvalidCoordinates_ReportsError()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralParser = new LateralParser(logHandler);

            var section = new IniSection("lateral");
            section.AddProperty("id", "some_id");
            section.AddProperty("name", "some_name");
            section.AddProperty("type", "discharge");
            section.AddProperty("locationType", "2d");
            section.AddProperty("numCoordinates", "3");
            section.AddProperty("xCoordinates", "1.oeps23 2.34 3.45");
            section.AddProperty("yCoordinates", "4.56 5.67 6./78");
            section.AddProperty("discharge", "7.89");

            // Call
            LateralDTO lateralDTO = lateralParser.Parse(section);

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
        public void Parse_ParsesLateralSectionWithTimeSeriesDischarge(string dischargeValue)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralParser = new LateralParser(logHandler);

            var section = new IniSection("lateral");
            section.AddProperty("id", "some_id");
            section.AddProperty("name", "some_name");
            section.AddProperty("type", "discharge");
            section.AddProperty("locationType", "2d");
            section.AddProperty("numCoordinates", "3");
            section.AddProperty("xCoordinates", "1.23 2.34 3.45");
            section.AddProperty("yCoordinates", "4.56 5.67 6.78");
            section.AddProperty("discharge", dischargeValue);

            // Call
            LateralDTO lateralDTO = lateralParser.Parse(section);

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
        public void Parse_ParsesLateralSectionWithRealTimeDischarge(string dischargeValue)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralParser = new LateralParser(logHandler);

            var section = new IniSection("lateral");
            section.AddProperty("id", "some_id");
            section.AddProperty("name", "some_name");
            section.AddProperty("type", "discharge");
            section.AddProperty("locationType", "2d");
            section.AddProperty("numCoordinates", "3");
            section.AddProperty("xCoordinates", "1.23 2.34 3.45");
            section.AddProperty("yCoordinates", "4.56 5.67 6.78");
            section.AddProperty("discharge", dischargeValue);

            // Call
            LateralDTO lateralDTO = lateralParser.Parse(section);

            // Assert
            var expXCoordinates = new[] { 1.23, 2.34, 3.45 };
            var expYCoordinates = new[] { 4.56, 5.67, 6.78 };
            var expDischarge = new Steerable { Mode = SteerableMode.External };
            var expLateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                               3, expXCoordinates, expYCoordinates, expDischarge);
            Assert.That(lateralDTO, Is.EqualTo(expLateralDTO).Using(new LateralDTOEqualityComparer()));
        }

        [Test]
        public void Parse_ParsesLateralSectionWithUnsupportedDischarge_ReportsError()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralParser = new LateralParser(logHandler);

            var section = new IniSection("lateral");
            section.AddProperty("id", "some_id");
            section.AddProperty("name", "some_name");
            section.AddProperty("type", "discharge");
            section.AddProperty("locationType", "2d");
            section.AddProperty("numCoordinates", "3");
            section.AddProperty("xCoordinates", "1.23 2.34 3.45");
            section.AddProperty("yCoordinates", "4.56 5.67 6.78");
            section.AddProperty("discharge", "unsupported");

            // Call
            LateralDTO lateralDTO = lateralParser.Parse(section);

            // Assert
            Assert.That(lateralDTO.Discharge, Is.Null);
            logHandler.Received(1).ReportError($"Discharge value 'unsupported' could not be parsed into either a scalar, time series file or \"realtime\". Line: 0");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Parse_ParsesLateralSectionWithEmptyValues_SetsPropertiesToNull(string emptyValue)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralParser = new LateralParser(logHandler);

            var section = new IniSection("lateral");
            section.AddProperty("id", "");
            section.AddProperty("name", "");
            section.AddProperty("type", "");
            section.AddProperty("locationType", "");
            section.AddProperty("numCoordinates", "");
            section.AddProperty("xCoordinates", "");
            section.AddProperty("yCoordinates", "");
            section.AddProperty("discharge", "");

            // Call
            LateralDTO lateralDTO = lateralParser.Parse(section);

            // Assert
            var expLateralDTO = new LateralDTO(null, null, LateralForcingType.None, LateralLocationType.None,
                                               null, null, null, null);
            Assert.That(lateralDTO, Is.EqualTo(expLateralDTO).Using(new LateralDTOEqualityComparer()));
        }

        [Test]
        public void Parse_ParsesLateralSectionWithoutValues()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var lateralParser = new LateralParser(logHandler);

            var section = new IniSection("lateral");

            // Call
            LateralDTO lateralDTO = lateralParser.Parse(section);

            // Assert
            var expLateralDTO = new LateralDTO(null, null, LateralForcingType.None, LateralLocationType.None,
                                               null, null, null, null);
            Assert.That(lateralDTO, Is.EqualTo(expLateralDTO).Using(new LateralDTOEqualityComparer()));
        }
    }
}