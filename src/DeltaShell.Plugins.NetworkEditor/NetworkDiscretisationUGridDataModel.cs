using System.Linq;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor
{
    public class NetworkDiscretisationUGridDataModel
    {
        public string Name;
        public int NetworkId;
        public int NumberOfMeshEdges;
        public int NumberOfDiscretisationPoints;
        public int[] BranchIdx = new int[0];
        public double[] Offset = new double[0];
        public NetworkDiscretisationUGridDataModel(IDiscretization discretisation)
        {
            SetNetworkDiscretisationData(discretisation);
        }

        public NetworkDiscretisationUGridDataModel(string name, int[] branchIndices, double[] offset, int networkId)
        {
            Name = name;
            BranchIdx = branchIndices;
            Offset = offset;
            NetworkId = networkId;
        }

        private void SetNetworkDiscretisationData(IDiscretization discretisation)
        {
            if (discretisation == null) return;

            // test for null etc. 

            Name = discretisation.Name;

            var discretisationPoints = discretisation.Locations.Values.ToArray();

            NumberOfDiscretisationPoints = discretisationPoints.Length;

            NumberOfMeshEdges = discretisationPoints.Length
                                - discretisation.Network.Nodes.Count
                                + discretisation.Network.Branches.Count;

            BranchIdx = discretisationPoints.Select(l => l.Branch)
                .ToArray()
                .Select(b => discretisation.Network.Branches.IndexOf(b) + 1)     // Index must be 1-based, not 0-based
                .ToArray();

            Offset = discretisationPoints.Select(l => l.Chainage).ToArray();
        }

        public static IDiscretization ReconstructNetworkDiscretisation(INetwork network, string name, int[] branchIndices, double[] offset)
        {
            if (network == null)
            {
                return null;
            }

            if (network.Branches == null)
            {
                return null;
            }

            var discretisation = new Discretization
            {
                Name = name,
                Network = network,
            };

            // check if size of branchindices and offsets are equal and > 0.
            if (branchIndices.Length != offset.Length)
            {
                return null;// throw new Exception(string.Format("Can't reconstruct the network discretisation because the "));
            }

            // make int[] of unique branch indices
            var uniqueBranchIndices = branchIndices.Distinct();
            
            // get the size and check if there are that many branches.
            if (network.Branches.Count != uniqueBranchIndices.Count())
            {
                return null;
            }

            for (int i = 0; i < branchIndices.Length; i++)
            {
                var branchIndex = branchIndices[i];
                var branch = network.Branches[branchIndex];

                var networkLocation = new NetworkLocation(branch, offset[i]);

                discretisation.Locations.Values.Add(networkLocation);
            }

            return discretisation;
        }
    }
}
