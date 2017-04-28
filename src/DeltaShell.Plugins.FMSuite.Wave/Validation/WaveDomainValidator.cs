using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;

namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    public static class WaveDomainValidator
    {
        public static ValidationReport Validate(WaveModel model)
        {
            return new ValidationReport("Waves Model Domain",
                                        WaveDomainHelper.GetAllDomains(model.OuterDomain)
                                                        .Select(ValidateDomain)
                                                        .ToList());
        }

        private static ValidationReport ValidateDomain(WaveDomainData domain)
        {
            var issues = new List<ValidationIssue>();

            issues.AddRange(ValidateGrid(domain));
            issues.AddRange(ValidateBathymetry(domain));
            
            if (domain.SpectralDomainData.NDir == 0 && !domain.SpectralDomainData.UseDefaultDirectionalSpace)
            {
                issues.Add(new ValidationIssue(domain, ValidationSeverity.Error, "Number of directions cannot be zero", domain));
            }
            if (domain.SpectralDomainData.NFreq == 0 && !domain.SpectralDomainData.UseDefaultFrequencySpace)
            {
                issues.Add(new ValidationIssue(domain, ValidationSeverity.Error, "Number of frequencies cannot be zero", domain));
            }
            
            return new ValidationReport(string.Format("Domain: {0}", domain.Name), issues);
        }

        private static IEnumerable<ValidationIssue> ValidateBathymetry(WaveDomainData domain)
        {
            if (domain.Bathymetry.Size1 != domain.Grid.Size1 ||
                domain.Bathymetry.Size2 != domain.Grid.Size2)
            {
                yield return
                    new ValidationIssue(domain.Bathymetry, ValidationSeverity.Error, "Bathymetry does not match grid",
                        domain.Bathymetry);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateGrid(WaveDomainData domain)
        {
            if (!domain.Grid.X.Values.Any() || !domain.Grid.Y.Values.Any())
            {
                yield return new ValidationIssue(domain.Grid, ValidationSeverity.Error, "Grid (" + domain.Name+ ") is empty", domain.Grid);
            }

            int nrZeroXs = domain.Grid.X.Values.Count(v => v.Equals(0.0));
            if (nrZeroXs > 0)
            {
                yield return
                    new ValidationIssue(domain.Grid, ValidationSeverity.Error,
                        "Grid contains " + nrZeroXs + " invalid x-coordinate(s); coordinates are not allowed to be exactly equal to 0.0",
                        domain.Grid);
            }

            int nrZeroYs = domain.Grid.Y.Values.Count(v => v.Equals(0.0));
            if (nrZeroYs > 0)
            {
                yield return
                    new ValidationIssue(domain.Grid, ValidationSeverity.Error,
                        "Grid contains " + nrZeroYs + " invalid y-coordinate(s); coordinates are not allowed to be exactly equal to 0.0",
                        domain.Grid);
            }
        }
    }
}