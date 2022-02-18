using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
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
                ComputationalGridValidator.Validate(model.NetworkDiscretization, model.Grid, model.MinimumSegmentLength),
                HydroNetworkValidator.Validate(model.Network),
                ValidateLinks(model),
                ValidateBathymetry(model),
                ValidatePhysicalProcesses(model),
                WaterFlowFMRoughnessValidator.Validate(model),
                WaterFlowFMWindValidator.Validate(model),
                WaterFlowFMMeteoValidation.Validate(model),
                WaterFlowFMModelDefinitionValidator.Validate(model),
                WaterFlowFMBoundaryConditionValidator.Validate(model),
                WaterFlowFMArea2DValidator.Validate(model),
                ValidateInitialConditions(model),
                WaterFlowFMEmbankmentValidator.Validate(model),
                WaterFlowFMEnclosureValidator.Validate(model),
                WaterFlowFMHydroLinksValidator.Validate(model)
            };

            var subReports = model.UseMorSed
                                 ? validationReports.Plus(WaterFlowFMSedimentMorphologyValidator.ValidateWithMorphologyBetaWarning(model))
                                 : validationReports;

            var validationReport = new ValidationReport(model.Name + " (Water Flow FM Model)",
                subReports);
            
            return validationReport;
        }
        
        private static ValidationReport ValidateLinks(WaterFlowFMModel model)
        {
            var invalidLinkIssues = new List<ValidationIssue>();
            foreach (ILink1D2D link in model.Links)
            {
                if (link.FaceIndex >= 0 && link.DiscretisationPointIndex >= 0)
                {
                    continue;
                }

                var message = $"Link {link.Name} has invalid indices (cell index {link.FaceIndex} - computation point {link.DiscretisationPointIndex})";
                invalidLinkIssues.Add(new ValidationIssue(model.Links, ValidationSeverity.Error, message, new ValidatedFeatures(model.Region, link)));
            }

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

            var coordinateSettingsShortcut = new FmValidationShortcut
            {
                FlowFmModel = model,
                TabName = "General"
            };
            
            if (model.CoordinateSystem == null)
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Info,
                                               "No coordinate system specified. The kernel will assume a projected system.", coordinateSettingsShortcut));
            }
            else if (model.CoordinateSystem.IsGeographic && model.CoordinateSystem.AuthorityCode != 4326 /*WGS84*/)
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Warning,
                    "The geographic coordinate system specified may lead to incorrect results in the calculation. The calculation kernel only supports the WGS84 spherical system.", coordinateSettingsShortcut));
            }
            return new ValidationReport("Model Coordinate System", issues);
        }
        private static ValidationReport ValidateInitialConditions(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();

            var initCondCategory = model.ModelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalValue1D).PropertyDefinition.Category;
            string restartFileName = model.ModelDefinition.GetModelProperty(KnownProperties.RestartFile).GetValueAsString();

            var validationShortcut = new FmValidationShortcut
            {
                FlowFmModel = model,
                TabName = "Initial conditions"
            };

            if (string.IsNullOrWhiteSpace(restartFileName))
            {
                // no restart file used. check global 2D water depth initial condition (not stored in mdu, spatial definition required)
                var initialWaterCondition2DQuantity = (InitialConditionQuantity)(int)model.ModelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalQuantity2D).Value;
                var waterDepthSpatialOperations = model.ModelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.InitialWaterDepthDataItemName);
                if (initialWaterCondition2DQuantity == InitialConditionQuantity.WaterDepth && (waterDepthSpatialOperations == null || !waterDepthSpatialOperations.Any()))
                {
                    issues.Add(new ValidationIssue(model, ValidationSeverity.Error,
                                                   "Initial 2D water depth selected, but no spatial operations defined for water depth. (Global water depth value currently not supported.) Add operations and re-validate saving model.)", validationShortcut));
                }
                return new ValidationReport("Initial Conditions", issues);
            }

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
