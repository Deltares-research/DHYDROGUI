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
            CompareAndAssertGeometryPoints(primaryBranch.Geometry, secondaryBranch.Geometry);
        }

        public static void CompareAndAssertGeometryPoints(IGeometry primaryGeometry, IGeometry secondaryGeometry)
        {
            Assert.AreEqual(primaryGeometry.Coordinates.Length, secondaryGeometry.Coordinates.Length);

            for (int i = 0; i < primaryGeometry.Coordinates.Length; i++)
            {
                Assert.AreEqual(primaryGeometry.Coordinates[i].X, secondaryGeometry.Coordinates[i].X);
                Assert.AreEqual(primaryGeometry.Coordinates[i].Y, secondaryGeometry.Coordinates[i].Y);
            }

        }
    }
}
