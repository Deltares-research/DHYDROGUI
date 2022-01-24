using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class ComputationalGridValidator
    {
        internal static readonly string CategoryName = "Computational grid";

        public static ValidationReport Validate(IDiscretization networkDiscretization, UnstructuredGrid unstructuredGrid, double minimumSegmentLength = 1)
        {
            var issues = new List<ValidationIssue>();
            
            if (networkDiscretization?.Locations == null || networkDiscretization.Locations.Values.Count == 0)
            {
                if (unstructuredGrid == null || unstructuredGrid.IsEmpty)
                {
                    issues.Add(new ValidationIssue(networkDiscretization, ValidationSeverity.Error, Resources.WaterFlowFMModelComputationalGridValidator_Validate_No_computational_grid_defined_));
                }
            
                return new ValidationReport(CategoryName, issues);
            }

            var segmentsPerBranch = networkDiscretization.Segments.Values
                                                         .GroupBy(v => v.Branch)
                                                         .ToDictionary(v => v.Key, v=> v.ToList());

            var finiteVolumeIssues = new List<ValidationIssue>();
            var segmentIssues = new List<ValidationIssue>();

            foreach (var branch in networkDiscretization.Network.Branches)
            {
                if (branch is ISewerConnection sewerConnection && Math.Abs(sewerConnection.Length) < 10e-6)
                    continue;

                var branchLocations = networkDiscretization.GetLocationsForBranch(branch);
                
                issues.AddRange(GetMissingStartEndPointIssues(networkDiscretization, branch));
                issues.AddRange(CheckBranchStructureLocations(branch, branchLocations));
                
                segmentIssues.AddRange(GetSegmentIssuesForBranch(networkDiscretization, minimumSegmentLength, segmentsPerBranch, branch));

                finiteVolumeIssues.AddRange(GetFiniteVolumeIssuesForBranch(networkDiscretization, branch, branchLocations));
            }

            var hasDuplicates = networkDiscretization.Locations.Values.GroupBy(lv => lv)
                                                     .Any(grp => grp.Count() > 1);
            if (hasDuplicates)
            {
                issues.Add(new ValidationIssue("Duplicate network calculation points found at same locations", ValidationSeverity.Error, $"There are duplicate calculation points at same the location. Kernel cannot handle this. Please remove one of the points."));
            }

            var reports = new List<ValidationReport>();

            var invalidIdsIssues = ValidationHelper.ValidateDuplicateNames(networkDiscretization.Locations.Values,
                                                                 "grid points", networkDiscretization, ValidationSeverity.Warning);
            if (invalidIdsIssues.Any())
            {
                reports.Add(new ValidationReport("General", invalidIdsIssues));
            }

            if (finiteVolumeIssues.Any())
            {
                reports.Add(new ValidationReport("Finite volume", finiteVolumeIssues));
            }

            if (segmentIssues.Any())
            {
                reports.Add(new ValidationReport("Segment issues", segmentIssues));
            }
            
            return new ValidationReport(CategoryName, issues, reports);
        }

        private static IEnumerable<ValidationIssue> GetMissingStartEndPointIssues(IDiscretization networkDiscretization, IBranch branch)
        {
            if (networkDiscretization.GetLocationForBranchNode(branch, BranchNodeType.Begin) == null)
            {
                var message = $"No computational grid cells defined for branch : {branch.Name}, not at start of branch; can not start calculation.";
                yield return new ValidationIssue(branch.Source, ValidationSeverity.Error, message, networkDiscretization);
            }

            if (networkDiscretization.GetLocationForBranchNode(branch, BranchNodeType.End) == null)
            {
                var message = $"No computational grid cells defined for branch : {branch.Name}, not at end of branch; can not start calculation.";
                yield return new ValidationIssue(branch.Target, ValidationSeverity.Error, message, networkDiscretization);
            }
        }

        private static IEnumerable<ValidationIssue> GetFiniteVolumeIssuesForBranch(IDiscretization networkDiscretization, IBranch branch, IList<INetworkLocation> branchLocations)
        {
            var structuresOnComputationPoint = branch.BranchFeatures.OfType<IStructure1D>()
                                                     .Where(s => branchLocations.Any(l => Math.Abs(l.Chainage - s.Chainage) < 1e-7))
                                                     .ToArray();

            foreach (var structure1D in structuresOnComputationPoint)
            {
                var msg = string.Format(Resources.WaterFlowFMModelComputationalGridValidator_FiniteVolumeCheckStructuresNotOnGridPoints_Original_discretization_is_invalid__structure__0__is_on_a_grid_point, structure1D.Name);
                yield return new ValidationIssue(structure1D, ValidationSeverity.Error, msg, networkDiscretization);
            }
        }

        private static IEnumerable<ValidationIssue> GetSegmentIssuesForBranch(IDiscretization networkDiscretization, double minimumSegmentLength, Dictionary<IBranch, List<INetworkSegment>> segmentsPerBranch, IBranch branch)
        {
            if (segmentsPerBranch.TryGetValue(branch, out var segments))
            {
                var toShortSegments = segments.Where(s => s.Length < minimumSegmentLength).ToArray();
                foreach (var segment in toShortSegments)
                {
                    yield return new ValidationIssue(segment, ValidationSeverity.Warning, $"Segment {segment.Name} on branch {segment.Branch} is shorter ({segment.Length} than minimum length provided (Dxmin1D) : {minimumSegmentLength}.");
                }
            }
            else
            {
                var message = string.Format(Resources.WaterFlowFMModelComputationalGridValidator_Validate_No_computational_grid_cells_defined_for_branch__0___can_not_start_calculation_, branch.Name);
                yield return new ValidationIssue(branch, ValidationSeverity.Error, message, networkDiscretization);
            }
        }

        private static IEnumerable<ValidationIssue> CheckBranchStructureLocations(IBranch branch, IList<INetworkLocation> branchLocations)
        {
            // There should be at least one grid point between structures
            var branchStructures =
                branch.BranchFeatures.OfType<IStructure1D>()
                      .Where(s => s.ParentStructure == null)
                      .OrderBy(s => s.Chainage)
                      .ToList();

            for (var i = 1; i < branchStructures.Count; i++)
            {
                var branchStructureFirst = branchStructures[i - 1];
                var branchStructureSecond = branchStructures[i];

                if (branchLocations.Any(l => l.Chainage >= branchStructureFirst.Chainage && l.Chainage <= branchStructureSecond.Chainage))
                    continue;

                var message = string.Format(Resources.WaterFlowFMModelComputationalGridValidator_CheckBranchStructureLocations_No_grid_points_defined_between_structure__0__and__1_,
                                            branchStructureFirst.Name, branchStructureSecond.Name);
                yield return new ValidationIssue(branchStructureSecond.GetStructureType(), ValidationSeverity.Error, message, branchStructureFirst.Chainage);
            }
        }
    }
}