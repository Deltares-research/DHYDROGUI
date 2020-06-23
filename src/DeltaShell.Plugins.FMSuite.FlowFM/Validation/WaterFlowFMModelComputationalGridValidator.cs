using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
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
            
            if (!NetworkDiscretizationIsValid(networkDiscretization))
            {
                var invalidGrid = flowFmModel != null && (flowFmModel.Grid == null || flowFmModel.Grid.IsEmpty);
                //We only show the invalid network error if the grid is also invalid.
                if (invalidGrid)
                {
                    issues.Add(new ValidationIssue(networkDiscretization, ValidationSeverity.Error,
                        Resources.WaterFlowFMModelComputationalGridValidator_Validate_No_computational_grid_defined_));
                }

                return new ValidationReport(CategoryName, issues);
            }

            var branchesWithoutGridSegments = GetBranchesWithoutGridSegments(networkDiscretization).ToList();
/*
            var branchLocationsLookup =
                networkDiscretization.Locations.Values.GroupBy(l => l.Branch).ToDictionary(g => g.Key, g => g);
*/

            foreach (var branch in networkDiscretization.Network.Branches)
            {
                var sewerConnection = branch as ISewerConnection;
                if(sewerConnection != null && Math.Abs(sewerConnection.Length) < 10e-6) continue;

                if (branchesWithoutGridSegments.Contains(branch))//|| !branchLocationsLookup.ContainsKey(branch)
                {
                    var message =
                        string.Format(
                            Resources.WaterFlowFMModelComputationalGridValidator_Validate_No_computational_grid_cells_defined_for_branch__0___can_not_start_calculation_,
                            branch.Name);
                    issues.Add(new ValidationIssue(branch, ValidationSeverity.Error, message, networkDiscretization));

                    continue; //no computational grid, so no sense reporting additional errors
                }

                /*var branchLocations = branchLocationsLookup[branch].ToList();*/

                //issues.AddRange(CheckBranchLocations(networkDiscretization, branch));

                issues.AddRange(CheckBranchStructureLocations(networkDiscretization, branch, networkDiscretization.GetLocationsForBranch(branch)));

                /* QBoundaries and Resistances tests removed as we do not need them now. 
                    * Once snapped features are available for the 1D they will be required, needed validations
                    * can be found on the WaterFlow1D Validation.
                */
            }

            var subReports = ValidateIds(networkDiscretization);
            var finiteVolumeIssues = FiniteVolumeCheckStructuresNotOnGridPoints(networkDiscretization);
            if (finiteVolumeIssues.Count > 0)
            {
                subReports = subReports.Concat(new[] { new ValidationReport("Finite volume", finiteVolumeIssues) });
            }

            if (flowFmModel != null)
            {
                double dxmin1D;
                if(double.TryParse(flowFmModel.ModelDefinition.GetModelProperty("Dxmin1D").GetValueAsString(), NumberStyles.AllowDecimalPoint|NumberStyles.AllowExponent|NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture, out dxmin1D))
                {
                    var segmentIssues = ValidateSegments(networkDiscretization, dxmin1D);
                    if(segmentIssues.Any())
                        subReports = subReports.Concat(new[] { new ValidationReport("Segment issues", segmentIssues) });
                }
            }
            return new ValidationReport(CategoryName, issues, subReports);
        }

        private static IEnumerable<ValidationIssue> ValidateSegments(IDiscretization networkDiscretization, double dxmin1D)
        {
            foreach (var segment in networkDiscretization.Segments.AllValues)
            {
                if (segment.Length < dxmin1D )
                {
                    var locations = networkDiscretization.GetLocationsForBranch(segment.Branch);
                    var startPoint = locations.FirstOrDefault(l => Math.Abs(l.Chainage - segment.Chainage) < double.Epsilon);
                    var endPoint = locations.FirstOrDefault(l => Math.Abs(l.Chainage - segment.EndChainage) < double.Epsilon);
                    if(startPoint == null || endPoint == null) continue;
                    yield return new ValidationIssue(segment, ValidationSeverity.Error, $"Segment {segment.Name} on branch {segment.Branch} between start point {startPoint} at chainage {segment.Chainage} and end point {endPoint} at chainage {segment.EndChainage} is shorter ({segment.Length} than minimum length provided (Dxmin1D) : {dxmin1D}.");
                }
            }
            yield break;
            
        }

        private static IEnumerable<ValidationIssue> CheckBranchLocations(IDiscretization networkDiscretization, IBranch branch)
        {
            /*var firstXCoordinate = branch?.Geometry?.Coordinates?.FirstOrDefault()?.X;
            if(!firstXCoordinate.HasValue) yield break;
            var firstYCoordinate = branch?.Geometry?.Coordinates?.FirstOrDefault()?.Y;
            if (!firstYCoordinate.HasValue) yield break;
            var lastXCoordinate = branch?.Geometry?.Coordinates?.LastOrDefault()?.X;
            if (!lastXCoordinate.HasValue) yield break;
            var lastYCoordinate = branch?.Geometry?.Coordinates?.LastOrDefault()?.Y;
            if (!lastYCoordinate.HasValue) yield break;
            if (networkDiscretization.GetLocationOnBranch(firstXCoordinate.Value, firstYCoordinate.Value) == null ||
                networkDiscretization.GetLocationOnBranch(lastXCoordinate.Value, lastYCoordinate.Value) == null)*/
            var locationsForThisBranch = networkDiscretization.GetLocationsForBranch(branch);
            if(!locationsForThisBranch.Any(l => l.Chainage <= BranchFeature.Epsilon) ||
               !locationsForThisBranch.Any(l => DoubleEquals(l.Chainage, branch.Length)))
            {
                var message = string.Format(
                    Resources
                        .WaterFlowFMModelComputationalGridValidator_CheckBranchLocations_Not_enough_grid_points_defined_for_branch__0___Make_sure_you_have_at_least_gridpoints_at_start_and_end_of_branch_,
                    branch.Name);

                yield return new ValidationIssue(branch, ValidationSeverity.Error, message, networkDiscretization);
            }
        }

        private static bool NetworkDiscretizationIsValid(IDiscretization networkDiscretization)
        {
            if (networkDiscretization == null 
                || networkDiscretization.Locations == null
                || networkDiscretization.Locations.Values.Count == 0)
                return false;
            return true;
        }

        private static IList<ValidationIssue> FiniteVolumeCheckStructuresNotOnGridPoints(IDiscretization networkDiscretization)
        {
            var issues = new List<ValidationIssue>();

            foreach (var branch in networkDiscretization.Network.Branches)
            {
                var sewerConnection = branch as ISewerConnection;
                if(sewerConnection != null) continue;

                var branchStructures = branch.BranchFeatures.OfType<IStructure1D>();
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
                                                                                  IList<INetworkLocation> branchLocations)
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

                var message = string.Format(Resources.WaterFlowFMModelComputationalGridValidator_CheckBranchStructureLocations_No_grid_points_defined_between_structure__0__and__1_,
                                            branchStructureFirst.Name, branchStructureSecond.Name);
                yield return
                    new ValidationIssue(branchStructureSecond.GetStructureType(), ValidationSeverity.Error, message, branchStructureFirst.Chainage);
            }
        }
        

        private static IEnumerable<IBranch> GetBranchesWithoutGridSegments(IDiscretization networkDiscretization)
        {
            var branches = new HashSet<IBranch>(networkDiscretization.Network.Branches);

            foreach(var seg in networkDiscretization.Segments.Values)
            {
                branches.Remove(seg.Branch);
            }
            var otherBranches = new HashSet<IBranch>();
            foreach (var branch in branches)
            {
                var b = networkDiscretization.Locations?.Values.Any(l => l.Geometry.Coordinate.Equals(branch.Source.Geometry.Coordinate));
                if (b != null)
                {
                    otherBranches.Add(branch);
                    continue;
                }
                b = networkDiscretization.Locations?.Values.Any(l => l.Geometry.Coordinate.Equals(branch.Target.Geometry.Coordinate));
                if (b != null)
                {
                    otherBranches.Add(branch);
                }
            }

            branches.ExceptWith(otherBranches);
            return branches;//.RemoveWhere(b=>networkDiscretization.Locations.Values.Select(l => l.Geometry.Coordinate.Equals(b.Source.Geometry.Coordinate)));
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