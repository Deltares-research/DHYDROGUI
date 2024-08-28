using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public class WaterFlowFMModelTimersValidator : ModelTimersValidator
    {
        public WaterFlowFMModelTimersValidator()
        {
            CalculationTimeStepShouldBePositive = false;
            CalculationTimeShouldBeTimeStepMultiple = false;
            CalculationTimeShouldBeOutputTimeStepMultiple = false;
            OutputTimeStepShouldBePositive = false;
            OutputTimeStepShouldBeTimeStepMultiple = false;
            Resolution = new TimeSpan(0, 0, 0, 0, 1);
        }

        public override IEnumerable<ValidationIssue> ValidateModelTimers(
            ITimeDependentModel model, TimeSpan outputTimeStep, object viewData = null)
        {
            var waterFlowFmModel = model as WaterFlowFMModel;
            if (waterFlowFmModel == null)
            {
                yield break;
            }

            OutputTimeStepShouldBePositive =
                (bool) waterFlowFmModel.ModelDefinition.GetModelProperty(GuiProperties.WriteMapFile).Value;

            Resolution = new TimeSpan(0, 0, 0, 1);

            string timerCategory = waterFlowFmModel.ModelDefinition.GetModelProperty(KnownProperties.StartDateTime)
                                                   .PropertyDefinition.Category;
            List<ValidationIssue> baseTimeIssues = base.ValidateModelTimers(model, outputTimeStep, viewData).ToList();
            if (timerCategory != null)
            {
                var clonedWithSubjectChanged = new List<ValidationIssue>();
                baseTimeIssues.ForEach(i => clonedWithSubjectChanged.Add(new ValidationIssue(timerCategory, i.Severity, i.Message, i.ViewData)));
                baseTimeIssues = clonedWithSubjectChanged;
            }

            foreach (ValidationIssue issue in baseTimeIssues)
            {
                yield return issue;
            }

            Resolution = new TimeSpan(0, 0, 0, 0, 1);

            if (waterFlowFmModel.ReferenceTime > waterFlowFmModel.StartTime)
            {
                var validationShortcut = new FmValidationShortcut()
                {
                    FlowFmModel = model as WaterFlowFMModel,
                    TabName = "Time Frame"
                };
                yield return new ValidationIssue(timerCategory, ValidationSeverity.Error,
                                                 Resources.WaterFlowFMModelTimersValidator_Model_start_time_precedes_reference_time, validationShortcut);
            }

            var issues = new List<ValidationIssue>();
            if (waterFlowFmModel.WriteHisFile)
            {
                ValidationIssue issue = CreateMultipleOfUserTimeStepIssue(waterFlowFmModel, GuiProperties.HisOutputDeltaT, "His output");
                issues.Add(issue);
            }

            if (waterFlowFmModel.WriteMapFile)
            {
                ValidationIssue issue = CreateMultipleOfUserTimeStepIssue(waterFlowFmModel, GuiProperties.MapOutputDeltaT, "Map output");
                issues.Add(issue);
            }
            
            if (waterFlowFmModel.WriteClassMapFile)
            {
                ValidationIssue issue = CreateMultipleOfUserTimeStepIssue(waterFlowFmModel, GuiProperties.ClassMapOutputDeltaT, "Class map output");
                issues.Add(issue);
            }
            
            if (waterFlowFmModel.WriteRstFile)
            {
                ValidationIssue issue = CreateMultipleOfUserTimeStepIssue(waterFlowFmModel, GuiProperties.RstOutputDeltaT, "Restart output");
                issues.Add(issue);
            }

            if (waterFlowFmModel.SpecifyWaqOutputInterval)
            {
                ValidationIssue issue = CreateMultipleOfUserTimeStepIssue(waterFlowFmModel, GuiProperties.WaqOutputDeltaT, "Waq output");
                issues.Add(issue);
            }

            foreach (ValidationIssue issue in issues)
            {
                if (issue != null)
                {
                    yield return issue;
                }
            }
        }

        private static ValidationIssue CreateMultipleOfUserTimeStepIssue(
            WaterFlowFMModel waterFlowFmModel, string guiTimeSpanParameter, string outputName)
        {
            WaterFlowFMProperty waterFlowFmProperty =
                waterFlowFmModel.ModelDefinition.GetModelProperty(guiTimeSpanParameter);
            var parameterTimeSpan = (TimeSpan) waterFlowFmProperty.Value;

            if (waterFlowFmModel.TimeStep.Ticks != 0 &&
                parameterTimeSpan.Ticks % waterFlowFmModel.TimeStep.Ticks == 0) // is multiple
            {
                return null;
            }

            string category = waterFlowFmProperty.PropertyDefinition.Category;
            string errorMessage = string.Format(Resources.Interval_must_be_a_multiple_of_the_user_timestep, outputName);

            return new ValidationIssue(category, ValidationSeverity.Error, errorMessage, waterFlowFmModel);
        }
    }
}