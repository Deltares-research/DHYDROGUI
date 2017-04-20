using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public class WaterFlowFMModelTimersValidator: ModelTimersValidator
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

        public override IEnumerable<ValidationIssue> ValidateModelTimers(ITimeDependentModel model, TimeSpan outputTimeStep, object viewData = null)
        {
            var waterFlowFmModel = model as WaterFlowFMModel;
            if (waterFlowFmModel == null) yield break;

            OutputTimeStepShouldBePositive = (bool) waterFlowFmModel.ModelDefinition.GetModelProperty(GuiProperties.WriteMapFile).Value;
            
            Resolution = new TimeSpan(0, 0, 0, 1);

            var timerCategory = waterFlowFmModel.ModelDefinition.GetModelProperty(GuiProperties.StartTime).PropertyDefinition.Category;
            var baseTimeIssues = base.ValidateModelTimers(model, outputTimeStep, viewData).ToList();
            if (timerCategory != null)
            {
                baseTimeIssues.ForEach(i => i.Subject = timerCategory);
            }
            
            foreach (var issue in baseTimeIssues)
                yield return issue;

            Resolution = new TimeSpan(0, 0, 0, 0, 1);
            
            if (waterFlowFmModel.ReferenceTime > waterFlowFmModel.StartTime)
            {
                yield return new ValidationIssue(timerCategory, ValidationSeverity.Error, "Model start time precedes reference time", model);
            }

            if (waterFlowFmModel.WriteRestart && waterFlowFmModel.SaveStateTimeStep.Ticks == 0)
            {
                yield return new ValidationIssue(timerCategory, ValidationSeverity.Error, "Restart time interval should be strictly positive if write restart is true");
            }

            var issues = new[]
                {
                    CreateMultipleOfModelTimeStepIssue(waterFlowFmModel, GuiProperties.HisOutputDeltaT, "His output"),
                    CreateMultipleOfModelTimeStepIssue(waterFlowFmModel, GuiProperties.MapOutputDeltaT, "Map output"),
                    CreateMultipleOfModelTimeStepIssue(waterFlowFmModel, GuiProperties.RstOutputDeltaT, "Rst output"),
                    CreateMultipleOfModelTimeStepIssue(waterFlowFmModel, GuiProperties.WaqOutputDeltaT, "Waq output")
                };

            foreach (var issue in issues.Where(i => i != null))
                yield return issue;
        }

        private static ValidationIssue CreateMultipleOfModelTimeStepIssue(WaterFlowFMModel waterFlowFmModel, string guiTimeSpanParameter, string outputName)
        {
            var waterFlowFmProperty = waterFlowFmModel.ModelDefinition.GetModelProperty(guiTimeSpanParameter);
            var parameterTimeSpan = (TimeSpan)waterFlowFmProperty.Value;

            if (waterFlowFmModel.TimeStep.Ticks != 0 && parameterTimeSpan.Ticks%waterFlowFmModel.TimeStep.Ticks == 0) // is multiple
                return null;

            var category = waterFlowFmProperty.PropertyDefinition.Category;
            var errorMessage = string.Format("{0} interval must be a multiple of the output timestep.", outputName);

            return new ValidationIssue(category, ValidationSeverity.Error, errorMessage,waterFlowFmModel);
        }
    }
}
