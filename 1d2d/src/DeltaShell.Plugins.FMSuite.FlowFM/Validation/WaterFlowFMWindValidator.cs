using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class WaterFlowFMWindValidator
    {
        private const string subject = "Wind";

        public static ValidationReport Validate(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();
            foreach (var windField in model.WindFields)
            {
                if (windField.Data != null && windField.Data.Arguments[0] is IVariable<DateTime>)
                {
                    var times = windField.Data.Arguments[0].GetValues<DateTime>();
                    if (!times.Any())
                    {
                        issues.Add(new ValidationIssue(subject, ValidationSeverity.Error,
                            string.Format("No data defined in wind time series {0}", windField.Name), windField));
                    }
                    else if (times.First() > model.StartTime || times.Last() < model.StopTime)
                    {
                        issues.Add(new ValidationIssue(subject, ValidationSeverity.Error,
                            string.Format("Time series interval does not span model run time for {0}", windField.Name),
                            windField));
                    }
                }

                var griddedWindField = windField as GriddedWindField;
                if (griddedWindField != null)
                {
                    if (!File.Exists(griddedWindField.WindFilePath))
                    {
                        issues.Add(new ValidationIssue(subject, ValidationSeverity.Error,
                            $"Could not find wind file {griddedWindField.WindFilePath} for {griddedWindField.Name}", griddedWindField));
                    }
                    if (griddedWindField.SeparateGridFile && !File.Exists(griddedWindField.GridFilePath))
                    {
                        issues.Add(new ValidationIssue(subject, ValidationSeverity.Error,
                            $"Could not find grid file {griddedWindField.GridFilePath} for {griddedWindField.Name}", griddedWindField));
                    }
                }

                if (windField is SpiderWebWindField spiderWebWindField && 
                    !File.Exists(spiderWebWindField.WindFilePath))
                {
                    issues.Add(new ValidationIssue(subject, ValidationSeverity.Error,
                        $"Could not find wind file {spiderWebWindField.WindFilePath} for {spiderWebWindField.Name}", spiderWebWindField));
                }
            }

            return new ValidationReport("Water flow FM model wind forcing", issues);
        }
    }
}
