using System.IO;
using DelftTools.Hydro;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Export
{
    [TestFixture]
    public class HydroRegionToShapeFileExporterTest
    {
        private IHydroNetwork hydroNetwork;
        private const string TestOutputDirectoryName = "./HydroRegionShapeFileExporterTestOutput";

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Directory.CreateDirectory(TestOutputDirectoryName);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            if (Directory.Exists(TestOutputDirectoryName))
            {
                Directory.Delete(TestOutputDirectoryName, true);
            }
        }

        [SetUp]
        public void SetUp()
        {
            CleanDirectory();

            hydroNetwork = new HydroNetwork();
            var node1 = new HydroNode { Name = "Node1", Network = hydroNetwork, Geometry = new Point(0.0, 0.0) };
            var node2 = new HydroNode { Name = "Node2", Network = hydroNetwork, Geometry = new Point(100.0, 0.0) };
            hydroNetwork.Nodes.Add(node1);
            hydroNetwork.Nodes.Add(node2);

            var branch = new Channel("branch1", node1, node2)
            {
                Geometry = new LineString(new[] { node1.Geometry.Coordinate, node2.Geometry.Coordinate })
            };
            hydroNetwork.Branches.Add(branch);
        }

        [TearDown]
        public void TearDown()
        {
            CleanDirectory();

            hydroNetwork = null;
        }

        private static void CleanDirectory()
        {
            var dirInfo = new DirectoryInfo(TestOutputDirectoryName);
            foreach (FileInfo file in dirInfo.GetFiles())
            {
                file.Delete();
            }
        }

/*
        [Test, Category(TestCategory.DataAccess)]
        public void ExportEmptyHydroNetworkToShapefile()
        {
            var fileExporter = new HydroRegionShapeFileExporter(TODO);
            Assert.IsFalse(fileExporter.Export(new HydroNetwork(), CreateFilepath("shapes.shp")));
            Assert.AreEqual(0, Directory.GetFiles(TestOutputDirectoryName).Length);
        }

        [Test, Category(TestCategory.DataAccess)]
        public void ExportBasicHydroNetworkToShapefile()
        {
            var fileExporter = new HydroRegionShapeFileExporter(TODO);
            Assert.IsTrue(fileExporter.Export(hydroNetwork, CreateFilepath("shapes.shp")));
            Assert.AreEqual(6, Directory.GetFiles(TestOutputDirectoryName).Length);
        }

        [Test, Category(TestCategory.DataAccess)]
        public void ExportHydroNetworkWithWeirToShapefile()
        {
            var weir = new Weir { Geometry = new Point(5, 0), OffsetY = 150, CrestWidth = 50, CrestLevel = 8 };
            NetworkHelper.AddBranchFeatureToBranch(weir, hydroNetwork.Branches.FirstOrDefault(), 5);

            var fileExporter = new HydroRegionShapeFileExporter(TODO);
            Assert.IsTrue(fileExporter.Export(hydroNetwork, CreateFilepath("shapes.shp")));
            Assert.AreEqual(9, Directory.GetFiles(TestOutputDirectoryName).Length);
        }

        [Test, Category(TestCategory.DataAccess)]
        public void ExportBasicHydroRegionToShapefile()
        {
            var fileExporter = new HydroRegionShapeFileExporter(TODO);
            Assert.IsFalse(fileExporter.Export(new HydroRegion(), CreateFilepath("shapes.shp")));
            Assert.AreEqual(0, Directory.GetFiles(TestOutputDirectoryName).Length);
        }

        [Test, Category(TestCategory.DataAccess)]
        public void ExporHydroRegionWithLinkedCatchmentBranchAndWeirToShapefile()
        {
            var weir = new Weir { Geometry = new Point(5, 0), OffsetY = 150, CrestWidth = 50, CrestLevel = 8 };
            NetworkHelper.AddBranchFeatureToBranch(weir, hydroNetwork.Branches.FirstOrDefault(), 5);

            var catchment = Catchment.CreateDefault();
            catchment.Name = "first";
            var basin = new DrainageBasin { Geometry = catchment.Geometry, Catchments = { catchment } };

            var hydroRegion = new HydroRegion { SubRegions = {basin, hydroNetwork}};

            var fileExporter = new HydroRegionShapeFileExporter(TODO);
            Assert.IsTrue(fileExporter.Export(hydroRegion, CreateFilepath("shapes.shp")));
            Assert.AreEqual(15, Directory.GetFiles(TestOutputDirectoryName).Length);
        }
*/

        private static string CreateFilepath(string filename)
        {
            return TestOutputDirectoryName + Path.DirectorySeparatorChar + filename;
        }
    }
}
