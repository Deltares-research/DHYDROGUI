using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.NetCdf;
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


        /// <summary>
        /// GIVEN
        ///   An UnstructuredGridFileHelper AND
        ///   A Path
        /// WHEN
        ///   UnstructuredGridFileHelper.WriteEmptyUnstructuredGridFile is called with Path
        /// Then
        ///   An empty Unstructured Grid file is created.
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnUnstructuredGridFileHelperAndAPath_WhenWriteEmptyUnstructuredGridFileWithThisPathIsCalled_ThenAnEmptyUnstructuredGridFileIsCreated()
        {
            const string fileName = "unstructured_grid_file_net.nc";
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                // Given
                var path = Path.Combine(tempDir, fileName);

                // When
                UnstructuredGridFileHelper.WriteEmptyUnstructuredGridFile(path);

                // Then
                using (var uGrid = new UGrid(path, mode: GridApiDataSet.NetcdfOpenMode.nf90_nowrite))
                {
                    uGrid.Initialize();
                    Assert.That(uGrid.IsValid());
                    Assert.That(uGrid.GetNumberOfNetworks(), Is.EqualTo(0));
                }
            });
        }


        /// <summary>
        /// GIVEN
        ///   An Empty Unstructured Grid File AND
        ///   A Null Coordinate System
        /// WHEN
        ///   This Coordinate System is written to this file
        /// THEN
        ///   The Unstructured Grid File should contain the Null Coordinate System
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        public void
            GivenAnEmptyUnstructuredGridFileAndANullCoordinateSystem_WhenThisCoordinateSystemIsWrittenToThisFile_ThenTheUnstructuredGridFileShouldContainTheNullCoordinateSystem()
        {
            const string fileName = "unstructured_grid_file_net.nc";

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                var path = Path.Combine(tempDir, fileName);

                // Construct empty file.
                UnstructuredGridFileHelper.WriteEmptyUnstructuredGridFile(path);

                // When
                UnstructuredGridFileHelper.WriteCoordinateSystemToFile(path, null, true);

                // Then
                using (var uGrid = new UGrid(path, mode: GridApiDataSet.NetcdfOpenMode.nf90_nowrite))
                {
                    uGrid.Initialize();
                    Assert.That(uGrid.IsValid());
                    Assert.That(uGrid.GetNumberOfNetworks(), Is.EqualTo(0));
                }

                AssertFileContainsNullCoordinateSystem(path);
            });
        }

        /// <summary>
        /// GIVEN
        ///   An Empty Unstructured Grid File AND
        ///   A Coordinate System with Authority code {epsg}
        /// WHEN
        ///   This Coordinate System is written to this file
        /// THEN
        ///   The Unstructured Grid File should contain the Coordinate System
        /// </summary>
        [TestCase(4326,  TestName = "GivenAnEmptyUnstructuredGridFileAndASphericalCoordinateSystem_WhenThisCoordinateSystemIsWrittenToThisFile_ThenThisCoordinateSystemShouldBeWrittenCorrectly")]
        [TestCase(28992, TestName = "GivenAnEmptyUnstructuredGridFileAndACartesianCoordinateSystem_WhenThisCoordinateSystemIsWrittenToThisFile_ThenThisCoordinateSystemShouldBeWrittenCorrectly")]
        [Category(TestCategory.DataAccess)]
        public void GivenAnEmptyUnstructuredGridFileAndACoordinateSystem_WhenThisCoordinateSystemIsWrittenToThisFile_ThenTheUnstructuredGridFileShouldContainTheCoordinateSystem(int epsg)
        {
            const string fileName = "unstructured_grid_file_net.nc";

            // Given
            var coordinateSystemFactory = new OgrCoordinateSystemFactory();
            var coordinateSystem = coordinateSystemFactory.CreateFromEPSG(epsg);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                var path = Path.Combine(tempDir, fileName);

                // Construct empty file.
                UnstructuredGridFileHelper.WriteEmptyUnstructuredGridFile(path);

                // When
                UnstructuredGridFileHelper.WriteCoordinateSystemToFile(path, coordinateSystem, true);

                // Then
                using (var uGrid = new UGrid(path, mode: GridApiDataSet.NetcdfOpenMode.nf90_nowrite))
                {
                    uGrid.Initialize();
                    Assert.That(uGrid.IsValid());
                    Assert.That(uGrid.GetNumberOfNetworks(), Is.EqualTo(0));
                    Assert.That(uGrid.CoordinateSystem, Is.Not.Null);
                    Assert.That(uGrid.CoordinateSystem.AuthorityCode, Is.EqualTo(coordinateSystem.AuthorityCode));
                }
            });
        }

        /// <summary>
        /// GIVEN
        ///   An Unstructured Grid File containing a cartesian coordinate system AND
        ///   A cartesian Coordinate System with a different AuthorityCode
        /// WHEN
        ///   This Coordinate System is written to this file
        /// THEN
        ///   The Unstructured Grid File should contain the new Coordinate System
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnUnstructuredGridFileContainingACoordinateSystemAndACoordinateSystemOfEqualType_WhenThisCoordinateSystemOfEqualTypeIsWrittenToTheGridFile_ThenThisUnstructuredGridFileShouldContainTheNewCoordinateSystem()
        {
            // Given
            const string fileName = "unstructured_grid_file_net.nc";

            var coordinateSystemFactory = new OgrCoordinateSystemFactory();
            var originalCoordinateSystem = coordinateSystemFactory.CreateFromEPSG(0);
            var newCoordinateSystem = coordinateSystemFactory.CreateFromEPSG(28992);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                var path = Path.Combine(tempDir, fileName);

                // Construct non empty file
                UnstructuredGridFileHelper.WriteEmptyUnstructuredGridFile(path);
                UnstructuredGridFileHelper.WriteCoordinateSystemToFile(path, originalCoordinateSystem, true);

                // ensure Given file is correct.
                using (var uGrid = new UGrid(path, mode: GridApiDataSet.NetcdfOpenMode.nf90_nowrite))
                {
                    uGrid.Initialize();
                    Assert.That(uGrid.IsValid());
                    Assert.That(uGrid.GetNumberOfNetworks(), Is.EqualTo(0));
                }

                AssertFileContainsNullCoordinateSystem(path);

                // When
                UnstructuredGridFileHelper.WriteCoordinateSystemToFile(path, newCoordinateSystem, true);

                // Then
                using (var uGrid = new UGrid(path, mode: GridApiDataSet.NetcdfOpenMode.nf90_nowrite))
                {
                    uGrid.Initialize();
                    Assert.That(uGrid.IsValid());
                    Assert.That(uGrid.GetNumberOfNetworks(), Is.EqualTo(0));
                    Assert.That(uGrid.CoordinateSystem, Is.Not.Null);
                    Assert.That(uGrid.CoordinateSystem.AuthorityCode, Is.EqualTo(newCoordinateSystem.AuthorityCode));
                }
            });
        }

        /// <summary>
        /// GIVEN
        ///   An Unstructured Grid File Containing A Coordinate System AND
        /// WHEN
        ///   The same coordinate system is written to the Unstructured Grid File
        /// THEN
        ///   The Unstructured Grid File should still contain the same Coordinate System
        /// </summary>
        [TestCase(4326,  TestName = "GivenAnUnstructuredGridFileContainingACartesianCoordinateSystem_WhenTheSameCoordinateSystemIsWrittenToThisFile_ThenTheFileShouldContainTheCorrectCoordinateSystem")]
        [TestCase(28992, TestName = "GivenAnUnstructuredGridFileContainingASphericalCoordinateSystem_WhenTheSameCoordinateSystemIsWrittenToThisFile_ThenTheFileShouldContainTheCorrectCoordinateSystem")]
        [Category(TestCategory.DataAccess)]
        public void GivenAnUnstructuredGridFileContainingACoordinateSystem_WhenTheSameCoordinateSystemIsWrittenToThisFile_ThenTheFileShouldContainTheCorrectCoordinateSystem(
                int epsg)
        {
            // Given
            const string fileName = "unstructured_grid_file_net.nc";

            var coordinateSystemFactory = new OgrCoordinateSystemFactory();
            var coordinateSystem = coordinateSystemFactory.CreateFromEPSG(epsg);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                var path = Path.Combine(tempDir, fileName);

                // Construct non empty file
                UnstructuredGridFileHelper.WriteEmptyUnstructuredGridFile(path);
                UnstructuredGridFileHelper.WriteCoordinateSystemToFile(path, coordinateSystem, true);

                // ensure Given file is correct.
                using (var uGrid = new UGrid(path, mode: GridApiDataSet.NetcdfOpenMode.nf90_nowrite))
                {
                    uGrid.Initialize();
                    Assert.That(uGrid.IsValid());
                    Assert.That(uGrid.GetNumberOfNetworks(), Is.EqualTo(0));
                    Assert.That(uGrid.CoordinateSystem, Is.Not.Null);
                    Assert.That(uGrid.CoordinateSystem.AuthorityCode, Is.EqualTo(coordinateSystem.AuthorityCode));
                }

                // When
                UnstructuredGridFileHelper.WriteCoordinateSystemToFile(path, coordinateSystem, true);

                // Then
                using (var uGrid = new UGrid(path, mode: GridApiDataSet.NetcdfOpenMode.nf90_nowrite))
                {
                    uGrid.Initialize();
                    Assert.That(uGrid.IsValid());
                    Assert.That(uGrid.GetNumberOfNetworks(), Is.EqualTo(0));
                    Assert.That(uGrid.CoordinateSystem, Is.Not.Null);
                    Assert.That(uGrid.CoordinateSystem.AuthorityCode, Is.EqualTo(coordinateSystem.AuthorityCode));
                }
            });
        }

        /// <summary>
        /// GIVEN
        ///   An Unstructured Grid File containing a null coordinate system.
        /// WHEN
        ///   A null coordinate system is written to this Unstructured Grid File
        /// THEN
        ///   The Unstructured Grid File should still contain a null coordinate system.
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnUnstructuredGridFileContainingANullCoordinateSystem_WhenTheSameCoordinateSystemIsWrittenToThisFile_ThenTheFileShouldContainTheCorrectCoordinateSystem()
        {
            // Given
            const string fileName = "unstructured_grid_file_net.nc";

            var coordinateSystemFactory = new OgrCoordinateSystemFactory();
            var coordinateSystem = coordinateSystemFactory.CreateFromEPSG(0);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                var path = Path.Combine(tempDir, fileName);

                // Construct non empty file
                UnstructuredGridFileHelper.WriteEmptyUnstructuredGridFile(path);
                UnstructuredGridFileHelper.WriteCoordinateSystemToFile(path, coordinateSystem, true);

                // ensure Given file is correct.
                using (var uGrid = new UGrid(path, mode: GridApiDataSet.NetcdfOpenMode.nf90_nowrite))
                {
                    uGrid.Initialize();
                    Assert.That(uGrid.IsValid());
                    Assert.That(uGrid.GetNumberOfNetworks(), Is.EqualTo(0));
                }
                AssertFileContainsNullCoordinateSystem(path);

                // When
                UnstructuredGridFileHelper.WriteCoordinateSystemToFile(path, coordinateSystem, true);

                // Then
                using (var uGrid = new UGrid(path, mode: GridApiDataSet.NetcdfOpenMode.nf90_nowrite))
                {
                    uGrid.Initialize();
                    Assert.That(uGrid.IsValid());
                    Assert.That(uGrid.GetNumberOfNetworks(), Is.EqualTo(0));
                }

                AssertFileContainsNullCoordinateSystem(path);
            });
        }

        // This test will have to be adjusted when the follow up of issue D3DFMIQ-512 is implemented.
        // Currently two variables will be written to the Grid file, instead of removing one, and 
        // adding the right one. This test confirms that at least the two variables both contain the 
        // same ESPG coordinate system.
        /// <summary>
        /// GIVEN
        ///   An unstructured grid file containing a coordinate system of type A AND
        ///   A coordinate system of type B
        /// WHEN
        ///   The coordinate system of type B is written to the Unstructured Grid File is written to file
        /// THEN
        ///   Both variables should contain the Authority Code of coordinate system of type B
        /// </summary>
        [TestCase(4326, 28992)]
        [TestCase(28992, 4326)]
        [Category(TestCategory.DataAccess)]
        public void GivenAnUnstructuredFileContainingACoordinateSystemAndACoordinateSystemOfADifferentType_WhenThisCoordinateSystemIsWrittenToTheUnstructuredGridFile_ThenBothVariablesAreSetToTheExpectedEPSG(
            int originalEpsg, int newEpsg)
        {
            // Given
            const string fileName = "unstructured_grid_file_net.nc";

            var coordinateSystemFactory = new OgrCoordinateSystemFactory();
            var originalCoordinateSystem = coordinateSystemFactory.CreateFromEPSG(originalEpsg);
            var newCoordinateSystem = coordinateSystemFactory.CreateFromEPSG(newEpsg);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                var path = Path.Combine(tempDir, fileName);

                // Construct non empty file
                UnstructuredGridFileHelper.WriteEmptyUnstructuredGridFile(path);
                UnstructuredGridFileHelper.WriteCoordinateSystemToFile(path, originalCoordinateSystem, true);

                // ensure Given file is correct.
                using (var uGrid = new UGrid(path, mode: GridApiDataSet.NetcdfOpenMode.nf90_nowrite))
                {
                    uGrid.Initialize();
                    Assert.That(uGrid.IsValid());
                    Assert.That(uGrid.GetNumberOfNetworks(), Is.EqualTo(0));
                    Assert.That(uGrid.CoordinateSystem, Is.Not.Null);
                    Assert.That(uGrid.CoordinateSystem.AuthorityCode, Is.EqualTo(originalCoordinateSystem.AuthorityCode));
                }

                // When
                UnstructuredGridFileHelper.WriteCoordinateSystemToFile(path, newCoordinateSystem, true);

                // Then
                using (var uGrid = new UGrid(path, mode: GridApiDataSet.NetcdfOpenMode.nf90_nowrite))
                {
                    uGrid.Initialize();
                    Assert.That(uGrid.IsValid());
                    Assert.That(uGrid.GetNumberOfNetworks(), Is.EqualTo(0));
                    Assert.That(uGrid.CoordinateSystem, Is.Not.Null);
                    Assert.That(uGrid.CoordinateSystem.AuthorityCode, Is.EqualTo(newCoordinateSystem.AuthorityCode));
                }

                // Check both variables exists, and point to the correct authority code.
                NetCdfFile netCdfFile = null;
                try
                {
                    netCdfFile = NetCdfFile.OpenExisting(path);
                    var projectedCoordinateSystemVariable = netCdfFile.GetVariableByName("projected_coordinate_system");
                    Assert.That(projectedCoordinateSystemVariable, Is.Not.Null);
                    var pcsVarAttributes = netCdfFile.GetAttributes(projectedCoordinateSystemVariable);
                    Assert.That(pcsVarAttributes.Keys.Contains("epsg"));
                    Assert.That((int) pcsVarAttributes["epsg"], Is.EqualTo((int)newCoordinateSystem.AuthorityCode));

                    var wgs84Variable = netCdfFile.GetVariableByName("wgs84");
                    Assert.That(wgs84Variable, Is.Not.Null);
                    var wgsVarAttributes = netCdfFile.GetAttributes(wgs84Variable);
                    Assert.That(wgsVarAttributes.Keys.Contains("epsg"));
                    Assert.That((int) wgsVarAttributes["epsg"], Is.EqualTo((int)newCoordinateSystem.AuthorityCode));
                }
                finally
                {
                    netCdfFile?.Close();
                }
            });
        }


        /// <summary>
        /// Assert the Unstructured Grid File located at <paramref name="path"/>
        /// contains a null coordinate system.
        /// </summary>
        /// <param name="path">The path at which the Unstructured Grid File is located</param>
        private static void AssertFileContainsNullCoordinateSystem(string path)
        {
            NetCdfFile netCdfFile = null;
            try
            {
                netCdfFile = NetCdfFile.OpenExisting(path);

                Assert.That(netCdfFile.GetVariableByName("wgs84"), Is.Null);

                var projectedCoordinateSystemVariable =
                    netCdfFile.GetVariableByName("projected_coordinate_system");
                Assert.That(projectedCoordinateSystemVariable, Is.Not.Null);
                Assert.That(netCdfFile.GetAttributeValue(projectedCoordinateSystemVariable, "epsg"), Is.EqualTo("0"));
            }
            finally
            {
                netCdfFile?.Close();
            }
        }
    }
}
