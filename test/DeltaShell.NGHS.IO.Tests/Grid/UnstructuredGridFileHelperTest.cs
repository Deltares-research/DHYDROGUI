using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Grid;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class UnstructuredGridFileHelperTest
    {
        [TestCase("fileDoesNotExist.nc", false)]
        [TestCase(@"ugrid\Custom_Ugrid.nc", true)]
        [TestCase(@"nonUgrid\TAK3_net.nc", true)]

        public void TestLoadFromFile(string filePath, bool gridShouldLoad)
        {
            var testFilePath = Path.Combine(TestHelper.GetDataDir(), filePath);
            Assert.AreEqual(gridShouldLoad, File.Exists(testFilePath));

            var grid = UnstructuredGridFileHelper.LoadFromFile(testFilePath);
            Assert.AreEqual(gridShouldLoad, grid != null);
        }

        [TestCase(@"ugrid\BedLevelValues_NodesAndFaces.nc", UnstructuredGridFileHelper.BedLevelLocation.Faces)]
        [TestCase(@"ugrid\BedLevelValues_NodesAndFaces.nc", UnstructuredGridFileHelper.BedLevelLocation.FacesMeanLevFromNodes)]
        [TestCase(@"ugrid\BedLevelValues_NodesAndFaces.nc", UnstructuredGridFileHelper.BedLevelLocation.NodesMaxLev)]
        [TestCase(@"ugrid\BedLevelValues_NodesAndFaces.nc", UnstructuredGridFileHelper.BedLevelLocation.NodesMeanLev)]
        [TestCase(@"ugrid\BedLevelValues_NodesAndFaces.nc", UnstructuredGridFileHelper.BedLevelLocation.NodesMinLev)]
        [TestCase(@"ugrid\BedLevelValues_NodesAndFaces.nc", UnstructuredGridFileHelper.BedLevelLocation.NodesMeanLev)]
        public void TestReadZValues(string filePath, UnstructuredGridFileHelper.BedLevelLocation location)
        {
            var testFilePath = Path.Combine(TestHelper.GetDataDir(), filePath);
            Assert.IsTrue(File.Exists(testFilePath));

            var zValues = UnstructuredGridFileHelper.ReadZValues(testFilePath, location);
            Assert.IsTrue(zValues.Length > 0);
            Assert.IsTrue(zValues.All(v => v > 0.0));
        }

        [Test]
        public void TestReadZValues_DoesNotThrowForNoZValuesInFile()
        {
            var testFilePath = Path.Combine(TestHelper.GetDataDir(), @"ugrid\Custom_Ugrid.nc");
            Assert.IsTrue(File.Exists(testFilePath));

            var zValues = UnstructuredGridFileHelper.ReadZValues(testFilePath, UnstructuredGridFileHelper.BedLevelLocation.Faces);
            Assert.AreEqual(0, zValues.Length);
        }

        [Test]
        public void TestReadZValues_GivesWarningForEdgeLocations()
        {
            var testFilePath = Path.Combine(TestHelper.GetDataDir(), @"ugrid\BedLevelValues_NodesAndFaces.nc");
            Assert.IsTrue(File.Exists(testFilePath));

            var zValues = new double[0];

            TestHelper.AssertAtLeastOneLogMessagesContains(() => 
                zValues = UnstructuredGridFileHelper.ReadZValues(testFilePath, UnstructuredGridFileHelper.BedLevelLocation.CellEdges),
                Properties.Resources.UnstructuredGridFileHelper_ReadZValues_Unable_to_read_z_values_at_this_location__CellEdges_are_not_currently_supported);

            Assert.AreEqual(0, zValues.Length);
        }

        [Test]
        public void TestReadZValues_GivesWarningForNonUgridFiles()
        {
            var testFilePath = Path.Combine(TestHelper.GetDataDir(), @"nonUgrid\TAK3_net.nc");
            Assert.IsTrue(File.Exists(testFilePath));

            var zValues = new double[0];

            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
                zValues = UnstructuredGridFileHelper.ReadZValues(testFilePath, UnstructuredGridFileHelper.BedLevelLocation.CellEdges),
                string.Format(Properties.Resources.UnstructuredGridFileHelper_ReadZValues_Unable_to_read_z_values_from_file___0___file_is_not_UGrid_convention, testFilePath));

            Assert.AreEqual(0, zValues.Length);
        }

        [TestCase(@"ugrid\Custom_Ugrid.nc", UnstructuredGridFileHelper.BedLevelLocation.Faces)]
        [TestCase(@"ugrid\Custom_Ugrid.nc", UnstructuredGridFileHelper.BedLevelLocation.FacesMeanLevFromNodes)]
        [TestCase(@"ugrid\Custom_Ugrid.nc", UnstructuredGridFileHelper.BedLevelLocation.NodesMaxLev)]
        [TestCase(@"ugrid\Custom_Ugrid.nc", UnstructuredGridFileHelper.BedLevelLocation.NodesMeanLev)]
        [TestCase(@"ugrid\Custom_Ugrid.nc", UnstructuredGridFileHelper.BedLevelLocation.NodesMinLev)]
        [TestCase(@"nonUgrid\TAK3_net.nc", UnstructuredGridFileHelper.BedLevelLocation.NodesMeanLev)]
        public void TestWriteZValues_DoesNotThrowForSupportedLocations(string filePath, UnstructuredGridFileHelper.BedLevelLocation location)
        {
            var testFilePath = Path.Combine(TestHelper.GetDataDir(), filePath);
            Assert.IsTrue(File.Exists(testFilePath));

            var localtestFile = TestHelper.CreateLocalCopy(testFilePath);
            var grid = UnstructuredGridFileHelper.LoadFromFile(localtestFile);

            var zValues = new double[0];

            switch (location)
            {
                case UnstructuredGridFileHelper.BedLevelLocation.Faces:
                case UnstructuredGridFileHelper.BedLevelLocation.FacesMeanLevFromNodes:
                    zValues = Enumerable.Repeat(123.456, grid.Cells.Count).ToArray();
                    break;
                case UnstructuredGridFileHelper.BedLevelLocation.NodesMeanLev:
                case UnstructuredGridFileHelper.BedLevelLocation.NodesMinLev:
                case UnstructuredGridFileHelper.BedLevelLocation.NodesMaxLev:
                    zValues = Enumerable.Repeat(123.456, grid.Vertices.Count).ToArray();
                    break;
            }
            
            UnstructuredGridFileHelper.WriteZValues(localtestFile, location, zValues);
            FileUtils.DeleteIfExists(localtestFile);
        }

        [Test]
        public void TestWriteZValues_GivesWarningForEdgeLocations()
        {
            var testFilePath = Path.Combine(TestHelper.GetDataDir(), @"ugrid\Custom_Ugrid.nc");
            Assert.IsTrue(File.Exists(testFilePath));

            var localtestFile = TestHelper.CreateLocalCopy(testFilePath);
            var grid = UnstructuredGridFileHelper.LoadFromFile(localtestFile);

            var location = UnstructuredGridFileHelper.BedLevelLocation.CellEdges;
            var zValues = Enumerable.Repeat(123.456, grid.Edges.Count).ToArray();
            
            TestHelper.AssertAtLeastOneLogMessagesContains(()=> UnstructuredGridFileHelper.WriteZValues(localtestFile, location, zValues),
                Properties.Resources.UnstructuredGridFileHelper_WriteZValues_Unable_to_write_z_values_at_this_location__CellEdges_are_not_currently_supported);

            FileUtils.DeleteIfExists(localtestFile);
        }

        [TestCase("fileDoesNotExist.nc", false, null)]
        [TestCase(@"ugrid\Custom_Ugrid.nc", true, 4326L)] // WGS84
        [TestCase(@"nonUgrid\small_net.nc", true, 28992L)] // Amersfoort / RD New
        public void TestGetCoordinateSystem(string filePath, bool testFileExists, long? expectedResult)
        {
            var testFilePath = Path.Combine(TestHelper.GetDataDir(), filePath);
            Assert.AreEqual(testFileExists, File.Exists(testFilePath));
            
            var coordinateSystemAuthorityCode = UnstructuredGridFileHelper.GetCoordinateSystem(testFilePath)?.AuthorityCode;
            Assert.AreEqual(expectedResult, coordinateSystemAuthorityCode);
        }

        [TestCase(@"ugrid\Custom_Ugrid.nc")]
        [TestCase(@"nonUgrid\TAK3_net.nc")]
        public void TestSetCoordinateSystem(string filePath)
        {
            var testFilePath = Path.Combine(TestHelper.GetDataDir(), filePath);
            Assert.IsTrue(File.Exists(testFilePath));

            var localtestFile = TestHelper.CreateLocalCopy(testFilePath);
            var coordinateSystemFactory = new OgrCoordinateSystemFactory();

            var coordinateSystem = coordinateSystemFactory.CreateFromEPSG(28992); // Amersfoort / RD New
            UnstructuredGridFileHelper.SetCoordinateSystem(localtestFile, coordinateSystem);

            coordinateSystem = coordinateSystemFactory.CreateFromEPSG(4326); // WGS84
            UnstructuredGridFileHelper.SetCoordinateSystem(localtestFile, coordinateSystem);

            FileUtils.DeleteIfExists(localtestFile);
        }

        [Test]
        public void TestWriteGridToFile_DoesNotThrowForExistingFile()
        {
            var testFilePath = Path.Combine(TestHelper.GetDataDir(), @"nonUgrid\TAK3_net.nc");
            Assert.IsTrue(File.Exists(testFilePath));

            var localtestFile = TestHelper.CreateLocalCopy(testFilePath);
            UnstructuredGridFileHelper.WriteGridToFile(localtestFile, new UnstructuredGrid());

            FileUtils.DeleteIfExists(localtestFile);
        }

        [Test]
        public void TestWriteGridToFile_CreateNewFileForNonExistingFile()
        {
            var testFilePath = Path.Combine(TestHelper.GetDataDir(), "fileDoesNotExist.nc");
            Assert.IsFalse(File.Exists(testFilePath));

            var localtestFile = TestHelper.CreateLocalCopy(testFilePath);
            Assert.IsFalse(File.Exists(testFilePath));

            UnstructuredGridFileHelper.WriteGridToFile(localtestFile, new UnstructuredGrid());
            Assert.IsTrue(File.Exists(localtestFile));

            FileUtils.DeleteIfExists(localtestFile);
        }

        [TestCase(@"ugrid\Custom_Ugrid.nc")]
        [TestCase(@"nonUgrid\TAK3_net.nc")]
        public void TestRewriteGridCoordinates(string filePath)
        {
            var testFilePath = Path.Combine(TestHelper.GetDataDir(), filePath);
            Assert.IsTrue(File.Exists(testFilePath));

            var localtestFile = TestHelper.CreateLocalCopy(testFilePath);
            var grid = UnstructuredGridFileHelper.LoadFromFile(localtestFile);

            foreach (var coordinate in grid.Vertices)
            {
                coordinate.X += 1.0;
                coordinate.Y -= 1.0;
            }

            UnstructuredGridFileHelper.RewriteGridCoordinates(localtestFile, grid);

            FileUtils.DeleteIfExists(localtestFile);
        }

        [TestCase(@"ugrid\Custom_Ugrid.nc", 1)]
        [TestCase(@"nonUgrid\TAK3_net.nc", 0)]
        public void TestDoIfUgrid(string filePath, int expectedCounter)
        {
            var testFilePath = Path.Combine(TestHelper.GetDataDir(), filePath);
            Assert.IsTrue(File.Exists(testFilePath));

            var counter = 0;
            UnstructuredGridFileHelper.DoIfUgrid(testFilePath, uGridAdaptor => { counter++; });
            Assert.AreEqual(expectedCounter, counter);
        }

    }
}
