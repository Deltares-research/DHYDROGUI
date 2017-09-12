using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class WaterFlowFMModelComputationalGridValidator
    {
        public static string CategoryName = "Computational grid";
        public static ValidationReport Validate(IDiscretization networkDiscretization,
            WaterFlowFMModel flowFmModel = null)
        {
            var issues = new List<ValidationIssue>();
            var invalidGrid = flowFmModel != null && (flowFmModel.Grid == null || flowFmModel.Grid.IsEmpty);
            if (flowFmModel != null && networkDiscretization.Locations.Values.Count == 0 && invalidGrid)
            {
                issues.Add(new ValidationIssue(networkDiscretization, ValidationSeverity.Error,
                    Resources.WaterFlowFMModelComputationalGridValidator_Validate_No_computational_grid_defined_));
            }
            else
            {
                var branchesWithoutGridSegments = GetBranchesWithoutGridSegments(networkDiscretization).ToList();
                var branchLocationsLookup =
                    networkDiscretization.Locations.Values.GroupBy(l => l.Branch).ToDictionary(g => g.Key, g => g);

                foreach (var branch in networkDiscretization.Network.Branches)
                {
                    if (branchesWithoutGridSegments.Contains(branch) || !branchLocationsLookup.ContainsKey(branch))
                    {
                        var message =
                            string.Format(
                                Resources.WaterFlowFMModelComputationalGridValidator_Validate_No_computational_grid_cells_defined_for_branch__0___can_not_start_calculation_,
                                branch.Name);
                        issues.Add(new ValidationIssue(branch, ValidationSeverity.Error, message, networkDiscretization));

                        continue; //no computational grid, so no sense reporting additional errors
                    }

                    var branchLocations = branchLocationsLookup[branch].ToList();

                    issues.AddRange(CheckBranchLocations(networkDiscretization, branch, branchLocations));

                    issues.AddRange(CheckBranchStructureLocations(networkDiscretization, branch, branchLocations));

                    /* QBoundaries and Resistances tests removed as we do not need them now. 
                     * Once snapped features are available for the 1D they will be required, needed validations
                     * can be found on the WaterFlow1D Validation.
                    */
                }
            }

            var subReports = ValidateIds(networkDiscretization);

                var finiteVolumeIssues = FiniteVolumeCheckStructuresNotOnGridPoints(networkDiscretization);
                if (finiteVolumeIssues.Count > 0)
                {
                    subReports = subReports.Concat(new[] { new ValidationReport("Finite volume", finiteVolumeIssues) });
                }

            return new ValidationReport(CategoryName, issues, subReports);
        }

        private static IList<ValidationIssue> FiniteVolumeCheckStructuresNotOnGridPoints(IDiscretization networkDiscretization)
        {
            var issues = new List<ValidationIssue>();

            foreach (var branch in networkDiscretization.Network.Branches)
            {
                var branchStructures = branch.BranchFeatures.OfType<IStructure>();
                var branchLocations = networkDiscretization.GetLocationsForBranch(branch);

                var structureWithInvalidLocation = branchStructures.FirstOrDefault(bs => branchLocations.Any(bl => Math.Abs(bl.Chainage - bs.Chainage) < BranchFeature.Epsilon));

                if (structureWithInvalidLocation == null) continue;

                var msg = string.Format(Resources.WaterFlowFMModelComputationalGridValidator_FiniteVolumeCheckStructuresNotOnGridPoints_Original_discretization_is_invalid__structure__0__is_on_a_grid_point, structureWithInvalidLocation.Name);

                issues.Add(new ValidationIssue(structureWithInvalidLocation, ValidationSeverity.Error, msg,
                                               networkDiscretization));
            }

            return issues;
        }

        private static IEnumerable<ValidationIssue> CheckBranchStructureLocations(IDiscretization networkDiscretization,
                                                                                  IBranch branch,
                                                                                  List<INetworkLocation> branchLocations)
        {
            // There should be at least one grid point between structures
            var branchStructures =
                branch.BranchFeatures.OfType<IStructure>()
                      .Where(s => s.ParentStructure == null)
                      .OrderBy(s => s.Chainage)
                      .ToList();

            for (var i = 1; i < branchStructures.Count(); i++)
            {
                var branchStructureFirst = branchStructures[i - 1];
                var branchStructureSecond = branchStructures[i];

                if (
                    branchLocations.Any(
                        l => l.Chainage >= branchStructureFirst.Chainage && l.Chainage <= branchStructureSecond.Chainage))
                    continue;

                var message = string.Format("No grid points defined between structure {0} and {1}",
                                            branchStructureFirst.Name, branchStructureSecond.Name);
                yield return
                    new ValidationIssue(branchStructureSecond.GetStructureType(), ValidationSeverity.Error, message, branchStructureFirst.Chainage);
            }
        }

        private static IEnumerable<ValidationIssue> CheckBranchLocations(IDiscretization networkDiscretization,
                                                                         IBranch branch,
                                                                         IList<INetworkLocation> branchLocations)
        {
            // Each branch should have calculation point at start and end
            if (!branchLocations.Any(l => Math.Abs(l.Chainage) < BranchFeature.Epsilon) ||
                !branchLocations.Any(l => DoubleEquals(l.Chainage, branch.Length)))
            {
                var message = string.Format("Not enough grid points defined for branch {0}. " +
                                            "Make sure you have at least gridpoints at start and end of branch.",
                                            branch.Name);

                yield return new ValidationIssue(branch, ValidationSeverity.Error, message, networkDiscretization);
            }
        }

        private static IEnumerable<IBranch> GetBranchesWithoutGridSegments(IDiscretization networkDiscretization)
        {
            var branches = new HashSet<IBranch>(networkDiscretization.Network.Branches);

            foreach(var seg in networkDiscretization.Segments.Values)
            {
                branches.Remove(seg.Branch);
            }

            return branches;
        }

        private static IEnumerable<ValidationReport> ValidateIds(IDiscretization networkDiscretization)
        {
            var reports = new List<ValidationReport>();
            var issues = ValidationHelper.ValidateDuplicateNames(networkDiscretization.Locations.Values,
                                                        "grid points", networkDiscretization, ValidationSeverity.Warning);
            if (issues.Any())
            {
                reports.Add(new ValidationReport("General", issues));
            }

            return reports;
        }

        private static bool DoubleEquals(double d1, double d2)
        {
            return Math.Abs(d1 - d2) < 0.000001;
        }
    }
}