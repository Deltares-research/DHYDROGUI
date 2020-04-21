using System;
using System.Linq;
using System.Threading.Tasks;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Geometries;

namespace DeltaShell.NGHS.IO.Grid
{
    public class NetworkDiscretisationUGridDataModel
    {
        public const int DIGITS = (int) 1E6;

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
                var branches = discretisation.Network.Branches.ToList();
                BranchIdx = new int[NumberOfDiscretisationPoints];
                Offsets = new double[NumberOfDiscretisationPoints];
                DiscretisationPointIds = new string[NumberOfDiscretisationPoints];
                DiscretisationPointDescriptions = new string[NumberOfDiscretisationPoints];
                DiscretisationPointsX = new double[NumberOfDiscretisationPoints];
                DiscretisationPointsY = new double[NumberOfDiscretisationPoints];
                Parallel.For(0, NumberOfDiscretisationPoints,discretisationPointIdx =>
                {
                    var discretisationPoint = discretisationPoints[discretisationPointIdx];
                    var discretisationPointBranch = discretisationPoint.Branch;
                    BranchIdx[discretisationPointIdx] = branches.IndexOf(discretisationPointBranch);
                    Offsets[discretisationPointIdx] = discretisationPointBranch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(discretisationPoint.Chainage);
                    DiscretisationPointIds[discretisationPointIdx] = discretisationPoint.Name;
                    DiscretisationPointDescriptions[discretisationPointIdx] = discretisationPoint.LongName;
                    DiscretisationPointsX[discretisationPointIdx] = discretisationPoint.Geometry.Coordinate != null
                        ? Math.Floor(discretisationPoint.Geometry.Coordinate.X * DIGITS) / DIGITS 
                        : 0.0d;
                    DiscretisationPointsY[discretisationPointIdx] = discretisationPoint.Geometry.Coordinate != null
                        ? Math.Floor(discretisationPoint.Geometry.Coordinate.Y * DIGITS) / DIGITS
                        : 0.0d;
                });
                
                var networkSegments = discretisation.Segments.Values.OfType<NetworkSegment>().ToArray();

                NumberOfMeshEdges = networkSegments.Length;
                EdgeIdx = new int[NumberOfMeshEdges];
                EdgeChainage = new double[NumberOfMeshEdges];
                EdgePointsX = new double[NumberOfMeshEdges];
                EdgePointsY = new double[NumberOfMeshEdges];
                EdgeNodes = new int[(NumberOfMeshEdges) * 2];
                var epsilonLocation = 1e-5;
                var discretisationPointByBranchByDiscretisationPointChainage =
                    discretisationPoints.ToLookup(p => p.Branch,
                        p => new
                        {
                            Chainage = p.Branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(p.Chainage),
                            NetworkLocation = p
                        });

