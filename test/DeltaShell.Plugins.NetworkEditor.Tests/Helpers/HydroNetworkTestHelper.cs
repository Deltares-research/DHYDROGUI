using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Helpers
{
    public static class HydroNetworkTestHelper
    {
        public static void CompareNetworks(INetwork primaryNetwork, INetwork secondaryNetwork)
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
            for (var i = 0; i < primaryNodes.Count; ++i)
            {
                var primaryNode = primaryNodes[i];
                var secondaryNode = secondaryNodes[i];

                CompareNodes(primaryNode, secondaryNode);
            }

            // loop over the branches and assert each item
            for (var i = 0; i < primaryBranches.Count; ++i)
            {
                var primaryBranch = primaryBranches[i];
                var secondaryBranch = secondaryBranches[i];

                CompareBranches(primaryBranch, secondaryBranch);
            }
        }

        public static void CompareNetworks(IHydroNetwork primaryNetwork, IHydroNetwork secondaryNetwork)
        {
            CompareNetworks((INetwork)primaryNetwork, secondaryNetwork);

            var primaryManholes = primaryNetwork.Manholes.ToList();
            var secondaryManholes = secondaryNetwork.Manholes.ToList();
            var primaryPipes = primaryNetwork.Pipes.ToList();
            var secondaryPipes = secondaryNetwork.Pipes.ToList();

            Assert.AreEqual(primaryManholes.Count, secondaryManholes.Count);
            Assert.AreEqual(primaryPipes.Count, secondaryPipes.Count);

            // loop over the manholes and assert each item
            for (var i = 0; i < primaryManholes.Count; ++i)
            {
                var primaryManhole = primaryManholes[i];
                var secondaryManhole = secondaryManholes[i];

                CompareManholes(primaryManhole, secondaryManhole);
            }

            // loop over the pipes and assert each item
            for (var i = 0; i < primaryPipes.Count; ++i)
            {
                var primaryPipe = (Pipe)primaryPipes[i];
                var secondaryPipe = (Pipe)secondaryPipes[i];

                ComparePipes(primaryPipe, secondaryPipe);
            }
        }

        public static void CompareNodes(INode primaryNode, INode secondaryNode)
        {
            Assert.AreEqual(primaryNode.Name, secondaryNode.Name);
            Assert.AreEqual(primaryNode.Geometry.Coordinate.X, secondaryNode.Geometry.Coordinate.X);
            Assert.AreEqual(primaryNode.Geometry.Coordinate.Y, secondaryNode.Geometry.Coordinate.Y);
            Assert.AreEqual(primaryNode.Description, secondaryNode.Description);
        }

        private static void CompareManholes(IManhole primaryManhole, IManhole secondaryManhole)
        {
            Assert.AreEqual(primaryManhole.Compartments.Count, secondaryManhole.Compartments.Count);
            foreach (var primaryCompartment in primaryManhole.Compartments)
            {
                var secondaryCompartment = secondaryManhole.Compartments.FirstOrDefault(c => c.Name.Equals(primaryCompartment.Name));
                Assert.IsNotNull(secondaryCompartment);
                Assert.That(primaryCompartment.BottomLevel, Is.EqualTo(secondaryCompartment.BottomLevel));
                Assert.That(primaryCompartment.SurfaceLevel, Is.EqualTo(secondaryCompartment.SurfaceLevel));
                Assert.That(primaryCompartment.ManholeLength, Is.EqualTo(secondaryCompartment.ManholeLength));
                Assert.That(primaryCompartment.ManholeWidth, Is.EqualTo(secondaryCompartment.ManholeWidth));
                Assert.That(primaryCompartment.ParentManhole.Name, Is.EqualTo(secondaryCompartment.ParentManhole.Name));
            }
        }

        public static void CompareBranches(IBranch primaryBranch, IBranch secondaryBranch)
        {
            Assert.AreEqual(primaryBranch.Name, secondaryBranch.Name);
            Assert.AreEqual(primaryBranch.Description, secondaryBranch.Description);
            Assert.AreEqual(primaryBranch.Length, secondaryBranch.Length);

            // Compare nodes
            CompareNodes(primaryBranch.Source, secondaryBranch.Source);
            CompareNodes(primaryBranch.Target, secondaryBranch.Target);

            // Compare geometries
            CompareGeometries(primaryBranch.Geometry, secondaryBranch.Geometry);
        }

        private static void ComparePipes(Pipe primaryPipe, Pipe secondaryPipe)
        {
            Assert.That(primaryPipe.Material, Is.EqualTo(secondaryPipe.Material));
            Assert.That(primaryPipe.PipeRoughness, Is.EqualTo(secondaryPipe.PipeRoughness));
            Assert.That(primaryPipe.PipeRoughnessType, Is.EqualTo(secondaryPipe.PipeRoughnessType));
            //Assert.That(primaryPipe.CrossSectionDefinition.Shape.Type, Is.EqualTo(secondaryPipe.CrossSectionDefinition.Shape.Type)); // To add when we can write/read cross section definitions
        }

        private static void CompareGeometries(IGeometry primaryGeometry, IGeometry secondaryGeometry)
        {
            Assert.AreEqual(primaryGeometry.Coordinates.Length, secondaryGeometry.Coordinates.Length);

            for (int i = 0; i < primaryGeometry.Coordinates.Length; i++)
            {
                Assert.AreEqual(primaryGeometry.Coordinates[i].X, secondaryGeometry.Coordinates[i].X);
                Assert.AreEqual(primaryGeometry.Coordinates[i].Y, secondaryGeometry.Coordinates[i].Y);
            }
        }

        public static void CompareDiscretisations(IDiscretization primaryDiscretisation, IDiscretization secondaryDiscretisation)
        {
            Assert.AreEqual(primaryDiscretisation.Name, secondaryDiscretisation.Name);
            Assert.AreEqual(primaryDiscretisation.Locations.Values.Count, secondaryDiscretisation.Locations.Values.Count);

            CompareNetworks(primaryDiscretisation.Network, secondaryDiscretisation.Network);

            for (int i = 0; i < primaryDiscretisation.Locations.Values.Count; i++)
            {
                var primaryLocation = primaryDiscretisation.Locations.Values[i];
                var secondaryLocation = secondaryDiscretisation.Locations.Values[i];
                
                Assert.AreEqual(primaryLocation.Chainage, secondaryLocation.Chainage);
                Assert.AreEqual(primaryLocation.Name, secondaryLocation.Name);
                Assert.AreEqual(primaryLocation.Description, secondaryLocation.Description);
                CompareBranches(primaryLocation.Branch, secondaryLocation.Branch);
            }
        }
    }
}
