using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.TestUtils;
using GeoAPI.Extensions.CoordinateSystems;
using log4net.Core;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class UnstructuredGridFileOperationsTest
    {
        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Constructor_FilePathNullOrEmpty_ThrowsArgumentException(string invalidFilePath)
        {
            // Call
            TestDelegate call = () => new UnstructuredGridFileOperations(invalidFilePath);

            // Assert
            Assert.That(call, Throws.TypeOf<ArgumentException>()
                                    .With.Property(nameof(ArgumentException.ParamName))
                                    .EqualTo("filePath"));
        }

        [Test]
        public void Constructor_WithNonExistingFileParameter_LogsErrorMessage()
        {
            // Setup
            string notExistingFile = GetTestFilePath(@"ugrid\DoesNotExist.nc");

            void Call() => new UnstructuredGridFileOperations(notExistingFile);

            IEnumerable<string> errors = TestHelper.GetAllRenderedMessages(Call, Level.Error);
            Assert.That(errors.Single(), Is.EqualTo($"Could not find grid at \"{notExistingFile}\""));
        }

        [Test]
        [TestCase(@"ugrid\Custom_Ugrid.nc", GridApiDataSet.DataSetConventions.CONV_UGRID)]
        [TestCase(@"nonUgrid\TAK3_net.nc", GridApiDataSet.DataSetConventions.CONV_OTHER)]
        public void Constructor_WithArguments_ExpectedValues(string filePath,
                                                             GridApiDataSet.DataSetConventions expectedDataSetConventions)
        {
            // Setup
            string testFilePath = GetTestFilePath(filePath);

            // Call
            var fileOperations = new UnstructuredGridFileOperations(testFilePath);

            // Assert
            Assert.That(fileOperations.DataSetConventions, Is.EqualTo(expectedDataSetConventions));
        }

        [Test]
        [TestCase(@"ugrid\Custom_Ugrid.nc")]
        [TestCase(@"nonUgrid\TAK3_net.nc")]
        public void GetGrid_WithVariousFiles_DoesNotReturnNull(string filePath)
        {
            // Setup
            string testFilePath = GetTestFilePath(filePath);
            var fileOperations = new UnstructuredGridFileOperations(testFilePath);

            // Call
            UnstructuredGrid grid = fileOperations.GetGrid();

            // Assert
            Assert.That(grid, Is.Not.Null);
        }

        [Test]
        [TestCase(@"ugrid\Custom_Ugrid.nc", 4326L)]  // WGS84
        [TestCase(@"nonUgrid\small_net.nc", 28992L)] // Amersfoort / RD New
        public void GetCoordinateSystem_WithVariousFiles_ReturnsExpectedCoordinateSystem(string filePath, long expectedCode)
        {
            // Setup
            string testFilePath = GetTestFilePath(filePath);
            var fileOperations = new UnstructuredGridFileOperations(testFilePath);

            // Call
            ICoordinateSystem coordinateSystem = fileOperations.GetCoordinateSystem();

            // Assert
            Assert.That(coordinateSystem, Is.Not.Null);
            Assert.That(coordinateSystem.AuthorityCode, Is.EqualTo(expectedCode));
        }

        [Test]
        public void DoIfUGrid_UgridActionNull_ThrowsArgumentNullException()
        {
            // Setup
            string testFilePath = GetTestFilePath(@"ugrid\Custom_Ugrid.nc");
            var fileOperations = new UnstructuredGridFileOperations(testFilePath);

            // Call
            TestDelegate call = () => fileOperations.DoIfUgrid(null);

            // Assert
            Assert.That(call, Throws.TypeOf<ArgumentNullException>()
                                    .With.Property(nameof(ArgumentNullException.ParamName))
                                    .EqualTo("ugridAction"));
        }

        [Test]
        public void DoIfUGrid_WithUGridFile_PerformsAction()
        {
            // Setup
            string testFilePath = GetTestFilePath(@"ugrid\Custom_Ugrid.nc");
            var fileOperations = new UnstructuredGridFileOperations(testFilePath);

            var counter = 0;

            // Precondition
            Assert.That(fileOperations.DataSetConventions, Is.EqualTo(GridApiDataSet.DataSetConventions.CONV_UGRID));

            // Call
            fileOperations.DoIfUgrid(adaptor => counter++);

            // Assert
            Assert.That(counter, Is.EqualTo(1));
        }

        [Test]
        public void DoIfUGrid_WithNonUGridFile_DoesNotPerformsAction()
        {
            // Setup
            string testFilePath = GetTestFilePath(@"nonUgrid\TAK3_net.nc");
            var fileOperations = new UnstructuredGridFileOperations(testFilePath);

            var counter = 0;

            // Precondition
            Assert.That(fileOperations.DataSetConventions, Is.Not.EqualTo(GridApiDataSet.DataSetConventions.CONV_UGRID));

            // Call
            fileOperations.DoIfUgrid(adaptor => counter++);

            // Assert
            Assert.That(counter, Is.EqualTo(0));
        }

        private static string GetTestFilePath(string filePath)
        {
            return Path.Combine(TestHelper.GetTestDataDirectory(), filePath);
        }
    }
}