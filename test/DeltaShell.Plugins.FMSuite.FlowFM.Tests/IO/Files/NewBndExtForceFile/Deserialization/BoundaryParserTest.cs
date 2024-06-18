using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Deserialization;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Data;
using DHYDRO.Common.IO.Ini;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Deserialization
{
    [TestFixture]
    public class BoundaryParserTest
    {
        [Test]
        public void Parse_ArgNull_ThrowsArgumentNullException()
        {
            // Setup
            var boundaryParser = new BoundaryParser();

            // Call
            void Call() => boundaryParser.Parse(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Parse_ParsesBoundarySectionWithValues()
        {
            // Setup
            var boundaryParser = new BoundaryParser();

            var section = new IniSection("boundary");
            section.AddProperty("quantity", "some_quantity");
            section.AddProperty("locationFile", "some_location_file");
            section.AddProperty("forcingFile", "some_forcing_file1");
            section.AddProperty("forcingFile", "some_forcing_file2");
            section.AddProperty("returnTime", "1.23");

            // Call
            BoundaryDTO boundaryDTO = boundaryParser.Parse(section);

            // Assert
            var expForcingFiles = new[] { "some_forcing_file1", "some_forcing_file2" };
            var expBoundaryDTO = new BoundaryDTO("some_quantity", "some_location_file", expForcingFiles, 1.23);
            Assert.That(boundaryDTO, Is.EqualTo(expBoundaryDTO).Using(new BoundaryDTOEqualityComparer()));
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Parse_ParsesBoundarySectionWithEmptyValues(string emptyValue)
        {
            // Setup
            var boundaryParser = new BoundaryParser();

            var section = new IniSection("boundary");
            section.AddProperty("quantity", emptyValue);
            section.AddProperty("locationFile", emptyValue);
            section.AddProperty("forcingFile", emptyValue);
            section.AddProperty("returnTime", emptyValue);

            // Call
            BoundaryDTO boundaryDTO = boundaryParser.Parse(section);

            // Assert
            IEnumerable<string> expForcingFiles = Enumerable.Empty<string>();
            var expBoundaryDTO = new BoundaryDTO(null, null, expForcingFiles, null);
            Assert.That(boundaryDTO, Is.EqualTo(expBoundaryDTO).Using(new BoundaryDTOEqualityComparer()));
        }

        [Test]
        public void Parse_ParsesBoundarySectionWithoutValues()
        {
            // Setup
            var boundaryParser = new BoundaryParser();

            var section = new IniSection("boundary");

            // Call
            BoundaryDTO boundaryDTO = boundaryParser.Parse(section);

            // Assert
            IEnumerable<string> expForcingFiles = Enumerable.Empty<string>();
            var expBoundaryDTO = new BoundaryDTO(null, null, expForcingFiles, null);
            Assert.That(boundaryDTO, Is.EqualTo(expBoundaryDTO).Using(new BoundaryDTOEqualityComparer()));
        }
    }
}