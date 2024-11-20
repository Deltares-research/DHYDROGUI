using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.Logging;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.NGHS.Utils;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.Grid
{
    public static class UGridFileHelperMesh1D
    {
        /// <summary>
        /// Sets the <paramref name="meshGeometry"/> to the <paramref name="discretization"/>
        /// </summary>
        /// <param name="discretization">Discretization to set</param>
        /// <param name="meshGeometry">Mesh geometry to set</param>
        /// <param name="network">Network that the <paramref name="meshGeometry"/> is based on</param>
        /// <param name="logHandler">Logging handler to report problems while reading.</param>
        /// <param name="canUseXYForMesh1DNodeCoordinates">Boolean which states if in the file also the X & Y values are available for the the network locations.</param>
        public static void SetMesh1DGeometry(IDiscretization discretization, Disposable1DMeshGeometry meshGeometry, IHydroNetwork network, ILogHandler logHandler = null, bool canUseXYForMesh1DNodeCoordinates = true)
        {
            logHandler = logHandler ?? new LogHandler(Resources.HydroUGridExtensions_Mesh1DGeometryLogHandlerActivityName, typeof(HydroUGridExtensions), 100);
            discretization.Network = network;

            IEnumerable<INetworkLocation> networkLocations = GetNetworkLocations(meshGeometry, network, logHandler, canUseXYForMesh1DNodeCoordinates);

            discretization.UpdateNetworkLocations(networkLocations);

            IList<INetworkSegment> segmentToRemove = null;
            var locationIdLookup = discretization.Locations.AllValues.ToArray().ToIndexDictionary();

            foreach (var segment in discretization.Segments.Values)
            {
                HydroUGridExtensions.GetLocationIndices(discretization, segment, locationIdLookup, logHandler, out segmentToRemove);
            }

            if (segmentToRemove != null)
            {
                foreach (var segment in segmentToRemove)
                {
                    discretization.Segments.Values.Remove(segment);
                }

            }

            logHandler.LogReport();
        }

        private static IEnumerable<INetworkLocation> GetNetworkLocations(Disposable1DMeshGeometry meshGeometry, IHydroNetwork network, ILogHandler logHandler, bool canUseXYForMesh1DNodeCoordinates)
        {
            var numberOfNodes = meshGeometry.NodeIds.Length;
            var networkLocations = new ConcurrentQueue<INetworkLocation>();
            var networkLocationImportErrors = new ConcurrentQueue<string>();
            const string indexOfVerticeInTheFile = "fileIndex";

            Parallel.For(0, numberOfNodes, i =>
            {
                var networkBranch = network.Branches[meshGeometry.BranchIDs[i]];
                var meshGeometryBranchChainage = meshGeometry.BranchOffsets[i];

                double chainage = Math.Abs(networkBranch.Length - meshGeometryBranchChainage) < 0.00001 ? networkBranch.Length : meshGeometryBranchChainage;
                if (chainage < 0)
                {
                    networkLocationImportErrors.Enqueue(string.Format(Resources.HydroUGridExtensions_Negative_chainage_of_network_location, networkBranch.Name));
                    return;
                }

                if (chainage > networkBranch.Length)
                {
                    networkLocationImportErrors.Enqueue(string.Format(Resources.HydroUGridExtensions_Chainage_of_network_location_too_large,
                                                                      meshGeometryBranchChainage, networkBranch.Name, networkBranch.Length));

                    chainage = networkBranch.Length;
                }

                var networkLocation = new NetworkLocation()
                {
                    Branch = networkBranch,
                    Chainage = chainage,
                    Name = meshGeometry.NodeIds[i],
                    LongName = meshGeometry.NodeLongNames[i],
                    Geometry = canUseXYForMesh1DNodeCoordinates
                                   ? new Point(meshGeometry.NodesX[i], meshGeometry.NodesY[i])
                                   : HydroNetworkHelper.GetStructureGeometry(networkBranch, DetermineMeshGeometryBranchChainage(networkBranch, meshGeometryBranchChainage))
                };
                networkLocation.Attributes[indexOfVerticeInTheFile] = i;
                networkLocations.Enqueue(networkLocation);
            });

            if (networkLocationImportErrors.Any())
            {
                logHandler.ReportError(string.Format(Resources.HydroUGridExtensions_GetNetworkLocations_While_reading_1d_discretization___calculation_point_from_the_netfile_we_encountered_the_following_errors___0__1_, Environment.NewLine, string.Join(Environment.NewLine, networkLocationImportErrors)));
            }
            return networkLocations.Distinct().OrderBy(nl => nl.Attributes[indexOfVerticeInTheFile]);
        }

        private static double DetermineMeshGeometryBranchChainage(IBranch networkBranch, double meshGeometryBranchChainage)
        {
            return networkBranch.Length - meshGeometryBranchChainage < 0.000001 ? networkBranch.Length : meshGeometryBranchChainage;
        }
    }
}