                var discretisationPointByCoordinate = discretisationPoints.ToLookup(p => p.Geometry?.Coordinate, p => p, new CoordinateComparison2D());
                Parallel.For(0, NumberOfMeshEdges, meshEdgesIdx =>
                {
                    var meshEdge = networkSegments[meshEdgesIdx];
                    var meshEdgeBranch = meshEdge.Branch;
                    EdgeIdx[meshEdgesIdx] = branches.IndexOf(meshEdgeBranch);
                    EdgeChainage[meshEdgesIdx] =
                        meshEdgeBranch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(
                            (meshEdge.Chainage + meshEdge.EndChainage) / 2);
                    
                    EdgePointsX[meshEdgesIdx] = meshEdge.Geometry?.Centroid != null ? Math.Floor(meshEdge.Geometry.Centroid.X * DIGITS) / DIGITS : 0.0d;
                    EdgePointsY[meshEdgesIdx] = meshEdge.Geometry?.Centroid != null ? Math.Floor(meshEdge.Geometry.Centroid.Y * DIGITS) / DIGITS : 0.0d;

                    var segmentStartChainage = meshEdge.Branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(meshEdge.Chainage);
                    EdgeNodes[meshEdgesIdx*2] = -1;

                    if (discretisationPointByBranchByDiscretisationPointChainage.Contains(meshEdgeBranch))
                    {
                        foreach (var pointWithStartChainageOnBranch in
                            discretisationPointByBranchByDiscretisationPointChainage[meshEdgeBranch]
                                .Where(o => Math.Abs(o.Chainage - segmentStartChainage) < epsilonLocation))
                        {
                            EdgeNodes[meshEdgesIdx*2] = Array.IndexOf(discretisationPoints,
                                pointWithStartChainageOnBranch.NetworkLocation);
                            if (EdgeNodes[meshEdgesIdx*2] != -1)
                                break;
                        }
                    }

                    if (EdgeNodes[meshEdgesIdx * 2] == -1)
                    {
                        var segmentStartCoordinate = meshEdge.Geometry?.Coordinates[0];

                        var discretizationPointsByThisStartCoordinate =
                            discretisationPointByCoordinate.FirstOrDefault(p =>
                                p.Key.Equals2D(segmentStartCoordinate, epsilonLocation));

                        if (segmentStartCoordinate != null &&
                            discretizationPointsByThisStartCoordinate != null)
                        {
                            foreach (var pointWithStartCoordinate in
                                discretisationPointByCoordinate[discretizationPointsByThisStartCoordinate.Key])
                            {
                                EdgeNodes[meshEdgesIdx * 2] = Array.IndexOf(discretisationPoints,
                                    pointWithStartCoordinate);
                                if (EdgeNodes[meshEdgesIdx * 2] != -1)
                                    break;
                            }
                        }
                    }

                    EdgeNodes[meshEdgesIdx*2 + 1] = -1;
                    var segmentEndChainage = meshEdge.Branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(meshEdge.EndChainage);
                    
                    if (discretisationPointByBranchByDiscretisationPointChainage.Contains(meshEdgeBranch))
                    {
                        foreach (var pointWithEndChainageOnBranch in
                            discretisationPointByBranchByDiscretisationPointChainage[meshEdgeBranch]
                                .Where(o => Math.Abs(o.Chainage - segmentEndChainage) < epsilonLocation))
                        {

                            EdgeNodes[meshEdgesIdx*2 + 1] = Array.IndexOf(discretisationPoints,
                                pointWithEndChainageOnBranch.NetworkLocation);
                            if (EdgeNodes[meshEdgesIdx*2 + 1] != -1)
                                break;
                        }
                    }

                    if (EdgeNodes[meshEdgesIdx * 2 + 1] == -1)
                    {
                        var segmentEndCoordinate = meshEdge.Geometry?.Coordinates?.LastOrDefault();

                        var discretizationPointsByThisEndCoordinate =
                            discretisationPointByCoordinate.FirstOrDefault(p =>
                                p.Key.Equals2D(segmentEndCoordinate, epsilonLocation));

                        if (segmentEndCoordinate != null &&
                            discretizationPointsByThisEndCoordinate != null)
                        {
                            foreach (var pointWithEndCoordinate in
                                discretisationPointByCoordinate[discretizationPointsByThisEndCoordinate.Key])
                            {

                                EdgeNodes[meshEdgesIdx * 2 + 1] = Array.IndexOf(discretisationPoints,
                                    pointWithEndCoordinate);
                                if (EdgeNodes[meshEdgesIdx * 2 + 1] != -1)
                                    break;
                            }
                        }
                    }
                });
            }
        }

        public int[] EdgeNodes { get; set; }


        public double[] EdgeChainage { get; set; }

        public int[] EdgeIdx { get; set; }

        public double[] EdgePointsX { get; set; }

        public double[] EdgePointsY { get; set; }

        public int[] Branch1dMeshCalculationNodesIdx { get; set; }
    }
}
