using System.IO;
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
        [TestCase(true, "simple_network_testFile.nc")]
        [TestCase(false, "save_load_network_testFile.nc")]
        [Category(TestCategory.DataAccess)]
        public void SaveAndLoadNetworkTest(bool useSimpleNetwork, string netFilePath)
        {
            var testFilePath =
            TestHelper.GetTestFilePath(UGRID_TEST_FOLDER + netFilePath);
            var testFolderPath = Path.GetDirectoryName(testFilePath);
            FileUtils.CreateDirectoryIfNotExists(testFolderPath);
            FileUtils.DeleteIfExists(testFilePath);
            try
            {
                var networkDiscretisation = useSimpleNetwork 
                    ? TestNetworkAndDiscretisationProvider.CreateSimpleNetworkAndDiscretisation() 
                    : TestNetworkAndDiscretisationProvider.CreateNetworkAndDiscretisation();
                var storedNetwork = (IHydroNetwork)networkDiscretisation.Network;

                UGridGlobalMetaData metaData = new UGridGlobalMetaData(storedNetwork.Name, "PluginName", "PluginVersion");
                UGridToNetworkAdapter.SaveNetwork(storedNetwork, testFilePath, metaData);

                var loadedNetwork = UGridToNetworkAdapter.LoadNetwork(testFilePath);
                Assert.AreEqual(loadedNetwork.Name, "DummyNetworkName"); // TODO: Implement the read/get network name functionality

                // Spaces in names are replaced by underscores while storing the network object. Do the same action for the network which is not stored.
                ReplaceSpacesInStrings(storedNetwork);
                
                HydroNetworkTestHelper.CompareAndAssertNetworks(storedNetwork, loadedNetwork);
            }
            finally
            {
                FileUtils.DeleteIfExists(testFilePath);
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
