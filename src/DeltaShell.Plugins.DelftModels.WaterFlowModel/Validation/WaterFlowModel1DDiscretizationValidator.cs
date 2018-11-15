using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation
{
    public static class WaterFlowModel1DDiscretizationValidator
    {
        public static ValidationReport Validate(IDiscretization networkDiscretization,
                                                WaterFlowModel1D waterFlowModel1D = null)
        {
            var issues = new List<ValidationIssue>();

            if (networkDiscretization.Locations.Values.Count == 0)
            {
                issues.Add(new ValidationIssue(networkDiscretization, ValidationSeverity.Error,
                                               "No computational grid defined."));
            }
            else
            {
                var branchesWithoutGridSegments = GetBranchesWithoutGridSegments(networkDiscretization);
                var branchLocationsLookup =
                    networkDiscretization.Locations.Values.GroupBy(l => l.Branch).ToDictionary(g => g.Key, g => g);
                var boundaryNodeData = waterFlowModel1D != null ? waterFlowModel1D.BoundaryConditions : null;

                foreach (var branch in networkDiscretization.Network.Branches)
                {
                    if (branchesWithoutGridSegments.Contains(branch) || !branchLocationsLookup.ContainsKey(branch))
                    {
                        var message =
                            String.Format(
                                "No computational grid cells defined for branch {0}; can not start calculation.",
                                branch.Name);
                        issues.Add(new ValidationIssue(branch, ValidationSeverity.Error, message, networkDiscretization));

                        continue; //no computational grid, so no sense reporting additional errors
                    }

                    var branchLocations = branchLocationsLookup[branch].ToList();

                    issues.AddRange(CheckBranchLocations(networkDiscretization, branch, branchLocations));

                    issues.AddRange(CheckBranchStructureLocations(networkDiscretization, branch, branchLocations));

                    if (waterFlowModel1D != null)
                    {
                        issues.AddRange(ValidateNetworkDiscretizationQBoundariesAndStructures(networkDiscretization, boundaryNodeData, branch, branchLocations));
                        issues.AddRange(ValidateNetworkDiscretizationBoundariesAndExtraResistances(networkDiscretization, boundaryNodeData, branch, branchLocations));
                    }
                }
            }

            var subReports = ValidateIds(networkDiscretization);

            if (CheckFiniteVolume(waterFlowModel1D)) //bleh
            {
                var finiteVolumeIssues = FiniteVolumeCheckStructuresNotOnGridPoints(networkDiscretization);
                if (finiteVolumeIssues.Count > 0)
                {
                    subReports = subReports.Concat(new[] {new ValidationReport("Finite volume", finiteVolumeIssues)});
                }
            }
            return new ValidationReport("Computational grid", issues, subReports);
        }

        private static bool CheckFiniteVolume(WaterFlowModel1D waterFlowModel1D)
        {
            if (waterFlowModel1D == null)
                return true; //hack: if no model, check finite volume errors (as finite volume calls this validation manually)

            var gridTypeParameter = waterFlowModel1D.OutputSettings.EngineParameters.First(p => p.Name == WaterFlowModelParameterNames.FiniteVolumeGridType);
            return gridTypeParameter.AggregationOptions != (AggregationOptions) FiniteVolumeDiscretizationType.None;
        }

        private static IList<ValidationIssue> FiniteVolumeCheckStructuresNotOnGridPoints(IDiscretization networkDiscretization)
        {
            var issues = new List<ValidationIssue>();

            foreach (var branch in networkDiscretization.Network.Branches)
            {
                var branchStructures = branch.BranchFeatures.OfType<IStructure1D>();
                var branchLocations = networkDiscretization.GetLocationsForBranch(branch);

                var structureWithInvalidLocation = branchStructures.FirstOrDefault(bs => branchLocations.Any(bl => Math.Abs(bl.Chainage - bs.Chainage) < BranchFeature.Epsilon));

                if (structureWithInvalidLocation == null) continue;

                var msg = String.Format("Original discretization is invalid: structure {0} is on a grid point", structureWithInvalidLocation.Name);

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
                branch.BranchFeatures.OfType<IStructure1D>()
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

                var message = String.Format("No grid points defined between structure {0} and {1}",
                                            branchStructureFirst.Name, branchStructureSecond.Name);
                yield return
                    new ValidationIssue(branchStructureSecond, ValidationSeverity.Error, message, networkDiscretization)
                    ;
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
                var message = string.Format(Resources.WaterFlowModel1DDiscretizationValidator_CheckBranchLocations_Not_enough_grid_points_defined_for_branch__0___Make_sure_you_have_at_least_gridpoints_at_start_and_end_of_branch_,
                                            branch.Name);

                yield return new ValidationIssue(branch, ValidationSeverity.Error, message, networkDiscretization);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateNetworkDiscretizationQBoundariesAndStructures(IDiscretization networkDiscretization,
            IEnumerable<WaterFlowModel1DBoundaryNodeData> boundaryNodeData, IBranch branch, List<INetworkLocation> branchLocations)
        {
            var structures = branch.BranchFeatures.OfType<ICompositeBranchStructure>().OrderBy(s => s.Chainage).ToList();
            if (!structures.Any())
            {
                yield break;
            }

            // source
            var qBoundary = GetQBoundary(boundaryNodeData, branch.Source);
            if (qBoundary != null)
            {

                var localStructure = structures.First();
                if (localStructure.Structures.Count == 1 && localStructure.Structures[0] is ExtraResistance)
                {
                    // Extra Resistance will be Checked Later, not here tp prevent double errors
                    yield break;
                }
                var startChainage = 0.0d;
                var endChainage = structures.First().Chainage;
                if (!branchLocations.Any(b => b.Chainage > startChainage && b.Chainage < endChainage))
                {
                    yield return new ValidationIssue(structures.First(), ValidationSeverity.Warning,
                                                     String.Format("A grid point should exist between structure {0} and Q-boundary {1}.",
                                                     structures.First().Name, branch.Source.Name),
                                                     networkDiscretization);
                }
            }

            // target
            qBoundary = GetQBoundary(boundaryNodeData, branch.Target);
            if (qBoundary != null)
            {
                var localStructure = structures.Last();
                if (localStructure.Structures.Count == 1 && localStructure.Structures[0] is ExtraResistance)
                {
                    // Extra Resistance will be Checked Later, not here tp prevent double errors
                    yield break;
                }
                var startChainage = structures.Last().Chainage;
                var endChainage = branch.Length;
                if (!branchLocations.Any(b => b.Chainage > startChainage && b.Chainage < endChainage))
                {
                    yield return new ValidationIssue(structures.First(), ValidationSeverity.Warning,
                                                     String.Format("A grid point should exist between structure {0} and Q-boundary {1}.",
                                                     structures.Last().Name, branch.Target.Name),
                                                     networkDiscretization);
                }
            }
        }

        private static IEnumerable<ValidationIssue> ValidateNetworkDiscretizationBoundariesAndExtraResistances(IDiscretization networkDiscretization,
            IEnumerable<WaterFlowModel1DBoundaryNodeData> boundaryNodeData, IBranch branch, List<INetworkLocation> branchLocations)
        {
            var extraresistances = branch.BranchFeatures.OfType<IExtraResistance>().OrderBy(s => s.Chainage).ToList();
            if (!extraresistances.Any())
            {
                yield break;
            }

            // source
            var startChainage = 0.0d;
            var endChainage = extraresistances.First().Chainage;
            if (!branchLocations.Any(b=>b.Chainage > startChainage && b.Chainage < endChainage))
            {
                yield return new ValidationIssue(extraresistances.First(), ValidationSeverity.Warning,
                                                 String.Format("A grid point should exist between Extra Resistance {0} and Boundary {1}.",
                                                 extraresistances.First().Name, branch.Source.Name),
                                                 networkDiscretization);
            }

            // target
            startChainage = extraresistances.Last().Chainage;
            endChainage = branch.Length;
            if (!branchLocations.Any(b => b.Chainage > startChainage && b.Chainage < endChainage))
            {
                yield return new ValidationIssue(extraresistances.First(), ValidationSeverity.Warning,
                                                    String.Format(
                                                        "A grid point should exist between Extra Resistance {0} and Boundary {1}.",
                                                        extraresistances.Last().Name, branch.Target.Name),
                                                    networkDiscretization);
            }
        }

        private static WaterFlowModel1DBoundaryNodeData GetQBoundary(IEnumerable<WaterFlowModel1DBoundaryNodeData> boundaryNodeData, INode node)
        {
            var boundary = boundaryNodeData.FirstOrDefault(bc => bc.Feature == node);
            if (boundary != null && (boundary.DataType == WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries ||
                                     boundary.DataType == WaterFlowModel1DBoundaryNodeDataType.FlowConstant))
            {
                return boundary;
            }
            return null;
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
            var issues = ValidationHelper.ValidateDuplicateNames(networkDiscretization.Locations.Values.Cast<INameable>(),
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