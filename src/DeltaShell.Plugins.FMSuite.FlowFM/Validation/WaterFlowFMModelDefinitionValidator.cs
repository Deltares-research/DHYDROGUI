using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class WaterFlowFMModelDefinitionValidator
    {
        public static ValidationReport Validate(WaterFlowFMModel model)
        {
            var modelDefinition = model.ModelDefinition;
            var groupReports = new List<ValidationReport>();
            var timerCategory = model.ModelDefinition.GetModelProperty(GuiProperties.StartTime).PropertyDefinition.Category;
            var solverProperty = modelDefinition.GetModelProperty(KnownProperties.SolverType);
            foreach (var propertyGroup in modelDefinition.Properties.GroupBy(p => p.PropertyDefinition.Category))
            {
                var issues = new List<ValidationIssue>();
                foreach (var waterFlowFmProperty in propertyGroup)
                {
                    if (waterFlowFmProperty.IsVisible(modelDefinition.Properties) &&
                        waterFlowFmProperty.IsEnabled(modelDefinition.Properties) && !waterFlowFmProperty.Validate())
                    {
                        issues.Add(new ValidationIssue(propertyGroup.Key, ValidationSeverity.Error,
                            "Parameter " + waterFlowFmProperty.PropertyDefinition.Caption +
                            " outside validity range" +
                            RangeToString(waterFlowFmProperty.MinValue,
                                waterFlowFmProperty.MaxValue) + ".", model));
                    }
                    if (waterFlowFmProperty == solverProperty)
                    {
                        var solver = int.Parse(waterFlowFmProperty.GetValueAsString());
                        if (solver > 4)
                        {
                            issues.Add(new ValidationIssue(propertyGroup.Key, ValidationSeverity.Error,
                                "Solver type selected for parallel run; this is currently not possible in GUI."));
                        }
                    }
                }
                if (propertyGroup.Key.Equals(timerCategory))
                {
                    var validator = new WaterFlowFMModelTimersValidator();
                    issues.AddRange(validator.ValidateModelTimers(model, model.OutputTimeStep, model));
                }
                groupReports.Add(new ValidationReport(propertyGroup.Key, issues));
            }
            return new ValidationReport("WaterFlow FM model definition", groupReports);
        }

        private static string RangeToString(object min, object max)
        {
            return " [" + (min == null ? "-inf" : Convert.ToDouble(min).ToString()) + "," + (max == null ? "+inf" : Convert.ToDouble(max).ToString()) + "]";
        }
    }
}