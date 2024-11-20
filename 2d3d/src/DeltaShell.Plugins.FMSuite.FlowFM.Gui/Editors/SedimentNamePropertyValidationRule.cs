using System;
using System.Globalization;
using System.Windows.Controls;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public class SedimentNamePropertyValidationRule : ValidationRule
    {
        #region Overrides of ValidationRule

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var strValue = Convert.ToString(value);

            if (string.IsNullOrEmpty(strValue))
            {
                return new ValidationResult(false, "Value cannot be coverted to string.");
            }

            ValidationIssue validationIssue = WaterFlowFMSedimentMorphologyValidator.ValidateSedimentName(strValue);
            if (validationIssue != null && validationIssue.Severity == ValidationSeverity.Error)
            {
                return new ValidationResult(false, validationIssue.Message);
            }

            return new ValidationResult(true, null);
        }

        #endregion
    }
}