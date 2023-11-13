using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Link1d2d;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.NGHS.Utils;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using static DeltaShell.NGHS.IO.Grid.DeltaresUGrid.HydroUGridExtensions;

namespace DeltaShell.NGHS.IO.Grid.DeltaresUGrid
{
    /// <summary>
    /// Extension class of <seealso cref="Disposable1DMeshGeometry"/>
    /// </summary>
    public static class Disposable1DMeshGeometryExtensions
    {
        /// <summary>
        /// Validate the 1d mesh which will be used by the kernels (<seealso cref="Disposable1DMeshGeometry"/>) against the network coverage (<seealso cref="IDiscretization"/> of the network in hydromodel.
        /// Check if there are network locations (<seealso cref="INetworkLocation"/>) in the network coverage which will be placed at the same location in the mesh.
        /// Because they are placed / have a chainage at the same location or further than the branch length.
        /// Als validate if mesh edges, similar to network segments (<seealso cref="INetworkSegment"/>) in the network coverage
        /// are all connected to calculation points or network locations.
        /// Warn the user about this if not valid.
        /// </summary>
        /// <param name="mesh1d">The 1d mesh (<seealso cref="Disposable1DMeshGeometry"/>) geometry which will be provided to the calculation cores.</param>
        /// <param name="networkDiscretization">The discretization of the network (<seealso cref="IDiscretization"/>) which is used for calculation points / network locations (<seealso cref="INetworkLocation"/>) and mesh edges / network segments (<seealso cref="INetworkSegment"/>).</param>
        /// <returns>An list of (warning) strings to inform the user about the vadility of the mesh geometry versus the visualized and modeled network discretization.</returns>
        public static IEnumerable<string> ValidateAgainstDiscretization(this Disposable1DMeshGeometry mesh1d, IDiscretization networkDiscretization)
        {
            // warnings about calculation points after length of branch
            var locations = networkDiscretization.Locations.Values.ToArray();

            var branchIdLookup = networkDiscretization.Network.Branches.ToIndexDictionary();

            foreach (INetworkLocation networkLocation in locations)
            {
                var currentBranchIdIndices = mesh1d.BranchIDs.Where(id => id == branchIdLookup[networkLocation.Branch]).Select((id, index) => index);
                foreach (var warningMessage in mesh1d.CheckForDoubleCalculationPoint(currentBranchIdIndices, networkLocation))
                {
                    yield return warningMessage;
                }
            }

            // warnings about no begin or end points found of edges
            var segments = networkDiscretization.Segments.Values.ToList();
            var locationIdLookup = locations.ToIndexDictionary();
            foreach (INetworkSegment segment in segments)
            {
                var segmentIndices = GetLocationIndices(networkDiscretization, segment, locationIdLookup, out IList<INetworkSegment> _);
                if (segmentIndices[0] == -1)
                {
                    // no begin point found
                    yield return string.Format(Resources.HydroUGridExtensions_Cannot_find_start_edge_node_of_section,
                                               segment.SegmentNumber, segment.Branch.Name, segment.Chainage, segment.Branch.Name);
                }

                if (segmentIndices[1] == -1)
                {
                    // no end point found
                    yield return string.Format(Resources.HydroUGridExtensions_Cannot_find_end_edge_node_of_section,
                                               segment.SegmentNumber, segment.Branch.Name, segment.EndChainage, segment.Branch.Name);
                }
            }
        }

