using System.Linq;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.NGHS.IO.Grid
{
    public class NetworkDiscretisationUGridDataModel
    {
        public string Name;
        public int NetworkId;
        public int NumberOfMeshEdges;
        public int NumberOfDiscretisationPoints;
        public int[] BranchIdx = new int[0];
        public double[] Offsets = new double[0];
        public double[] DiscretisationPointsX = new double[0];
        public double[] DiscretisationPointsY = new double[0];
        public string[] DiscretisationPointIds = new string[0];
        public string[] DiscretisationPointDescriptions = new string[0];

        public NetworkDiscretisationUGridDataModel(IDiscretization discretisation)
        {
            SetNetworkDiscretisationData(discretisation);
        }

        public NetworkDiscretisationUGridDataModel(string name, int[] branchIndices, double[] offsets, double[] discretisationPointsX, double[] discretisationPointsY, int networkId, string[] discretisationPointIds, string[] discretisationPointDescriptions)
        {
            Name = name;
            BranchIdx = branchIndices;
            Offsets = offsets;
            DiscretisationPointsX = discretisationPointsX;
            DiscretisationPointsY = discretisationPointsY;
            NetworkId = networkId;
            DiscretisationPointIds = discretisationPointIds;
            DiscretisationPointDescriptions = discretisationPointDescriptions;
        }

        private void SetNetworkDiscretisationData(IDiscretization discretisation)
        {
            if (discretisation == null) return;

            // test for null etc. 

            Name = discretisation.Name;

            var discretisationPoints = discretisation.Locations.Values.ToArray();

            NumberOfDiscretisationPoints = discretisationPoints.Length;

            if (discretisation.Network != null)
            {
                NumberOfMeshEdges = discretisationPoints.Length
                                    - discretisation.Network.Nodes.Count
                                    + discretisation.Network.Branches.Count;

                BranchIdx = discretisationPoints.Select(l => l.Branch)
                    .ToArray()
                    .Select(b => discretisation.Network.Branches.IndexOf(b))
                    .ToArray();
            }

            Offsets = discretisationPoints.Select(l => l.Chainage).ToArray();

            DiscretisationPointIds = discretisationPoints.Select(p => p.Name).ToArray();
            DiscretisationPointDescriptions = discretisationPoints.Select(p => p.LongName).ToArray();
            DiscretisationPointsX = discretisationPoints.Select(dp => dp.Geometry.Coordinate.X).ToArray();
            DiscretisationPointsY = discretisationPoints.Select(dp => dp.Geometry.Coordinate.Y).ToArray();
        }
    }
}
