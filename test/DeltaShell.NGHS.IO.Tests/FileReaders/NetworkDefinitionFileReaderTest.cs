using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.Network;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.TestUtils;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class NetworkDefinitionFileReaderTest
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

            // Read from file
            IHydroNetwork readNetwork = new HydroNetwork();
            IDiscretization readDiscretization = new Discretization();
            
            var networkDefinitionFileReader = new NetworkDefinitionFileReader();
            var nodes = networkDefinitionFileReader.ReadHydroNodes(FileWriterTestHelper.ModelFileNames.Network);
            readNetwork.Nodes.AddRange(nodes);
            var branches = networkDefinitionFileReader.ReadBranches(FileWriterTestHelper.ModelFileNames.Network, readNetwork);
            readNetwork.Branches.AddRange(branches);
            var readNetworkLocations = networkDefinitionFileReader.ReadNetworkLocations(FileWriterTestHelper.ModelFileNames.Network, readNetwork.Branches);
            readDiscretization.Locations.Values.AddRange(readNetworkLocations.ToList());


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
        [ExpectedException(typeof(FileReadingException))]
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
            
            // Read from model
            var networkDefinitionFileReader = new NetworkDefinitionFileReader();
            networkDefinitionFileReader.ReadHydroNodes(FileWriterTestHelper.ModelFileNames.Network);
        }

        [Test]
        [ExpectedException(typeof(FileReadingException))]
        public void GivenNoFile_WhenTryingToExecuteReadFile_ThenAFileReadingExceptionIsThrown()
        {
            const string nonExistingFilePath = @"This/File/Does/Not/Exist";
            DelftIniFileParser.ReadFile(nonExistingFilePath);
        }

        [Test]
        [ExpectedException(typeof(FileReadingException))]
        public void GivenAFileWithZeroCategories_WhenTryingToExecuteReadFile_ThenAFileReadingExceptionIsThrown()
        {
            // Setup network data
            originalNetwork.Nodes[0].Geometry = new Point(NetworkAndGridReaderTestHelper.NODE1_X,
                NetworkAndGridReaderTestHelper.NODE1_Y);
            originalNetwork.Nodes[1].Geometry = new Point(NetworkAndGridReaderTestHelper.NODE2_X,
                NetworkAndGridReaderTestHelper.NODE2_Y);

            var branch = originalNetwork.Channels.First();
            var originalDiscretization = NetworkAndGridReaderTestHelper.GenerateDiscretization(branch,
                NetworkAndGridReaderTestHelper.NUM_DISCRETIZATION_LOCATIONS);

            // Write to file
            NetworkAndGridWriter.WriteFile(FileWriterTestHelper.ModelFileNames.Network, originalNetwork,
                originalDiscretization);

            //Remove categories
            var categories = new List<DelftIniCategory>();
            new IniFileWriter().WriteIniFile(categories, FileWriterTestHelper.ModelFileNames.Network);

            //Read from model
            var networkDefinitionFileReader = new NetworkDefinitionFileReader();
            networkDefinitionFileReader.ReadHydroNodes(FileWriterTestHelper.ModelFileNames.Network);

        }
    }
}
