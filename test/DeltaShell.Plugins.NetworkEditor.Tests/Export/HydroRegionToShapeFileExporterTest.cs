using System.IO;
using DelftTools.Hydro;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Export
{
    [TestFixture]
    public class HydroRegionToShapeFileExporterTest
    {
        private IHydroNetwork hydroNetwork;
        private const string TestOutputDirectoryName = "./HydroRegionShapeFileExporterTestOutput";

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            Directory.CreateDirectory(TestOutputDirectoryName);
        }

        [OneTimeTearDown]
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
    }
}
