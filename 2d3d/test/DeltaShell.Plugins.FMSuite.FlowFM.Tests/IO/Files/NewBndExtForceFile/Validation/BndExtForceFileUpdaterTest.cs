using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.IO;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Validation;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Validation
{
    [TestFixture]
    public class BndExtForceFileUpdaterTest
    {
        private string referencePath;
        private string parentFilePath;
        private ILogHandler logHandler;
        private MockFileSystem fileSystem;

        [SetUp]
        public void SetUp()
        {
            referencePath = "C:\\some_parent_data_directory";
            parentFilePath = "some_parent_file_path.ext";
            logHandler = Substitute.For<ILogHandler>();
            fileSystem = new MockFileSystem();
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Constructor_ReferencePathIsNullOrWhiteSpace_ThrowsArgumentException(string arg)
        {
            Assert.That(() => _ = new BndExtForceFileUpdater(arg, parentFilePath, logHandler, fileSystem), Throws.ArgumentException);
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Constructor_ParentFilePathIsNullOrWhiteSpace_ThrowsArgumentException(string arg)
        {
            Assert.That(() => _ = new BndExtForceFileUpdater(referencePath, arg, logHandler, fileSystem), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_LogHandlerIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => _ = new BndExtForceFileUpdater(referencePath, parentFilePath, null, fileSystem), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_FileSystemIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => _ = new BndExtForceFileUpdater(referencePath, parentFilePath, logHandler, null), Throws.ArgumentNullException);
        }

        [Test]
        public void Update_DTOIsNull_ThrowsArgumentNullException()
        {
            BndExtForceFileUpdater updater = CreateUpdater();
            Assert.That(() => updater.Update(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Update_WithValidFile_ItemsArePreserved()
        {
            BndExtForceFileUpdater updater = CreateUpdater();

            BoundaryDTO boundary = CreateValidBoundary();
            LateralDTO lateral = CreateValidLateral();

            var dto = new BndExtForceFileDTO();
            dto.AddBoundary(boundary);
            dto.AddLateral(lateral);

            updater.Update(dto);

            Assert.That(dto.Boundaries, Does.Contain(boundary));
            Assert.That(dto.Laterals, Does.Contain(lateral));
        }

        [Test]
        public void Update_WithInvalidBoundary_BoundaryIsRemoved()
        {
            BndExtForceFileUpdater updater = CreateUpdater();

            BoundaryDTO boundary = CreateInvalidBoundary();

            var dto = new BndExtForceFileDTO();
            dto.AddBoundary(boundary);

            updater.Update(dto);

            Assert.That(dto.Boundaries, Is.Empty);
        }

        [Test]
        public void Update_WithInvalidLateral_LateralIsRemoved()
        {
            BndExtForceFileUpdater updater = CreateUpdater();

            LateralDTO lateral = CreateInvalidLateral();

            var dto = new BndExtForceFileDTO();
            dto.AddLateral(lateral);

            updater.Update(dto);

            Assert.That(dto.Boundaries, Is.Empty);
        }

        private BoundaryDTO CreateValidBoundary()
        {
            string locationFile = GetValidFile();
            return new BoundaryDTO("some_quantity", locationFile, Enumerable.Empty<string>(), 1.23) { LineNumber = 3 };
        }

        private static BoundaryDTO CreateInvalidBoundary()
        {
            return new BoundaryDTO("some_quantity", "file_does_not_exist.pli", Enumerable.Empty<string>(), 1.23) { LineNumber = 3 };
        }

        private BndExtForceFileUpdater CreateUpdater()
        {
            return new BndExtForceFileUpdater(referencePath, parentFilePath, logHandler, fileSystem);
        }

        private static LateralDTO CreateInvalidLateral()
        {
            var xCoordinates = new[] { 1.23 };
            var yCoordinates = new[] { 4.56 };
            var discharge = new Steerable { Mode = SteerableMode.External };
            return new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                  3, xCoordinates, yCoordinates, discharge) { LineNumber = 3 };
        }

        private static LateralDTO CreateValidLateral()
        {
            var xCoordinates = new[] { 1.23 };
            var yCoordinates = new[] { 4.56 };
            var discharge = new Steerable { Mode = SteerableMode.External };
            return new LateralDTO("some_id", "some_name", LateralForcingType.Discharge, LateralLocationType.TwoD,
                                  1, xCoordinates, yCoordinates, discharge) { LineNumber = 3 };
        }

        private string GetValidFile()
        {
            string fileName = Path.GetRandomFileName();
            string filePath = fileSystem.GetAbsolutePath(referencePath, fileName);
            fileSystem.AddEmptyFile(filePath);
            return fileName;
        }
    }
}