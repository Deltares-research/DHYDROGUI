using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class SobekToFMExporterTest
    {
        [Test]
        public void ExportDemoSobekGridToFMGrid()
        {
            const string ncPath = "test_net.nc";

            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            
            var exporter = new SobekToFMExporter();
            
            exporter.Export(model, ncPath);

            Assert.IsTrue(File.Exists(ncPath));
        }

        [Test]
        public void ExportSloterplasSobekGridToFMGrid()
        {
            const string ncPath = "sloterplas_net.nc";

            var modelImporter = new SobekWaterFlowModel1DImporter { TargetItem = new WaterFlowModel1D() };

            var zipFile = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"ExpSBI.lit.zip");

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                ZipFileUtils.Extract(zipFile, tempDir);

                var modelPath = Path.Combine(tempDir, "ExpSBI.lit", "1", "NETWORK.TP");
                var exportPath = Path.Combine(tempDir, ncPath);

                var model = (WaterFlowModel1D)modelImporter.ImportItem(modelPath);
                var exporter = new SobekToFMExporter();
                exporter.Export(model, exportPath);

                Assert.IsTrue(File.Exists(exportPath));
            });
        }

        [Test]
        public void ExportImportNetworkDiscretization()
        {
            var node1 = new HydroNode("bottom-left") { Geometry = new Point(-100, -100) };
            var node2 = new HydroNode("bottom-right") { Geometry = new Point(100, -100) };
            var node3 = new HydroNode("center") { Geometry = new Point(0, 0) };
            var node4 = new HydroNode("top") { Geometry = new Point(0, 100) };

            var line1 = new LineString(new[] { node1.Geometry.Coordinate, node3.Geometry.Coordinate });
            var line2 = new LineString(new[] { node3.Geometry.Coordinate, node2.Geometry.Coordinate });
            var line3 = new LineString(new[] { node3.Geometry.Coordinate, node4.Geometry.Coordinate });

            var branch1 = new Channel(node1, node3) { Geometry = line1 };
            var branch2 = new Channel(node3, node2) { Geometry = line2 };
            var branch3 = new Channel(node3, node4) { Geometry = line3 };

            var mercedes = new HydroNetwork();
            mercedes.Nodes.AddRange(new[] { node1, node2, node3, node4 });
            mercedes.Branches.AddRange(new[] { branch1, branch2, branch3 });

            var waterFlowModel1D = new WaterFlowModel1D("wfm1d") { Network = mercedes };
            HydroNetworkHelper.GenerateDiscretization(waterFlowModel1D.NetworkDiscretization, false, false, 75.0, false, 0,
                false, false, true, 75.0);

            var exporter = new SobekToFMExporter();
            Assert.IsTrue(exporter.CanExportFor(waterFlowModel1D));

            exporter.Export(waterFlowModel1D, "test_net.nc");

            var unstructuredGrid = NetFileImporter.ImportGrid("test_net.nc");


            var uniqueNetworkLocations = waterFlowModel1D.NetworkDiscretization.Locations.GetValues<INetworkLocation>()
                .ToList()
                .Select(l => l.Geometry.Coordinate).OrderBy(c => c).Distinct();

            var vertices = unstructuredGrid.Vertices.OrderBy(c => c);

            Assert.IsTrue(uniqueNetworkLocations.SequenceEqual(vertices));
        }
    }
}