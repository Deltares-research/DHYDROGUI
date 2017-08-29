using DelftTools.Hydro;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Helpers
{
    public static class HydroNetworkTestHelper
    {
        public static void CompareAndAssertNodes(INode primaryNode, INode secondaryNode)
        {
            Assert.AreEqual(primaryNode.Name, secondaryNode.Name);
            Assert.AreEqual(primaryNode.Geometry.Coordinate.X, secondaryNode.Geometry.Coordinate.X);
            Assert.AreEqual(primaryNode.Geometry.Coordinate.Y, secondaryNode.Geometry.Coordinate.Y);
            Assert.AreEqual(primaryNode.Name, secondaryNode.Name);
            Assert.AreEqual(primaryNode.Description, secondaryNode.Description);
        }

        public static void CompareAndAssertBranches(IBranch primaryBranch, IBranch secondaryBranch)
        {
            Assert.AreEqual(primaryBranch.Name, secondaryBranch.Name);
            Assert.AreEqual(primaryBranch.Description, secondaryBranch.Description);
            Assert.AreEqual(primaryBranch.Length, secondaryBranch.Length);

            // Compare nodes
            CompareAndAssertNodes(primaryBranch.Source, secondaryBranch.Source);
            CompareAndAssertNodes(primaryBranch.Target, secondaryBranch.Target);

            // Compare geometries
            CompareAndAssertGeometry(primaryBranch.Geometry, secondaryBranch.Geometry);
        }

        private static void CompareAndAssertGeometry(IGeometry primaryGeometry, IGeometry secondaryGeometry)
        {
            Assert.AreEqual(primaryGeometry.Coordinates.Length, secondaryGeometry.Coordinates.Length);

            for (int i = 0; i < primaryGeometry.Coordinates.Length; i++)
            {
                Assert.AreEqual(primaryGeometry.Coordinates[i].X, secondaryGeometry.Coordinates[i].X);
                Assert.AreEqual(primaryGeometry.Coordinates[i].Y, secondaryGeometry.Coordinates[i].Y);
            }
        }

        public static void CompareAndAssertNetworks(INetwork primaryNetwork, INetwork secondaryNetwork)
        {
            Assert.AreEqual(primaryNetwork.Name, secondaryNetwork.Name);
            Assert.AreEqual(primaryNetwork.CoordinateSystem, secondaryNetwork.CoordinateSystem);

            var primaryNodes = primaryNetwork.Nodes;
            var secondaryNodes = secondaryNetwork.Nodes;
            var primaryBranches = primaryNetwork.Branches;
            var secondaryBranches = secondaryNetwork.Branches;

            Assert.AreEqual(primaryNodes.Count, secondaryNodes.Count);
            Assert.AreEqual(primaryBranches.Count, secondaryBranches.Count);

            // loop over the nodes and assert each item
            for (int i = 0; i < primaryNodes.Count; ++i)
            {
                var primaryNode = primaryNodes[i];
                var secondaryNode = secondaryNodes[i];

                CompareAndAssertNodes(primaryNode, secondaryNode);
            }

            // loop over the branches and assert each item
            for (int i = 0; i < primaryBranches.Count; ++i)
            {
                var primaryBranch = primaryBranches[i];
                var secondaryBranch = secondaryBranches[i];

                CompareAndAssertBranches(primaryBranch, secondaryBranch);
            }
        }

        public static void CompareAndAssertDiscretisations(IDiscretization primaryDiscretisation, IDiscretization secondaryDiscretisation)
        {
            Assert.AreEqual(primaryDiscretisation.Name, secondaryDiscretisation.Name);
            Assert.AreEqual(primaryDiscretisation.Locations.Values.Count, secondaryDiscretisation.Locations.Values.Count);

            CompareAndAssertNetworks(primaryDiscretisation.Network, secondaryDiscretisation.Network);

            for (int i = 0; i < primaryDiscretisation.Locations.Values.Count; i++)
            {
                var primaryLocation = primaryDiscretisation.Locations.Values[i];
                var secondaryLocation = secondaryDiscretisation.Locations.Values[i];
                
                Assert.AreEqual(primaryLocation.Chainage, secondaryLocation.Chainage);
                Assert.AreEqual(primaryLocation.Name, secondaryLocation.Name);
                Assert.AreEqual(primaryLocation.Description, secondaryLocation.Description);
                CompareAndAssertBranches(primaryLocation.Branch, secondaryLocation.Branch);
            }
        }
    }
}
