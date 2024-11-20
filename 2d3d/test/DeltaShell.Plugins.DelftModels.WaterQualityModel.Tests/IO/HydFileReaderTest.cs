using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class HydFileReaderTest
    {
        [Test]
        public void ReadNonExistentFileThrowsInvalidOperationException()
        {
            // setup
            var hydFile = new FileInfo("nonexistentfile.hyd");

            // call
            TestDelegate call = () => HydFileReader.ReadAll(hydFile);

            // assert
            var exception = Assert.Throws<InvalidOperationException>(call);
            Assert.AreEqual("Cannot find hydrodynamics file (" + hydFile.FullName + ").",
                            exception.Message);
        }

        [Test]
        public void ImportGridSquareTest()
        {
            string squareHydPath = TestHelper.GetTestFilePath(@"IO\square\square.hyd");

            HydFileData data = HydFileReader.ReadAll(new FileInfo(squareHydPath));

            UnstructuredGrid grid = data.Grid;
            Assert.IsNotNull(grid);
            Assert.IsFalse(grid.IsEmpty);
            Assert.AreEqual(5.0d, grid.Vertices[grid.Cells[0].VertexIndices[0]].X);
            Assert.AreEqual(100.0d, grid.Vertices[grid.Cells[0].VertexIndices[0]].Y);
            Assert.AreEqual(2500, grid.Cells.Count);
            Assert.AreEqual(2601, grid.Vertices.Count);

            Assert.AreEqual(HydroDynamicModelType.Unstructured, data.HydroDynamicModelType);
            Assert.AreEqual(0, data.ZTop);
            Assert.AreEqual(1, data.ZBot);

            Assert.IsFalse(data.HasDataFor("temp"));
            Assert.Throws<InvalidOperationException>(() => data.GetFilePathFor("temp"));
            Assert.IsTrue(data.HasDataFor("tau"));
            Assert.AreEqual(Path.Combine(Path.GetDirectoryName(squareHydPath), "square.tau"), data.GetFilePathFor("tau"));
            Assert.IsFalse(data.HasDataFor("salinity"));
            Assert.Throws<InvalidOperationException>(() => data.GetFilePathFor("salinity"));
            Assert.IsFalse(data.HasDataFor("chezy"));
            Assert.Throws<InvalidOperationException>(() => data.GetFilePathFor("chezy"));
            Assert.IsFalse(data.HasDataFor("velocity"));
            Assert.Throws<InvalidOperationException>(() => data.GetFilePathFor("velocity"));
            Assert.IsFalse(data.HasDataFor("width"));
            Assert.Throws<InvalidOperationException>(() => data.GetFilePathFor("width"));
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ImportGridRealTest()
        {
            string hydFilePath = TestHelper.GetTestFilePath(@"IO\real\uni3d.hyd");

            HydFileData data = HydFileReader.ReadAll(new FileInfo(hydFilePath));

            UnstructuredGrid grid = data.Grid;
            Assert.IsNotNull(grid);
            Assert.IsFalse(grid.IsEmpty);
            Assert.AreEqual(619190.8086686889d, grid.Vertices[grid.Cells[0].VertexIndices[0]].X);
            Assert.AreEqual(4212559.096632215d, grid.Vertices[grid.Cells[0].VertexIndices[0]].Y);
            Assert.AreEqual(63814, grid.Cells.Count);
            Assert.AreEqual(77527, grid.Vertices.Count);

            Assert.AreEqual(HydroDynamicModelType.Unstructured, data.HydroDynamicModelType);
            Assert.AreEqual(LayerType.Sigma, data.LayerType);
            Assert.AreEqual(0, data.ZTop);
            Assert.AreEqual(1, data.ZBot);

            Assert.IsFalse(data.HasDataFor("temp"));
            Assert.Throws<InvalidOperationException>(() => data.GetFilePathFor("temp"));
            Assert.IsTrue(data.HasDataFor("tau"));
            Assert.AreEqual(Path.Combine(Path.GetDirectoryName(hydFilePath), "uni3d.tau"), data.GetFilePathFor("tau"));
            Assert.IsTrue(data.HasDataFor("salinity"));
            Assert.AreEqual(Path.Combine(Path.GetDirectoryName(hydFilePath), "uni3d.sal"), data.GetFilePathFor("salinity"));
            Assert.IsFalse(data.HasDataFor("chezy"));
            Assert.Throws<InvalidOperationException>(() => data.GetFilePathFor("chezy"));
            Assert.IsFalse(data.HasDataFor("velocity"));
            Assert.Throws<InvalidOperationException>(() => data.GetFilePathFor("velocity"));
            Assert.IsFalse(data.HasDataFor("width"));
            Assert.Throws<InvalidOperationException>(() => data.GetFilePathFor("width"));
        }

        [Test]
        public void ImportGridRealZLayerTest()
        {
            string hydFilePath = TestHelper.GetTestFilePath(@"IO\real_Zlayer\z20_par.hyd");

            HydFileData data = HydFileReader.ReadAll(new FileInfo(hydFilePath));

            UnstructuredGrid grid = data.Grid;
            Assert.IsNotNull(grid);
            Assert.IsFalse(grid.IsEmpty);
            Assert.AreEqual(545542.05895678734d, grid.Vertices[grid.Cells[0].VertexIndices[0]].X);
            Assert.AreEqual(4216633.76345695d, grid.Vertices[grid.Cells[0].VertexIndices[0]].Y);
            Assert.AreEqual(5995, grid.Cells.Count);
            Assert.AreEqual(6442, grid.Vertices.Count);

            Assert.AreEqual(HydroDynamicModelType.Unstructured, data.HydroDynamicModelType);
            Assert.AreEqual(LayerType.ZLayer, data.LayerType);
            Assert.AreEqual(0, data.ZTop);
            Assert.AreEqual(-36.200921, data.ZBot);

            Assert.IsFalse(data.HasDataFor("temp"));
            Assert.Throws<InvalidOperationException>(() => data.GetFilePathFor("temp"));
            Assert.IsTrue(data.HasDataFor("tau"));
            Assert.AreEqual(Path.Combine(Path.GetDirectoryName(hydFilePath), "z20_par.tau"), data.GetFilePathFor("tau"));
            Assert.IsTrue(data.HasDataFor("salinity"));
            Assert.AreEqual(Path.Combine(Path.GetDirectoryName(hydFilePath), "z20_par.sal"), data.GetFilePathFor("salinity"));
            Assert.IsFalse(data.HasDataFor("chezy"));
            Assert.Throws<InvalidOperationException>(() => data.GetFilePathFor("chezy"));
            Assert.IsFalse(data.HasDataFor("velocity"));
            Assert.Throws<InvalidOperationException>(() => data.GetFilePathFor("velocity"));
            Assert.IsFalse(data.HasDataFor("width"));
            Assert.Throws<InvalidOperationException>(() => data.GetFilePathFor("width"));
        }

        [Test]
        public void ReadDelft3DGrid_f34()
        {
            string hydPath = TestHelper.GetTestFilePath(@"IO\f34\com-f34_unstructured.hyd");

            HydFileData data = HydFileReader.ReadAll(new FileInfo(hydPath));

            UnstructuredGrid grid = data.Grid;
            Assert.IsNotNull(grid);
            Assert.IsFalse(grid.IsEmpty);
            Assert.AreEqual(185535.359d, grid.Vertices[grid.Cells[0].VertexIndices[0]].X);
            Assert.AreEqual(606607.93799999997d, grid.Vertices[grid.Cells[0].VertexIndices[0]].Y);
            Assert.AreEqual(223, grid.Cells.Count);
            Assert.AreEqual(260, grid.Vertices.Count);

            Assert.AreEqual(HydroDynamicModelType.Unstructured, data.HydroDynamicModelType);
            Assert.AreEqual(0, data.ZTop);
            Assert.AreEqual(1, data.ZBot);

            Assert.AreEqual(new DateTime(1990, 8, 5), data.ConversionReferenceTime);
            Assert.AreEqual(new DateTime(1990, 8, 5), data.ConversionStartTime);
            Assert.AreEqual(new DateTime(1990, 8, 6, 1, 0, 0), data.ConversionStopTime);
            Assert.AreEqual(new TimeSpan(0, 0, 5, 0), data.ConversionTimeStep);
            Assert.AreEqual(5, data.NumberOfHydrodynamicLayers);
            Assert.AreEqual(2115, data.NumberOfHorizontalExchanges);
            Assert.AreEqual(892, data.NumberOfVerticalExchanges);
            Assert.AreEqual(223, data.NumberOfDelwaqSegmentsPerHydrodynamicLayer);
            Assert.AreEqual(5, data.NumberOfWaqSegmentLayers);

            Assert.AreEqual(1, data.Boundaries.Count);

            Assert.IsTrue(data.HasDataFor("temp"));
            Assert.AreEqual(Path.Combine(Path.GetDirectoryName(hydPath), "com-f34.tem"), data.GetFilePathFor("temp"));
            Assert.IsTrue(data.HasDataFor("tau"));
            Assert.AreEqual(Path.Combine(Path.GetDirectoryName(hydPath), "com-f34.tau"), data.GetFilePathFor("tau"));
            Assert.IsTrue(data.HasDataFor("salinity"));
            Assert.AreEqual(Path.Combine(Path.GetDirectoryName(hydPath), "com-f34.sal"), data.GetFilePathFor("salinity"));
            Assert.IsFalse(data.HasDataFor("chezy"));
            Assert.Throws<InvalidOperationException>(() => data.GetFilePathFor("chezy"));
            Assert.IsFalse(data.HasDataFor("velocity"));
            Assert.Throws<InvalidOperationException>(() => data.GetFilePathFor("velocity"));
            Assert.IsFalse(data.HasDataFor("width"));
            Assert.Throws<InvalidOperationException>(() => data.GetFilePathFor("width"));

            CollectionAssert.AreEquivalent(new[]
            {
                0.4,
                0.27,
                0.18,
                0.1,
                0.05
            }, data.HydrodynamicLayerThicknesses);
        }

        [Test]
        public void ReadDelft3DGrid_f34_aggregated()
        {
            string hydPath = TestHelper.GetTestFilePath(@"IO\f34_aggr\com-f34_unstructured.hyd");

            HydFileData data = HydFileReader.ReadAll(new FileInfo(hydPath));

            UnstructuredGrid grid = data.Grid;
            Assert.IsNotNull(grid);
            Assert.IsFalse(grid.IsEmpty);
            Assert.AreEqual(185535.359d, grid.Vertices[grid.Cells[0].VertexIndices[0]].X);
            Assert.AreEqual(606607.93799999997d, grid.Vertices[grid.Cells[0].VertexIndices[0]].Y);
            Assert.AreEqual(73, grid.Cells.Count);
            Assert.AreEqual(261, grid.Vertices.Count);

            Assert.AreEqual(HydroDynamicModelType.Unstructured, data.HydroDynamicModelType);
            Assert.AreEqual(0, data.ZTop);
            Assert.AreEqual(1, data.ZBot);

            Assert.AreEqual(new DateTime(1990, 8, 5), data.ConversionReferenceTime);
            Assert.AreEqual(new DateTime(1990, 8, 5), data.ConversionStartTime);
            Assert.AreEqual(new DateTime(1990, 8, 6, 1, 0, 0), data.ConversionStopTime);
            Assert.AreEqual(new TimeSpan(0, 0, 5, 0), data.ConversionTimeStep);
            Assert.AreEqual(5, data.NumberOfHydrodynamicLayers);
            Assert.AreEqual(720, data.NumberOfHorizontalExchanges);
            Assert.AreEqual(292, data.NumberOfVerticalExchanges);
            Assert.AreEqual(73, data.NumberOfDelwaqSegmentsPerHydrodynamicLayer);
            Assert.AreEqual(5, data.NumberOfWaqSegmentLayers);

            Assert.AreEqual(1, data.Boundaries.Count);

            Assert.IsTrue(data.HasDataFor("temp"));
            Assert.AreEqual(Path.Combine(Path.GetDirectoryName(hydPath), "com-f34.tem"), data.GetFilePathFor("temp"));
            Assert.IsTrue(data.HasDataFor("tau"));
            Assert.AreEqual(Path.Combine(Path.GetDirectoryName(hydPath), "com-f34.tau"), data.GetFilePathFor("tau"));
            Assert.IsTrue(data.HasDataFor("salinity"));
            Assert.AreEqual(Path.Combine(Path.GetDirectoryName(hydPath), "com-f34.sal"), data.GetFilePathFor("salinity"));
            Assert.IsFalse(data.HasDataFor("chezy"));
            Assert.Throws<InvalidOperationException>(() => data.GetFilePathFor("chezy"));
            Assert.IsFalse(data.HasDataFor("velocity"));
            Assert.Throws<InvalidOperationException>(() => data.GetFilePathFor("velocity"));
            Assert.IsFalse(data.HasDataFor("width"));
            Assert.Throws<InvalidOperationException>(() => data.GetFilePathFor("width"));

            CollectionAssert.AreEquivalent(new[]
            {
                0.4,
                0.27,
                0.18,
                0.1,
                0.05
            }, data.HydrodynamicLayerThicknesses);
        }

        [Test]
        public void ReadValidHydFileReturnsAllDataRead()
        {
            // setup
            string commonFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO");
            var hydFile = new FileInfo(Path.Combine(commonFilePath, "uni3d.hyd"));
            Assert.IsTrue(hydFile.Exists,
                          "Expected .hyd file to exist, but is missing.");

            // call
            HydFileData data = HydFileReader.ReadAll(hydFile);

            // assert
            Assert.AreEqual(hydFile, data.Path);
            Assert.AreEqual(32, data.Checksum.Length, "Expecting a checksum of 32 characters.");
            Assert.AreEqual(HydroDynamicModelType.Unstructured, data.HydroDynamicModelType);
            Assert.AreEqual(new DateTime(1999, 12, 16, 0, 0, 0), data.ConversionReferenceTime);
            Assert.AreEqual(new DateTime(1999, 12, 16, 0, 0, 0), data.ConversionStartTime);
            Assert.AreEqual(new DateTime(1999, 12, 18, 0, 0, 0), data.ConversionStopTime);
            Assert.AreEqual(new TimeSpan(0, 1, 0, 0), data.ConversionTimeStep);
            Assert.AreEqual(788900, data.NumberOfHorizontalExchanges);
            Assert.AreEqual(382884, data.NumberOfVerticalExchanges);
            Assert.AreEqual(63814, data.NumberOfDelwaqSegmentsPerHydrodynamicLayer);

            Assert.AreEqual(7, data.NumberOfHydrodynamicLayers);
            Assert.AreEqual(7, data.HydrodynamicLayerThicknesses.Length);
            CollectionAssert.AreEqual(new[]
                                      {
                                          0.143,
                                          0.143,
                                          0.143,
                                          0.143,
                                          0.143,
                                          0.143,
                                          0.143
                                      },
                                      data.HydrodynamicLayerThicknesses);

            Assert.AreEqual(7, data.NumberOfWaqSegmentLayers);
            Assert.AreEqual(7, data.NumberOfHydrodynamicLayersPerWaqSegmentLayer.Length);
            CollectionAssert.AreEqual(new[]
                                      {
                                          1,
                                          1,
                                          1,
                                          1,
                                          1,
                                          1,
                                          1
                                      },
                                      data.NumberOfHydrodynamicLayersPerWaqSegmentLayer);

            Assert.AreEqual("uni3d.bnd", data.BoundariesRelativePath);
            Assert.AreEqual("uni3d_flowgeom.nc", data.GridRelativePath);

            Assert.AreEqual("uni3d.vol", data.VolumesRelativePath);
            Assert.AreEqual("uni3d.are", data.AreasRelativePath);
            Assert.AreEqual("uni3d.flo", data.FlowsRelativePath);
            Assert.AreEqual("uni3d.poi", data.PointersRelativePath);
            Assert.AreEqual("uni3d.len", data.LengthsRelativePath);
            Assert.AreEqual("uni3d.sal", data.SalinityRelativePath);
            Assert.AreEqual(string.Empty, data.TemperatureRelativePath);
            Assert.AreEqual("uni3d.vdf", data.VerticalDiffusionRelativePath);
            Assert.AreEqual("uni3d.srf", data.SurfacesRelativePath);
            Assert.AreEqual("uni3d.tau", data.ShearStressesRelativePath);
        }

        [Test]
        public void ImportTimesSquareTest()
        {
            string squareHydPath = TestHelper.GetTestFilePath(@"IO\square\square.hyd");

            HydFileData data = HydFileReader.ReadAll(new FileInfo(squareHydPath));

            Assert.AreEqual(new DateTime(2001, 1, 1, 0, 0, 0), data.ConversionStartTime);
            Assert.AreEqual(new DateTime(2001, 1, 1, 0, 10, 0), data.ConversionStopTime);
            Assert.AreEqual(new TimeSpan(0, 0, 5, 0), data.ConversionTimeStep);
            Assert.AreEqual(new DateTime(2001, 1, 1, 0, 0, 0), data.ConversionReferenceTime);
        }

        [Test]
        public void ImportBulkFilesTest()
        {
            string squareHydPath = TestHelper.GetTestFilePath(@"IO\square\square.hyd");

            HydFileData data = HydFileReader.ReadAll(new FileInfo(squareHydPath));

            Assert.AreEqual("square.are", data.AreasRelativePath);
            Assert.AreEqual("square.vol", data.VolumesRelativePath);
            Assert.AreEqual("square.flo", data.FlowsRelativePath);
            Assert.AreEqual("square.poi", data.PointersRelativePath);
            Assert.AreEqual("square.len", data.LengthsRelativePath);
            Assert.AreEqual(string.Empty, data.SalinityRelativePath);
            Assert.AreEqual(string.Empty, data.TemperatureRelativePath);
            Assert.AreEqual(string.Empty, data.VerticalDiffusionRelativePath);
            Assert.AreEqual("square.srf", data.SurfacesRelativePath);
            Assert.AreEqual("square.tau", data.ShearStressesRelativePath);
            Assert.AreEqual("square_flowgeom.nc", data.GridRelativePath);
        }

        [Test]
        public void ImportMetaDataTest()
        {
            string squareHydPath = TestHelper.GetTestFilePath(@"IO\square\square.hyd");

            HydFileData data = HydFileReader.ReadAll(new FileInfo(squareHydPath));

            Assert.AreEqual(4900, data.NumberOfHorizontalExchanges);
            Assert.AreEqual(0, data.NumberOfVerticalExchanges);
            Assert.AreEqual(1, data.NumberOfHydrodynamicLayers);
            Assert.AreEqual(2500, data.NumberOfDelwaqSegmentsPerHydrodynamicLayer);
            Assert.AreEqual(1, data.NumberOfWaqSegmentLayers);
            Assert.AreEqual(HydroDynamicModelType.Unstructured, data.HydroDynamicModelType);
            Assert.AreEqual(0, data.ZTop);
            Assert.AreEqual(1, data.ZBot);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ImportLayersMetaDataTest()
        {
            // setup
            string squareHydPath = TestHelper.GetTestFilePath(@"IO\real\uni3d.hyd");

            // call
            HydFileData data = HydFileReader.ReadAll(new FileInfo(squareHydPath));

            // assert
            double[] thicknesses = data.HydrodynamicLayerThicknesses;
            Assert.AreEqual(7, thicknesses.Length);
            CollectionAssert.AreEqual(new[]
            {
                0.142857,
                0.142857,
                0.142857,
                0.142857,
                0.142857,
                0.142857,
                0.142857
            }, thicknesses);

            int[] hydroLayerPerWaqLayers = data.NumberOfHydrodynamicLayersPerWaqSegmentLayer;
            Assert.AreEqual(7, hydroLayerPerWaqLayers.Length);
            CollectionAssert.AreEqual(new[]
            {
                1,
                1,
                1,
                1,
                1,
                1,
                1
            }, hydroLayerPerWaqLayers);
        }

        [Test]
        public void ImportBoundariesTest()
        {
            string squareHydPath = TestHelper.GetTestFilePath(@"IO\real\uni3d.hyd");

            HydFileData data = HydFileReader.ReadAll(new FileInfo(squareHydPath));

            IEventedList<WaterQualityBoundary> boundaries = data.GetBoundaries();
            Assert.AreEqual(6, boundaries.Count);
            var expectedBoundaries = new[]
            {
                "sea_002.pli",
                "sacra_001.pli",
                "sanjoa_001.pli",
                "yolo_001.pli",
                "CC.pli",
                "tracy.pli"
            };
            CollectionAssert.AreEqual(expectedBoundaries, boundaries.Select(b => b.Name).ToArray());

            IDictionary<WaterQualityBoundary, int[]> boundaryNodeIds = data.GetBoundaryNodeIds();
            Assert.AreEqual(boundaries.Count, boundaryNodeIds.Count);
            var expectedNumberOfBoundaryNodeIds = new[]
            {
                105,
                4,
                3,
                24,
                1,
                1
            };
            for (var i = 0; i < boundaries.Count; i++)
            {
                int[] ids = boundaryNodeIds[boundaries[i]];
                Assert.AreEqual(expectedNumberOfBoundaryNodeIds[i], ids.Length);
            }
        }

        [Test]
        public void HydFileImporterToString()
        {
            using (var data = new HydFileData())
            {
                Assert.AreEqual("", data.ToString());

                string path = Path.GetFullPath(TestHelper.GetTestFilePath(@"IO\real\uni3d.hyd"));
                data.Path = new FileInfo(path);

                Assert.AreEqual(path, data.ToString());
            }
        }

        [Test]
        public void Test_HydFileReader_SegmentProperties_AreSet()
        {
            string testPath = TestHelper.GetTestFilePath(@"ValidWaqModels\Flow1D\sobek.hyd");
            Assert.IsTrue(File.Exists(testPath));
            testPath = TestHelper.CreateLocalCopy(testPath);
            Assert.IsTrue(File.Exists(testPath));

            HydFileData data = HydFileReader.ReadAll(new FileInfo(testPath));
            Assert.IsNotNull(data);

            Assert.IsFalse(string.IsNullOrEmpty(data.SurfacesRelativePath));
            Assert.IsFalse(string.IsNullOrEmpty(data.VelocitiesRelativePath));
            Assert.IsFalse(string.IsNullOrEmpty(data.WidthsRelativePath));
            Assert.IsFalse(string.IsNullOrEmpty(data.ChezyCoefficientsRelativePath));
        }
    }
}