using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.Common.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class WaterFlowFmModelValidationExtensions
    {
        public static ValidationReport Validate(this WaterFlowFMModel model)
        {
            var outputParametersShortcut = new FmValidationShortcut
            {
                FlowFmModel = model,
                TabName = model.ModelDefinition.GetModelProperty(GuiProperties.RstOutputDeltaT).PropertyDefinition.Category
            };

            ValidationReport[] validationReports =
            {
                ValidateSpatiallyVaryingSedimentCoverage(model),
                ValidateCoordinateSystem(model),
                WaterFlowFMGridValidator.Validate(model),
                ValidateBathymetry(model),
                WaterFlowFMProcessesValidator.Validate(model),
                WaterFlowFMWindValidator.Validate(model),
                WaterFlowFMModelDefinitionValidator.Validate(model),
                WaterFlowFMBoundaryConditionValidator.Validate(model),
                WaterFlowFMLateralValidator.Validate(model),
                FMStructuresValidator.Validate(model),
                WaterFlowFMRestartInputValidator.Validate(model),
                RestartTimeRangeValidator.ValidateWriteRestartSettings(model.WriteRestart,
                                                                       model.RestartStartTime, model.RestartStopTime, model.RestartTimeStep,
                                                                       model.StartTime, model.TimeStep, outputParametersShortcut),
                WaterFlowFMEnclosureValidator.Validate(model)
            };

            IEnumerable<ValidationReport> subReports = model.UseMorSed
                                                           ? validationReports.Plus(
                                                               WaterFlowFMSedimentMorphologyValidator
                                                                   .ValidateMorphology(model))
                                                           : validationReports;

            var validationReport = new ValidationReport(model.Name + " (Water Flow FM Model)",
                                                        subReports);

            return validationReport;
        }

        private static ValidationReport ValidateSpatiallyVaryingSedimentCoverage(WaterFlowFMModel model)
        {
            IEnumerable<UnstructuredGridCoverage> unstructuredGridCoverages =
                GetSpatiallyVaryingSedimentCoveragesWithTag(model, "IniSedThick");

            List<ValidationIssue> issues = unstructuredGridCoverages
                                           .Where(HasNoDataValue)
                                           .Select(c => new ValidationIssue(
                                                       model, ValidationSeverity.Error,
                                                       $"SedimentThickness {c.Name} is not fully covering the grid, please cover entire grid"))
                                           .ToList();

            return new ValidationReport("SedimentThickness", issues);
        }

        private static bool HasNoDataValue(UnstructuredGridCoverage coverage)
        {
            IVariable component = coverage?.Components.FirstOrDefault();
            if (component == null)
            {
                return false;
            }

            IMultiDimensionalArray<double> coverageValues = component?.GetValues<double>();
            if (coverageValues == null)
            {
                return true;
            }

            return coverageValues.Any(v => Equals(v, component.NoDataValue));
        }

        private static IEnumerable<UnstructuredGridCoverage> GetSpatiallyVaryingSedimentCoveragesWithTag(
            WaterFlowFMModel model, string tag)
        {
            IEnumerable<string> spatiallyVaryingSedimentPropertyNames = model.SedimentFractions
                                                                             .SelectMany(
                                                                                 f => f.CurrentSedimentType.Properties)
                                                                             .OfType<ISpatiallyVaryingSedimentProperty
                                                                             >()
                                                                             .Where(p => p.IsSpatiallyVarying)
                                                                             .Select(p => p.SpatiallyVaryingName);

            IEnumerable<IDataItem> sedimentThicknessDataItems = spatiallyVaryingSedimentPropertyNames
                                                                .Select(n => model.AllDataItems.FirstOrDefault(
                                                                            di => di.Name == n &&
                                                                                  di.Name.Contains(tag)))
                                                                .Where(di => di != null);

            return sedimentThicknessDataItems.Select(di => di.Value as UnstructuredGridCoverage).Where(c => c != null);
        }

        private static ValidationReport ValidateBathymetry(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();

            IMultiDimensionalArray<double> values = model.SpatialData.Bathymetry.GetValues<double>();
            if (values.Contains(model.SpatialData.Bathymetry.Components[0].NoDataValue))
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Info,
                                               "Bathymetry contains unspecified points, the calculation kernel will replace these with default values"));
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

            return new ValidationReport("Coordinate System", issues);
        }
    }
}