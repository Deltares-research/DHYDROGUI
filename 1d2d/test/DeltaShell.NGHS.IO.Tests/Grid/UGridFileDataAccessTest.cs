using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Link1d2d;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using Deltares.Infrastructure.API.Logging;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net.Core;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap;
using SharpMap.Converters.WellKnownText;
using SharpMap.Extensions.CoordinateSystems;
using SharpMapTestUtils;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class UGridFileDataAccessTest
    {
        [TestCase("fileDoesNotExist.nc", false)]
        [TestCase(@"ugrid\Custom_Ugrid.nc", true)]
        [TestCase(@"nonUgrid\TAK3_net.nc", true)]
        public void TestLoadFromFile(string filePath, bool gridShouldLoad)
        {
            string testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), filePath);
            Assert.AreEqual(gridShouldLoad, File.Exists(testFilePath));

            var grid = new UnstructuredGrid();
            using (var ugridFile = new UGridFile(testFilePath))
                ugridFile.SetUnstructuredGrid(grid);
            Assert.AreEqual(gridShouldLoad, !grid.IsEmpty);
        }

        [Test]
        public void CheckUGridFileDeltaresVersion()
        {
            // Setup
            string filePath = TestHelper.CreateLocalCopySingleFile(Path.Combine(TestHelper.GetTestDataDirectory(), "ugrid", "Custom_Ugrid.nc"));

            // Call
            void Call()
            {
                using (var ugridFile = new UGridFile(filePath))
                    ugridFile.IsUGridFile();
            }

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
            void Call()
            {
                using (var ugridFile = new UGridFile(filePath))
                    ugridFile.IsUGridFile();
            }

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
                () => Assert.Throws<FileNotFoundException>(() => new RemoteUGridApi().Open(filePath))
                , $"While reading Deltares netcdf file version type from file {filePath} we encounter the following problem:");
        }

        [TestCase(@"ugrid\BedLevelValues_NodesAndFaces.nc", BedLevelLocation.Faces)]
        [TestCase(@"ugrid\BedLevelValues_NodesAndFaces.nc", BedLevelLocation.FacesMeanLevFromNodes)]
        [TestCase(@"ugrid\BedLevelValues_NodesAndFaces.nc", BedLevelLocation.NodesMaxLev)]
        [TestCase(@"ugrid\BedLevelValues_NodesAndFaces.nc", BedLevelLocation.NodesMeanLev)]
        [TestCase(@"ugrid\BedLevelValues_NodesAndFaces.nc", BedLevelLocation.NodesMinLev)]
        [TestCase(@"ugrid\BedLevelValues_NodesAndFaces.nc", BedLevelLocation.NodesMeanLev)]
        public void TestReadZValues(string filePath, BedLevelLocation location)
        {
            string testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), filePath);
            Assert.IsTrue(File.Exists(testFilePath));
            using (var uGridFile = new UGridFile(testFilePath))
            {
                double[] zValues = uGridFile.ReadZValues(location);
                Assert.IsTrue(zValues.Length > 0);
                Assert.IsTrue(zValues.All(v => v > 0.0));
            }
        }

        [Test]
        public void TestReadZValues_DoesNotThrowForNoZValuesInFile()
        {
            string testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), @"ugrid\Custom_Ugrid.nc");
            Assert.IsTrue(File.Exists(testFilePath));

            using (var uGridFile = new UGridFile(testFilePath))
            {
                double[] zValues = uGridFile.ReadZValues(BedLevelLocation.Faces);
                Assert.AreEqual(0, zValues.Length);
            }
        }

        [Test]
        public void TestReadZValues_GivesWarningForEdgeLocations()
        {
            string testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), @"ugrid\BedLevelValues_NodesAndFaces.nc");
            Assert.IsTrue(File.Exists(testFilePath));

            var zValues = new double[0];

            using (var uGridFile = new UGridFile(testFilePath))
            {
                TestHelper.AssertAtLeastOneLogMessagesContains(() =>
                                                                   zValues = uGridFile.ReadZValues(BedLevelLocation.CellEdges),
                                                               Resources.ReadZValues_Unable_to_read_z_values_at_this_location__CellEdges_are_not_currently_supported);

                Assert.AreEqual(0, zValues.Length);
            }
        }

        [Test]
        public void TestReadZValues_GivesWarningForNonUgridFiles()
        {
            string testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), @"nonUgrid\TAK3_net.nc");
            Assert.IsTrue(File.Exists(testFilePath));

            var zValues = new double[0];

            using (var uGridFile = new UGridFile(testFilePath))
            {
                TestHelper.AssertAtLeastOneLogMessagesContains(() =>
                                                                   zValues = uGridFile.ReadZValues(BedLevelLocation.CellEdges),
                                                               string.Format(Resources.ReadZValues_Unable_to_read_z_values_from_file___0___file_is_not_UGrid_convention, testFilePath));

                Assert.AreEqual(0, zValues.Length);
            }
        }

        [TestCase(@"ugrid\Custom_Ugrid.nc", BedLevelLocation.Faces)]
        [TestCase(@"ugrid\Custom_Ugrid.nc", BedLevelLocation.FacesMeanLevFromNodes)]
        [TestCase(@"ugrid\Custom_Ugrid.nc", BedLevelLocation.NodesMaxLev)]
        [TestCase(@"ugrid\Custom_Ugrid.nc", BedLevelLocation.NodesMeanLev)]
        [TestCase(@"ugrid\Custom_Ugrid.nc", BedLevelLocation.NodesMinLev)]
        [TestCase(@"nonUgrid\TAK3_net.nc", BedLevelLocation.NodesMeanLev)]
        public void TestWriteZValues_DoesNotThrowForSupportedLocations(string filePath, BedLevelLocation location)
        {
            string testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), filePath);
            Assert.IsTrue(File.Exists(testFilePath));

            string localtestFile = TestHelper.CreateLocalCopy(testFilePath);
            var grid = new UnstructuredGrid();
            using (var ugridFile = new UGridFile(localtestFile))
                ugridFile.SetUnstructuredGrid(grid);

            var zValues = new double[0];

            switch (location)
            {
                case BedLevelLocation.Faces:
                case BedLevelLocation.FacesMeanLevFromNodes:
                    zValues = Enumerable.Repeat(123.456, grid.Cells.Count).ToArray();
                    break;
                case BedLevelLocation.NodesMeanLev:
                case BedLevelLocation.NodesMinLev:
                case BedLevelLocation.NodesMaxLev:
                    zValues = Enumerable.Repeat(123.456, grid.Vertices.Count).ToArray();
                    break;
            }

            using (var ugridFile = new UGridFile(localtestFile))
                ugridFile.WriteZValues(location, zValues);
            FileUtils.DeleteIfExists(localtestFile);
        }

        [TestCase(BedLevelLocation.Faces)]
        [TestCase(BedLevelLocation.NodesMeanLev)]
        public void TestWriteZValues_SupportedLocations(BedLevelLocation location)
        {
            string testFilePath = TestHelper.GetTestWorkingDirectoryGeneratedTestFilePath(".nc");

            FileUtils.DeleteIfExists(testFilePath);

            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 3, 100, 100);
            var zValues = new double[0];

            switch (location)
            {
                case BedLevelLocation.Faces:
                case BedLevelLocation.FacesMeanLevFromNodes:
                    zValues = Enumerable.Repeat(123.456, grid.Cells.Count).ToArray();
                    break;
                case BedLevelLocation.NodesMeanLev:
                case BedLevelLocation.NodesMinLev:
                case BedLevelLocation.NodesMaxLev:
                    zValues = Enumerable.Repeat(123.456, grid.Vertices.Count).ToArray();
                    break;
            }

            using (var ugridFile = new UGridFile(testFilePath))
            {
                var api = Substitute.For<IUGridApi>();
                api.GetMeshIdsByMeshType(UGridMeshType.Mesh2D).Returns(new[] { 1 });
                ugridFile.Api = api;
                ugridFile.InitializeMetaData("abc", "dummy", "1");
                ugridFile.WriteGridToFile(grid, null, null, null, location, zValues);
                api.Received(1).CreateFile(testFilePath, Arg.Any<FileMetaData>());
                api.Received(1).WriteMesh2D(Arg.Any<Disposable2DMeshGeometry>());
                api.Received(1).GetMeshIdsByMeshType(UGridMeshType.Mesh2D);
                api.Received(1).SetVariableValues(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<GridLocationType>(), Arg.Any<double[]>(), Arg.Any<double>());
                api.Received(1).Close();
            }

            FileUtils.DeleteIfExists(testFilePath);
        }

        [Test]
        public void TestWriteZValues_GivesWarningForEdgeLocations()
        {
            // arrange
            string testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), @"ugrid\Custom_Ugrid.nc");
            var grid = new UnstructuredGrid();
            var location = BedLevelLocation.CellEdges;
            double[] zValues = Enumerable.Repeat(123.456, grid.Edges.Count).ToArray();
            var logHandler = Substitute.For<ILogHandler>();

            // act
            string localtestFile = TestHelper.CreateLocalCopy(testFilePath);
            using (var ugridFile = new UGridFile(localtestFile))
            {
                ugridFile.SetUnstructuredGrid(grid);
                ugridFile.WriteZValues(location, zValues, logHandler);
            }

            // assert
            logHandler.Received(1).ReportWarning(Resources.ReadZValues_Unable_to_read_z_values_at_this_location__CellEdges_are_not_currently_supported);
            logHandler.Received(1).LogReport();
            FileUtils.DeleteIfExists(localtestFile);
        }

        [TestCase("fileDoesNotExist.nc", false, null)]
        [TestCase(@"ugrid\Custom_Ugrid.nc", true, 4326L)]  // WGS84
        [TestCase(@"nonUgrid\small_net.nc", true, 28992L)] // Amersfoort / RD New
        public void TestGetCoordinateSystem(string filePath, bool testFileExists, long? expectedResult)
        {
            string testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), filePath);
            Assert.AreEqual(testFileExists, File.Exists(testFilePath));

            long? coordinateSystemAuthorityCode;
            using (var ugridFile = new UGridFile(testFilePath))
                coordinateSystemAuthorityCode = ugridFile.ReadCoordinateSystem()?.AuthorityCode;
            Assert.AreEqual(expectedResult, coordinateSystemAuthorityCode);
        }

        [TestCase(@"ugrid\Custom_Ugrid.nc")]
        [TestCase(@"nonUgrid\TAK3_net.nc")]
        public void TestSetCoordinateSystem(string filePath)
        {
            string testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), filePath);
            Assert.IsTrue(File.Exists(testFilePath));

            string localtestFile = TestHelper.CreateLocalCopy(testFilePath);
            var coordinateSystemFactory = new OgrCoordinateSystemFactory();

            ICoordinateSystem coordinateSystem = coordinateSystemFactory.CreateFromEPSG(28992); // Amersfoort / RD New
            using (var ugridFile = new UGridFile(localtestFile))
                ugridFile.WriteCoordinateSystem(coordinateSystem);

            coordinateSystem = coordinateSystemFactory.CreateFromEPSG(4326); // WGS84
            using (var ugridFile = new UGridFile(localtestFile))
                ugridFile.WriteCoordinateSystem(coordinateSystem);

            FileUtils.DeleteIfExists(localtestFile);
        }

        [Test]
        public void TestWriteGridToFile_DoesNotThrowForExistingFile()
        {
            string testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), @"nonUgrid\TAK3_net.nc");
            Assert.IsTrue(File.Exists(testFilePath));

            string localtestFile = TestHelper.CreateLocalCopy(testFilePath);
            using (var ugridFile = new UGridFile(localtestFile))
                ugridFile.WriteGridToFile(new UnstructuredGrid(), new HydroNetwork(), new Discretization(), new List<ILink1D2D>(), BedLevelLocation.NodesMaxLev, new double[] {});

            FileUtils.DeleteIfExists(localtestFile);
        }

        [Test]
        public void TestWriteGridToFile_CreateNewFileForNonExistingFile()
        {
            string testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "fileDoesNotExist.nc");
            Assert.IsFalse(File.Exists(testFilePath));

            string localtestFilePath = TestHelper.CreateLocalCopy(testFilePath);
            Assert.IsFalse(File.Exists(testFilePath));

            UnstructuredGrid unstructuredGrid = UnstructuredGridTestHelper.GenerateRegularGrid(5, 5, 10, 10);
            IHydroNetwork hydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(4, true);

            var networkDiscretization = new Discretization()
            {
                Network = hydroNetwork,
                Name = "Mydiscreatisation"
            };
            HydroNetworkHelper.GenerateDiscretization(networkDiscretization, true, false, 0.5, false, 1.0, false, false, true, 2, null);

            var link1D2Ds = new List<ILink1D2D>() { { new Link1D2D(1, 1, "my link") { TypeOfLink = LinkStorageType.Embedded } } };

            double[] zValues = Enumerable.Range(1, unstructuredGrid.Vertices.Count).Select(Convert.ToDouble).ToArray();
            using (var ugridFile = new UGridFile(localtestFilePath))
            {
                ugridFile.InitializeMetaData("myName", "myPlugin", "myVersion");
                ugridFile.WriteGridToFile(unstructuredGrid, hydroNetwork, networkDiscretization, link1D2Ds, BedLevelLocation.NodesMaxLev, zValues);
            }

            Assert.IsTrue(File.Exists(localtestFilePath));

            FileUtils.DeleteIfExists(localtestFilePath);
        }

        [TestCase(@"ugrid\Custom_Ugrid.nc")]
        [TestCase(@"nonUgrid\TAK3_net.nc")]
        public void TestRewriteGridCoordinates(string filePath)
        {
            string testFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), filePath);
            Assert.IsTrue(File.Exists(testFilePath));

            string localtestFile = TestHelper.CreateLocalCopy(testFilePath);
            var grid = new UnstructuredGrid();
            using (var ugridFile = new UGridFile(localtestFile))
                ugridFile.SetUnstructuredGrid(grid);

            foreach (Coordinate coordinate in grid.Vertices)
            {
                coordinate.X += 1.0;
                coordinate.Y -= 1.0;
            }

            using (var ugridFile = new UGridFile(localtestFile))
                ugridFile.RewriteGridCoordinates(grid);

            FileUtils.DeleteIfExists(localtestFile);
        }

        [Test]
        public void GivenUGridFileHelper_WriteRead_ShouldGiveTheSameCoordinates()
        {
            //Arrange
            IHydroNetwork network = HydroNetworkHelper.GetSnakeHydroNetwork(2);
            var mesh1d = new Discretization();
            HydroNetworkHelper.GenerateDiscretization(mesh1d, true, true, 0, true, 0, true, true, true, 100);

            string path = TestHelper.GetTestWorkingDirectoryGeneratedTestFilePath("nc");

            // Act
            using (var ugridFile = new UGridFile(path))
            {
                ugridFile.InitializeMetaData("model1", "myPlugin", "1.0");
                ugridFile.WriteGridToFile(null, network, mesh1d, null, BedLevelLocation.Faces, null);
            }

            var readNetwork = new HydroNetwork();
            var readMesh1d = new Discretization();
            IConvertedUgridFileObjects convertedUGridFileObjects = new ConvertedUgridFileObjects()
            {
                Discretization = readMesh1d,
                HydroNetwork = readNetwork
            };

            using (var ugridFile = new UGridFile(path))
                ugridFile.ReadNetFileDataIntoModel(convertedUGridFileObjects);

            // Assert
            IBranch networkBranch = network.Branches[0];
            IBranch readNetworkBranch = readNetwork.Branches[0];

            Assert.AreEqual(networkBranch.Geometry.Coordinates, readNetworkBranch.Geometry.Coordinates);
            Assert.AreEqual(networkBranch.Length, readNetworkBranch.Length);
            Assert.AreEqual(network.Nodes[0].Geometry.Coordinate, readNetwork.Nodes[0].Geometry.Coordinate);

            Assert.AreEqual(mesh1d.GetLocationsForBranch(networkBranch), readMesh1d.GetLocationsForBranch(readNetworkBranch));
        }

        [Test]
        public void GivenUGridFileHelper_ReadingNetworkWithCoordinateSystem_ShouldSetBranchesGeodeticLength()
        {
            //Arrange
            string path = TestHelper.GetTestFilePath(@"ugrid\ReadGeodeticLengthTest.nc");
            var network = new HydroNetwork();
            var discretization = new Discretization();

            if (Map.CoordinateSystemFactory == null)
            {
                Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
            }

            IConvertedUgridFileObjects convertedUGridFileObjects = new ConvertedUgridFileObjects()
            {
                Discretization = discretization,
                HydroNetwork = network
            };

            // Act
            using (var ugridFile = new UGridFile(path))
                ugridFile.ReadNetFileDataIntoModel(convertedUGridFileObjects);

            // Assert
            Assert.NotNull(network.CoordinateSystem);

            IBranch branch = network.Branches[0];
            Assert.False(double.IsNaN(branch.GeodeticLength));
            Assert.AreNotEqual(branch.Geometry.Length, branch.GeodeticLength);
        }

        [Test]
        public void GivenUGridFileHelper_ReadingNetworkWithChainageBeyondBranch_ShouldSetChainageToGeodeticLength()
        {
            //Arrange
            string path = TestHelper.GetTestFilePath(@"ugrid\ChainageBeyondBranch.nc");
            var network = new HydroNetwork();
            var discretization = new Discretization();

            if (Map.CoordinateSystemFactory == null)
            {
                Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
            }

            IConvertedUgridFileObjects convertedUGridFileObjects = new ConvertedUgridFileObjects()
            {
                Discretization = discretization,
                HydroNetwork = network
            };

            // Act
            using (var ugridFile = new UGridFile(path))
                ugridFile.ReadNetFileDataIntoModel(convertedUGridFileObjects);

            // Assert
            Assert.NotNull(discretization.Locations);
            Assert.IsFalse(discretization.Locations.Values.Any(x => x.Chainage > x.Branch.Length));
        }

        [Test]
        public void Given1D2DModelWithLinkOnCalculationPointLocationPastChannelLength_WhenSaveModel_ThenModelSavedButErrorMessageIsLogged()
        {
            //arrange
            var network = new HydroNetwork();
            var node1 = new HydroNode("n1") { Geometry = new Point(0, 0) };
            network.Nodes.Add(node1);
            var node2 = new HydroNode("n2") { Geometry = new Point(100, 0) };
            network.Nodes.Add(node2);
            var channel1 = new Channel("c1", node1, node2)
            {
                Geometry = GeometryFromWKT.Parse("LINESTRING(0 0, 100 0)"),
            };
            network.Branches.Add(channel1);

            var discretization = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsAndConnectedBranchesWithoutLocationOnThemFullyCovered
            };
            discretization.Locations.AddValues(Enumerable.Range(0, 13).Select(i => new NetworkLocation(channel1, i * 10)));
            IEnumerable<string> otherDiscretizationPointNames = new[] { discretization.Locations.Values[discretization.Locations.Values.Count - 3].Name }
                .Plus(discretization.Locations.Values[discretization.Locations.Values.Count - 2].Name);

            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(5, 5, 10, 10);
            double[] zValues = Enumerable.Range(1, grid.Vertices.Count).Select(Convert.ToDouble).ToArray();
            var correctLink = new Link1D2D(0, 1, "link1");
            var faultyLink = new Link1D2D(discretization.Locations.Values.Count - 1, 5, "linkFaulty");
            string testFilePath = TestHelper.GetTestWorkingDirectoryGeneratedTestFilePath(".nc");

            FileUtils.DeleteIfExists(testFilePath);
            var logHandler = Substitute.For<ILogHandler>();
            string expectedError = string.Format(Resources.UGridFileHelper_ValidateMesh1DSourceLocationsOnlyExistOnce_ErrorMessage_part1, faultyLink.Name, discretization.Locations.Values[discretization.Locations.Values.Count - 1].Name, faultyLink.FaceIndex) +
                                   Environment.NewLine +
                                   string.Format(Resources.UGridFileHelper_ValidateMesh1DSourceLocationsOnlyExistOnce_ErrorMessage_part2, string.Join(", ", otherDiscretizationPointNames));

            // Act
            using (var ugridFile = new UGridFile(testFilePath))
            {
                ugridFile.InitializeMetaData("abc", "dummy", "1");
                ugridFile.WriteGridToFile(grid, network, discretization, new ILink1D2D[] { correctLink, faultyLink }, BedLevelLocation.NodesMaxLev, zValues, logHandler);
            }

            // Assert
            logHandler.Received(1).ReportError(expectedError);
            logHandler.Received(1).LogReport();
            Assert.That(File.Exists(testFilePath), Is.True);

            FileUtils.DeleteIfExists(testFilePath);
        }

        [Test]
        public void Given1D2DModelWithValidLinksAndOnCalculationPointLocationPastChannelLength_WhenSaveModel_ThenModelSavedAndNoLinkErrorMessageIsLogged()
        {
            //arrange
            var network = new HydroNetwork();
            var node1 = new HydroNode("n1") { Geometry = new Point(0, 0) };
            network.Nodes.Add(node1);
            var node2 = new HydroNode("n2") { Geometry = new Point(100, 0) };
            network.Nodes.Add(node2);
            var channel1 = new Channel("c1", node1, node2)
            {
                Geometry = GeometryFromWKT.Parse("LINESTRING(0 0, 100 0)"),
            };
            network.Branches.Add(channel1);

            var discretization = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsAndConnectedBranchesWithoutLocationOnThemFullyCovered
            };
            discretization.Locations.AddValues(Enumerable.Range(0, 13).Select(i => new NetworkLocation(channel1, i * 10)));
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(5, 5, 10, 10);
            double[] zValues = Enumerable.Range(1, grid.Vertices.Count).Select(Convert.ToDouble).ToArray();
            var correctLink = new Link1D2D(0, 1, "link1");
            var otherCorrectLink = new Link1D2D(5, 5, "linkAlsoCorrect");
            string testFilePath = TestHelper.GetTestWorkingDirectoryGeneratedTestFilePath(".nc");

            FileUtils.DeleteIfExists(testFilePath);

            //act
            string renderedMessages = string.Join(Environment.NewLine, TestHelper.GetAllRenderedMessages(() =>
            {
                using (var ugridFile = new UGridFile(testFilePath))
                {
                    ugridFile.InitializeMetaData("abc", "dummy", "1");
                    ugridFile.WriteGridToFile(grid, network, discretization, new ILink1D2D[] { correctLink, otherCorrectLink }, BedLevelLocation.NodesMaxLev, zValues);
                }
            }));

            StringAssert.DoesNotContain(Resources.UGridFileHelper_ValidateMesh1DSourceLocationsOnlyExistOnce_ErrorMessageHeader, renderedMessages);

            Assert.That(File.Exists(testFilePath), Is.True);

            FileUtils.DeleteIfExists(testFilePath);
        }
    }
}