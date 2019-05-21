using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

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

            string timerCategory = waterFlowFmModel.ModelDefinition.GetModelProperty(GuiProperties.StartTime)
                                                   .PropertyDefinition.Category;
            List<ValidationIssue> baseTimeIssues = base.ValidateModelTimers(model, outputTimeStep, viewData).ToList();
            if (timerCategory != null)
            {
                baseTimeIssues.ForEach(i => i.Subject = timerCategory);
            }

            foreach (ValidationIssue issue in baseTimeIssues)
            {
                yield return issue;
            }

            Resolution = new TimeSpan(0, 0, 0, 0, 1);

            if (waterFlowFmModel.ReferenceTime > waterFlowFmModel.StartTime)
            {
                var validationShortcut = new FmValidationShortcut
                {
                    FlowFmModel = (WaterFlowFMModel) model,
                    TabName = "Time Frame"
                };
                yield return new ValidationIssue(timerCategory, ValidationSeverity.Error,
                                                 "Model start time precedes reference time", validationShortcut);
            }

            if (waterFlowFmModel.WriteRestart && waterFlowFmModel.SaveStateTimeStep.Ticks == 0)
            {
                yield return new ValidationIssue(timerCategory, ValidationSeverity.Error,
                                                 "Restart time interval should be strictly positive if write restart is true");
            }

            ValidationIssue[] issues = new[]
            {
                CreateMultipleOfModelTimeStepIssue(waterFlowFmModel, GuiProperties.HisOutputDeltaT, "His output"),
                CreateMultipleOfModelTimeStepIssue(waterFlowFmModel, GuiProperties.MapOutputDeltaT, "Map output"),
                CreateMultipleOfModelTimeStepIssue(waterFlowFmModel, GuiProperties.RstOutputDeltaT, "Rst output"),
                CreateMultipleOfModelTimeStepIssue(waterFlowFmModel, GuiProperties.WaqOutputDeltaT, "Waq output")
            };

            foreach (ValidationIssue issue in issues.Where(i => i != null))
            {
                yield return issue;
            }
        }

        private static ValidationIssue CreateMultipleOfModelTimeStepIssue(
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
            string errorMessage = string.Format("{0} interval must be a multiple of the output timestep.", outputName);

            return new ValidationIssue(category, ValidationSeverity.Error, errorMessage, waterFlowFmModel);
        }
    }
}