        /// <summary>
        /// Validates 1d mesh geometry (<seealso cref="Disposable1DMeshGeometry"/>) with the 1d2d links using the calculation points / network locations (<seealso cref="Disposable1DMeshGeometry"/> in the mesh
        /// </summary>
        /// <param name="mesh1d">The 1d mesh (<seealso cref="Disposable1DMeshGeometry"/>) geometry which will be provided to the calculation cores.</param>
        /// <param name="link1D2Ds">The links between the 1d mesh and the 2d mesh.</param>
        /// <returns>An list of (error) strings to inform the user about the vadility of the mesh geometry versus the links of the modeled network discretization.</returns>
        public static IEnumerable<string> ValidateAgainstLinks(this Disposable1DMeshGeometry mesh1d, IEnumerable<ILink1D2D> link1D2Ds)
        {
            int index = 0;
            var coordinatesDiscretizationPoints = mesh1d.NodesX
                                                        .Select((x, i) => new Coordinate(x, mesh1d.NodesY[i]))
                                                        .ToDictionary<Coordinate, int>(c => index++);

            foreach (ILink1D2D link in link1D2Ds)
            {
                var linkDiscretizationPointIndex = link.DiscretisationPointIndex;
                var linkDiscretizationPointCoordinate = coordinatesDiscretizationPoints[linkDiscretizationPointIndex];
                var otherDiscretizationPointCoordinatesAtSameLocationIndices = coordinatesDiscretizationPoints
                                                                               .Where(kvp => kvp.Value.Equals2D(linkDiscretizationPointCoordinate)
                                                                                             && kvp.Key != linkDiscretizationPointIndex)
                                                                               .Select(kvp => kvp.Key).ToArray();
                if (otherDiscretizationPointCoordinatesAtSameLocationIndices.Length > 0)
                {
                    var otherDiscretizationPointNames = otherDiscretizationPointCoordinatesAtSameLocationIndices.Select(j => mesh1d.NodeIds[j]);
                    string linksName = link.Name;
                    yield return string.Format(Resources.UGridFileHelper_ValidateMesh1DSourceLocationsOnlyExistOnce_ErrorMessage_part1, linksName, mesh1d.NodeIds[linkDiscretizationPointIndex], link.FaceIndex) +
                                 Environment.NewLine +
                                 string.Format(Resources.UGridFileHelper_ValidateMesh1DSourceLocationsOnlyExistOnce_ErrorMessage_part2, string.Join(", ", otherDiscretizationPointNames));
                }
            }
        }

        /// <summary>
        /// If a calculation point on a 1d mesh (<seealso cref="Disposable1DMeshGeometry"/>) is modeled in the network coverage (<seealso cref="IDiscretization"/>)
        /// past the length of the branch it will be placed / moved to the end of the branch. This could lead to double calculation points for the 1d mesh
        /// The calculation core will remove the doubling which could lead for instance to invalid 1d2d links (<seealso cref="ILink1D2D"/>).
        /// The source discretization point could be removed which is unwanted behavior.
        /// </summary>
        /// <param name="mesh">The 1d mesh (<seealso cref="Disposable1DMeshGeometry"/>) geometry which will be provided to the calculation cores.</param>
        /// <param name="currentBranchIdOffsetIndices">
        /// The indexes / indices of the chainages on the current branch in the 1d mesh geometry (<seealso cref="Disposable1DMeshGeometry"/>) we are checking.
        /// Used to get the branch offset as stored in the 1d mesh which will be used by the calculation core.
        /// </param>
        /// <param name="locationInDiscretizationOfNetwork">The network location (<seealso cref="INetworkLocation"/>) in the modeled discretization (<seealso cref="IDiscretization"/>) of the network.</param>
        /// <returns>A list of strings if this branchId & chainage of the modeled network location already exist in the 1d mesh.</returns>
        private static IEnumerable<string> CheckForDoubleCalculationPoint(this Disposable1DMeshGeometry mesh, IEnumerable<int> currentBranchIdOffsetIndices, INetworkLocation locationInDiscretizationOfNetwork)
        {

            foreach (int currentBranchIdIndex in currentBranchIdOffsetIndices.Reverse())
            {
                if (!string.IsNullOrWhiteSpace(mesh.NodeIds[currentBranchIdIndex])
                    && !mesh.NodeIds[currentBranchIdIndex].Equals(locationInDiscretizationOfNetwork.Name, StringComparison.InvariantCultureIgnoreCase)
                    && mesh.BranchOffsets[currentBranchIdIndex].Equals(locationInDiscretizationOfNetwork.Chainage))
                {
                    // this branchId & chainage is already added to mesh. Inform user:
                    yield return string.Format(Resources.HydroUGridExtensions_CheckForDoubleCalculationPoint_In_Mesh1D_With_Existing_NetworkLocations_Of_Model, mesh.BranchIDs[currentBranchIdIndex], locationInDiscretizationOfNetwork.Branch.Name, locationInDiscretizationOfNetwork.Branch.Length, locationInDiscretizationOfNetwork.Chainage, locationInDiscretizationOfNetwork.Chainage, locationInDiscretizationOfNetwork.Name, mesh.NodeIds[currentBranchIdIndex]);
                }
            }
        }

    }
}