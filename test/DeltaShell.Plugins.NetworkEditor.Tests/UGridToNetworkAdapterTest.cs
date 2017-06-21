using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.NetworkEditor.Tests.Helpers;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture]
    public class UGridToNetworkAdapterTest
    {
        private const string UGRID_TEST_FOLDER = @"ugrid\";
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void SaveAndLoadSimpleNetworkTest()
        {
            var testFilePath =
            TestHelper.GetTestFilePath(UGRID_TEST_FOLDER + "simple_network_testFile.nc");
            var testFolderPath = Path.GetDirectoryName(testFilePath);
            FileUtils.CreateDirectoryIfNotExists(testFolderPath);
            FileUtils.DeleteIfExists(testFilePath);
            try
            {
                var networkDiscretisation = TestNetworkAndDiscretisationProvider.CreateSimpleNetworkAndDiscretisation();
                var storedNetwork = (IHydroNetwork)networkDiscretisation.Network;

                UGridGlobalMetaData metaData = new UGridGlobalMetaData(storedNetwork.Name, "PluginName", "PluginVersion");
                
                UGridToNetworkAdapter.SaveNetwork(storedNetwork, testFilePath, metaData);

                var loadedNetwork = UGridToNetworkAdapter.LoadNetwork(testFilePath);

                Assert.AreEqual(loadedNetwork.Name, "DummyNetworkName"); // TODO: Implement the read/get network name functionality

                // Spaces in names are replaced by underscores while storing the network object. Do the same action for the network which is not stored.
                ReplaceSpacesInStrings(storedNetwork);
                
                CompareAndAssertNetworks(storedNetwork, loadedNetwork);
            }
            finally
            {
                FileUtils.DeleteIfExists(testFilePath);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void SaveAndLoadNetworkTest()
        {
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FOLDER + "save_load_network_testFile.nc");
            var testFolderPath = Path.GetDirectoryName(testFilePath);
            FileUtils.CreateDirectoryIfNotExists(testFolderPath);
            FileUtils.DeleteIfExists(testFilePath);

            try
            {
                var networkDiscretization = TestNetworkAndDiscretisationProvider.CreateNetworkAndDiscretisation();
                var storedNetwork = (IHydroNetwork)networkDiscretization.Network;
                
                UGridGlobalMetaData metaData = new UGridGlobalMetaData(storedNetwork.Name, "PluginName", "PluginVersion");

                UGridToNetworkAdapter.SaveNetwork(storedNetwork, testFilePath, metaData);

                var loadedNetwork = UGridToNetworkAdapter.LoadNetwork(testFilePath);

                Assert.AreEqual(loadedNetwork.Name, "DummyNetworkName"); // TODO: Implement the read/get network name functionality

                // Spaces in names are replaced by underscores while storing the network object. Do the same action for the network which is not stored.
                ReplaceSpacesInStrings(storedNetwork);
                
                CompareAndAssertNetworks((HydroNetwork)storedNetwork, loadedNetwork);
            }
            finally
            {
                FileUtils.DeleteIfExists(testFilePath);
            }
        }

        private static void CompareAndAssertNetworks(IHydroNetwork storedNetwork, IHydroNetwork loadedNetwork)
        {
            var storedNodes = storedNetwork.Nodes;
            var loadedNodes = loadedNetwork.Nodes;
            var storedBranches = storedNetwork.Branches;
            var loadedBranches = loadedNetwork.Branches;

            Assert.AreEqual(storedNodes.Count, loadedNodes.Count);
            Assert.AreEqual(storedBranches.Count, loadedBranches.Count);

            // loop over the nodes and assert each item
            for (int i = 0; i < storedNodes.Count; ++i)
            {
                var storedNode = storedNodes[i];
                var loadedNode = loadedNodes[i];

                HydroNetworkTestHelper.CompareAndAssertNodes(storedNode, loadedNode);
            }

            // loop over the branches and assert each item
            for (int i = 0; i < storedBranches.Count; ++i)
            {
                var storedBranch = storedBranches[i];
                var loadedBranch = loadedBranches[i];
               
                HydroNetworkTestHelper.CompareAndAssertBranches(storedBranch, loadedBranch);
            }
            
            var storedGeometryPoints = storedNetwork.Branches.SelectMany(b => b.Geometry.Coordinates).ToList();
            var loadedGeometryPoints = loadedNetwork.Branches.SelectMany(b => b.Geometry.Coordinates).ToList();

            Assert.AreEqual(storedGeometryPoints.Count, loadedGeometryPoints.Count);
            // loop over geometrypoints and assert each item
            for (int i = 0; i < storedGeometryPoints.Count; ++i)
            {
                Assert.AreEqual(storedGeometryPoints[i].X, loadedGeometryPoints[i].X);
                Assert.AreEqual(storedGeometryPoints[i].Y, loadedGeometryPoints[i].Y);
            }
        }

        private void ReplaceSpacesInStrings(IHydroNetwork storedNetwork)
        {
            foreach (var node in storedNetwork.Nodes)
            {
                if (node.Name != null)
                {
                    node.Name = node.Name.Trim().Replace(" ", "_");
                }
                if (node.Description != null)
                {
                    node.Description = node.Description.Trim().Replace(" ", "_");
                }
            }

            foreach (var branch in storedNetwork.Branches)
            {
                if (branch.Name != null)
                {
                    branch.Name = branch.Name.Trim().Replace(" ", "_");
                }
                if (branch.Description != null)
                {
                    branch.Description = branch.Description.Trim().Replace(" ", "_");
                }
            }
        }
    }
}
