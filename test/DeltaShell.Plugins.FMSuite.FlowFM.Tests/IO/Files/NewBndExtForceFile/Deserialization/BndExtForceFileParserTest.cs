using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Deserialization;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Data;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Deserialization
{
    [TestFixture]
    public class BndExtForceFileParserTest
    {
        [Test]
        public void Constructor_ArgNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new BndExtForceFileParser(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Parse_ArgNull_ThrowsArgumentNullException()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var bndExtForceFileParser = new BndExtForceFileParser(logHandler);

            // Call
            void Call() => bndExtForceFileParser.Parse(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Parse_ParsedBoundarySectionWithValuesIsAdded()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var bndExtForceFileParser = new BndExtForceFileParser(logHandler);

            var section = new IniSection("boundary");
            section.AddProperty("quantity", "some_quantity");
            section.AddProperty("locationFile", "some_location_file");
            section.AddProperty("forcingFile", "some_forcing_file1");
            section.AddProperty("forcingFile", "some_forcing_file2");
            section.AddProperty("returnTime", "1.23");
            var iniData = new IniData();
            iniData.AddSection(section);

            // Call
            BndExtForceFileDTO bndExtForceFileDTO = bndExtForceFileParser.Parse(iniData);

            // Assert
            var expLocationFiles = new[] { "some_location_file" };
            var expForcingFiles = new[] { "some_forcing_file1", "some_forcing_file2" };
            var expBoundaryDTO = new BoundaryDTO("some_quantity", "some_location_file", expForcingFiles, 1.23);
            Assert.That(bndExtForceFileDTO.LocationFiles, Is.EquivalentTo(expLocationFiles));
            Assert.That(bndExtForceFileDTO.ForcingFiles, Is.EquivalentTo(expForcingFiles));
            Assert.That(bndExtForceFileDTO.Boundaries.Single(), Is.EqualTo(expBoundaryDTO).Using(new BoundaryDTOEqualityComparer()));
            Assert.That(bndExtForceFileDTO.Laterals, Is.Empty);
        }

        [Test]
        public void Parse_ParsedBoundarySectionWithoutValuesIsAdded()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var bndExtForceFileParser = new BndExtForceFileParser(logHandler);

            var section = new IniSection("boundary");
            var iniData = new IniData();
            iniData.AddSection(section);
            
            // Call
            BndExtForceFileDTO bndExtForceFileDTO = bndExtForceFileParser.Parse(iniData);

            // Assert
            Assert.That(bndExtForceFileDTO.LocationFiles, Is.Empty);
            Assert.That(bndExtForceFileDTO.ForcingFiles, Is.Empty);
            var expBoundaryDTO = new BoundaryDTO(null, null, Enumerable.Empty<string>(), null);
            Assert.That(bndExtForceFileDTO.Boundaries.Single(), Is.EqualTo(expBoundaryDTO).Using(new BoundaryDTOEqualityComparer()));
            Assert.That(bndExtForceFileDTO.Laterals, Is.Empty);
        }

        [Test]
        public void Parse_ValidParsedLateralSectionIsAdded()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var bndExtForceFileParser = new BndExtForceFileParser(logHandler);

            var section = new IniSection("lateral");
            section.AddProperty("id", "some_id");
            section.AddProperty("name", "some_name");
            section.AddProperty("type", "discharge");
            section.AddProperty("locationType", "2d");
            section.AddProperty("numCoordinates", "3");
            section.AddProperty("xCoordinates", "1.23 2.34 3.45");
            section.AddProperty("yCoordinates", "4.56 5.67 6.78");
            section.AddProperty("discharge", "some_forcing_file.bc");
            var iniData = new IniData();
            iniData.AddSection(section);

            // Call
            BndExtForceFileDTO bndExtForceFileDTO = bndExtForceFileParser.Parse(iniData);

            // Assert
            var expForcingFiles = new[] { "some_forcing_file.bc" };
            var expXCoordinates = new[] { 1.23, 2.34, 3.45 };
            var expYCoordinates = new[] { 4.56, 5.67, 6.78 };
            var expDischarge = new Steerable
            {
                Mode = SteerableMode.TimeSeries,
                TimeSeriesFilename = "some_forcing_file.bc"
            };
            var expLateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                               3, expXCoordinates, expYCoordinates, expDischarge);
            Assert.That(bndExtForceFileDTO.LocationFiles, Is.Empty);
            Assert.That(bndExtForceFileDTO.ForcingFiles, Is.EquivalentTo(expForcingFiles));
            Assert.That(bndExtForceFileDTO.Boundaries, Is.Empty);
            Assert.That(bndExtForceFileDTO.Laterals.Single(), Is.EqualTo(expLateralDTO).Using(new LateralDTOEqualityComparer()));
        }

        [Test]
        public void Parse_InvalidParsedLateralSectionIsNotAdded()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var bndExtForceFileParser = new BndExtForceFileParser(logHandler);

            var section = new IniSection("lateral");
            var iniData = new IniData();
            iniData.AddSection(section);

            // Call
            BndExtForceFileDTO bndExtForceFileDTO = bndExtForceFileParser.Parse(iniData);

            // Assert
            Assert.That(bndExtForceFileDTO.LocationFiles, Is.Empty);
            Assert.That(bndExtForceFileDTO.ForcingFiles, Is.Empty);
            Assert.That(bndExtForceFileDTO.Boundaries, Is.Empty);
            Assert.That(bndExtForceFileDTO.Laterals, Is.Empty);
        }

        [Test]
        public void Parse_DoesNotParseUnknownSectionsAndLogsAWarning()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var bndExtForceFileParser = new BndExtForceFileParser(logHandler);

            var section = new IniSection("unknown") {LineNumber = 5};
            var iniData = new IniData();
            iniData.AddSection(section);

            // Call
            BndExtForceFileDTO bndExtForceFileDTO = bndExtForceFileParser.Parse(iniData);

            // Assert
            Assert.That(bndExtForceFileDTO.LocationFiles, Is.Empty);
            Assert.That(bndExtForceFileDTO.ForcingFiles, Is.Empty);
            Assert.That(bndExtForceFileDTO.Boundaries, Is.Empty);
            Assert.That(bndExtForceFileDTO.Laterals, Is.Empty);
            logHandler.Received(1).ReportWarningFormat("Section '{0}' has an unknown header and cannot be parsed. Line: {1}", "unknown", 5);
        }
        
        [Test]
        public void Parse_GeneralSections_NothingHappens()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var bndExtForceFileParser = new BndExtForceFileParser(logHandler);

            var section = new IniSection("general") {LineNumber = 5};
            var iniData = new IniData();
            iniData.AddSection(section);

            // Call
            BndExtForceFileDTO bndExtForceFileDTO = bndExtForceFileParser.Parse(iniData);

            // Assert
            Assert.That(bndExtForceFileDTO.LocationFiles, Is.Empty);
            Assert.That(bndExtForceFileDTO.ForcingFiles, Is.Empty);
            Assert.That(bndExtForceFileDTO.Boundaries, Is.Empty);
            Assert.That(bndExtForceFileDTO.Laterals, Is.Empty);
            Assert.That(logHandler.ReceivedCalls(), Is.Empty);
        }
    }
}