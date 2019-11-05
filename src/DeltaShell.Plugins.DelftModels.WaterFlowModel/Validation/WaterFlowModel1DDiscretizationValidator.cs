using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Validators;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Networks;
using AggregationOptions = DeltaShell.NGHS.IO.DataObjects.Model1D.AggregationOptions;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation
{
    public static class WaterFlowModel1DDiscretizationValidator
    {
        public static ValidationReport Validate(IDiscretization networkDiscretization,
            WaterFlowModel1D waterFlowModel1D)
        {
            var report = DiscretizationValidator.Validate(networkDiscretization);
            var issues = new List<ValidationIssue>();
            var boundaryNodeData = waterFlowModel1D != null ? waterFlowModel1D.BoundaryConditions : null;
            if (networkDiscretization.Locations.Values.Count == 0) return report;

            var branchesWithoutGridSegments = DiscretizationValidator.GetBranchesWithoutGridSegments(networkDiscretization);
            var branchLocationsLookup = networkDiscretization.Locations.Values.GroupBy(l => l.Branch).ToDictionary(g => g.Key, g => g);

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

                if (waterFlowModel1D != null)
                {
                    var branchLocations = branchLocationsLookup[branch].ToList();

                    issues.AddRange(ValidateNetworkDiscretizationQBoundariesAndStructures(networkDiscretization,
                        boundaryNodeData, branch, branchLocations));
                    issues.AddRange(ValidateNetworkDiscretizationBoundariesAndExtraResistances(networkDiscretization,
                        boundaryNodeData, branch, branchLocations));
                }
            }
            IEnumerable<ValidationReport> finiteVolumeValidationReport = null;
            if (CheckFiniteVolume(waterFlowModel1D)) //bleh
            {
                var finiteVolumeIssues = FiniteVolumeCheckStructuresNotOnGridPoints(networkDiscretization);
                if (finiteVolumeIssues.Count > 0)
                {
                    finiteVolumeValidationReport = report.SubReports.Concat(new[] { new ValidationReport("Finite volume", finiteVolumeIssues) });
                }
            }
            return new ValidationReport(report.Category, issues.Count > 0 ? report.Issues.Concat(issues) : report.Issues, finiteVolumeValidationReport ?? report.SubReports);
        }

        private static bool CheckFiniteVolume(WaterFlowModel1D waterFlowModel1D)
        {
            if (waterFlowModel1D == null)
                return true; //hack: if no model, check finite volume errors (as finite volume calls this validation manually)

            var gridTypeParameter = waterFlowModel1D.OutputSettings.EngineParameters.First(p => p.Name == Model1DParameterNames.FiniteVolumeGridType);
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
        
        private static IEnumerable<ValidationIssue> ValidateNetworkDiscretizationQBoundariesAndStructures(IDiscretization networkDiscretization,
            IEnumerable<Model1DBoundaryNodeData> boundaryNodeData, IBranch branch, List<INetworkLocation> branchLocations)
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
            IEnumerable<Model1DBoundaryNodeData> boundaryNodeData, IBranch branch, List<INetworkLocation> branchLocations)
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

        private static Model1DBoundaryNodeData GetQBoundary(IEnumerable<Model1DBoundaryNodeData> boundaryNodeData, INode node)
        {
            var boundary = boundaryNodeData.FirstOrDefault(bc => bc.Feature == node);
            if (boundary != null && (boundary.DataType == Model1DBoundaryNodeDataType.FlowTimeSeries ||
                                     boundary.DataType == Model1DBoundaryNodeDataType.FlowConstant))
            {
                return boundary;
            }
            return null;
        }
    }
}