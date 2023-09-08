using System.Linq;
using DelftTools.Hydro.Properties;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors
{
    /// <summary>
    /// Class to connect branches to nodes.
    /// </summary>
    public class BranchNodeConnector
    {
        /// <summary>
        /// Sets the source and target nodes of the provided branch.
        /// The node can be an existing node from the network or a newly created node.
        /// </summary>
        /// <param name="branch"> The branch to set the nodes for. </param>
        /// <param name="network"> The network. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="branch"/> or <paramref name="network"/> is <c>null</c>.
        /// </exception>
        public void ConnectNodes(IBranch branch, INetwork network)
        {
            Ensure.NotNull(branch, nameof(branch));
            Ensure.NotNull(network, nameof(network));

            Coordinate[] coordinates = branch.Geometry.Coordinates;

            branch.Source = GetExistingOrNewNode(network, coordinates[0]);
            branch.Target = GetExistingOrNewNode(network, coordinates[coordinates.Length - 1]);
        }

        private static INode GetExistingOrNewNode(INetwork network, Coordinate coordinate)
        {
            INode node = network.Nodes.FirstOrDefault(n => IsSameCoordinate(coordinate, n));

            var manhole = node as IManhole;
            if (node != null && manhole == null)
            {
                return node;
            }

            INode newNode = CreateNode(network, coordinate);

            if (manhole != null)
            {
                ReplaceManhole(manhole, newNode);
            }

            network.Nodes.Add(newNode);
            network.Nodes.Remove(manhole);

            return newNode;
        }

        private static void ReplaceManhole(IManhole manhole, INode newNode)
        {
            foreach (IBranch incomingBranch in manhole.IncomingBranches.ToArray())
            {
                incomingBranch.Target = newNode;
            }

            foreach (IBranch outgoingBranch in manhole.OutgoingBranches.ToArray())
            {
                outgoingBranch.Source = newNode;
            }
        }

        private static INode CreateNode(INetwork network, Coordinate coordinate)
        {
            INode node = network.NewNode();
            node.Name = NetworkHelper.GetUniqueName(Resources.UniqueNodeNameFilter, network.Nodes, "Node");
            node.Geometry = new Point(coordinate);

            return node;
        }

        private static bool IsSameCoordinate(Coordinate coordinate, INode node) => 
            node.Geometry.Coordinate.Equals2D(coordinate);
    }
}