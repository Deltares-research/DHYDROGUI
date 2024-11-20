using System;
using System.Collections.Generic;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    /// <summary>
    /// Class for validating all the <see cref="Lateral"/> in <see cref="WaterFlowFMModel"/>.
    /// </summary>
    public static class WaterFlowFMLateralValidator
    {
        /// <summary>
        /// Validates all <see cref="Lateral"/> in the <see cref="WaterFlowFMModel"/>.
        /// </summary>
        /// <param name="model">Models of which the laterals will be validated</param>
        /// <returns>A <see cref="ValidationReport"/> containing all messages from lateral validation.</returns>
        public static ValidationReport Validate(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();

            IEventedList<Lateral> laterals = model.Laterals;

            if (laterals == null)
            {
                return ValidationReport.Empty(Resources.WaterFlowFMLateralValidator_Validate_Water_flow_FM_model_Laterals);
            }

            foreach (Lateral lateral in laterals)
            {
                ValidateDischargeTimeZone(issues, lateral.Data.Discharge.TimeSeries, lateral.Name);
            }

            return new ValidationReport(Resources.WaterFlowFMLateralValidator_Validate_Water_flow_FM_model_Laterals, issues);
        }

        private static void ValidateDischargeTimeZone(List<ValidationIssue> issues, LateralDischargeFunction timeSeries, string lateralName)
        {
            if (OutsideAllowedTimeZoneRange(timeSeries.TimeZone))
            {
                issues.Add(new ValidationIssue(lateralName, ValidationSeverity.Error,
                                               string.Format(Resources.WaterFlowFMLateralValidator_ValidateDischargeTimeZone_Time_zone_of_lateral___0___falls_outside_of_allowed_range__12_00_and__12_00, lateralName)));
            }
        }

        private static bool OutsideAllowedTimeZoneRange(TimeSpan timeZone)
        {
            return timeZone > new TimeSpan(12, 0, 0) || timeZone < new TimeSpan(-12, 0, 0);
        }
    }
}