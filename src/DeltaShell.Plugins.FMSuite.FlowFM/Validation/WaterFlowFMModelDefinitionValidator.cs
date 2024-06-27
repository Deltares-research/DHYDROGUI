using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.Extensions;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    /// <summary>
    /// Class for validating the <see cref="WaterFlowFMModelDefinition"/>.
    /// </summary>
    public static class WaterFlowFMModelDefinitionValidator
    {
        private const string physicalParametersTabName = "Physical Parameters";
        private const string layers3DTabName = "3D Layers";

        /// <summary>
        /// Validates the <see cref="WaterFlowFMModelDefinition"/> for the given model.
        /// </summary>
        /// <param name="model">The model to validate.</param>
        /// <returns>A <see cref="ValidationReport"/> containing the validation issues.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is <c>null</c>.</exception>
        public static ValidationReport Validate(WaterFlowFMModel model)
        {
            Ensure.NotNull(model, nameof(model));

            WaterFlowFMModelDefinition modelDefinition = model.ModelDefinition;

            string timeCategory = modelDefinition.GetModelProperty(KnownProperties.StartDateTime).PropertyDefinition.Category;
            WaterFlowFMProperty solverProperty = modelDefinition.GetModelProperty(KnownProperties.SolverType);
            WaterFlowFMProperty bedLevelTypeProperty = modelDefinition.GetModelProperty(KnownProperties.BedlevType);
            WaterFlowFMProperty conveyanceTypeProperty = modelDefinition.GetModelProperty(KnownProperties.Conveyance2d);

            var groupReports = new List<ValidationReport>();

            foreach (IGrouping<string, WaterFlowFMProperty> propertyGroup in GetPropertiesByGroup(modelDefinition))
            {
                var issues = new List<ValidationIssue>();

                string categoryName = propertyGroup.Key;
                foreach (WaterFlowFMProperty propertyToValidate in propertyGroup)
                {
                    ValidatePropertyUsingPropertyDefinition(propertyToValidate, model, categoryName, issues);

                    if (IsSolverTypeProperty(propertyToValidate, solverProperty))
                    {
                        ValidateSolverTypeProperty(propertyToValidate, issues, categoryName);
                    }

                    if (IsBedLevelProperty(propertyToValidate, bedLevelTypeProperty))
                    {
                        ValidateBedLevelProperty(propertyToValidate, model, issues);
                    }

                    if (IsConveyanceTypeProperty(propertyToValidate, conveyanceTypeProperty))
                    {
                        ValidateConveyanceType(propertyToValidate, model, issues);
                    }

                    if (Is3DLayerProperty(propertyToValidate))
                    {
                        Validate3DLayerProperty(propertyToValidate, propertyGroup, model, issues);
                    }
                }

                if (IsTimeCategory(categoryName, timeCategory))
                {
                    ValidateTimeCategory(model, issues);
                }

                groupReports.Add(new ValidationReport(propertyGroup.Key, issues));
            }

            return new ValidationReport(Resources.WaterFlowFMModelDefinitionValidator_WaterFlow_FM_model_definition, groupReports);
        }

        private static IEnumerable<IGrouping<string, WaterFlowFMProperty>> GetPropertiesByGroup(WaterFlowFMModelDefinition modelDefinition)
        {
            return modelDefinition.Properties.GroupBy(p => p.PropertyDefinition.Category);
        }

        private static void ValidatePropertyUsingPropertyDefinition(WaterFlowFMProperty property,
                                                                    IWaterFlowFMModel model,
                                                                    string category,
                                                                    ICollection<ValidationIssue> issues)
        {
            if (property.IsVisible(model.ModelDefinition.Properties)
                && property.IsEnabled(model.ModelDefinition.Properties)
                && !property.Validate())
            {
                string errorMessage = string.Format(Resources.WaterFlowFMModelDefinitionValidator_Parameter__0__outside_validity_range__1__,
                                                    property.PropertyDefinition.Caption,
                                                    RangeToString(property.MinValue, property.MaxValue));
                issues.Add(new ValidationIssue(category, ValidationSeverity.Error, errorMessage, model));
            }
        }

        private static string RangeToString(object min, object max)
        {
            return " [" + (min == null ? "-inf" : Convert.ToDouble(min).ToString()) + "," +
                   (max == null ? "+inf" : Convert.ToDouble(max).ToString()) + "]";
        }

        private static bool IsSolverTypeProperty(WaterFlowFMProperty propertyToValidate, WaterFlowFMProperty solverProperty)
        {
            return solverProperty != null && propertyToValidate == solverProperty;
        }

        private static void ValidateSolverTypeProperty(ModelProperty waterFlowFmProperty,
                                                       ICollection<ValidationIssue> issues,
                                                       string category)
        {
            int solver = int.Parse(waterFlowFmProperty.GetValueAsString());
            if (solver > 4)
            {
                issues.Add(new ValidationIssue(category,
                                               ValidationSeverity.Error,
                                               Resources.WaterFlowFMModelDefinitionValidator_Solver_type_selected_for_parallel_run__this_is_currently_not_possible_in_GUI_));
            }
        }

        private static bool IsBedLevelProperty(WaterFlowFMProperty propertyToValidate, WaterFlowFMProperty bedLevelTypeProperty)
        {
            return bedLevelTypeProperty != null && propertyToValidate == bedLevelTypeProperty;
        }

        private static void ValidateBedLevelProperty(ModelProperty waterFlowFmProperty,
                                                     WaterFlowFMModel model,
                                                     ICollection<ValidationIssue> issues)
        {
            // Whenever morphology is active, give an error in the validation report
            // in case the bed level locations is not set to 'cells' (BedlevType.val0)
            bool useMorSed = model.ModelDefinition.UseMorphologySediment;
            if (useMorSed
                && int.TryParse(waterFlowFmProperty.GetValueAsString(), out int bedLevelTypeNumber) &&
                !bedLevelTypeNumber.Equals((int)UnstructuredGridFileHelper.BedLevelLocation.Faces))
            {
                var validationShortcut = new FmValidationShortcut
                {
                    FlowFmModel = model,
                    TabName = physicalParametersTabName
                };
                issues.Add(new ValidationIssue(
                               model,
                               ValidationSeverity.Error,
                               Resources.WaterFlowFMModelDefinitionValidator_Validate_Bed_level_locations_should_be_set_to__faces__when_morphology_is_active_,
                               validationShortcut)
                );
            }
        }

        private static bool IsConveyanceTypeProperty(WaterFlowFMProperty propertyToValidate, WaterFlowFMProperty conveyanceTypeProperty)
        {
            return conveyanceTypeProperty != null && propertyToValidate == conveyanceTypeProperty;
        }

        private static void ValidateConveyanceType(ModelProperty propertyToValidate,
                                                   IWaterFlowFMModel model,
                                                   ICollection<ValidationIssue> issues)
        {
            // Whenever morphology is active, give an error in the validation report 
            // when if conveyance 2d type is not set to:
            // * R=HU 
            // * R=H  
            // * R=A/P
            bool useMorSed = model.ModelDefinition.UseMorphologySediment;
            if (useMorSed &&
                Enum.TryParse(propertyToValidate.GetValueAsString(), out Conveyance2DType currentConveyanceType) &&
                currentConveyanceType != Conveyance2DType.RisHU &&
                currentConveyanceType != Conveyance2DType.RisH &&
                currentConveyanceType != Conveyance2DType.RisAperP)
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Error, Resources.WaterFlowFMModelDefinitionValidator_Validate_));
            }
        }

        private static bool Is3DLayerProperty(WaterFlowFMProperty propertyToValidate)
        {
            string propertyName = propertyToValidate.PropertyDefinition.MduPropertyName;

            return propertyName.EqualsCaseInsensitive(KnownProperties.DzTop)
                   || propertyName.EqualsCaseInsensitive(KnownProperties.FloorLevTopLay)
                   || propertyName.EqualsCaseInsensitive(KnownProperties.DzTopUniAboveZ)
                   || propertyName.EqualsCaseInsensitive(KnownProperties.NumTopSig);
        }

        private static void Validate3DLayerProperty(WaterFlowFMProperty propertyToValidate,
                                                    IEnumerable<WaterFlowFMProperty> propertyGroup,
                                                    WaterFlowFMModel model,
                                                    ICollection<ValidationIssue> issues)
        {
            string errorMessage = WaterFlowFM3DLayerPropertyValidator.Validate(propertyToValidate, propertyGroup);
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                return;
            }

            var validationShortcut = new FmValidationShortcut
            {
                FlowFmModel = model,
                TabName = layers3DTabName
            };

            issues.Add(new ValidationIssue(string.Empty, ValidationSeverity.Error, errorMessage, validationShortcut));
        }

        private static bool IsTimeCategory(string currentCategory, string timerCategory)
        {
            return currentCategory.Equals(timerCategory);
        }

        private static void ValidateTimeCategory(WaterFlowFMModel model, List<ValidationIssue> issues)
        {
            var validator = new WaterFlowFMModelTimersValidator();
            issues.AddRange(validator.ValidateModelTimers(model, model.OutputTimeStep, model));
        }
    }
}