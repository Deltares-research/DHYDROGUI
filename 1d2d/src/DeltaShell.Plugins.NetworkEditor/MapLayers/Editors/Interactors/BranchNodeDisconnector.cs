using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using Deltares.Infrastructure.API.Guards;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors
{
    /// <summary>
    /// Class to disconnect nodes from branches.
    /// </summary>
    public class BranchNodeDisconnector
    {
        /// <summary>
        /// Replace the node on the connected branches with a manhole if needed.
        /// </summary>
        /// <param name="node"> The branch node to disconnect. </param>
        /// <param name="hydroNetwork"> The hydro network. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="node"/> or <paramref name="hydroNetwork"/> is
        /// <c>null</c>.
        /// </exception>
        public void DisconnectNodes(INode node, IHydroNetwork hydroNetwork)
        {
            Ensure.NotNull(node, nameof(node));
            Ensure.NotNull(hydroNetwork, nameof(hydroNetwork));

            Disconnect(node, hydroNetwork);
        }

        private static void Disconnect(INode node, IHydroNetwork hydroNetwork)
        {
            IBranch[] connectedBranches = node.IncomingBranches.Concat(node.OutgoingBranches).ToArray();

            if (!NodeShouldBeReplacedWithManhole(connectedBranches))
            {
                return;
            }

            INode newManhole = SewerFactory.CreateDefaultManholeAndAddToNetwork(hydroNetwork, node.Geometry.Coordinate);

            foreach (IBranch branch in connectedBranches)
            {
                if (branch.Source.Equals(node))
                {
                    branch.Source = newManhole;
                }

                if (branch.Target.Equals(node))
                {
                    branch.Target = newManhole;
                }
            }

            hydroNetwork.Nodes.Remove(node);
        }

        private static bool NodeShouldBeReplacedWithManhole(IBranch[] connectedBranches)
        {
            bool hasBranches = connectedBranches.Any();
            if (!hasBranches)
            {
                return false;
            }

            bool hasChannels = connectedBranches.OfType<IChannel>().Any();
            return !hasChannels;
        }
    }
}