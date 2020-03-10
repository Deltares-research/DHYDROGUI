using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Networks;

namespace DelftTools.Hydro.Validators
{
    public static class DiscretizationValidator
    {
        public static ValidationReport Validate(IDiscretization networkDiscretization)
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
                var branchLocationsLookup = networkDiscretization.Locations.Values.GroupBy(l => l.Branch).ToDictionary(g => g.Key, g => g);
                
                foreach (var branch in networkDiscretization.Network.Branches)
                {
                    if (branchesWithoutGridSegments.Contains(branch) || !branchLocationsLookup.ContainsKey(branch))
                    {
                        //ok, we have a branch without network calculation locations.
                        //This does not mean this is wrong. The calculation location could be on another branch
                        //check if start coordinate has a calculation location on another branch, check by coordinate!

                        if (!networkDiscretization
                            .Locations
                            .Values
                            .Any(l => l.Geometry.Coordinate.Equals2D(branch.Source?.Geometry?.Coordinate, 0.001)))
                        {
                            var message = $"No computational grid cells defined for branch : {branch.Name}, not at start of branch; can not start calculation.";
                            issues.Add(new ValidationIssue(branch, ValidationSeverity.Error, message,
                                networkDiscretization));
                        }
                        if (!networkDiscretization
                            .Locations
                            .Values
                            .Any(l => l.Geometry.Coordinate.Equals2D(branch.Target?.Geometry?.Coordinate, 0.001)))
                        {
                            var message = $"No computational grid cells defined for branch : {branch.Name}, not at end of branch; can not start calculation.";
                            issues.Add(new ValidationIssue(branch, ValidationSeverity.Error, message,
                                networkDiscretization));
                        }



                        continue; //no computational grid, so no sense reporting additional errors
                    }

                    var branchLocations = branchLocationsLookup[branch].ToList();

                    issues.AddRange(CheckBranchLocations(networkDiscretization, branch, branchLocations));

                    issues.AddRange(CheckBranchStructureLocations(networkDiscretization, branch, branchLocations));
                }
            }

            var subReports = ValidateIds(networkDiscretization);
            
            return new ValidationReport("Computational grid", issues, subReports);
        }
        private static IEnumerable<ValidationReport> ValidateIds(IDiscretization networkDiscretization)
        {
            var reports = new List<ValidationReport>();

            //FM1D2D-636
            // var issues = ValidationHelper.ValidateDuplicateNames(networkDiscretization.Locations.Values.Cast<INameable>(),
            //     "grid points", networkDiscretization, ValidationSeverity.Warning);
            // if (issues.Any())
            // {
            //     reports.Add(new ValidationReport("General", issues));
            // }

            return reports;
        }

        public static IEnumerable<IBranch> GetBranchesWithoutGridSegments(IDiscretization networkDiscretization)
        {
            var branches = new HashSet<IBranch>(networkDiscretization.Network.Branches);

            foreach (var seg in networkDiscretization.Segments.Values)
            {
                branches.Remove(seg.Branch);
            }

            return branches;
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
                    new ValidationIssue(branchStructureSecond, ValidationSeverity.Error, message, networkDiscretization);
            }
        }

        private static IEnumerable<ValidationIssue> CheckBranchLocations(IDiscretization networkDiscretization,
            IBranch branch,
            IList<INetworkLocation> branchLocations)
        {
            if (networkDiscretization.SegmentGenerationMethod == SegmentGenerationMethod.SegmentBetweenLocationsAndConnectedBranchesWithoutLocationOnThemFullyCovered)
            {
                var hasCalculationPointAtStartOfBranchOnOtherBranch = false;
                //Check if we have a location of this branch at begin of branch
                if (branchLocations.Any(l => Math.Abs(l.Chainage) < BranchFeature.Epsilon))
                {
                    hasCalculationPointAtStartOfBranchOnOtherBranch = true;
                }
                else
                {
                    //Ah no location at begin of this branch
                    //check if we have a network location in our complete discretization
                    //at the same location as the source node coordinate (start of our branch)
                    if (networkDiscretization.Locations.Values.Any(l =>
                        l?.Geometry?.Coordinate != null &&
                        branch?.Source?.Geometry?.Coordinate != null &&
                        l.Geometry.Coordinate.Equals2D(branch.Source.Geometry.Coordinate, 0.01)))
                        hasCalculationPointAtStartOfBranchOnOtherBranch = true;
                }
                //Check if we have a location of this branch at end of branch
                var hasCalculationPointAtEndOfBranchOnOtherBranch = false;
                if (branchLocations.Any(l => DoubleEquals(l.Chainage, branch.Length)))
                {
                    hasCalculationPointAtEndOfBranchOnOtherBranch = true;
                }
                else
                {
                    //Ah no location at end of this branch
                    //check if we have a network location in our complete discretization
                    //at the same location as the target node coordinate (end of our branch)
                    if (networkDiscretization.Locations.Values.Any(l =>
                        l?.Geometry?.Coordinate != null &&
                        branch?.Target?.Geometry?.Coordinate != null &&
                        l.Geometry.Coordinate.Equals2D(branch.Target.Geometry.Coordinate, 0.01)))
                        hasCalculationPointAtEndOfBranchOnOtherBranch = true;
                }
                //ok, i need to make this nicer... but i don't have time..
                if (hasCalculationPointAtStartOfBranchOnOtherBranch && hasCalculationPointAtEndOfBranchOnOtherBranch)
                    yield break;

            }
            // Each branch should have calculation point at start and end
            else if (!branchLocations.Any(l => Math.Abs(l.Chainage) < BranchFeature.Epsilon) ||
                !branchLocations.Any(l => DoubleEquals(l.Chainage, branch.Length)))
            {
                var message = String.Format("Not enough grid points defined for branch {0}. " +
                                            "Make sure you have at least gridpoints at start and end of branch.",
                    branch.Name);

                yield return new ValidationIssue(branch, ValidationSeverity.Error, message, networkDiscretization);
            }
        }
        private static bool DoubleEquals(double d1, double d2)
        {
            return Math.Abs(d1 - d2) < 0.000001;
        }
    }
}