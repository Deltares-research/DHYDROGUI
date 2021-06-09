using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders.Network;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.TestUtils;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class NetworkAndGridReaderTest
    {
        private IHydroNetwork originalNetwork;

        [SetUp]
        public void SetUp()
        {
            originalNetwork = FileWriterTestHelper.SetupSimpleHydroNetworkWith2NodesAnd1Branch();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void TestNetworkAndGridReaderGivesExpectedResults()
        {
            // Setup network data
            originalNetwork.Nodes[0].Geometry = new Point(NetworkAndGridReaderTestHelper.NODE1_X, NetworkAndGridReaderTestHelper.NODE1_Y);
            originalNetwork.Nodes[1].Geometry = new Point(NetworkAndGridReaderTestHelper.NODE2_X, NetworkAndGridReaderTestHelper.NODE2_Y);

            var branch = originalNetwork.Channels.First();
            var originalDiscretization = NetworkAndGridReaderTestHelper.GenerateDiscretization(branch, NetworkAndGridReaderTestHelper.NUM_DISCRETIZATION_LOCATIONS);

            // Write to file
            NetworkAndGridWriter.WriteFile(FileWriterTestHelper.ModelFileNames.Network, originalNetwork, originalDiscretization);

            // Read to file
            IHydroNetwork readNetwork = new HydroNetwork();
            IDiscretization readDiscretization = new Discretization();

            NetworkAndGridReader.ReadFile(FileWriterTestHelper.ModelFileNames.Network, readNetwork, readDiscretization);

            // Comparison
            var originalNodes = originalNetwork.HydroNodes.ToArray();
            var readNodes = readNetwork.HydroNodes.ToArray();
            Assert.AreEqual(originalNodes.Length, readNodes.Length);

            for (var i = 0; i < originalNodes.Length; i++)
            {
                Assert.IsTrue(NetworkAndGridReaderTestHelper.CompareNodes(originalNodes[i], readNodes[i]));
            }

            var originalBranches = originalNetwork.Channels.ToArray();
            var readBranches = readNetwork.Channels.ToArray();
            Assert.AreEqual(originalBranches.Length, readBranches.Length);

            for (var i = 0; i < originalBranches.Length; i++)
            {
                Assert.IsTrue(NetworkAndGridReaderTestHelper.CompareBranches(originalBranches[i], readBranches[i]));
            }

            for (var i = 0; i < originalDiscretization.Locations.Values.Count; i++)
            {
                Assert.IsTrue(NetworkAndGridReaderTestHelper.CompareDiscretizationsValues(originalDiscretization.Locations.Values[i], readDiscretization.Locations.Values[i]));
            }
        }

        [Test]
        public void TestNetworkAndGridReaderGivesExpectedException()
        {
            // Setup network data
            originalNetwork.Nodes[0].Geometry = new Point(NetworkAndGridReaderTestHelper.NODE1_X, NetworkAndGridReaderTestHelper.NODE1_Y);
            originalNetwork.Nodes[1].Geometry = new Point(NetworkAndGridReaderTestHelper.NODE2_X, NetworkAndGridReaderTestHelper.NODE2_Y);

            var branch = originalNetwork.Channels.First();
            var originalDiscretization = NetworkAndGridReaderTestHelper.GenerateDiscretization(branch, NetworkAndGridReaderTestHelper.NUM_DISCRETIZATION_LOCATIONS);

            // Write to file
            NetworkAndGridWriter.WriteFile(FileWriterTestHelper.ModelFileNames.Network, originalNetwork, originalDiscretization);

            //screw up file
            var categories = new DelftIniReader().ReadDelftIniFile(FileWriterTestHelper.ModelFileNames.Network);
            var firstNode = categories.FirstOrDefault(category => category.Name == "Node");
            if (firstNode == null) return;
            firstNode.Properties.RemoveAt(1);
            new IniFileWriter().WriteIniFile(categories, FileWriterTestHelper.ModelFileNames.Network);


            // Read to model
            IHydroNetwork readNetwork = new HydroNetwork();
            IDiscretization readDiscretization = new Discretization();

            Assert.DoesNotThrow(() => NetworkAndGridReader.ReadFile(FileWriterTestHelper.ModelFileNames.Network, readNetwork, readDiscretization));
        }
    }
}
