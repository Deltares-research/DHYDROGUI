using System;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Data
{
    [TestFixture]
    public class BoundaryDTOTest
    {
        [Test]
        public void Constructor_SetsProperties()
        {
            // Setup
            var forcingFiles = new[] { "some_forcing_file1", "some_forcing_file1", "some_forcing_file2" };

            // Call
            var boundaryDTO = new BoundaryDTO("some_quantity", "some_location_file", forcingFiles, 123);

            // Assert
            var expForcingFiles = new[] { "some_forcing_file1", "some_forcing_file2" };
            Assert.That(boundaryDTO.Quantity, Is.EqualTo("some_quantity"));
            Assert.That(boundaryDTO.LocationFile, Is.EqualTo("some_location_file"));
            Assert.That(boundaryDTO.ForcingFiles, Is.EquivalentTo(expForcingFiles));
            Assert.That(boundaryDTO.ReturnTime, Is.EqualTo(123));
            Assert.That(boundaryDTO.LineNumber, Is.Zero);
        }

        [Test]
        public void SetLineNumber_ValueIsNegative_ThrowsArgumentOutOfRangeException()
        {
            // Setup
            BoundaryDTO boundaryDTO = CreateBoundaryDTO();

            // Assert
            Assert.That(() => boundaryDTO.LineNumber = -1, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public void SetLineNumber_CanSetZeroOrPositiveValue(int lineNumber)
        {
            // Setup
            BoundaryDTO boundaryDTO = CreateBoundaryDTO();

            // Call
            boundaryDTO.LineNumber = lineNumber;

            // Assert
            Assert.That(boundaryDTO.LineNumber, Is.EqualTo(lineNumber));
        }

        [Test]
        public void AddForcingFile_ArgNull_ThrowsArgumentNullException()
        {
            // Setup
            BoundaryDTO boundaryDTO = CreateBoundaryDTO();

            // Call
            void Call() => boundaryDTO.AddForcingFile(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void AddForcingFile_WithNewForcingFile_AddsNewForcingFile()
        {
            // Setup
            BoundaryDTO boundaryDTO = CreateBoundaryDTO();

            // Call
            boundaryDTO.AddForcingFile("some_forcing_file3");

            // Assert
            var expForcingFiles = new[] { "some_forcing_file1", "some_forcing_file2", "some_forcing_file3" };
            Assert.That(boundaryDTO.ForcingFiles, Is.EquivalentTo(expForcingFiles));
        }

        [Test]
        public void AddForcingFile_WithExistingForcingFile_DoesNotAddNewForcingFile()
        {
            // Setup
            BoundaryDTO boundaryDTO = CreateBoundaryDTO();

            // Call
            boundaryDTO.AddForcingFile("some_forcing_file2");

            // Assert
            var expForcingFiles = new[] { "some_forcing_file1", "some_forcing_file2" };
            Assert.That(boundaryDTO.ForcingFiles, Is.EquivalentTo(expForcingFiles));
        }

        [Test]
        public void RemoveForcingFile_ArgNull_ThrowsArgumentNullException()
        {
            // Setup
            BoundaryDTO boundaryDTO = CreateBoundaryDTO();

            // Call
            void Call()
            {
                boundaryDTO.AddForcingFile(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void RemoveForcingFile_WithNewForcingFile_DoesNotRemoveAnything()
        {
            // Setup
            BoundaryDTO boundaryDTO = CreateBoundaryDTO();
            string[] expForcingFiles = boundaryDTO.ForcingFiles.ToArray();

            // Call
            boundaryDTO.RemoveForcingFile("some_forcing_file3");

            // Assert
            Assert.That(boundaryDTO.ForcingFiles, Is.EquivalentTo(expForcingFiles));
        }

        [Test]
        public void RemoveForcingFile_WithExistingForcingFile_RemovesExistingForcingFile()
        {
            // Setup
            BoundaryDTO boundaryDTO = CreateBoundaryDTO();

            // Call
            boundaryDTO.RemoveForcingFile("some_forcing_file2");

            // Assert
            var expForcingFiles = new[] { "some_forcing_file1" };
            Assert.That(boundaryDTO.ForcingFiles, Is.EquivalentTo(expForcingFiles));
        }

        private static BoundaryDTO CreateBoundaryDTO()
        {
            var forcingFiles = new[] { "some_forcing_file1", "some_forcing_file2" };
            return new BoundaryDTO("some_quantity", "some_location_file", forcingFiles, 123);
        }
    }
}