using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    /// <summary>
    /// Static class responsible for adding validation issues for FM Meteo fields.
    /// </summary>
    public static class WaterFlowFMMeteoValidation
    {
        private const string subject = "Meteo";
         
        public static ValidationReport Validate(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();

            issues.AddRange(ValidateFmMeteoLocationTypesOfModelDefinitionFMMeteoFields(model.ModelDefinition.FmMeteoFields));

            issues.AddRange(ValidateFmMeteoQuantitiesCanHaveOnlyOneGlobalLocationType(model.ModelDefinition.FmMeteoFields));

            return new ValidationReport(Resources.WaterFlowFMMeteoValidation_Validate_Water_flow_FM_model_meteo_items, issues);
        }

        private static IEnumerable<ValidationIssue> ValidateFmMeteoQuantitiesCanHaveOnlyOneGlobalLocationType(IEventedList<IFmMeteoField> modelFmMeteoFields)
        {
            foreach (var fmMeteoQuantity in (FmMeteoQuantity[])Enum.GetValues(typeof(FmMeteoQuantity)))
            {
                if (modelFmMeteoFields.Count(fmMeteoField => fmMeteoField.Quantity == fmMeteoQuantity && fmMeteoField.FmMeteoLocationType == FmMeteoLocationType.Global) > 1)
                {
                    yield return new ValidationIssue(subject, ValidationSeverity.Error, string.Format(Resources.WaterFlowFMMeteoValidation_ValidateFmMeteoQuantitiesCanHaveOnlyOneGlobalLocationType_There_is_more_than_one_global__0__present__only__1__will_be_used_in_the_calculation, fmMeteoQuantity, modelFmMeteoFields.FirstOrDefault(field => field.Quantity == fmMeteoQuantity)?.Name));
                }
            }
        }

        private static Dictionary<FmMeteoLocationType, Func<ValidationIssue>> ValidateFmMeteoLocationTypesOfModelDefinitionFMMeteoFieldsGenerators = new Dictionary<FmMeteoLocationType, Func<ValidationIssue>>()
        {
            {FmMeteoLocationType.Feature, ValidateFmMeteoLocationTypesOfModelDefinitionFMMeteoFieldsNotPossibleGenerator},
            {FmMeteoLocationType.Grid, ValidateFmMeteoLocationTypesOfModelDefinitionFMMeteoFieldsNotPossibleGenerator},
            {FmMeteoLocationType.Polygon, ValidateFmMeteoLocationTypesOfModelDefinitionFMMeteoFieldsNotPossibleGenerator}
        };

        private static ValidationIssue ValidateFmMeteoLocationTypesOfModelDefinitionFMMeteoFieldsNotPossibleGenerator()
        {
            return new ValidationIssue(subject, ValidationSeverity.Error,
                Resources.WaterFlowFMMeteoValidation_ValidateFmMeteoLocationTypesOfModelDefinitionFMMeteoFieldsNotPossibleGenerator_Meteo_location_types__feature__grid___polygon_are_not_yet_supported_);
        }
        private static IEnumerable<ValidationIssue> ValidateFmMeteoLocationTypesOfModelDefinitionFMMeteoFields(IEnumerable<IFmMeteoField> fmMeteoFields)
        {
            foreach (var meteoField in fmMeteoFields)
            {
                Func<ValidationIssue> ValidateIssueGenerator = null;
                if (ValidateFmMeteoLocationTypesOfModelDefinitionFMMeteoFieldsGenerators.TryGetValue(meteoField.FmMeteoLocationType, out ValidateIssueGenerator))
                    yield return ValidateIssueGenerator();

            }
        }
    }
}
