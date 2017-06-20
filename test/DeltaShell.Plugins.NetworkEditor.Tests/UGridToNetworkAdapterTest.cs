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

                // TODO: This must be done fancier. With a function. And stuff.
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
                //UGridToNetworkAdapter.SaveNetworkDiscretisation(networkDiscretization, localCopyOfTestFile);

                var loadedNetwork = UGridToNetworkAdapter.LoadNetwork(testFilePath);

                Assert.AreEqual(loadedNetwork.Name, "DummyNetworkName"); // TODO: Implement the read/get network name functionality

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
                //if (storedNode.Name != null)
                //{
                //    storedNode.Name = storedNode.Name.Trim().Replace(" ", "_");
                //}
                //if (storedNode.Description != null)
                //{
                //    storedNode.Description = storedNode.Description.Trim().Replace(" ", "_");
                //}
                HydroNetworkTestHelper.CompareAndAssertNodes(storedNode, loadedNode);
                
                //// test node names
                //string storedNodeName = storedNodes[i].Name.Trim().Replace(" ", "_");
                //string loadedNodeName = loadedNodes[i].Name.Trim();
                //Assert.AreEqual(storedNodeName, loadedNodeName);

                //// test x coordinate
                //double storedNodeCoordinateX = storedNodes[i].Geometry.Coordinates[0].X;
                //double loadedNodeCoordinateX = loadedNodes[i].Geometry.Coordinates[0].X;
                //Assert.AreEqual(storedNodeCoordinateX, loadedNodeCoordinateX);

                //// test y coordinate
                //double storedNodeCoordinateY = storedNodes[i].Geometry.Coordinates[0].Y;
                //double loadedNodeCoordinateY = loadedNodes[i].Geometry.Coordinates[0].Y;
                //Assert.AreEqual(storedNodeCoordinateY, loadedNodeCoordinateY);

                //// test node description
                //string storedNodeDescription = storedNodes[i].Description != null
                //    ? storedNodes[i].Description.Trim().Replace(" ", "_")
                //    : "";
                //string loadedNodeDescription = loadedNodes[i].Description.Trim();
                //Assert.AreEqual(storedNodeDescription, loadedNodeDescription);
            }

            // loop over the branches and assert each item
            for (int i = 0; i < storedBranches.Count; ++i)
            {
                var storedBranch = storedBranches[i];
                var loadedBranch = loadedBranches[i];
                //storedBranch.Name = storedBranch.Name.Trim().Replace(" ", "_");
                //storedBranch.Description = storedBranch.Description.Trim().Replace(" ", "_");

                HydroNetworkTestHelper.CompareAndAssertBranches(storedBranch, loadedBranch);

                //// test source nodes
                //INode storedBranchSourceNode = storedBranch.Source;
                //storedBranchSourceNode.Name = storedBranchSourceNode.Name.Replace(" ", "_");
                //INode loadedBranchSourceNode = loadedBranch.Source;
                //HydroNetworkTestHelper.CompareAndAssertNodes(storedBranchSourceNode, loadedBranchSourceNode);

                //// test target nodes
                //INode storedBranchTargetNode = storedBranch.Target;
                //storedBranchTargetNode.Name = storedBranchTargetNode.Name.Replace(" ", "_");
                //INode loadedBranchTargetNode = loadedBranch.Target;
                //HydroNetworkTestHelper.CompareAndAssertNodes(storedBranchTargetNode, loadedBranchTargetNode);
                
                //// test branch lengths
                //var storedBranchLength = storedBranch.Length;
                //var loadedBranchLength = loadedBranch.Length;
                //Assert.AreEqual(storedBranchLength, loadedBranchLength);

                //// test number of geometry points per branch
                //var storedBranchGeometryPointsCount = storedBranch.Geometry.Coordinates.Length;
                //var loadedBranchGeometryPointsCount = loadedBranch.Geometry.Coordinates.Length;
                //Assert.AreEqual(storedBranchGeometryPointsCount, loadedBranchGeometryPointsCount);

                //// test branch names
                //var storedBranchName = storedBranch.Name.Trim().Replace(" ", "_");
                //var loadedBranchName = loadedBranch.Name.Trim();
                //Assert.AreEqual(storedBranchName, loadedBranchName);

                //// test branch descriptions
                //var storedBranchDescription = storedBranch.Description != null
                //    ? storedBranch.Description.Trim().Replace(" ", "_")
                //    : "";
                //var loadedBranchDescription = loadedBranch.Description.Trim();
                //Assert.AreEqual(storedBranchDescription, loadedBranchDescription);
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
    }
}
