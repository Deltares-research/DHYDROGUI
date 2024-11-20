using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using SharpMap.Api.SpatialOperations;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class WaterFlowFMSedimentMorphologyValidator
    {
        public static ValidationIssue ValidateSedimentName(string name) // Also used in WPF validation!
        {
            var regex = new Regex("^[a-zA-Z0-9_-]*$");
            if (!regex.IsMatch(name) || string.IsNullOrEmpty(name))
            {
                return new ValidationIssue(name, ValidationSeverity.Error,
                                           Resources
                                               .WaterFlowFMSedimentMorphologyValidator_ValidateSedimentName_Value_cannot_be_coverted_to_valid_sediment_fraction_name__You_can_only_use_characters__numbers__underscore_____and_hyphen_____and_it_cannot_start_only_with_a______it_NEEDS_a_closing_____);
            }

            return null;
        }

        public static ValidationReport ValidateMorphology(WaterFlowFMModel model)
        {
            if (!model.UseMorSed)
            {
                return new ValidationReport(
                    Resources.WaterFlowFMSedimentMorphologyValidator_ValidateMorphology_Morphology___Sediment,
                    Enumerable.Empty<ValidationIssue>());
            }

            var issues = new List<ValidationIssue>();

            issues.AddRange(ValidateAtLeastOneSedimentFractionInModel(model));

            issues.AddRange(model.SedimentFractions
                                 .Select(sedimentFraction => ValidateSedimentName(sedimentFraction.Name))
                                 .Where(issue => issue != null));

            issues.AddRange(ValidateInitialSedimentThicknessOfSedimentFractionsInModel(model));

            issues.AddRange(ValidateSpaciallyVaryingSedimentFractionProperties(model));

            return new ValidationReport(
                Resources.WaterFlowFMSedimentMorphologyValidator_ValidateMorphology_Morphology___Sediment, issues);
        }

        /// <summary>
        /// Validates if there is at least one Sediment Fraction in the model.
        /// When Morphology is used in the model, one Sediment Fraction is required.
        /// </summary>
        /// <param name="model"> The WaterFlowFM model. </param>
        /// <returns> </returns>
        private static IEnumerable<ValidationIssue> ValidateAtLeastOneSedimentFractionInModel(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();

            if (model.SedimentFractions != null && model.SedimentFractions.Any())
            {
                return issues;
            }

            string tabName = WaterFlowFMModelDefinition.GetTabName(KnownProperties.SedFile, fmModel: model);
            var validationShortcut = new FmValidationShortcut
            {
                FlowFmModel = model,
                TabName = tabName
            };
            issues.Add(new ValidationIssue(
                           tabName,
                           ValidationSeverity.Error,
                           Resources
                               .WaterFlowFMSedimentMorphologyValidator_ValidateAtLeastOneSedimentFractionInModel_At_least_one_sediment_fraction_is_required_when_using_morphology,
                           validationShortcut));

            return issues;
        }

        private static IEnumerable<ValidationIssue> ValidateInitialSedimentThicknessOfSedimentFractionsInModel(
            WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();

            IEventedList<ISedimentFraction> sedimentFraction = model.SedimentFractions;
            if (!sedimentFraction.Any())
            {
                return issues;
            }

            bool anySedimentFractionsWithInitialSedimentThicknessGreaterThanZero =
                sedimentFraction.Any(
                    sf =>
                        sf.CurrentSedimentType.Properties.OfType<SedimentProperty<double>>()
                          .Any(p => p.Name == "IniSedThick" && p.Value > 0));

            if (!anySedimentFractionsWithInitialSedimentThicknessGreaterThanZero)
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Error,
                                               Resources
                                                   .WaterFlowFMSedimentMorphologyValidator_ValidateInitialSedimentThicknessOfSedimentFractionsInModel_At_least_one_sediment_fraction_should_have_a_positive_thickness));
            }

            return issues;
        }

        /// <summary>
        /// Validates if the spatial operations of the FM model are interpolated, such that an xyz-file can be written.
        /// </summary>
        /// <param name="model"> The WaterFlowFMModel that is being </param>
        /// <returns> </returns>
        private static IEnumerable<ValidationIssue> ValidateSpaciallyVaryingSedimentFractionProperties(
            WaterFlowFMModel model)
        {
            List<string> spaciallyVaryingPropertyNames = model.SedimentFractions
                                                              .SelectMany(
                                                                  s => s.GetAllActiveSpatiallyVaryingPropertyNames())
                                                              .Where(n => !n.EndsWith("SedConc"))
                                                              .ToList();
            IDataItem[] dataItemsFound = spaciallyVaryingPropertyNames
                                         .SelectMany(spaceVarName =>
                                                         model.AllDataItems.Where(di => di.Name.Equals(spaceVarName)))
                                         .ToArray();
            List<IDataItem> dataItemsWithConverter = dataItemsFound
                                                     .Where(d => d.ValueConverter is SpatialOperationSetValueConverter)
                                                     .ToList();

            Dictionary<string, IList<ISpatialOperation>> spatialOperations =
                model.GetSpatialOperationsLookupTable(dataItemsWithConverter);

            // If spatial operation is ValueOperationBase, then add a new ValidationIssue
            var issues = new List<ValidationIssue>();
            foreach (KeyValuePair<string, IList<ISpatialOperation>> operations in spatialOperations)
            {
                issues.AddRange(operations.Value.OfType<ValueOperationBase>().Select(
                                    vo => new ValidationIssue(model, ValidationSeverity.Warning,
                                                              string.Format(
                                                                  Resources
                                                                      .SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_Cannot_create_xyz_file_for_spatial_varying_initial_condition__0__because_it_is_a_value_spatial_operation__please_interpolate_the_operation_to_the_grid_or,
                                                                  operations.Key))));
            }

            return issues;
        }
    }
}