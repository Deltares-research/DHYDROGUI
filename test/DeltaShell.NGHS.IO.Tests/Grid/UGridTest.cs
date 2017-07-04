using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Dimr;
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
        public void AssertIONetCDFDllIsXpCompatible()
        {
            // The problem is that platform toolsets (vc++ runtime dependencies) of 110 & higher (eg above 100) 
            // aren't xp compatible. This appears the default on VS2012 and up. If you encounter this problem, 
            // rebuild the dll using a toolset compatible with xp (eg 100, or 110_xp).

            // We use a hacky but effective way to check if the current dll is xp compatible, namely we check
            // for the occurance of 'GetTickCount64' in the dll imports. This method is only available on Vista
            // and above.

            foreach (
                var dllVersion in
                new[]
                {
                    Path.Combine(DimrApiDataSet.SharedDllPath, "io_netcdf.dll"),
                    Path.Combine(DimrApiDataSet.SharedDllPath.Contains("x86") ? DimrApiDataSet.SharedDllPath.Replace("x86","x64"): DimrApiDataSet.SharedDllPath, "io_netcdf.dll")
                })
            {
                foreach (var line in File.ReadLines(dllVersion))
                {
                    if (line.Contains("GetTickCount64"))
                        Assert.Fail("Current dflowfm.dll is not compatible with XP: " + dllVersion);
                }
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallValidateFileIsUgridWithCallToOpenClose()
        {
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            using (var uGrid = new UGrid(localCopyOfTestFile))
            {
                uGrid.Initialize();
                Assert.That(uGrid.IsValid(), Is.True);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [ExpectedException]
        public void TestIfFilenameIsNullUgridShouldThrowException()
        {
            using (var uGrid = new UGrid(null))
            {
                uGrid.Initialize();
            }

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [ExpectedException]
        public void TestIfFilenameIsEmptyStringUgridShouldThrowException()
        {
            using (var uGrid = new UGrid(""))
            {
                uGrid.Initialize();
            }

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [ExpectedException]
        public void TestIfFileDoesNotExistUgridShouldThrowException()
        {
            using (var uGrid = new UGrid("thisFileDoesntExist.nc"))
            {
                uGrid.Initialize();
            }
        }

        [Test]
        [Ignore("doesn't work yet, should check with arthur van dam")]
        [Category(TestCategory.DataAccess)]
        public void TestCallValidateFileIsUgridWithCallToApi()
        {
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            using (var uGrid = new UGridStub(localCopyOfTestFile))
            {
                Assert.IsTrue(uGrid.IsValidViaApi());
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallGetDataSetConvention()
        {
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            using (var uGrid = new UGrid(localCopyOfTestFile))
            {
                uGrid.Initialize();
                Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_UGRID, uGrid.GetDataSetConvention());
            }
        }

        [Test]
        public void TestDefaultGlobalMetaData()
        {
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            var metadata = new UGridGlobalMetaData();
            using (var uGrid = new UGrid(localCopyOfTestFile))
            {
                Assert.AreEqual(metadata, uGrid.GlobalMetaData);
            }
        }

        [Test]
        public void TestValidGlobalMetaData()
        {
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            var metadata = new UGridGlobalMetaData("MyModel", "MySource", "MyVersion");
            using (var uGrid = new UGrid(localCopyOfTestFile, metadata))
            {
                Assert.AreEqual(metadata, uGrid.GlobalMetaData);
            }
        }

        [Test]
        [ExpectedException(typeof(NullReferenceException), ExpectedMessage = "Object reference not set to an instance of an object.")]
        public void TestNullGlobalMetaData()
        {
            var testFilePath =
                TestHelper.GetTestFilePath(DUMMY_TEST_FILE);
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            FileUtils.DeleteIfExists(localCopyOfTestFile);
            //var metadata = new UGridGlobalMetaData();
            using (var uGrid = new UGrid(localCopyOfTestFile, null))
            {
                uGrid.CreateFile();
            }
        }

        [Test]
        [Ignore("doesn't work yet, should check with arthur van dam")]
        [Category(TestCategory.DataAccess)]
        public void TestCallGetDataSetConventionViaApi()
        {
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            using (var uGrid = new UGridStub(localCopyOfTestFile))
            {
                Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_UGRID, uGrid.GetDataSetConvention());
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallRewriteGridCoordinates()
        {
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);

            IList<double> currentXValues, currentYValues;
            using (NetCdfFileWrapper ncFile = new NetCdfFileWrapper(localCopyOfTestFile))
            {
                currentXValues = ncFile.GetValues1D<double>("mesh2d_node_x");
                currentYValues = ncFile.GetValues1D<double>("mesh2d_node_y");
            }

            var newXValues = currentXValues.Select(x =>
            {
                x = 123.456;
                return x;
            }).ToArray();
            var newYValues = currentYValues.Select(y =>
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
            using (NetCdfFileWrapper ncFile = new NetCdfFileWrapper(localCopyOfTestFile))
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
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            IList<double> currentZValues;
            using (NetCdfFileWrapper ncFile = new NetCdfFileWrapper(localCopyOfTestFile))
            {
                currentZValues = ncFile.GetValues1D<double>("mesh2d_node_z");
            }
            
            var newZValues = currentZValues.Select(z => { z = 123.456; return z; }).ToArray();

            using (var uGrid = new UGrid(localCopyOfTestFile,GridApiDataSet.NetcdfOpenMode.nf90_write))
            {
                uGrid.WriteZValuesAtNodesForMeshId(1, newZValues);
            }
            IList<double> zValues;
            using (NetCdfFileWrapper ncFile = new NetCdfFileWrapper(localCopyOfTestFile))
            {
                zValues = ncFile.GetValues1D<double>("mesh2d_node_z");
            }
            Assert.That(zValues.All(z => Math.Abs(z - 123.456) < 0.0001), Is.True);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallSetZValues_Faces()
        {
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            
            var newZValues = new[] {123.456, 123.456}; // Test file has 2 faces
            using (var uGrid = new UGrid(localCopyOfTestFile, GridApiDataSet.NetcdfOpenMode.nf90_write))
            {
                uGrid.WriteZValuesAtFacesForMeshId(1, newZValues);
            }

            IList<double> zValues;
            using (NetCdfFileWrapper ncFile = new NetCdfFileWrapper(localCopyOfTestFile))
            {
                zValues = ncFile.GetValues1D<double>("mesh2d_face_z");
            }

            Assert.NotNull(zValues); // variable should have been created by ionc_def_var
            Assert.That(zValues.All(z => Math.Abs(z - 123.456) < 0.0001), Is.True);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallNumberOfMesh()
        {
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            
            using (var uGrid = new UGrid(localCopyOfTestFile))
            {
                Assert.That(uGrid.GetNumberOf2DMeshes(), Is.EqualTo(1));
            }
        }
        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallNameOfMesh()
        {
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            using (var uGrid = new UGrid(localCopyOfTestFile))
            {
                Assert.That(uGrid.GetMeshName(1), Is.EqualTo("mesh2d"));
            }
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallNumberOfNodesInFirstMesh()
        {
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
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
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
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
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
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
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
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
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            using (var uGrid = new UGrid(localCopyOfTestFile))
            {
                var meshId = 1;
                uGrid.GetAllNodeCoordinatesForMeshId(meshId);
                Assert.That(uGrid.GetNumberOf2DMeshes(), Is.EqualTo(1));
                Assert.That(uGrid.NodeCoordinatesByMeshId[meshId-1], Is.Not.Null);
                Assert.That(uGrid.NodeCoordinatesByMeshId[meshId-1].Count(), Is.EqualTo(uGrid.GetNumberOfNodesForMeshId(meshId)));
            }
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallFaceNodesInFirstMesh()
        {
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
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
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_MAP_TEST_FILE);
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
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
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_MAP_TEST_FILE);
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            const int meshId = 1;
            var locationType = GridApiDataSet.LocationType.UG_LOC_FACE;
            using (var uGrid = new UGrid(localCopyOfTestFile))
            {
                Assert.That(uGrid.GetNumberOf2DMeshes(), Is.EqualTo(1));
                uGrid.GetNamesAtLocation(meshId, locationType);
                Assert.That(uGrid.VarNameIdsAtLocationInMesh[meshId -1][locationType], Is.EqualTo(new []{20, 21}));
            }
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestCallInitialize()
        {
            using (var gridApi = GridApiFactory.CreateNew())
            {
                Assert.That(gridApi.Initialize(), Is.EqualTo(0));
            }
        }

        [Test]
        [Ignore]
        [Category(TestCategory.DataAccess)]
        public void TestCallWriteGeometry()
        {
            var testDir = FileUtils.CreateTempDirectory();
            var testFilePath = Path.Combine(testDir, "Custom_Ugrid.nc");
            UGridStub.TestWrite(testFilePath);
        }
        
        [Test]
        [Ignore]
        [Category(TestCategory.DataAccess)]
        public void TestCallWriteMapFile()
        {
            var testDir = FileUtils.CreateTempDirectory();
            var testFilePath = Path.Combine(testDir, "Custom_Ugrid_map.nc");
            UGridStub.TestWriteMap(testFilePath);
        }

        [TestCase(@"ugrid\Custom_Ugrid.nc", 4326)] // (WGS84)
        [TestCase(@"ugrid\FlowFM_net.nc", 2005)] // (St. Kitts 1955 / British West Indies Grid)
        public void TestGetCoordinateSystem(string netFile, long expectedAuthorityCode)
        {
            var testFilePath = TestHelper.GetTestFilePath(netFile);

            using (var uGrid = new UGrid(testFilePath))
            {
                uGrid.Initialize();
                Assert.AreEqual(expectedAuthorityCode, uGrid.CoordinateSystem.AuthorityCode);
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
            var testDir = FileUtils.CreateTempDirectory();
            var testFilePath = Path.Combine(testDir, "Custom_Ugrid.nc");
            using (var gridApi = GridApiFactory.CreateNew())
            {
                gridApi.ionc_write_geom_ugrid(testFilePath);
            }
            
            
            const int meshId = 1;
            try
            {
                using (var uGrid = new UGridStub(testFilePath))
                {
                    uGrid.Initialize();
                    Assert.That(uGrid.CoordinateSystem, Is.EqualTo(new OgrCoordinateSystemFactory().CreateFromEPSG(4326))); // mag dit?
                    Assert.That(uGrid.IsValid(), Is.True);
                    //Assert.That(uGrid.IsValidViaApi(), Is.True);
                    Assert.That(uGrid.GetDataSetConvention(), Is.EqualTo(GridApiDataSet.DataSetConventions.IONC_CONV_UGRID));
                    //Assert.That(uGrid.GetDataSetConventionViaApi(), Is.EqualTo(GridApiDataSet.DataSetConventions.IONC_CONV_UGRID));
                    Assert.That(uGrid.GetNumberOf2DMeshes(), Is.EqualTo(meshId));
                    Assert.That(uGrid.GetNumberOfNodesForMeshId(meshId), Is.EqualTo(5));
                    Assert.That(uGrid.GetNumberOfEdgesForMeshId(meshId), Is.EqualTo(6));
                    Assert.That(uGrid.GetNumberOfFacesForMeshId(meshId), Is.EqualTo(2));
                    Assert.That(uGrid.GetNumberOfMaxFaceNodesForMeshId(meshId), Is.EqualTo(4));

                    uGrid.GetAllNodeCoordinatesForMeshId(meshId);
                    var nodeCoordinates = uGrid.NodeCoordinatesByMeshId[meshId-1].ToList();
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

                    var nodeCoordinateForThisMesh = uGrid.NodeCoordinatesByMeshId[meshId - 1];
                    var fillValue = uGrid.ZCoordinateFillValue;
                    Assert.That(nodeCoordinateForThisMesh[0], Is.EqualTo(new Coordinate(0, 0, fillValue)));
                    Assert.That(nodeCoordinateForThisMesh[1], Is.EqualTo(new Coordinate(10, 0, fillValue)));
                    Assert.That(nodeCoordinateForThisMesh[2], Is.EqualTo(new Coordinate(15, 5, fillValue)));
                    Assert.That(nodeCoordinateForThisMesh[3], Is.EqualTo(new Coordinate(10, 10, fillValue)));
                    Assert.That(nodeCoordinateForThisMesh[4], Is.EqualTo(new Coordinate(5, 5, fillValue)));

                    uGrid.GetFaceNodesForMeshId(meshId);
                    
                    Assert.That(uGrid.FaceNodesByMeshId[meshId - 1], Is.Not.Null);
                    //cast from int[,] (2d int array) to int[][] (2 1d int arrays)
                    var faceNodesForThisMesh = uGrid.FaceNodesByMeshId[meshId - 1].ConvertToTwoOneDimensionalArrays();
                    Assert.That(faceNodesForThisMesh[0], Is.EqualTo(new[] { 1, 2, 5, -999 }));
                    Assert.That(faceNodesForThisMesh[1], Is.EqualTo(new[] { 2, 3, 4, 5 }));
                    
                    uGrid.GetEdgeNodesForMeshId(meshId);
                    
                    Assert.That(uGrid.EdgeNodesByMeshId, Is.Not.Null);
                    //cast from int[,] (2d int array) to int[][] (2 1d int arrays)
                    var edgeNodesForThisMesh = uGrid.EdgeNodesByMeshId[meshId - 1].ConvertToTwoOneDimensionalArrays();
                    Assert.That(edgeNodesForThisMesh[0], Is.EqualTo(new[] { 5, 2 }));
                    Assert.That(edgeNodesForThisMesh[1], Is.EqualTo(new[] { 2, 1 }));
                    Assert.That(edgeNodesForThisMesh[2], Is.EqualTo(new[] { 1, 5 }));
                    Assert.That(edgeNodesForThisMesh[3], Is.EqualTo(new[] { 5, 4 }));
                    Assert.That(edgeNodesForThisMesh[4], Is.EqualTo(new[] { 4, 3 }));
                    Assert.That(edgeNodesForThisMesh[5], Is.EqualTo(new[] { 3, 2 }));
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }

        }
    }
}
