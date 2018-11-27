using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils;
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
                }
            }

            var subReports = ValidateIds(networkDiscretization);
            
            return new ValidationReport("Computational grid", issues, subReports);
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