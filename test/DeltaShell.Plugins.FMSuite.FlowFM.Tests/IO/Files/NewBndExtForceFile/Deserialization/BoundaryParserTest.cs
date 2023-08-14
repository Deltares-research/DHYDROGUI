using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Deserialization;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Data;
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
            var boundaryCategoryParser = new BoundaryParser();

            // Call
            void Call() => boundaryCategoryParser.Parse(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Parse_ParsesBoundaryCategoryWithValues()
        {
            // Setup
            var boundaryCategoryParser = new BoundaryParser();

            var delftIniCategory = new DelftIniCategory("boundary");
            delftIniCategory.AddProperty("quantity", "some_quantity");
            delftIniCategory.AddProperty("locationFile", "some_location_file");
            delftIniCategory.AddProperty("forcingFile", "some_forcing_file1");
            delftIniCategory.AddProperty("forcingFile", "some_forcing_file2");
            delftIniCategory.AddProperty("returnTime", "1.23");

            // Call
            BoundaryDTO boundaryDTO = boundaryCategoryParser.Parse(delftIniCategory);

            // Assert
            var expForcingFiles = new[] { "some_forcing_file1", "some_forcing_file2" };
            var expBoundaryDTO = new BoundaryDTO("some_quantity", "some_location_file", expForcingFiles, 1.23);
            Assert.That(boundaryDTO, Is.EqualTo(expBoundaryDTO).Using(new BoundaryDTOEqualityComparer()));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Parse_ParsesBoundaryCategoryWithEmptyValues(string emptyValue)
        {
            // Setup
            var boundaryCategoryParser = new BoundaryParser();

            var delftIniCategory = new DelftIniCategory("boundary");
            delftIniCategory.AddProperty("quantity", emptyValue);
            delftIniCategory.AddProperty("locationFile", emptyValue);
            delftIniCategory.AddProperty("forcingFile", emptyValue);
            delftIniCategory.AddProperty("returnTime", emptyValue);

            // Call
            BoundaryDTO boundaryDTO = boundaryCategoryParser.Parse(delftIniCategory);

            // Assert
            IEnumerable<string> expForcingFiles = Enumerable.Empty<string>();
            var expBoundaryDTO = new BoundaryDTO(null, null, expForcingFiles, null);
            Assert.That(boundaryDTO, Is.EqualTo(expBoundaryDTO).Using(new BoundaryDTOEqualityComparer()));
        }

        [Test]
        public void Parse_ParsesBoundaryCategoryWithoutValues()
        {
            // Setup
            var boundaryCategoryParser = new BoundaryParser();

            var delftIniCategory = new DelftIniCategory("boundary");

            // Call
            BoundaryDTO boundaryDTO = boundaryCategoryParser.Parse(delftIniCategory);

            // Assert
            IEnumerable<string> expForcingFiles = Enumerable.Empty<string>();
            var expBoundaryDTO = new BoundaryDTO(null, null, expForcingFiles, null);
            Assert.That(boundaryDTO, Is.EqualTo(expBoundaryDTO).Using(new BoundaryDTOEqualityComparer()));
        }
    }
}