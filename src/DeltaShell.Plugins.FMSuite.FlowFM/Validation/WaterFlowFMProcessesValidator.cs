using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    /// <summary>
    /// A validator for the Processes of a <see cref="WaterFlowFMModel"/>.
    /// </summary>
    public static class WaterFlowFMProcessesValidator
    {
        private const string title = "Physical Processes";

        /// <summary>
        /// Creates a validation report for the Processes of this <paramref name="model"/>.
        /// </summary>
        /// <param name="model"> The model to validate the Processes for. </param>
        /// <returns>
        /// A <see cref="ValidationReport"/> with the issues found.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="model"/> is <c>null</c>.
        /// </exception>
        public static ValidationReport Validate(WaterFlowFMModel model)
        {
            Ensure.NotNull(model, nameof(model));

            var issues = new List<ValidationIssue>();

            ValidateCoverage(model.SpatialData.Roughness, model, issues);
            ValidateCoverage(model.SpatialData.Viscosity, model, issues);
            ValidateCoverage(model.SpatialData.Diffusivity, model, issues);
            ValidateHeatFluxModel(model, issues);

            return new ValidationReport(title, issues);
        }

        private static void ValidateHeatFluxModel(WaterFlowFMModel model, ICollection<ValidationIssue> issues)
        {
            HeatFluxModel heatFluxModel = model.ModelDefinition.HeatFluxModel;
            if (heatFluxModel.Type == HeatFluxModelType.Composite
                && !heatFluxModel.MeteoData.GetValues<double>().Any())
            {
                issues.Add(model, ValidationSeverity.Error,
                           Resources.ValidatePhysicalProcesses_HeatFluxModel_has_composite_model_option_selected_for_temperature_but_no_meteo_data_was_specified,
                           heatFluxModel);
            }
        }

        private static void Add(this ICollection<ValidationIssue> issues, IDataItemOwner model, ValidationSeverity severity, string message, object viewData = null)
        {
            issues.Add(new ValidationIssue(model, severity, message, viewData));
        }

        private static void ValidateCoverage(IFunction coverage, IDataItemOwner model, ICollection<ValidationIssue> issues)
        {
            IMultiDimensionalArray<double> values = coverage.GetValues<double>();
            if (values.Any(v => Equals(v, coverage.Components[0].NoDataValue)))
            {
                issues.Add(model, ValidationSeverity.Info,
                           string.Format(Resources.WaterFlowFMProcessesValidator_Coverage_contains_unspecified_points, coverage.Name));
            }
        }
    }
}