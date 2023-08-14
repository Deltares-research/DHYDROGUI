using System.Collections.Generic;
using System.Linq;
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
        public void Parse_ParsedBoundaryCategoryWithValuesIsAdded()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var bndExtForceFileParser = new BndExtForceFileParser(logHandler);

            var delftIniCategory = new DelftIniCategory("boundary");
            delftIniCategory.AddProperty("quantity", "some_quantity");
            delftIniCategory.AddProperty("locationFile", "some_location_file");
            delftIniCategory.AddProperty("forcingFile", "some_forcing_file1");
            delftIniCategory.AddProperty("forcingFile", "some_forcing_file2");
            delftIniCategory.AddProperty("returnTime", "1.23");
            var delftIniCategories = new List<DelftIniCategory> { delftIniCategory };

            // Call
            BndExtForceFileDTO bndExtForceFileDTO = bndExtForceFileParser.Parse(delftIniCategories);

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
        public void Parse_ParsedBoundaryCategoryWithoutValuesIsAdded()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var bndExtForceFileParser = new BndExtForceFileParser(logHandler);

            var delftIniCategory = new DelftIniCategory("boundary");
            var delftIniCategories = new List<DelftIniCategory> { delftIniCategory };

            // Call
            BndExtForceFileDTO bndExtForceFileDTO = bndExtForceFileParser.Parse(delftIniCategories);

            // Assert
            Assert.That(bndExtForceFileDTO.LocationFiles, Is.Empty);
            Assert.That(bndExtForceFileDTO.ForcingFiles, Is.Empty);
            var expBoundaryDTO = new BoundaryDTO(null, null, Enumerable.Empty<string>(), null);
            Assert.That(bndExtForceFileDTO.Boundaries.Single(), Is.EqualTo(expBoundaryDTO).Using(new BoundaryDTOEqualityComparer()));
            Assert.That(bndExtForceFileDTO.Laterals, Is.Empty);
        }

        [Test]
        public void Parse_ValidParsedLateralCategoryIsAdded()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var bndExtForceFileParser = new BndExtForceFileParser(logHandler);

            var delftIniCategory = new DelftIniCategory("lateral");
            delftIniCategory.AddProperty("id", "some_id");
            delftIniCategory.AddProperty("name", "some_name");
            delftIniCategory.AddProperty("type", "discharge");
            delftIniCategory.AddProperty("locationType", "2d");
            delftIniCategory.AddProperty("numCoordinates", "3");
            delftIniCategory.AddProperty("xCoordinates", "1.23 2.34 3.45");
            delftIniCategory.AddProperty("yCoordinates", "4.56 5.67 6.78");
            delftIniCategory.AddProperty("discharge", "some_forcing_file.bc");
            var delftIniCategories = new List<DelftIniCategory> { delftIniCategory };

            // Call
            BndExtForceFileDTO bndExtForceFileDTO = bndExtForceFileParser.Parse(delftIniCategories);

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
        public void Parse_InvalidParsedLateralCategoryIsNotAdded()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var bndExtForceFileParser = new BndExtForceFileParser(logHandler);

            var delftIniCategory = new DelftIniCategory("lateral");
            var delftIniCategories = new List<DelftIniCategory> { delftIniCategory };

            // Call
            BndExtForceFileDTO bndExtForceFileDTO = bndExtForceFileParser.Parse(delftIniCategories);

            // Assert
            Assert.That(bndExtForceFileDTO.LocationFiles, Is.Empty);
            Assert.That(bndExtForceFileDTO.ForcingFiles, Is.Empty);
            Assert.That(bndExtForceFileDTO.Boundaries, Is.Empty);
            Assert.That(bndExtForceFileDTO.Laterals, Is.Empty);
        }

        [Test]
        public void Parse_DoesNotParseUnknownCategoriesAndLogsAWarning()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var bndExtForceFileParser = new BndExtForceFileParser(logHandler);

            var delftIniCategory = new DelftIniCategory("unknown", 5);
            var delftIniCategories = new List<DelftIniCategory> { delftIniCategory };

            // Call
            BndExtForceFileDTO bndExtForceFileDTO = bndExtForceFileParser.Parse(delftIniCategories);

            // Assert
            Assert.That(bndExtForceFileDTO.LocationFiles, Is.Empty);
            Assert.That(bndExtForceFileDTO.ForcingFiles, Is.Empty);
            Assert.That(bndExtForceFileDTO.Boundaries, Is.Empty);
            Assert.That(bndExtForceFileDTO.Laterals, Is.Empty);
            logHandler.Received(1).ReportWarningFormat("Category {0} has an unknown header and cannot be parsed. Line: {1}", "unknown", 5);
        }
        
        [Test]
        public void Parse_GeneralCategories_NothingHappens()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var bndExtForceFileParser = new BndExtForceFileParser(logHandler);

            var delftIniCategory = new DelftIniCategory("general", 5);
            var delftIniCategories = new List<DelftIniCategory> { delftIniCategory };

            // Call
            BndExtForceFileDTO bndExtForceFileDTO = bndExtForceFileParser.Parse(delftIniCategories);

            // Assert
            Assert.That(bndExtForceFileDTO.LocationFiles, Is.Empty);
            Assert.That(bndExtForceFileDTO.ForcingFiles, Is.Empty);
            Assert.That(bndExtForceFileDTO.Boundaries, Is.Empty);
            Assert.That(bndExtForceFileDTO.Laterals, Is.Empty);
            Assert.That(logHandler.ReceivedCalls(), Is.Empty);
        }
    }
}