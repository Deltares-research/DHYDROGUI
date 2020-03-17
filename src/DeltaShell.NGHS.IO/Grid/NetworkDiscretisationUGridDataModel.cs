using System;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

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

            var discretisationPoints = discretisation.Locations.Values.GroupBy(lv => lv.Geometry.Coordinate).Select(crdGroup => crdGroup.First()).ToArray();


            NumberOfDiscretisationPoints = discretisationPoints.Length;

            if (discretisation.Network != null)
            {
                /*NumberOfMeshEdges = discretisationPoints.Length
                                    - discretisation.Network.Nodes.Count
                                    + discretisation.Network.Branches.Count;
*/

                BranchIdx = discretisationPoints.Select(l => l.Branch)
                    .ToArray()
                    .Select(b => discretisation.Network.Branches.IndexOf(b))
                    .ToArray();
                
            }

            Offsets = discretisationPoints.Select(l => l.Branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(l.Chainage)).ToArray();

            DiscretisationPointIds = discretisationPoints.Select(p => p.Name).ToArray();
            //Branch1dMeshCalculationNodesIdx = discretisationPoints.Select(l => l.) 
                /*discretisationPoints.Select(l => l.Branch)
                .Select(b => discretisation.Network.Nodes.IndexOf(b.Source)).ToArray();*/
                /*.Plus(discretisationPoints.Select(l => l.Branch)
                    .Select(b => discretisation.Network.Nodes.IndexOf(b.Target))).ToArray();
*/

            DiscretisationPointDescriptions = discretisationPoints.Select(p => p.LongName).ToArray();
            DiscretisationPointsX = discretisationPoints.Select(dp => Math.Round(dp.Geometry.Coordinate.X - 0.0000005, 6)).ToArray();
            DiscretisationPointsY = discretisationPoints.Select(dp => Math.Round(dp.Geometry.Coordinate.Y - 0.0000005, 6)).ToArray();
            var networkSegments = discretisation.Segments.Values.OfType<NetworkSegment>().ToArray();
            EdgeIdx = networkSegments.Select(s => discretisation.Network.Branches.IndexOf(s.Branch)).ToArray();
            
            EdgeChainage = networkSegments.Select(s => s.Branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch((s.Chainage + s.EndChainage)/ 2)).ToArray(); 
            EdgePointsX = networkSegments.Select(s => Math.Round(s.Geometry.Centroid.X - 0.0000005, 6)).ToArray();
            EdgePointsY = networkSegments.Select(s => Math.Round(s.Geometry.Centroid.Y - 0.0000005, 6)).ToArray();

            var epsilonLocation = 0.01;
            EdgeNodes = networkSegments.SelectMany(s => new int[]
            {
                Array.IndexOf(discretisationPoints,
                    discretisationPoints.FirstOrDefault(p =>
                    {
                        var pointChainage = p.Branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(p.Chainage);
                        var segmentStartChainage = s.Branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(s.Chainage);
                        return p.Branch == s.Branch && Math.Abs(pointChainage - segmentStartChainage) < double.Epsilon ||
                               p.Geometry.Coordinate.Equals2D(s.Geometry.Coordinates[0], epsilonLocation);
                    })),
                Array.IndexOf(discretisationPoints,
                    discretisationPoints.FirstOrDefault(p =>
                    {
                        var pointChainage = p.Branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(p.Chainage);
                        var segmentEndChainage = s.Branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(s.EndChainage);

                        return p.Branch == s.Branch && Math.Abs(pointChainage - segmentEndChainage) < double.Epsilon ||
                               p.Geometry.Coordinate.Equals2D(s.Geometry.Coordinates.Last(), epsilonLocation);
                    }))
            }).ToArray();

            NumberOfMeshEdges = EdgeIdx.Length;
        }

        public int[] EdgeNodes { get; set; }


        public double[] EdgeChainage { get; set; }

        public int[] EdgeIdx { get; set; }

        public double[] EdgePointsX { get; set; }

        public double[] EdgePointsY { get; set; }

        public int[] Branch1dMeshCalculationNodesIdx { get; set; }
    }
}
