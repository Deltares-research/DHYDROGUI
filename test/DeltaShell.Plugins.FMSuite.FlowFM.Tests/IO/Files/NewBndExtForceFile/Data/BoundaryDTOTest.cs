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
        }

        [Test]
        public void AddForcingFile_ArgNull_ThrowsArgumentNullException()
        {
            // Setup
            var forcingFiles = new[] { "some_forcing_file1", "some_forcing_file2" };

            var boundaryDTO = new BoundaryDTO("some_quantity", "some_location_file", forcingFiles, 123);

            // Call
            void Call() => boundaryDTO.AddForcingFile(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void AddForcingFile_WithNewForcingFile_AddsNewForcingFile()
        {
            // Setup
            var forcingFiles = new[] { "some_forcing_file1", "some_forcing_file2" };

            var boundaryDTO = new BoundaryDTO("some_quantity", "some_location_file", forcingFiles, 123);

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
            var forcingFiles = new[] { "some_forcing_file1", "some_forcing_file2" };

            var boundaryDTO = new BoundaryDTO("some_quantity", "some_location_file", forcingFiles, 123);

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
            var forcingFiles = new[] { "some_forcing_file1", "some_forcing_file2" };

            var boundaryDTO = new BoundaryDTO("some_quantity", "some_location_file", forcingFiles, 123);

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
            var forcingFiles = new[] { "some_forcing_file1", "some_forcing_file2" };

            var boundaryDTO = new BoundaryDTO("some_quantity", "some_location_file", forcingFiles, 123);

            // Call
            boundaryDTO.RemoveForcingFile("some_forcing_file3");

            // Assert
            Assert.That(boundaryDTO.ForcingFiles, Is.EquivalentTo(forcingFiles));
        }

        [Test]
        public void RemoveForcingFile_WithExistingForcingFile_RemovesExistingForcingFile()
        {
            // Setup
            var forcingFiles = new[] { "some_forcing_file1", "some_forcing_file2" };

            var boundaryDTO = new BoundaryDTO("some_quantity", "some_location_file", forcingFiles, 123);

            // Call
            boundaryDTO.RemoveForcingFile("some_forcing_file2");

            // Assert
            var expForcingFiles = new[] { "some_forcing_file1" };
            Assert.That(boundaryDTO.ForcingFiles, Is.EquivalentTo(expForcingFiles));
        }
    }
}