using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Hydro.Validators;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class WaterFlowFmModelValidationExtensions
    {
        public static ValidationReport Validate(this WaterFlowFMModel model)
        {
            var validationReports = new[]
            {
                ValidateSpatiallyVaryingSedimentCoverage(model),
                ValidateCoordinateSystem(model),
                WaterFlowFMModelComputationalGridValidator.Validate(model.NetworkDiscretization, model),
                HydroNetworkValidator.Validate(model.Network),
                WaterFlowFMGridValidator.Validate(model),
                ValidateLinks(model.Links),
                ValidateBathymetry(model),
                ValidatePhysicalProcesses(model),
                WaterFlowFMRoughnessValidator.Validate(model),
                WaterFlowFMWindValidator.Validate(model),
                WaterFlowFMMeteoValidation.Validate(model),
                WaterFlowFMModelDefinitionValidator.Validate(model),
                WaterFlowFMBoundaryConditionValidator.Validate(model),
                WaterFlowFMArea2DValidator.Validate(model),
                ValidateRestartInput(model),
                WaterFlowFMEmbankmentValidator.Validate(model),
                WaterFlowFMEnclosureValidator.Validate(model)
            };

            var subReports = model.UseMorSed
                ? validationReports.Plus(WaterFlowFMSedimentMorphologyValidator.ValidateWithMorphologyBetaWarning(model))
                : validationReports;

            var validationReport = new ValidationReport(model.Name + " (Water Flow FM Model)",
                subReports);
            
            return validationReport;
        }

        private static ValidationReport ValidateLinks(IEnumerable<ILink1D2D> links)
        {
            var invalidLinkIssues = links
                .Where(l => l.FaceIndex < 0 || l.DiscretisationPointIndex < 0)
                .Select(l => $"Link {l.Name} has invalid indices (cell index {l.FaceIndex} - computation point {l.DiscretisationPointIndex})")
                .Select(m => new ValidationIssue(links, ValidationSeverity.Error, m, links))
                .ToArray();

            return new ValidationReport("Links", invalidLinkIssues);
        }

        private static ValidationReport ValidateSpatiallyVaryingSedimentCoverage(WaterFlowFMModel model)
        {
             var unstructuredGridCoverages = GetSpatiallyVaryingSedimentCoveragesWithTag(model, "IniSedThick");

            var issues = unstructuredGridCoverages
                .Where(HasNoDataValue)
                .Select(c => new ValidationIssue(model, ValidationSeverity.Error, $"SedimentThickness {c.Name} is not fully covering the grid, please cover entire grid"))
                .ToList();
            
            return new ValidationReport("SedimentThickness", issues);
        }

        private static bool HasNoDataValue(UnstructuredGridCoverage coverage)
        {
            var component = coverage?.Components.FirstOrDefault();
            if (component == null) return false;

            var coverageValues = component?.GetValues<double>();
            if (coverageValues == null) return true;

            return coverageValues.Any(v => Equals(v, component.NoDataValue));
        }

        private static IEnumerable<UnstructuredGridCoverage> GetSpatiallyVaryingSedimentCoveragesWithTag(WaterFlowFMModel model, string tag)
        {
            var spatiallyVaryingSedimentPropertyNames = model.SedimentFractions
                .SelectMany(f => f.CurrentSedimentType.Properties)
                .OfType<ISpatiallyVaryingSedimentProperty>()
                .Where(p => p.IsSpatiallyVarying)
                .Select(p => p.SpatiallyVaryingName);

            var sedimentThicknessDataItems = spatiallyVaryingSedimentPropertyNames
                .Select(n => model.DataItems.FirstOrDefault(di => di.Name == n && di.Name.Contains(tag)))
                .Where(di => di != null);
            
            return sedimentThicknessDataItems.Select(di => di.Value as UnstructuredGridCoverage).Where(c => c != null);
        }

        private static ValidationReport ValidatePhysicalProcesses(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();

            ValidateCoverageValues(model, model.Roughness, issues);
            ValidateCoverageValues(model, model.Viscosity, issues);
            ValidateCoverageValues(model, model.Diffusivity, issues);
            ValidateCoverageValues(model, model.Infiltration, issues);

            return new ValidationReport("Physical Processes", issues);
        }

        private static void ValidateCoverageValues(IModel model, UnstructuredGridCoverage coverage, ICollection<ValidationIssue> issues)
        {
            var values = coverage.GetValues<double>();
            if (values.Contains(coverage.Components[0].NoDataValue))
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Info,
                                               $"{coverage.Name} contains unspecified points, the calculation kernel will replace these with default values."));
            }
        }

        private static ValidationReport ValidateBathymetry(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();

            var values = model.Bathymetry.GetValues<double>();
            if (values.Contains(model.Bathymetry.Components[0].NoDataValue))
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Info,
                    string.Format("Bathymetry contains unspecified points, the calculation kernel will replace these with default values")));
            }
            return new ValidationReport("Bathymetry", issues);
        }

        private static ValidationReport ValidateCoordinateSystem(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();

            if (model.CoordinateSystem == null)
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Info,
                                               "No coordinate system specified. The kernel will assume a projected system."));
            }
            else if (model.CoordinateSystem.IsGeographic && model.CoordinateSystem.AuthorityCode != 4326 /*WGS84*/)
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Warning,
                    "The geographic coordinate system specified may lead to incorrect results in the calculation. The calculation kernel only supports the WGS84 spherical system."));
            }
            return new ValidationReport("Model Coordinate System", issues);
        }

        private static ValidationReport ValidateRestartInput(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();

            var initCondCategory = model.ModelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalValue1D).PropertyDefinition.Category;
            string restartFileName = model.ModelDefinition.GetModelProperty(KnownProperties.RestartFile).GetValueAsString();

            if (string.IsNullOrWhiteSpace(restartFileName))
            {
                return new ValidationReport("Initial Conditions", issues);
            }

            var validationShortcut = new FmValidationShortcut
            {
                FlowFmModel = model,
                TabName = "Initial conditions"
            };

            var splitFileName = restartFileName.Split(new[] { '_', '.' }, StringSplitOptions.RemoveEmptyEntries);
            var length = splitFileName.Length;

            bool nameOK = splitFileName.Last() == "nc";
            if (length < 5 || length > 2 && splitFileName[length - 2] != "rst")
            {
                nameOK = false;
            }

            if (length > 4)
            {
                var dateTimeString = string.Concat(splitFileName[length - 4], splitFileName[length - 3]);
                DateTime dateTime;
                if (!DateTime.TryParseExact(dateTimeString, "yyyyMMddhhmmss", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out dateTime))
                {
                    nameOK = false;
                }
            }

            if (!nameOK)
            {
                issues.Add(new ValidationIssue(initCondCategory, ValidationSeverity.Error,
                    $"Invalid restart file name \"{restartFileName}\": your file should be formatted as <name>_yyyyMMdd_HHmmss_rst.nc", validationShortcut));
            }

            if (string.IsNullOrWhiteSpace(model.ModelDefinition.ModelDirectory)) 
                return new ValidationReport("Initial Conditions", issues);

            // model has been saved, restart file can be checked
            string restartFilePath = Path.Combine(model.ModelDefinition.ModelDirectory, restartFileName);
            if (!File.Exists(restartFilePath))
            {
                issues.Add(new ValidationIssue(initCondCategory, ValidationSeverity.Error,
                    $"Restart file {restartFileName} does not exist (full path: {restartFilePath}).", validationShortcut));
            }

            return new ValidationReport("Initial Conditions", issues);
        }
    }
}
