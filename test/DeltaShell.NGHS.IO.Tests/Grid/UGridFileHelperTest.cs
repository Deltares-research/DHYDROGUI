using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Link1d2d;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using log4net.Core;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;
using SharpMapTestUtils;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class UGridFileHelperTest
    {
        [TestCase("fileDoesNotExist.nc", false)]
        [TestCase(@"ugrid\Custom_Ugrid.nc", true)]
        [TestCase(@"nonUgrid\TAK3_net.nc", true)]

        public void TestLoadFromFile(string filePath, bool gridShouldLoad)
        {
            var testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), filePath);
            Assert.AreEqual(gridShouldLoad, File.Exists(testFilePath));

            var grid = UGridFileHelper.ReadUnstructuredGrid(testFilePath);
            Assert.AreEqual(gridShouldLoad, grid != null);
        }

        [Test]
        public void CheckUGridFileDeltaresVersion()
        {
            // Setup
            string filePath = TestHelper.CreateLocalCopySingleFile(Path.Combine(TestHelper.GetTestDataDirectory(), "ugrid", "Custom_Ugrid.nc"));

            // Call
            void Call() => UGridFileHelper.IsUGridFile(filePath);

            // Assert
            string[] warnings = TestHelper.GetAllRenderedMessages(Call, Level.Warn).ToArray();
            var expMessage = $"The minimum required NetCDF convention for this file is Deltares-0.10 but the provided convention is CF-1.6 UGRID-1.0 Deltares-0.8. See global attribute \"Conventions\" in file {filePath}";
            Assert.That(warnings, Does.Contain(expMessage));
        }

        [Test]
        public void CheckDeltaresVersionInFileWithoutGlobalAttributeConventions()
        {
            // Setup
            string filePath = TestHelper.CreateLocalCopySingleFile(Path.Combine(TestHelper.GetTestDataDirectory(), "nonUgrid", "small_net.nc"));

            // Call
            void Call() => UGridFileHelper.IsUGridFile(filePath);

            // Assert
            string[] warnings = TestHelper.GetAllRenderedMessages(Call, Level.Warn).ToArray();
            var expMessage = $"Global attribute \"Conventions\" is missing in file {filePath}";
            Assert.That(warnings, Does.Contain(expMessage));
        }

        [Test]
        public void CheckDeltaresVersionNonExitingFile()
        {
            var filePath = "fileDoesNotExist.nc";
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => Assert.Throws<FileNotFoundException>(() =>new RemoteUGridApi().Open(filePath))
                , $"While reading Deltares netcdf file version type from file {filePath} we encounter the following problem:");
        }

        [TestCase(@"ugrid\BedLevelValues_NodesAndFaces.nc", UGridFileHelper.BedLevelLocation.Faces)]
        [TestCase(@"ugrid\BedLevelValues_NodesAndFaces.nc", UGridFileHelper.BedLevelLocation.FacesMeanLevFromNodes)]
        [TestCase(@"ugrid\BedLevelValues_NodesAndFaces.nc", UGridFileHelper.BedLevelLocation.NodesMaxLev)]
        [TestCase(@"ugrid\BedLevelValues_NodesAndFaces.nc", UGridFileHelper.BedLevelLocation.NodesMeanLev)]
        [TestCase(@"ugrid\BedLevelValues_NodesAndFaces.nc", UGridFileHelper.BedLevelLocation.NodesMinLev)]
        [TestCase(@"ugrid\BedLevelValues_NodesAndFaces.nc", UGridFileHelper.BedLevelLocation.NodesMeanLev)]
        public void TestReadZValues(string filePath, UGridFileHelper.BedLevelLocation location)
        {
            var testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), filePath);
            Assert.IsTrue(File.Exists(testFilePath));

            var zValues = UGridFileHelper.ReadZValues(testFilePath, location);
            Assert.IsTrue(zValues.Length > 0);
            Assert.IsTrue(zValues.All(v => v > 0.0));
        }

        [Test]
        public void TestReadZValues_DoesNotThrowForNoZValuesInFile()
        {
            var testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), @"ugrid\Custom_Ugrid.nc");
            Assert.IsTrue(File.Exists(testFilePath));

            var zValues = UGridFileHelper.ReadZValues(testFilePath, UGridFileHelper.BedLevelLocation.Faces);
            Assert.AreEqual(0, zValues.Length);
        }

        [Test]
        public void TestReadZValues_GivesWarningForEdgeLocations()
        {
            var testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), @"ugrid\BedLevelValues_NodesAndFaces.nc");
            Assert.IsTrue(File.Exists(testFilePath));

            var zValues = new double[0];

            TestHelper.AssertAtLeastOneLogMessagesContains(() => 
                zValues = UGridFileHelper.ReadZValues(testFilePath, UGridFileHelper.BedLevelLocation.CellEdges),
                Properties.Resources.UGridFileHelper_ReadZValues_Unable_to_read_z_values_at_this_location__CellEdges_are_not_currently_supported);

            Assert.AreEqual(0, zValues.Length);
        }

        [Test]
        public void TestReadZValues_GivesWarningForNonUgridFiles()
        {
            var testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), @"nonUgrid\TAK3_net.nc");
            Assert.IsTrue(File.Exists(testFilePath));

            var zValues = new double[0];

            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
                zValues = UGridFileHelper.ReadZValues(testFilePath, UGridFileHelper.BedLevelLocation.CellEdges),
                string.Format(Properties.Resources.UGridFileHelper_ReadZValues_Unable_to_read_z_values_from_file___0___file_is_not_UGrid_convention, testFilePath));

            Assert.AreEqual(0, zValues.Length);
        }

        [TestCase(@"ugrid\Custom_Ugrid.nc", UGridFileHelper.BedLevelLocation.Faces)]
        [TestCase(@"ugrid\Custom_Ugrid.nc", UGridFileHelper.BedLevelLocation.FacesMeanLevFromNodes)]
        [TestCase(@"ugrid\Custom_Ugrid.nc", UGridFileHelper.BedLevelLocation.NodesMaxLev)]
        [TestCase(@"ugrid\Custom_Ugrid.nc", UGridFileHelper.BedLevelLocation.NodesMeanLev)]
        [TestCase(@"ugrid\Custom_Ugrid.nc", UGridFileHelper.BedLevelLocation.NodesMinLev)]
        [TestCase(@"nonUgrid\TAK3_net.nc", UGridFileHelper.BedLevelLocation.NodesMeanLev)]
        public void TestWriteZValues_DoesNotThrowForSupportedLocations(string filePath, UGridFileHelper.BedLevelLocation location)
        {
            var testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), filePath);
            Assert.IsTrue(File.Exists(testFilePath));

            var localtestFile = TestHelper.CreateLocalCopy(testFilePath);
            var grid = UGridFileHelper.ReadUnstructuredGrid(localtestFile);

            var zValues = new double[0];

            switch (location)
            {
                case UGridFileHelper.BedLevelLocation.Faces:
                case UGridFileHelper.BedLevelLocation.FacesMeanLevFromNodes:
                    zValues = Enumerable.Repeat(123.456, grid.Cells.Count).ToArray();
                    break;
                case UGridFileHelper.BedLevelLocation.NodesMeanLev:
                case UGridFileHelper.BedLevelLocation.NodesMinLev:
                case UGridFileHelper.BedLevelLocation.NodesMaxLev:
                    zValues = Enumerable.Repeat(123.456, grid.Vertices.Count).ToArray();
                    break;
            }
            
            UGridFileHelper.WriteZValues(localtestFile, location, zValues);
            FileUtils.DeleteIfExists(localtestFile);
        }

        [TestCase(UGridFileHelper.BedLevelLocation.Faces)]
        [TestCase(UGridFileHelper.BedLevelLocation.NodesMeanLev)]
        public void TestWriteZValues_SupportedLocations(UGridFileHelper.BedLevelLocation location)
        {
            var testFilePath = TestHelper.GetTestWorkingDirectoryGeneratedTestFilePath(".nc");

            FileUtils.DeleteIfExists(testFilePath);

            var grid = UnstructuredGridTestHelper.GenerateRegularGrid(2,3,100,100);
            var zValues = new double[0];

            switch (location)
            {
                case UGridFileHelper.BedLevelLocation.Faces:
                case UGridFileHelper.BedLevelLocation.FacesMeanLevFromNodes:
                    zValues = Enumerable.Repeat(123.456, grid.Cells.Count).ToArray();
                    break;
                case UGridFileHelper.BedLevelLocation.NodesMeanLev:
                case UGridFileHelper.BedLevelLocation.NodesMinLev:
                case UGridFileHelper.BedLevelLocation.NodesMaxLev:
                    zValues = Enumerable.Repeat(123.456, grid.Vertices.Count).ToArray();
                    break;
            }

            UGridFileHelper.WriteGridToFile(testFilePath, grid, null, null, null, "abc", "dummy", "1", location, zValues);

            FileUtils.DeleteIfExists(testFilePath);
        }

        [Test]
        public void TestWriteZValues_GivesWarningForEdgeLocations()
        {
            var testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), @"ugrid\Custom_Ugrid.nc");
            Assert.IsTrue(File.Exists(testFilePath));

            var localtestFile = TestHelper.CreateLocalCopy(testFilePath);
            var grid = UGridFileHelper.ReadUnstructuredGrid(localtestFile);

            var location = UGridFileHelper.BedLevelLocation.CellEdges;
            var zValues = Enumerable.Repeat(123.456, grid.Edges.Count).ToArray();

            TestHelper.AssertAtLeastOneLogMessagesContains(()=> UGridFileHelper.WriteZValues(localtestFile, location, zValues),
                Properties.Resources.UGridFileHelper_ReadZValues_Unable_to_read_z_values_at_this_location__CellEdges_are_not_currently_supported);

            FileUtils.DeleteIfExists(localtestFile);
        }

        [TestCase("fileDoesNotExist.nc", false, null)]
        [TestCase(@"ugrid\Custom_Ugrid.nc", true, 4326L)] // WGS84
        [TestCase(@"nonUgrid\small_net.nc", true, 28992L)] // Amersfoort / RD New
        public void TestGetCoordinateSystem(string filePath, bool testFileExists, long? expectedResult)
        {
            var testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), filePath);
            Assert.AreEqual(testFileExists, File.Exists(testFilePath));
            
            var coordinateSystemAuthorityCode = UGridFileHelper.ReadCoordinateSystem(testFilePath)?.AuthorityCode;
            Assert.AreEqual(expectedResult, coordinateSystemAuthorityCode);
        }

        [TestCase(@"ugrid\Custom_Ugrid.nc")]
        [TestCase(@"nonUgrid\TAK3_net.nc")]
        public void TestSetCoordinateSystem(string filePath)
        {
            var testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), filePath);
            Assert.IsTrue(File.Exists(testFilePath));

            var localtestFile = TestHelper.CreateLocalCopy(testFilePath);
            var coordinateSystemFactory = new OgrCoordinateSystemFactory();

            var coordinateSystem = coordinateSystemFactory.CreateFromEPSG(28992); // Amersfoort / RD New
            UGridFileHelper.WriteCoordinateSystem(localtestFile, coordinateSystem);

            coordinateSystem = coordinateSystemFactory.CreateFromEPSG(4326); // WGS84
            UGridFileHelper.WriteCoordinateSystem(localtestFile, coordinateSystem);

            FileUtils.DeleteIfExists(localtestFile);
        }

        [Test]
        public void TestWriteGridToFile_DoesNotThrowForExistingFile()
        {
            var testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), @"nonUgrid\TAK3_net.nc");
            Assert.IsTrue(File.Exists(testFilePath));

            var localtestFile = TestHelper.CreateLocalCopy(testFilePath);
            UGridFileHelper.WriteGridToFile(localtestFile, new UnstructuredGrid(), new HydroNetwork(), new Discretization(), new List<ILink1D2D>(), "myName", "myPlugin", "myVersion",UGridFileHelper.BedLevelLocation.NodesMaxLev,new double[]{});

            FileUtils.DeleteIfExists(localtestFile);
        }

        [Test]
        public void TestWriteGridToFile_CreateNewFileForNonExistingFile()
        {
            var testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "fileDoesNotExist.nc");
            Assert.IsFalse(File.Exists(testFilePath));

            var localtestFilePath = TestHelper.CreateLocalCopy(testFilePath);
            Assert.IsFalse(File.Exists(testFilePath));

            var unstructuredGrid = UnstructuredGridTestHelper.GenerateRegularGrid(5,5,10,10);
            var hydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(4, true);

            var networkDiscretization = new Discretization() {Network = hydroNetwork, Name = "Mydiscreatisation"};
            HydroNetworkHelper.GenerateDiscretization(networkDiscretization, true, false, 0.5, false, 1.0, false, false, true, 2, null);

            var link1D2Ds = new List<ILink1D2D>()
            {
                {new Link1D2D(1, 1, "my link") {TypeOfLink = LinkStorageType.Embedded}}
            };

            var zValues = Enumerable.Range(1, unstructuredGrid.Vertices.Count).Select(Convert.ToDouble).ToArray();
            UGridFileHelper.WriteGridToFile(localtestFilePath, unstructuredGrid, hydroNetwork, networkDiscretization, link1D2Ds, "myName", "myPlugin", "myVersion", UGridFileHelper.BedLevelLocation.NodesMaxLev, zValues);

            Assert.IsTrue(File.Exists(localtestFilePath));

            FileUtils.DeleteIfExists(localtestFilePath);
        }

        [TestCase(@"ugrid\Custom_Ugrid.nc")]
        [TestCase(@"nonUgrid\TAK3_net.nc")]
        public void TestRewriteGridCoordinates(string filePath)
        {
            var testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), filePath);
            Assert.IsTrue(File.Exists(testFilePath));

            var localtestFile = TestHelper.CreateLocalCopy(testFilePath);
            var grid = UGridFileHelper.ReadUnstructuredGrid(localtestFile);

            foreach (var coordinate in grid.Vertices)
            {
                coordinate.X += 1.0;
                coordinate.Y -= 1.0;
            }

            UGridFileHelper.RewriteGridCoordinates(localtestFile, grid);

            FileUtils.DeleteIfExists(localtestFile);
        }
    }
}
