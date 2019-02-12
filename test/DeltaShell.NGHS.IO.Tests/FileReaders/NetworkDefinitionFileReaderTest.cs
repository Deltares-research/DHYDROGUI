using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
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
            originalNetwork = FileWriterTestHelper.SetupSimpleHydroNetworkWith2NodesAnd1Branch("node1", "node2", "branch");
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        [Category(TestCategory.DataAccess)]
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

            var networkDefinitionFileReader = new NetworkDefinitionFileReader((header, errorMessages) => {});
            var readNetworkLocations = networkDefinitionFileReader.ReadNetworkDefinitionFile(FileWriterTestHelper.ModelFileNames.Network, readNetwork);
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
        [Category(TestCategory.DataAccess)]
        public void GivenAnIncorrectTestIniFile_WhenTryingToRead_ThenAnExceptionIsThrown()
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
            var errorReport = new List<string>();
            var networkDefinitionFileReader = new NetworkDefinitionFileReader((header, errorMessages) => { errorReport.AddRange(errorMessages); });
            Assert.Throws<Exception>(() => networkDefinitionFileReader.ReadNetworkDefinitionFile(FileWriterTestHelper.ModelFileNames.Network, new HydroNetwork()));

            Assert.That(errorReport.Count, Is.GreaterThan(0));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAFileWithZeroCategories_WhenTryingToExecuteReadFile_ThenAnFileReadingExceptionIsThrown()
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
            new IniFileWriter().WriteIniFile(new List<DelftIniCategory>(), FileWriterTestHelper.ModelFileNames.Network);

            //Read from model
            var networkDefinitionFileReader = new NetworkDefinitionFileReader((header, errorMessages) => { });
            Assert.Throws<FileReadingException>(() => 
                networkDefinitionFileReader.ReadNetworkDefinitionFile(FileWriterTestHelper.ModelFileNames.Network, new HydroNetwork()));
        }

        [Test]
        public void GivenNetworkDefinitionFileReader_WhenReadingHydroNodesFromNonExistentFilePath_ThenFileNotFoundExceptionIsThrown()
        {
            // Given
            void ErrorReportAction(string header, IList<string> errorMessages)
            {
            }

            var reader = new NetworkDefinitionFileReader(ErrorReportAction);

            // When - Then
            Assert.Throws<FileNotFoundException>(() => reader.ReadNetworkDefinitionFile("NonExistentPath.me", new HydroNetwork()));
        }
    }
}
