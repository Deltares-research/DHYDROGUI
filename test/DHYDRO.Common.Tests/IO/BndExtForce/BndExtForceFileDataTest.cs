using System.Collections.Generic;
using System.IO.Abstractions;
using Deltares.Infrastructure.API.Logging;
using DHYDRO.Common.IO.BndExtForce;
using DHYDRO.Common.TestUtils.IO.BndExtForce;
using NSubstitute;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.BndExtForce
{
    [TestFixture]
    public class BndExtForceFileDataTest
    {
        [Test]
        public void Constructor_ShouldInitializeCollectionsAndFileInfo()
        {
            var fileData = new BndExtForceFileData();

            Assert.Multiple(() =>
            {
                Assert.That(fileData.BoundaryForcings, Is.Empty);
                Assert.That(fileData.LateralForcings, Is.Empty);
                Assert.That(fileData.MeteoForcings, Is.Empty);
                Assert.That(fileData.FileInfo, Is.Not.Null);
            });
        }

        [Test]
        public void FileInfo_SetNullValue_ThrowsArgumentNullException()
        {
            var fileData = new BndExtForceFileData();

            Assert.That(() => fileData.FileInfo = null, Throws.ArgumentNullException);
        }

        [Test]
        public void FileInfo_SetValidValue_SetsFileInfo()
        {
            var fileData = new BndExtForceFileData();
            var newFileInfo = new BndExtForceFileInfo();

            fileData.FileInfo = newFileInfo;

            Assert.That(fileData.FileInfo, Is.SameAs(newFileInfo));
        }

        [Test]
        public void AnyForcings_ForcingsAreEmpty_ReturnsFalse()
        {
            var fileData = new BndExtForceFileData();

            bool any = fileData.AnyForcings();

            Assert.That(any, Is.False);
        }

        [Test]
        public void AnyForcings_WithBoundaryData_ReturnsTrue()
        {
            var fileData = new BndExtForceFileData();
            var boundaryData = new BndExtForceBoundaryData();

            fileData.AddBoundaryForcing(boundaryData);

            bool any = fileData.AnyForcings();

            Assert.That(any, Is.True);
        }

        [Test]
        public void AnyForcings_WithLateralData_ReturnsTrue()
        {
            var fileData = new BndExtForceFileData();
            var lateralData = new BndExtForceLateralData();

            fileData.AddLateralForcing(lateralData);

            bool any = fileData.AnyForcings();

            Assert.That(any, Is.True);
        }

        [Test]
        public void AnyForcings_WithMeteoData_ReturnsTrue()
        {
            var fileData = new BndExtForceFileData();
            var meteoData = new BndExtForceMeteoData();

            fileData.AddMeteoForcing(meteoData);

            bool any = fileData.AnyForcings();

            Assert.That(any, Is.True);
        }

        [Test]
        public void GetForcingFiles_ForcingsAreEmpty_ReturnsEmptyCollection()
        {
            var fileData = new BndExtForceFileData();

            IEnumerable<string> forcingFiles = fileData.GetForcingFiles();

            Assert.That(forcingFiles, Is.Not.Null.And.Empty);
        }

        [Test]
        public void GetForcingFiles_WithForcingFiles_ReturnsForcingFiles()
        {
            var fileData = new BndExtForceFileData();
            var boundaryData = new BndExtForceBoundaryData { ForcingFiles = new[] { "boundaryforcings.bc" } };
            var meteoData = new BndExtForceMeteoData { ForcingFile = "meteoforcings.bc" };

            fileData.AddBoundaryForcing(boundaryData);
            fileData.AddMeteoForcing(meteoData);

            IEnumerable<string> forcingFiles = fileData.GetForcingFiles();

            Assert.That(forcingFiles, Is.EqualTo(new[] { "boundaryforcings.bc", "meteoforcings.bc" }));
        }

        [Test]
        public void GetForcingFiles_WithDuplicateForcingFiles_ReturnsDistinctForcingFiles()
        {
            var fileData = new BndExtForceFileData();
            var meteoData = new BndExtForceMeteoData { ForcingFile = "forcings.bc" };
            var boundaryData = new BndExtForceBoundaryData { ForcingFiles = new[] { "forcings.bc" } };

            fileData.AddMeteoForcing(meteoData);
            fileData.AddBoundaryForcing(boundaryData);

            IEnumerable<string> forcingFiles = fileData.GetForcingFiles();

            Assert.That(forcingFiles, Is.EqualTo(new[] { "forcings.bc" }));
        }

        [Test]
        public void GetForcingFiles_WithLocationFiles_ReturnsLocationFiles()
        {
            var fileData = new BndExtForceFileData();
            var boundaryData = new BndExtForceBoundaryData { LocationFile = "boundaries.pli" };
            var lateralData = new BndExtForceLateralData { LocationFile = "laterals.pol" };

            fileData.AddBoundaryForcing(boundaryData);
            fileData.AddLateralForcing(lateralData);

            IEnumerable<string> forcingFiles = fileData.GetLocationFiles();

            Assert.That(forcingFiles, Is.EqualTo(new[] { "boundaries.pli", "laterals.pol" }));
        }

        [Test]
        public void GetForcingFiles_WithDuplicateLocationFiles_ReturnsDistinctLocationFiles()
        {
            var fileData = new BndExtForceFileData();
            var boundaryData = new BndExtForceBoundaryData { LocationFile = "forcings.pli" };
            var lateralData = new BndExtForceLateralData { LocationFile = "forcings.pli" };

            fileData.AddBoundaryForcing(boundaryData);
            fileData.AddLateralForcing(lateralData);

            IEnumerable<string> forcingFiles = fileData.GetLocationFiles();

            Assert.That(forcingFiles, Is.EqualTo(new[] { "forcings.pli" }));
        }

        [Test]
        public void AddBoundaryForcing_NullBoundaryData_ThrowsArgumentNullException()
        {
            var fileData = new BndExtForceFileData();

            Assert.That(() => fileData.AddBoundaryForcing(null), Throws.ArgumentNullException);
        }

        [Test]
        public void AddBoundaryForcing_ValidBoundaryData_AddsBoundaryDataToCollection()
        {
            var fileData = new BndExtForceFileData();
            var boundaryData = new BndExtForceBoundaryData();

            fileData.AddBoundaryForcing(boundaryData);

            Assert.That(fileData.BoundaryForcings, Has.Exactly(1).SameAs(boundaryData));
        }

        [Test]
        public void AddBoundaryForcings_NullBoundaryData_ThrowsArgumentNullException()
        {
            var fileData = new BndExtForceFileData();

            Assert.That(() => fileData.AddBoundaryForcings(null), Throws.ArgumentNullException);
        }

        [Test]
        public void AddBoundaryForcings_ValidBoundaryData_AddsBoundaryDataToCollection()
        {
            var fileData = new BndExtForceFileData();
            var boundaryData1 = new BndExtForceBoundaryData();
            var boundaryData2 = new BndExtForceBoundaryData();

            BndExtForceBoundaryData[] forcings = { boundaryData1, boundaryData2 };

            fileData.AddBoundaryForcings(forcings);

            Assert.That(fileData.BoundaryForcings, Is.EquivalentTo(forcings));
        }

        [Test]
        public void AddLateralForcing_NullLateralData_ThrowsArgumentNullException()
        {
            var fileData = new BndExtForceFileData();

            Assert.That(() => fileData.AddLateralForcing(null), Throws.ArgumentNullException);
        }

        [Test]
        public void AddLateralForcing_ValidLateralData_AddsLateralDataToCollection()
        {
            var fileData = new BndExtForceFileData();
            var lateralData = new BndExtForceLateralData();

            fileData.AddLateralForcing(lateralData);

            Assert.That(fileData.LateralForcings, Has.Exactly(1).SameAs(lateralData));
        }

        [Test]
        public void AddLateralForcings_NullLateralData_ThrowsArgumentNullException()
        {
            var fileData = new BndExtForceFileData();

            Assert.That(() => fileData.AddLateralForcings(null), Throws.ArgumentNullException);
        }

        [Test]
        public void AddLateralForcings_ValidBoundaryData_AddsLateralDataToCollection()
        {
            var fileData = new BndExtForceFileData();
            var lateralData1 = new BndExtForceLateralData();
            var lateralData2 = new BndExtForceLateralData();

            BndExtForceLateralData[] forcings = { lateralData1, lateralData2 };

            fileData.AddLateralForcings(forcings);

            Assert.That(fileData.LateralForcings, Is.EquivalentTo(forcings));
        }

        [Test]
        public void AddMeteoForcing_NullMeteoData_ThrowsArgumentNullException()
        {
            var fileData = new BndExtForceFileData();

            Assert.That(() => fileData.AddMeteoForcing(null), Throws.ArgumentNullException);
        }

        [Test]
        public void AddMeteoForcing_ValidMeteoData_AddsMeteoDataToCollection()
        {
            var fileData = new BndExtForceFileData();
            var meteoData = new BndExtForceMeteoData();

            fileData.AddMeteoForcing(meteoData);

            Assert.That(fileData.MeteoForcings, Has.Exactly(1).SameAs(meteoData));
        }

        [Test]
        public void AddMeteoForcings_NullMeteoData_ThrowsArgumentNullException()
        {
            var fileData = new BndExtForceFileData();

            Assert.That(() => fileData.AddMeteoForcings(null), Throws.ArgumentNullException);
        }

        [Test]
        public void AddMeteoForcings_ValidBoundaryData_AddsMeteoDataToCollection()
        {
            var fileData = new BndExtForceFileData();
            var meteoData1 = new BndExtForceMeteoData();
            var meteoData2 = new BndExtForceMeteoData();

            BndExtForceMeteoData[] forcings = { meteoData1, meteoData2 };

            fileData.AddMeteoForcings(forcings);

            Assert.That(fileData.MeteoForcings, Is.EquivalentTo(forcings));
        }

        [Test]
        public void RemoveInvalidForcings_NullLogHandler_ThrowsArgumentNullException()
        {
            var fileData = new BndExtForceFileData();

            Assert.That(() => fileData.RemoveInvalidForcings(null), Throws.ArgumentNullException);
        }

        [Test]
        public void RemoveInvalidForcings_ValidForcingData_ForcingDataNotRemoved()
        {
            BndExtForceFileData fileData = BndExtForceFileDataBuilder.Start()
                                                                     .AddValidForcingData()
                                                                     .Build();

            var fileSystem = Substitute.For<IFileSystem>();
            var logHandler = Substitute.For<ILogHandler>();

            fileSystem.File.Exists(Arg.Any<string>()).Returns(true);
            fileData.BoundaryDataValidator.FileSystem = fileSystem;
            fileData.LateralDataValidator.FileSystem = fileSystem;
            fileData.MeteoDataValidator.FileSystem = fileSystem;

            fileData.RemoveInvalidForcings(logHandler);

            Assert.Multiple(() =>
            {
                Assert.That(fileData.BoundaryForcings, Is.Not.Empty);
                Assert.That(fileData.LateralForcings, Is.Not.Empty);
                Assert.That(fileData.MeteoForcings, Is.Not.Empty);
            });
        }

        [Test]
        public void RemoveInvalidForcings_InvalidForcingData_ForcingDataRemoved()
        {
            BndExtForceFileData fileData = BndExtForceFileDataBuilder.Start()
                                                                     .AddValidForcingData()
                                                                     .Build();
            var logHandler = Substitute.For<ILogHandler>();

            fileData.RemoveInvalidForcings(logHandler);

            Assert.Multiple(() =>
            {
                Assert.That(fileData.BoundaryForcings, Is.Empty);
                Assert.That(fileData.LateralForcings, Is.Empty);
                Assert.That(fileData.MeteoForcings, Is.Empty);
            });
        }

        [Test]
        public void RemoveInvalidForcings_InvalidForcingData_LogsValidationMessages()
        {
            BndExtForceFileData fileData = BndExtForceFileDataBuilder.Start()
                                                                     .AddValidForcingData()
                                                                     .Build();
            var logHandler = Substitute.For<ILogHandler>();

            fileData.RemoveInvalidForcings(logHandler);

            logHandler.Received().ReportError(Arg.Is<string>(s => s.Contains("Location file does not exist")));
            logHandler.Received().ReportError(Arg.Is<string>(s => s.Contains("Forcing file does not exist")));
            logHandler.Received().ReportError(Arg.Is<string>(s => s.Contains("Discharge file does not exist")));
        }
    }
}