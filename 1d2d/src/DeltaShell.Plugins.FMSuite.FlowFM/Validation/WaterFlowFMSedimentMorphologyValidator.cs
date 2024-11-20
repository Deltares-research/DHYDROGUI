using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class WaterFlowFMSedimentMorphologyValidator
    {
        public static ValidationIssue ValidateSedimentName(string name) // Also used in WPF validation!
        {
            Regex regex = new Regex("^[a-zA-Z0-9_-]*$");
            if (!regex.IsMatch(name) || string.IsNullOrEmpty(name))
            {
                return new ValidationIssue(name, ValidationSeverity.Error,
                    Resources.WaterFlowFMSedimentMorphologyValidator_ValidateSedimentName_Value_cannot_be_coverted_to_valid_sediment_fraction_name__You_can_only_use_characters__numbers__underscore_____and_hyphen_____and_it_cannot_start_only_with_a______it_NEEDS_a_closing_____);
            }
            return null;
        }

        public static ValidationReport ValidateWithMorphologyBetaWarning(WaterFlowFMModel model)
        {
            if(!model.UseMorSed) return new ValidationReport("Sediment & Morphology", Enumerable.Empty<ValidationIssue>());

            var issues = new List<ValidationIssue>();

            issues.AddRange(ValidateAtLeastOneSedimentFractionInModel(model));

            issues.AddRange(model.SedimentFractions.Select(sedimentFraction => ValidateSedimentName(sedimentFraction.Name)).Where(issue => issue != null));

            issues.AddRange(ValidateInitialSedimentThicknessOfSedimentFractionsInModel(model));

            issues.AddRange(ValidateSpaciallyVaryingSedimentFractionProperties(model));

            issues.Add(new ValidationIssue(model, ValidationSeverity.Warning,
                string.Format(Resources.WaterFlowFMSedimentMorphologyValidator_ValidateMorphologyBetaWarning_________Morphology_is_beta_version_________0_You_are_using_morphology___sediment_in_this_model__Please_be_aware_this_feature_is_in_beta_, Environment.NewLine)));
            
            return new ValidationReport(Resources.WaterFlowFMSedimentMorphologyValidator_ValidateMorphologyBetaWarning_Morphology___Sediment_Beta_warning, issues);
        }

        /// <summary>
        /// Validates if there is at least one Sediment Fraction in the model.
        /// When Morphology is used in the model, one Sediment Fraction is required.
        /// </summary>
        /// <param name="model">The WaterFlowFM model.</param>
        /// <returns></returns>
        private static IEnumerable<ValidationIssue> ValidateAtLeastOneSedimentFractionInModel(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();

            if (model.SedimentFractions != null && model.SedimentFractions.Any()) return issues;

            issues.Add(new ValidationIssue(model.ModelDefinition.GetTabName(KnownProperties.SedFile, fmModel:model),
                ValidationSeverity.Error,
                Resources
                    .WaterFlowFMSedimentMorphologyValidator_ValidateAtLeastOneSedimentFractionInModel_At_least_one_sediment_fraction_is_required_when_using_morphology,
                model));

            return issues;
        }

        private static IEnumerable<ValidationIssue> ValidateInitialSedimentThicknessOfSedimentFractionsInModel(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();

            var sedimentFraction = model.SedimentFractions;
            if (!sedimentFraction.Any()) return issues;
            
            var anySedimentFractionsWithInitialSedimentThicknessGreaterThanZero =
                sedimentFraction.Any(
                    sf =>
                        sf.CurrentSedimentType.Properties.OfType<SedimentProperty<double>>()
                            .Any(p => p.Name == "IniSedThick" && p.Value > 0));

            if (!anySedimentFractionsWithInitialSedimentThicknessGreaterThanZero)
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Error, Resources.WaterFlowFMSedimentMorphologyValidator_ValidateInitialSedimentThicknessOfSedimentFractionsInModel_At_least_one_sediment_fraction_should_have_a_positive_thickness));
            }

            return issues;
        }

        /// <summary>
        /// Validates if the spatial operations of the FM model are interpolated, such that an xyz-file can be written.
        /// </summary>
        /// <param name="model">The WaterFlowFMModel that is being </param>
        /// <returns></returns>
        private static IEnumerable<ValidationIssue> ValidateSpaciallyVaryingSedimentFractionProperties(WaterFlowFMModel model)
        {
            var spaciallyVaryingPropertyNames = model.SedimentFractions
                .SelectMany(s => s.GetAllActiveSpatiallyVaryingPropertyNames()).Where(n => !n.EndsWith("SedConc"))
                .ToList();
            var dataItemsFound = spaciallyVaryingPropertyNames
                .SelectMany(spaceVarName => model.DataItems.Where(di => di.Name.Equals(spaceVarName)))
                .ToArray();
            var dataItemsWithConverter = dataItemsFound
                .Where(d => d.ValueConverter is SpatialOperationSetValueConverter).ToList();
            
            var spatialOperations = model.GetSpatialOperationsLookupTable(dataItemsWithConverter);

            // If spatial operation is ValueOperationBase, then add a new ValidationIssue
            var issues = new List<ValidationIssue>();
            foreach (var operations in spatialOperations)
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
