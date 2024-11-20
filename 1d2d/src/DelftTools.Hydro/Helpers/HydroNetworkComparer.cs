using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Geometries;

namespace DelftTools.Hydro.Helpers
{
    public class HydroNetworkComparer : IEqualityComparer<IHydroNetwork>
    {
        public bool Equals(IHydroNetwork x, IHydroNetwork y)
        {
            return NetworksEqual(x, y,
                n => ((IHydroNetwork) n).HydroNodes.OfType<INode>().ToArray(),
                n => ((IHydroNetwork) n).Channels.OfType<IBranch>().ToArray()) && NetworksSewersEqual(x, y);
        }

        private bool NetworksSewersEqual(IHydroNetwork primaryNetwork, IHydroNetwork secondaryNetwork)
        {
            var primaryManholes = primaryNetwork.Manholes.ToList();
            var secondaryManholes = secondaryNetwork.Manholes.ToList();
            var primaryPipes = primaryNetwork.Pipes.ToList();
            var secondaryPipes = secondaryNetwork.Pipes.ToList();

            if (!Equals(primaryManholes.Count, secondaryManholes.Count)) return false;
            if (!Equals(primaryPipes.Count, secondaryPipes.Count)) return false;

            // loop over the manholes and assert each item
            for (var i = 0; i < primaryManholes.Count; ++i)
            {
                var primaryManhole = primaryManholes[i];
                var secondaryManhole = secondaryManholes[i];

                if (!ManholesEqual(primaryManhole, secondaryManhole)) return false;
            }

            // loop over the pipes and assert each item
            for (var i = 0; i < primaryPipes.Count; ++i)
            {
                var primaryPipe = primaryPipes[i];
                var secondaryPipe = secondaryPipes[i];

                if (!PipesEqual(primaryPipe, secondaryPipe)) return false;
            }

            return true;
        }

        private bool PipesEqual(IPipe primaryPipe, IPipe secondaryPipe)
        {
            if (!Equals(primaryPipe.Material, secondaryPipe.Material)) return false;
            if (!Equals(primaryPipe.Profile.Shape.Type, secondaryPipe.Profile.Shape.Type)) return false;
            return true;
        }

        private bool ManholesEqual(IManhole primaryManhole, IManhole secondaryManhole)
        {
            if (!Equals(primaryManhole.Compartments.Count, secondaryManhole.Compartments.Count)) return false;
            foreach (var primaryCompartment in primaryManhole.Compartments)
            {
                var secondaryCompartment = secondaryManhole.Compartments.FirstOrDefault(c => c.Name.Equals(primaryCompartment.Name));
                if ( secondaryCompartment == null ) return false;
                if (!Equals(primaryCompartment.BottomLevel, secondaryCompartment.BottomLevel)) return false;
                if (!Equals(primaryCompartment.SurfaceLevel, secondaryCompartment.SurfaceLevel)) return false;
                if (!Equals(primaryCompartment.ManholeLength, secondaryCompartment.ManholeLength)) return false;
                if (!Equals(primaryCompartment.ManholeWidth, secondaryCompartment.ManholeWidth)) return false;
                if (!Equals(primaryCompartment.ParentManhole.Name, secondaryCompartment.ParentManhole.Name)) return false;
            }

            return true;
        }

        private bool NetworksEqual(IHydroNetwork primaryNetwork, IHydroNetwork secondaryNetwork, Func<INetwork, INode[]> getNodes = null, Func<INetwork, IBranch[]> getBranches = null)
        {
            if (!Equals(primaryNetwork.Name, secondaryNetwork.Name)) return false;
            if (!Equals(primaryNetwork.CoordinateSystem.AuthorityCode, secondaryNetwork.CoordinateSystem.AuthorityCode)) return false;

            if (getNodes == null)
            {
                getNodes = (n) => n.Nodes.ToArray();
            }

            if (getBranches == null)
            {
                getBranches = (n) => n.Branches.ToArray();
            }

            var primaryNodes = getNodes(primaryNetwork);
            var secondaryNodes = getNodes(secondaryNetwork);
            var primaryBranches = getBranches(primaryNetwork);
            var secondaryBranches = getBranches(secondaryNetwork);

            if (!Equals(primaryNodes.Length, secondaryNodes.Length)) return false;
            if (!Equals(primaryBranches.Length, secondaryBranches.Length)) return false;

            // loop over the nodes and assert each item
            for (var i = 0; i < primaryNodes.Length; ++i)
            {
                var primaryNode = primaryNodes[i];
                var secondaryNode = secondaryNodes[i];

                if (!NodesEqual(primaryNode, secondaryNode))
                    return false;
            }

            // loop over the branches and assert each item
            for (var i = 0; i < primaryBranches.Length; ++i)
            {
                var primaryBranch = primaryBranches[i];
                var secondaryBranch = secondaryBranches[i];

                if (!BranchesEqual(primaryBranch, secondaryBranch))
                    return false;
            }

            return true;
        }

        private bool BranchesEqual(IBranch primaryBranch, IBranch secondaryBranch)
        {
            if (!Equals(primaryBranch.Name, secondaryBranch.Name)) return false;
            if (!Equals(primaryBranch.Description, secondaryBranch.Description)) return false;
            if (!Equals(primaryBranch.Length, secondaryBranch.Length)) return false;

            // Compare nodes
            if (!NodesEqual(primaryBranch.Source, secondaryBranch.Source)) return false;
            if (!NodesEqual(primaryBranch.Target, secondaryBranch.Target)) return false;

            // Compare geometries
            if (!BranchesGeometriesEqual(primaryBranch.Geometry, secondaryBranch.Geometry)) return false;
            return true;
        }

        private bool BranchesGeometriesEqual(IGeometry primaryBranchGeometry, IGeometry secondaryBranchGeometry)
        {
            if (!Equals(primaryBranchGeometry.Coordinates.Length, secondaryBranchGeometry.Coordinates.Length)) return false;
            var comparator = new CoordinateComparison2D();
            for (int i = 0; i < primaryBranchGeometry.Coordinates.Length; i++)
            {
                if (!comparator.Equals(primaryBranchGeometry.Coordinates[i], secondaryBranchGeometry.Coordinates[i])) return false;
            }
            return true;
        }

        private bool NodesEqual(INode primaryNode, INode secondaryNode)
        {
            if (!Equals(primaryNode.Name, secondaryNode.Name)) return false;
            if (!Equals(primaryNode.Geometry.Coordinate.X, secondaryNode.Geometry.Coordinate.X)) return false;
            if (!Equals(primaryNode.Geometry.Coordinate.Y, secondaryNode.Geometry.Coordinate.Y)) return false;
            if (!(primaryNode is IHydroNode)) return false;
            if (!(secondaryNode is IHydroNode)) return false;
            if (!Equals(((IHydroNode)primaryNode).LongName, ((IHydroNode)secondaryNode).LongName)) return false;
            return true;
        }

        public int GetHashCode(IHydroNetwork obj)
        {
            return obj.GetType().GetHashCode();
        }

    }
}