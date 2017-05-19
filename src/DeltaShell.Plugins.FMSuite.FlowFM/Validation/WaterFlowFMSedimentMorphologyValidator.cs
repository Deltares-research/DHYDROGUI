using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DelftTools.Utils.Validation;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class WaterFlowFMSedimentMorphologyValidator
    {
        public static ValidationIssue ValidateSedimentName(string name)
        {
            Regex regex = new Regex("^[a-zA-Z0-9_-]*$");
            if (!regex.IsMatch(name))
            {
                return new ValidationIssue(name, ValidationSeverity.Error,
                    "Value cannot be coverted to valid sediment fraction name. You can only use characters, numbers, underscore (_) and hyphen (-)");
            }
            return null;
        }

        public static ValidationReport ValidateMorphologyBetaWarning(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();

            if (model.UseMorSed)
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Warning,
                    string.Format("********Morphology is beta version********{0}You are using morphology / sediment in this model. Please be aware this feature is in beta!", Environment.NewLine)));

                foreach (var sedimentFraction in model.SedimentFractions)
                {
                    var issue = ValidateSedimentName(sedimentFraction.Name);
                    if(issue != null)
                        issues.Add(issue);
                }
            }
            
            return new ValidationReport("Morphology / Sediment Beta warning", issues);
        }
    }
}
