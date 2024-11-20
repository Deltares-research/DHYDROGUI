using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    public static class WaveDomainValidator
    {
        public static ValidationReport Validate(WaveModel model)
        {
            return new ValidationReport("Waves Model Domain", ValidateAllDomains(model));
        }

        private static IEnumerable<ValidationReport> ValidateAllDomains(WaveModel model)
        {
            var reportList = new List<ValidationReport> {ValidateAllDomainsShareCoordinateSystem(model)};
            
            reportList.AddRange(WaveDomainHelper.GetAllDomains(model.OuterDomain)
                                                .Select((domain) => ValidateDomain(domain, model))
                                                .ToList());
            return reportList;
        }

        private static ValidationReport ValidateAllDomainsShareCoordinateSystem(WaveModel model)
        {
            var issues = new List<ValidationIssue>();
            IWaveDomainData domain = model.OuterDomain;
            IList<IWaveDomainData> domains = WaveDomainHelper.GetAllDomains(domain);
            List<IWaveDomainData> sphericalDomains =
                domains.Where(d => CheckDomainGrid(d, WaveModel.CoordinateSystemType.Spherical)).ToList();

            if (sphericalDomains.Any() &&
                domains.Any(d => CheckDomainGrid(d, WaveModel.CoordinateSystemType.Cartesian)))
            {
                issues.Add(new ValidationIssue(domain, ValidationSeverity.Error,
                                               Resources.WaveDomainValidator_ValidateAllDomainsShareCoordinateSystem_All_the_grids_Coordinate_System_should_be_the_same__either_Spherical_or_Cardesian));
            }
            else if (sphericalDomains.Count == domains.Count && model.ModelDefinition.WaveSetup)
            {
                var waveValidationShortcut = new WaveValidationShortcut
                {
                    WaveModel = model,
                    TabName = "Physical Processes"
                };
                issues.Add(new ValidationIssue(domain, ValidationSeverity.Error,
                                               Resources.WaveDomainValidator_ValidateAllDomainsShareCoordinateSystem_WaveSetup_should_be_false_when_using_Spherical_Coordinate_Systems_,
                                               waveValidationShortcut));
            }

            return new ValidationReport("Model domains", issues);
        }

        private static bool CheckDomainGrid(IWaveDomainData domain, string coordinateSystemName)
        {
            if (domain.Grid == null)
            {
                return false;
            }

            if (domain.Grid.Attributes.TryGetValue("CoordinateSystem", out string coordinateSystem))
            {
                return coordinateSystem == coordinateSystemName;
            }

            return false;
        }

        private static ValidationReport ValidateDomain(IWaveDomainData domain, WaveModel model)
        {
            var issues = new List<ValidationIssue>();

            issues.AddRange(ValidateGrid(domain));
            issues.AddRange(ValidateBathymetry(domain));

            if (domain.SpectralDomainData.NDir == 0 && !domain.SpectralDomainData.UseDefaultDirectionalSpace)
            {
                issues.Add(new ValidationIssue(domain, ValidationSeverity.Error, "Number of directions cannot be zero",
                                               domain));
            }

            if (domain.SpectralDomainData.NFreq == 0 && !domain.SpectralDomainData.UseDefaultFrequencySpace)
            {
                issues.Add(new ValidationIssue(domain, ValidationSeverity.Error, "Number of frequencies cannot be zero",
                                               domain));
            }

            if (!domain.UseGlobalMeteoData)
            {
                var validationShortcut = new DomainSpecificValidationShortcut(model, domain);
                
                foreach (string validationMessage in DomainMeteoDataValidator.Validate(domain.MeteoData))
                {
                    issues.Add(new ValidationIssue(domain, ValidationSeverity.Warning, validationMessage, validationShortcut));
                }
            }
            
            return new ValidationReport($"Domain: {domain.Name}", issues);
        }

        private static IEnumerable<ValidationIssue> ValidateBathymetry(IWaveDomainData domain)
        {
            if (domain.Bathymetry.Size1 != domain.Grid.Size1 ||
                domain.Bathymetry.Size2 != domain.Grid.Size2)
            {
                yield return new ValidationIssue(domain.Bathymetry, ValidationSeverity.Error, "Bathymetry does not match grid",
                                                 domain.Bathymetry);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateGrid(IWaveDomainData domain)
        {
            if (!domain.Grid.X.Values.Any() || !domain.Grid.Y.Values.Any())
            {
                yield return new ValidationIssue(domain.Grid, ValidationSeverity.Error,
                                                 "Grid (" + domain.Name + ") is empty", domain.Grid);
            }

            int nrZeroXs = domain.Grid.X.Values.Count(v => v.Equals(0.0));
            if (nrZeroXs > 0)
            {
                yield return new ValidationIssue(domain.Grid, ValidationSeverity.Error,
                                                 "Grid contains " + nrZeroXs +
                                                 " invalid x-coordinate(s); coordinates are not allowed to be exactly equal to 0.0",
                                                 domain.Grid);
            }

            int nrZeroYs = domain.Grid.Y.Values.Count(v => v.Equals(0.0));
            if (nrZeroYs > 0)
            {
                yield return new ValidationIssue(domain.Grid, ValidationSeverity.Error,
                                                 "Grid contains " + nrZeroYs +
                                                 " invalid y-coordinate(s); coordinates are not allowed to be exactly equal to 0.0",
                                                 domain.Grid);
            }
        }
    }
}