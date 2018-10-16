using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;

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

            ValidateFmMeteoLocationTypes(model, issues);

            ValidateFmMeteoQuantitiesCanHaveOnlyOneGlobalLocationType(model.FmMeteoFields, issues);

            return new ValidationReport("Water flow FM model meteo items", issues);
        }

        public static void ValidateFmMeteoQuantitiesCanHaveOnlyOneGlobalLocationType(IEventedList<IFmMeteoField> modelFmMeteoFields, List<ValidationIssue> issues)
        {
            foreach (FmMeteoQuantity quantity in (FmMeteoQuantity[])Enum.GetValues(typeof(FmMeteoQuantity)))
            {
                if (modelFmMeteoFields.Count(fmMeteoField => fmMeteoField.Quantity == quantity && fmMeteoField.FmMeteoLocationType == FmMeteoLocationType.Global) > 1)
                {
                    issues.Add(new ValidationIssue(subject, ValidationSeverity.Error, $"There is more than one global {quantity} present, only {modelFmMeteoFields.FirstOrDefault(field => field.Quantity == quantity)?.Name} will be used in the calculation"));
                }
            }
        }

        public static void ValidateFmMeteoLocationTypes(WaterFlowFMModel model, List<ValidationIssue> issues)
        {
            foreach (var meteoField in model.FmMeteoFields)
            {
                if (meteoField.FmMeteoLocationType == FmMeteoLocationType.Feature ||
                    meteoField.FmMeteoLocationType == FmMeteoLocationType.Grid ||
                    meteoField.FmMeteoLocationType == FmMeteoLocationType.Polygon)
                {
                    issues.Add(new ValidationIssue(subject, ValidationSeverity.Error,
                        "Meteo location types: feature, grid & polygon are not yet supported."));
                }
            }
        }
    }
}
