using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Data
{
    [TestFixture]
    public class BndExtForceFileDTOTest
    {
        [Test]
        public void AddBoundary_BoundaryDTONull_ThrowsArgumentNullException()
        {
            // Setup
            var bndExtForceFileDTO = new BndExtForceFileDTO();

            // Call
            void Call() => bndExtForceFileDTO.AddBoundary(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void AddBoundary_AddsTheBoundaryAndLocationFilesAndForcingFiles()
        {
            // Setup
            var bndExtForceFileDTO = new BndExtForceFileDTO();
            var forcingFiles = new[] { "some_forcing_file1", "some_forcing_file2" };
            const string locationFile = "some_location_file1";
            var boundaryDTO = new BoundaryDTO("some_quantity", locationFile, forcingFiles, 123);

            // Call
            bndExtForceFileDTO.AddBoundary(boundaryDTO);

            // Assert
            CollectionContainsOnlyAssert.AssertContainsOnly(bndExtForceFileDTO.Boundaries, boundaryDTO);
            Assert.That(bndExtForceFileDTO.Laterals, Is.Empty);
            CollectionContainsOnlyAssert.AssertContainsOnly(bndExtForceFileDTO.LocationFiles, locationFile);
            Assert.That(bndExtForceFileDTO.ForcingFiles, Is.EquivalentTo(forcingFiles));
        }

        [Test]
        public void AddBoundary_ForMultipleBoundaries_AddsTheBoundariesAndTheirLocationFilesAndForcingFiles()
        {
            // Setup
            var bndExtForceFileDTO = new BndExtForceFileDTO();
            var forcingFiles1 = new[] { "some_forcing_file1", "some_forcing_file2" };
            var forcingFiles2 = new[] { "some_forcing_file2", "some_forcing_file3" };
            var forcingFiles3 = new[] { "some_forcing_file3", "some_forcing_file4" };
            const string locationFile1 = "some_location_file1";
            const string locationFile2 = "some_location_file1";
            const string locationFile3 = "some_location_file2";

            var boundaryDTO1 = new BoundaryDTO("some_quantity1", locationFile1, forcingFiles1, 123);
            var boundaryDTO2 = new BoundaryDTO("some_quantity2", locationFile2, forcingFiles2, 234);
            var boundaryDTO3 = new BoundaryDTO("some_quantity3", locationFile3, forcingFiles3, 456);

            // Calls
            bndExtForceFileDTO.AddBoundary(boundaryDTO1);
            bndExtForceFileDTO.AddBoundary(boundaryDTO2);
            bndExtForceFileDTO.AddBoundary(boundaryDTO3);

            // Assert
            BoundaryDTO[] expBoundaryDTOs = { boundaryDTO1, boundaryDTO2, boundaryDTO3 };
            var expLocationFiles = new[] { "some_location_file1", "some_location_file2" };
            var expForcingFiles = new[] { "some_forcing_file1", "some_forcing_file2", "some_forcing_file3", "some_forcing_file4" };
            Assert.That(bndExtForceFileDTO.Boundaries, Is.EquivalentTo(expBoundaryDTOs));
            Assert.That(bndExtForceFileDTO.Laterals, Is.Empty);
            Assert.That(bndExtForceFileDTO.LocationFiles, Is.EquivalentTo(expLocationFiles));
            Assert.That(bndExtForceFileDTO.ForcingFiles, Is.EquivalentTo(expForcingFiles));
        }

        [Test]
        public void AddLateral_LateralDTONull_ThrowsArgumentNullException()
        {
            // Setup
            var bndExtForceFileDTO = new BndExtForceFileDTO();

            // Call
            void Call() => bndExtForceFileDTO.AddLateral(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void AddLateral_AddsTheForcingFiles()
        {
            // Setup
            var bndExtForceFileDTO = new BndExtForceFileDTO();
            const string forcingFile = "some_forcing_file";
            var discharge = new Steerable
            {
                Mode = SteerableMode.TimeSeries,
                TimeSeriesFilename = forcingFile
            };

            var lateralDTO = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                            null, null, null, discharge);

            // Call
            bndExtForceFileDTO.AddLateral(lateralDTO);

            // Assert
            Assert.That(bndExtForceFileDTO.Boundaries, Is.Empty);
            CollectionContainsOnlyAssert.AssertContainsOnly(bndExtForceFileDTO.Laterals, lateralDTO);
            Assert.That(bndExtForceFileDTO.LocationFiles, Is.Empty);
            CollectionContainsOnlyAssert.AssertContainsOnly(bndExtForceFileDTO.ForcingFiles, forcingFile);
        }

        [Test]
        public void AddLateral_ForMultipleLaterals_AddsTheLateralsAndTheirForcingFiles()
        {
            // Setup
            var bndExtForceFileDTO = new BndExtForceFileDTO();
            const string forcingFile1 = "some_forcing_file1";
            const string forcingFile2 = "some_forcing_file2";
            const string forcingFile3 = "some_forcing_file2";
            var discharge1 = new Steerable
            {
                Mode = SteerableMode.TimeSeries,
                TimeSeriesFilename = forcingFile1
            };
            var discharge2 = new Steerable
            {
                Mode = SteerableMode.TimeSeries,
                TimeSeriesFilename = forcingFile2
            };
            var discharge3 = new Steerable
            {
                Mode = SteerableMode.TimeSeries,
                TimeSeriesFilename = forcingFile3
            };

            var lateralDTO1 = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                             null, null, null, discharge1);
            var lateralDTO2 = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                             null, null, null, discharge2);
            var lateralDTO3 = new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                             null, null, null, discharge3);

            // Calls
            bndExtForceFileDTO.AddLateral(lateralDTO1);
            bndExtForceFileDTO.AddLateral(lateralDTO2);
            bndExtForceFileDTO.AddLateral(lateralDTO3);

            // Assert
            LateralDTO[] expLateralDTOs = { lateralDTO1, lateralDTO2, lateralDTO3 };
            var expForcingFiles = new[] { "some_forcing_file1", "some_forcing_file2" };

            Assert.That(bndExtForceFileDTO.Boundaries, Is.Empty);
            Assert.That(bndExtForceFileDTO.Laterals, Is.EquivalentTo(expLateralDTOs));
            Assert.That(bndExtForceFileDTO.LocationFiles, Is.Empty);
            Assert.That(bndExtForceFileDTO.ForcingFiles, Is.EquivalentTo(expForcingFiles));
        }
    }
}