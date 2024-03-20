using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class UGridTest
    {
        private const string UGRID_TEST_FILE = @"ugrid\Custom_Ugrid.nc"; //@"ugrid\c090_wetbed_map.nc";
        private const string UGRID_MAP_TEST_FILE = @"ugrid\Custom_Ugrid_map.nc";
        private const string DUMMY_TEST_FILE = @"ugrid\Dummy.nc";

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallValidateFileIsUgridWithCallToOpenClose()
        {
            string testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            string localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            using (var uGrid = new UGrid(localCopyOfTestFile))
            {
                uGrid.Initialize();
                Assert.That(uGrid.IsValid(), Is.True);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestIfFilenameIsNullUgridShouldThrowException()
        {
            Assert.That(() =>
            {
                using (var uGrid = new UGrid(null))
                {
                    uGrid.Initialize();
                }
            }, Throws.Exception);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestIfFilenameIsEmptyStringUgridShouldThrowException()
        {
            Assert.That(() =>
            {
                using (var uGrid = new UGrid(string.Empty))
                {
                    uGrid.Initialize();
                }
            }, Throws.Exception);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestIfFileDoesNotExistUgridShouldThrowException()
        {
            Assert.That(() =>
            {
                using (var uGrid = new UGrid("thisFileDoesntExist.nc"))
                {
                    uGrid.Initialize();
                }
            }, Throws.Exception);
        }

        [Test]
        [Ignore("doesn't work yet, should check with arthur van dam")]
        [Category(TestCategory.DataAccess)]
        public void TestCallValidateFileIsUgridWithCallToApi()
        {
            string testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            string localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            using (var uGrid = new UGrid(localCopyOfTestFile))
            {
                Assert.IsTrue(uGrid.IsValid());
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallGetDataSetConvention()
        {
            string testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            string localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            using (var uGrid = new UGrid(localCopyOfTestFile))
            {
                uGrid.Initialize();
                Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_UGRID, uGrid.GetDataSetConvention());
            }
        }

        [Test]
        public void TestDefaultGlobalMetaData()
        {
            string testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            string localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            var metadata = new UGridGlobalMetaData();
            using (var uGrid = new UGrid(localCopyOfTestFile))
            {
                Assert.AreEqual(metadata, uGrid.GlobalMetaData);
            }
        }

        [Test]
        public void TestValidGlobalMetaData()
        {
            string testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            string localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            var metadata = new UGridGlobalMetaData("MyModel", "MySource", "MyVersion");
            using (var uGrid = new UGrid(localCopyOfTestFile, metadata))
            {
                Assert.AreEqual(metadata, uGrid.GlobalMetaData);
            }
        }

        [Test]
        public void TestNullGlobalMetaData()
        {
            string testFilePath = TestHelper.GetTestFilePath(DUMMY_TEST_FILE);
            string localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            FileUtils.DeleteIfExists(localCopyOfTestFile);

            using (var uGrid = new UGrid(localCopyOfTestFile, null))
            {
                Assert.That(() => uGrid.CreateFile(), Throws.InstanceOf<NullReferenceException>());
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallGetDataSetConventionViaApi()
        {
            string testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            string localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            using (var uGrid = new UGrid(localCopyOfTestFile))
            {
                Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_UGRID, uGrid.GetDataSetConvention());
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallRewriteGridCoordinates()
        {
            string testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            string localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);

            IList<double> currentXValues, currentYValues;
            using (var ncFile = new NetCdfFileWrapper(localCopyOfTestFile))
            {
                currentXValues = ncFile.GetValues1D<double>("mesh2d_node_x");
                currentYValues = ncFile.GetValues1D<double>("mesh2d_node_y");
            }

            double[] newXValues = currentXValues.Select(x =>
            {
                x = 123.456;
                return x;
            }).ToArray();
            double[] newYValues = currentYValues.Select(y =>
            {
                y = 654.321;
                return y;
            }).ToArray();

            var grid = new UnstructuredGrid();
            for (var i = 0; i < newXValues.Length; i++)
            {
                grid.Vertices.Add(new Coordinate(newXValues[i], newYValues[i]));
            }

            using (var uGrid = new UGrid(localCopyOfTestFile, GridApiDataSet.NetcdfOpenMode.nf90_write))
            {
                try
                {
                    uGrid.RewriteGridCoordinatesForMeshId(1, grid.Vertices.Select(v => v.X).ToArray(), grid.Vertices.Select(v => v.Y).ToArray());
                }
                catch (Exception)
                {
                    Assert.Fail("Couldn't write coordinates again.");
                }
            }

            IList<double> xValues, yValues;
            using (var ncFile = new NetCdfFileWrapper(localCopyOfTestFile))
            {
                xValues = ncFile.GetValues1D<double>("mesh2d_node_x");
                yValues = ncFile.GetValues1D<double>("mesh2d_node_y");
            }

            Assert.IsTrue(xValues.All(z => Math.Abs(z - 123.456) < 0.0001));
            Assert.IsTrue(yValues.All(z => Math.Abs(z - 654.321) < 0.0001));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallSetZValues_Nodes()
        {
            string testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            string localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            IList<double> currentZValues;
            using (var ncFile = new NetCdfFileWrapper(localCopyOfTestFile))
            {
                currentZValues = ncFile.GetValues1D<double>("mesh2d_node_z");
            }

            double[] newZValues = currentZValues.Select(z =>
            {
                z = 123.456;
                return z;
            }).ToArray();

            using (var uGrid = new UGrid(localCopyOfTestFile, GridApiDataSet.NetcdfOpenMode.nf90_write))
            {
                uGrid.WriteZValuesAtNodesForMeshId(1, newZValues);
            }

            IList<double> zValues;
            using (var ncFile = new NetCdfFileWrapper(localCopyOfTestFile))
            {
                zValues = ncFile.GetValues1D<double>("mesh2d_node_z");
            }

            Assert.That(zValues.All(z => Math.Abs(z - 123.456) < 0.0001), Is.True);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallSetZValues_Faces()
        {
            string testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            string localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);

            var newZValues = new[]
            {
                123.456,
                123.456
            }; // Test file has 2 faces
            using (var uGrid = new UGrid(localCopyOfTestFile, GridApiDataSet.NetcdfOpenMode.nf90_write))
            {
                uGrid.WriteZValuesAtFacesForMeshId(1, newZValues);
            }

            IList<double> zValues;
            using (var ncFile = new NetCdfFileWrapper(localCopyOfTestFile))
            {
                zValues = ncFile.GetValues1D<double>("mesh2d_face_z");
            }

            Assert.NotNull(zValues); // variable should have been created by DefineVariable
            Assert.That(zValues.All(z => Math.Abs(z - 123.456) < 0.0001), Is.True);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallNumberOfMesh()
        {
            string testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            string localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);

            using (var uGrid = new UGrid(localCopyOfTestFile))
            {
                Assert.That(uGrid.GetNumberOf2DMeshes(), Is.EqualTo(1));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallNameOfMesh()
        {
            string testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            string localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            using (var uGrid = new UGrid(localCopyOfTestFile))
            {
                Assert.That(uGrid.GetMeshName(1), Is.EqualTo("mesh2d"));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallNumberOfNodesInFirstMesh()
        {
            string testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            string localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            using (var uGrid = new UGrid(localCopyOfTestFile))
            {
                Assert.That(uGrid.GetNumberOf2DMeshes(), Is.EqualTo(1));
                Assert.That(uGrid.GetNumberOfNodesForMeshId(1), Is.EqualTo(5));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallNumberOfEdgesInFirstMesh()
        {
            string testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            string localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            using (var uGrid = new UGrid(localCopyOfTestFile))
            {
                Assert.That(uGrid.GetNumberOf2DMeshes(), Is.EqualTo(1));
                Assert.That(uGrid.GetNumberOfEdgesForMeshId(1), Is.EqualTo(6));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallNumberOfFacesInFirstMesh()
        {
            string testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            string localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            using (var uGrid = new UGrid(localCopyOfTestFile))
            {
                Assert.That(uGrid.GetNumberOf2DMeshes(), Is.EqualTo(1));
                Assert.That(uGrid.GetNumberOfFacesForMeshId(1), Is.EqualTo(2));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallNumberOfMaxFaceNodesInFirstMesh()
        {
            string testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            string localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            using (var uGrid = new UGrid(localCopyOfTestFile))
            {
                Assert.That(uGrid.GetNumberOf2DMeshes(), Is.EqualTo(1));
                Assert.That(uGrid.GetNumberOfMaxFaceNodesForMeshId(1), Is.EqualTo(4));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallNodeCoordinatesInFirstMesh()
        {
            string testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            string localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            using (var uGrid = new UGrid(localCopyOfTestFile))
            {
                var meshId = 1;
                uGrid.GetAllNodeCoordinatesForMeshId(meshId);
                Assert.That(uGrid.GetNumberOf2DMeshes(), Is.EqualTo(1));
                Assert.That(uGrid.NodeCoordinatesByMeshId[meshId - 1], Is.Not.Null);
                Assert.That(uGrid.NodeCoordinatesByMeshId[meshId - 1].Count(), Is.EqualTo(uGrid.GetNumberOfNodesForMeshId(meshId)));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallFaceNodesInFirstMesh()
        {
            string testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            string localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            using (var uGrid = new UGrid(localCopyOfTestFile))
            {
                Assert.AreEqual(1, uGrid.GetNumberOf2DMeshes());
                uGrid.GetFaceNodesForMeshId(1);
                Assert.That(uGrid.FaceNodesByMeshId[0], Is.Not.Null);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallNumberOfNamesInMeshAtLocationFaces()
        {
            string testFilePath =
                TestHelper.GetTestFilePath(UGRID_MAP_TEST_FILE);
            string localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            const int meshId = 1;
            var locationType = GridApiDataSet.LocationType.UG_LOC_FACE;
            using (var uGrid = new UGrid(localCopyOfTestFile))
            {
                Assert.That(uGrid.NumberOfNamesForLocationType(meshId, locationType), Is.EqualTo(2));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallNamesInMeshAtLocationFaces()
        {
            string testFilePath =
                TestHelper.GetTestFilePath(UGRID_MAP_TEST_FILE);
            string localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            const int meshId = 1;
            var locationType = GridApiDataSet.LocationType.UG_LOC_FACE;
            using (var uGrid = new UGrid(localCopyOfTestFile))
            {
                Assert.That(uGrid.GetNumberOf2DMeshes(), Is.EqualTo(1));
                uGrid.GetNamesAtLocation(meshId, locationType);
                Assert.That(uGrid.VarNameIdsByLocationTypeByMeshId[meshId - 1][locationType], Is.EqualTo(new[]
                {
                    20,
                    21
                }));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallInitialize()
        {
            using (IUGridApi gridApi = GridApiFactory.CreateNew())
            {
                Assert.That(gridApi.Initialize(), Is.EqualTo(0));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestWriteGeomUGrid()
        {
            string testDir = FileUtils.CreateTempDirectory();
            string testFilePath = Path.Combine(testDir, "Custom_Ugrid.nc");
            using (IUGridApi gridApi = GridApiFactory.CreateNew())
            {
                gridApi.write_geom_ugrid(testFilePath);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestCallAll()
        {
            /*
             * 
                                              +-+
                                             X+-+X
                                        XXXXX     XXX
                            edge6   XXX     node4     XX
                                XXX         (10,10)    XX
                             XXX                        XX
                         XXX                             XX
                       +-+                                XX edge5
                      X+-+XX                               XX
                   XXX     XX                                X
                XXXX  node5  XX                               XX
               XX     (5,5)   XXX               face2          XX
              XX                XXX   edge1                      XXX
             XX                    XX                              +-+
            XX                       XX                            +-+
  edge2     X                         XXX                         XX
           XX                             X                     XX  node3
          XX           face1              XXX                XXX  (15,5)
         XX                                 XX            XXX
        XX                                   X        XXXX
        X                                    XX     XXX      edge4
        X                                     X   XXX
      +X+                                    +-+XX
      +XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX-+
                        edge3
    node1                                   node2
    (0,0)                                   (10,0)


             */
            string testDir = FileUtils.CreateTempDirectory();
            string testFilePath = Path.Combine(testDir, "Custom_Ugrid.nc");
            using (IUGridApi gridApi = GridApiFactory.CreateNew())
            {
                gridApi.write_geom_ugrid(testFilePath);
            }

            const int meshId = 1;
            try
            {
                using (var uGrid = new UGrid(testFilePath))
                {
                    uGrid.Initialize();
                    Assert.That(uGrid.CoordinateSystem, Is.EqualTo(new OgrCoordinateSystemFactory().CreateFromEPSG(4326))); // mag dit?
                    Assert.That(uGrid.IsValid(), Is.True);
                    //Assert.That(uGrid.IsValidViaApi(), Is.True);
                    Assert.That(uGrid.GetDataSetConvention(), Is.EqualTo(GridApiDataSet.DataSetConventions.CONV_UGRID));
                    //Assert.That(uGrid.GetDataSetConventionViaApi(), Is.EqualTo(GridApiDataSet.DataSetConventions.CONV_UGRID));
                    Assert.That(uGrid.GetNumberOf2DMeshes(), Is.EqualTo(meshId));
                    Assert.That(uGrid.GetNumberOfNodesForMeshId(meshId), Is.EqualTo(5));
                    Assert.That(uGrid.GetNumberOfEdgesForMeshId(meshId), Is.EqualTo(6));
                    Assert.That(uGrid.GetNumberOfFacesForMeshId(meshId), Is.EqualTo(2));
                    Assert.That(uGrid.GetNumberOfMaxFaceNodesForMeshId(meshId), Is.EqualTo(4));

                    uGrid.GetAllNodeCoordinatesForMeshId(meshId);
                    List<Coordinate> nodeCoordinates = uGrid.NodeCoordinatesByMeshId[meshId - 1].ToList();
                    Assert.That(nodeCoordinates, Is.Not.Null);
                    Assert.That(nodeCoordinates.Count(), Is.EqualTo(5));

                    Assert.That(nodeCoordinates.ElementAt(0).X, Is.EqualTo(0));
                    Assert.That(nodeCoordinates.ElementAt(0).Y, Is.EqualTo(0));

                    Assert.That(nodeCoordinates.ElementAt(1).X, Is.EqualTo(10));
                    Assert.That(nodeCoordinates.ElementAt(1).Y, Is.EqualTo(0));

                    Assert.That(nodeCoordinates.ElementAt(2).X, Is.EqualTo(15));
                    Assert.That(nodeCoordinates.ElementAt(2).Y, Is.EqualTo(5));

                    Assert.That(nodeCoordinates.ElementAt(3).X, Is.EqualTo(10));
                    Assert.That(nodeCoordinates.ElementAt(3).Y, Is.EqualTo(10));

                    Assert.That(nodeCoordinates.ElementAt(4).X, Is.EqualTo(5));
                    Assert.That(nodeCoordinates.ElementAt(4).Y, Is.EqualTo(5));

                    Coordinate[] nodeCoordinateForThisMesh = uGrid.NodeCoordinatesByMeshId[meshId - 1];
                    double fillValue = uGrid.ZCoordinateFillValue;
                    Assert.That(nodeCoordinateForThisMesh[0], Is.EqualTo(new Coordinate(0, 0, fillValue)));
                    Assert.That(nodeCoordinateForThisMesh[1], Is.EqualTo(new Coordinate(10, 0, fillValue)));
                    Assert.That(nodeCoordinateForThisMesh[2], Is.EqualTo(new Coordinate(15, 5, fillValue)));
                    Assert.That(nodeCoordinateForThisMesh[3], Is.EqualTo(new Coordinate(10, 10, fillValue)));
                    Assert.That(nodeCoordinateForThisMesh[4], Is.EqualTo(new Coordinate(5, 5, fillValue)));

                    uGrid.GetFaceNodesForMeshId(meshId);

                    Assert.That(uGrid.FaceNodesByMeshId[meshId - 1], Is.Not.Null);
                    //cast from int[,] (2d int array) to int[][] (2 1d int arrays)
                    IEnumerable<int>[] faceNodesForThisMesh = uGrid.FaceNodesByMeshId[meshId - 1].ConvertToTwoOneDimensionalArrays();
                    Assert.That(faceNodesForThisMesh[0], Is.EqualTo(new[]
                    {
                        1,
                        2,
                        5,
                        -999
                    }));
                    Assert.That(faceNodesForThisMesh[1], Is.EqualTo(new[]
                    {
                        2,
                        3,
                        4,
                        5
                    }));

                    uGrid.GetEdgeNodesForMeshId(meshId);

                    Assert.That(uGrid.EdgeNodesByMeshId, Is.Not.Null);
                    //cast from int[,] (2d int array) to int[][] (2 1d int arrays)
                    IEnumerable<int>[] edgeNodesForThisMesh = uGrid.EdgeNodesByMeshId[meshId - 1].ConvertToTwoOneDimensionalArrays();

                    Assert.That(edgeNodesForThisMesh[0], Is.EqualTo(new[]
                    {
                        5,
                        2
                    }));
                    Assert.That(edgeNodesForThisMesh[1], Is.EqualTo(new[]
                    {
                        2,
                        1
                    }));
                    Assert.That(edgeNodesForThisMesh[2], Is.EqualTo(new[]
                    {
                        1,
                        5
                    }));
                    Assert.That(edgeNodesForThisMesh[3], Is.EqualTo(new[]
                    {
                        5,
                        4
                    }));
                    Assert.That(edgeNodesForThisMesh[4], Is.EqualTo(new[]
                    {
                        4,
                        3
                    }));
                    Assert.That(edgeNodesForThisMesh[5], Is.EqualTo(new[]
                    {
                        3,
                        2
                    }));
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [TestCase(GridApiDataSet.LocationType.UG_LOC_NODE, 5, "node_z")]
        [TestCase(GridApiDataSet.LocationType.UG_LOC_FACE, 2, "face_z")]
        [Category(TestCategory.DataAccess)]
        public void TestWriteZValuesAtLocation_SetsCorrectFillValue(GridApiDataSet.LocationType location, int nLocations, string varName)
        {
            string testFilePath = TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            string localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);

            var meshId = 1;
            double[] zValues = Enumerable.Repeat(123.456, nLocations).ToArray();

            using (var uGrid = new UGrid(localCopyOfTestFile, GridApiDataSet.NetcdfOpenMode.nf90_write))
            {
                switch (location)
                {
                    case GridApiDataSet.LocationType.UG_LOC_NODE:
                        uGrid.WriteZValuesAtNodesForMeshId(meshId, zValues);
                        break;
                    case GridApiDataSet.LocationType.UG_LOC_FACE:
                        uGrid.WriteZValuesAtFacesForMeshId(meshId, zValues);
                        break;
                    default:
                        Assert.Fail(string.Format("Please add support for Location: {0} to this test"));
                        break;
                }
            }

            using (IUGridApi gridApi = GridApiFactory.CreateNew(false)) // explicitly use local api... otherwise TypeUtils doesn't work
            {
                try
                {
                    gridApi.Open(localCopyOfTestFile, GridApiDataSet.NetcdfOpenMode.nf90_nowrite);

                    var ioncid = (int) TypeUtils.GetField(gridApi, "ioncId");
                    var locationId = (int) location;
                    var fillValue = double.MinValue;
                    IntPtr zPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nLocations);

                    // check result
                    var wrapper = new GridWrapper();
                    int ierr = wrapper.GetVariable(ioncid, meshId, locationId, varName, ref zPtr, nLocations, ref fillValue);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                    Assert.AreEqual(GridApiDataSet.GridConstants.DEFAULT_FILL_VALUE, fillValue, 0.1);
                }
                finally
                {
                    if (gridApi != null)
                    {
                        gridApi.Close();
                    }
                }
            }
        }

        [TestCase(@"ugrid\Custom_Ugrid.nc", 4326)] // (WGS84)
        [TestCase(@"ugrid\FlowFM_net.nc", 2005)]   // (St. Kitts 1955 / British West Indies Grid)
        public void TestGetCoordinateSystem(string netFile, long expectedAuthorityCode)
        {
            string testFilePath = TestHelper.GetTestFilePath(netFile);

            using (var uGrid = new UGrid(testFilePath))
            {
                uGrid.Initialize();
                Assert.AreEqual(expectedAuthorityCode, uGrid.CoordinateSystem.AuthorityCode);
            }
        }
    }
}