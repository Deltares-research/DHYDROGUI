using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Filters;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Coverages;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekComputationalGridImporter : PartialSobekImporterBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekComputationalGridImporter));

        public override string DisplayName
        {
            get { return "Grid points (computational grid)"; }
        }

        public override SobekImporterCategories Category { get; } = SobekImporterCategories.WaterFlow1D;

        protected override void PartialImport()
        {
            if (!HydroNetwork.Branches.Any())
            {
                Log.Error("Network has no branches; cannot import computational grid.");
                return;
            }

            Log.DebugFormat("Importing computational grid ...");
            var waterFlowFMModel = GetModel<WaterFlowFMModel>();

            var channels = HydroNetwork.Channels.ToDictionary(c => c.Name, c => c);

            try
            {
                waterFlowFMModel.NetworkDiscretization.SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsAndConnectedBranchesWithoutLocationOnThemFullyCovered;
                ImportCalculationGrids(waterFlowFMModel.NetworkDiscretization, channels);
                ImportFixedGridPointData(waterFlowFMModel.NetworkDiscretization);
            }
            catch (Exception exception)
            {
                Log.ErrorFormat("Error reading computational grid: {0}", exception.Message);
            }

            if (SobekType == DeltaShell.Sobek.Readers.SobekType.SobekRE)
            {
                // for sobekRe import generate a default grid for branches that have no grid points
                var theHaves = waterFlowFMModel.NetworkDiscretization.Locations.Values.Select(s => s.Branch).Distinct();
                foreach (var channel in HydroNetwork.Branches)
                {
                    if (theHaves.Contains(channel) || !(channel is IChannel)) continue;

                    Log.InfoFormat("Generate default grid (on cross sections) for branch {0}.", channel.Name);
                    HydroNetworkHelper.GenerateDiscretization(waterFlowFMModel.NetworkDiscretization, channel as IChannel,
                                                              /*minimumCellLength*/0.5, /*gridAtStructure*/false,
                                                              /*structureDistance*/0.0, /*gridAtCrossSection*/true,
                                                              /*gridAtLaterals*/false, /*gridAtFixedLength*/false,
                                                              /*fixedLength*/0.0);
                }
            }
        }

        private void ImportFixedGridPointData(IDiscretization networkDiscretization)
        {
            var sobekObjectTypePath = GetFilePath(SobekFileNames.SobekObjectTypeFileName);
            if (!File.Exists(sobekObjectTypePath))
            {
                Log.WarnFormat("Could not import fixed grid points data; file {0} not found.", sobekObjectTypePath);
                return;
            }

            var fixedGridPointData = new HashSet<string>(new SobekObjectTypeReader().Read(sobekObjectTypePath).Where(sot => sot.Type == SobekObjectType.SBK_GRIDPOINTFIXED).Select(sot => sot.ID));
            var gridPointToBecomeFixed = networkDiscretization.Locations.Values.Where(gridPoint => fixedGridPointData.Contains(gridPoint.Name)).ToArray();
            if(gridPointToBecomeFixed.Length >0)
                networkDiscretization.SetValues(Enumerable.Repeat(1.0d, gridPointToBecomeFixed.Length), new VariableValueFilter<INetworkLocation>(networkDiscretization.Locations, gridPointToBecomeFixed));
        }

        private void ImportCalculationGrids(IDiscretization networkDiscretization, Dictionary<string, IChannel> channels)
        {
            string gridPath = GetFilePath(SobekFileNames.SobekNetworkGridFileName);
            if (!File.Exists(gridPath))
            {
                Log.WarnFormat("Could not import computational grid; file {0} not found.", gridPath);
                return;
            }

            //channels.ContainsKey -> calculation points on pipes should not be imported
            IList<CalcGrid> calcGrids = new SobekGridPointsReader().Read(gridPath).Where(cg => channels.ContainsKey(cg.BranchID)).ToList();

            var locations = new List<NetworkLocation>();

            //add the grids in the order of the branches in network..because adding slices to underlying coverage is only supported 
            //when values are monotonous ascending..
            foreach (CalcGrid grid in calcGrids.OrderBy(g => HydroNetwork.Branches.IndexOf(channels[g.BranchID])))
            {
                var branch = channels[grid.BranchID];

                var branchLocations = CreateFractionSegment(branch, grid,
                                                            branch.Structures.Where(s => s is ICompositeBranchStructure).OrderBy(
                                                                s => s.Chainage).Select(s => s.Chainage).ToList());

                if (branchLocations != null && branchLocations.Any())
                {
                    locations.AddRange(branchLocations);
                }
            }

            //change stupid duplicate names of locations
            while (!locations.Select(ls => ls.Name).AllUnique())
            {
                NamingHelper.MakeNamesUnique(locations);
            }

            networkDiscretization.UpdateNetworkLocations(locations);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="grid"></param>
        /// <param name="ignoreOffsets">
        /// offsets to ignore for grid point generations. These offsets are already updated for the branch.Geometry.Length / branch.Length ratio
        /// </param>
        private IEnumerable<NetworkLocation> CreateFractionSegment(IChannel branch, CalcGrid grid, IEnumerable<double> ignoreOffsets)
        {
            if (!grid.GridPoints.Any())
            {
                if (SobekType != DeltaShell.Sobek.Readers.SobekType.SobekRE)
                {
                    Log.WarnFormat("No computational grid points defined for channel {0}", branch.Name);
                }
                return null;
            }

            // To ensure that the grid points cover the entire branch geometry add an extra
            // grid points at start and end if necessary.
            if (grid.GridPoints[0].Offset > 1.0e-3)
            {
                var sobekCalcGridPoint = new SobekCalcGridPoint { Offset = 0.0, Id = "extra" };
                grid.GridPoints.Insert(0, sobekCalcGridPoint);
            }
            var distanceToEnd = branch.Length - grid.GridPoints[grid.GridPoints.Count - 1].Offset;
            if (distanceToEnd > 1.0e-3)
            {
                if (distanceToEnd < 0.5)
                {
                    // last grid point near end branch move it to end
                    grid.GridPoints[grid.GridPoints.Count - 1].Offset = branch.Length;
                }
                else
                {
                    var sobekCalcGridPoint = new SobekCalcGridPoint { Offset = branch.Length, Id = "extra" };
                    grid.GridPoints.Add(sobekCalcGridPoint);
                }
            }
            else if (distanceToEnd < -1.0e-3)
            {
                // grid point given outside branch; move to end (see test SobekReCalcGridWithUserLengthAndGridPointOutsideBranch)
                grid.GridPoints[grid.GridPoints.Count - 1].Offset = branch.Length;
            }

            IList<NetworkLocation> networkLocations = new List<NetworkLocation>(grid.GridPoints.Count);

            var skippedPoints = new List<double>();
            foreach (var gridPoint in grid.GridPoints)
            {
                SobekCalcGridPoint point = gridPoint;
                if (ignoreOffsets.Any(o => Math.Abs(o - point.Offset) < BranchFeature.Epsilon))
                {
                    skippedPoints.Add(gridPoint.Offset);
                    continue;
                }
                // last point will always put on top end node to avoid rounding errors
                var networkLocation = new NetworkLocation
                {
                    Branch = branch,
                    Chainage = BranchFeature.SnapChainage(branch.Length, gridPoint.Offset),
                    Name = gridPoint.Id,
                    LongName = gridPoint.Name
                };
                networkLocations.Add(networkLocation);
            }
            if (skippedPoints.Count > 0)
            {
                Log.WarnFormat("Computational grid import : {0} grid points were skipped in branch '{1}' because there are structures defined at these locations.", skippedPoints.Count, branch.Name);
            }

            return networkLocations;
        }
    }
}
