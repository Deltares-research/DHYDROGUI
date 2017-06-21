using DelftTools.Hydro;
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

        public static void CompareAndAssertNetworks(IHydroNetwork primaryNetwork, IHydroNetwork secondaryNetwork)
        {
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
    }
}
