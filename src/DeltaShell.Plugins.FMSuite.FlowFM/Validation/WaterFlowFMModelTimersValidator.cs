using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Feature;

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
            baseTimeIssues = baseTimeIssues.Select(bti => new ValidationIssue(timerCategory, bti.Severity, bti.Message, bti.ViewData)).ToList();

            foreach (var issue in baseTimeIssues)
                yield return issue;

            Resolution = new TimeSpan(0, 0, 0, 0, 1);

            if (waterFlowFmModel.ReferenceTime > waterFlowFmModel.StartTime)
            {
                yield return new ValidationIssue(timerCategory, ValidationSeverity.Error, "Model start time precedes reference time", new FmValidationShortcut() { FlowFmModel = model as WaterFlowFMModel, TabName = "Time Frame" });
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
            // check if boundary 1d && lateral timeseries fit into timeframe
            foreach (var featureData in waterFlowFmModel.BoundaryConditions1D.Where(bc1d => bc1d.DataType == Model1DBoundaryNodeDataType.WaterLevelTimeSeries || bc1d.DataType == Model1DBoundaryNodeDataType.FlowTimeSeries).Cast<IFeatureData>().Concat(waterFlowFmModel.LateralSourcesData.Where(lsd => lsd.DataType == Model1DLateralDataType.FlowTimeSeries)))
            {
                var data = featureData?.Data as IFunction;
                if (data?.Arguments ==null || !data.Arguments.Any() || data.Arguments[0]?.ValueType != typeof(DateTime)) continue;
                var startTimeDefinedInFunction = (DateTime)data.Arguments[0].MinValue ;
                if(startTimeDefinedInFunction > model.StartTime)
                    yield return new ValidationIssue(timerCategory, ValidationSeverity.Error, $"Time Series function {featureData.Name} has times with values defined which starts ({startTimeDefinedInFunction:s}) later than expected starttime of the timeframe of the model ({model.StartTime:s}). Please adjust timeseries so it will fit in model time frame, or adjust model Start and Stop time so it will fit the timeseries.", featureData);
                var stopTimeDefinedInFunction = (DateTime)data.Arguments[0].MaxValue;
                if (stopTimeDefinedInFunction < model.StopTime)
                    yield return new ValidationIssue(timerCategory, ValidationSeverity.Error, $"Time Series function {featureData.Name} has time with values defined which ends earlier ({stopTimeDefinedInFunction}) than expected stoptime of the timeframe of model ({model.StopTime:s}). Please adjust timeseries so it will fit in model time frame, or adjust model Start and Stop time so it will fit the timeseries.",featureData);
                if ((startTimeDefinedInFunction > model.StartTime || stopTimeDefinedInFunction < model.StopTime ) && data.Arguments[0].ExtrapolationType != ExtrapolationType.None)
                    yield return new ValidationIssue(timerCategory, ValidationSeverity.Error, $"The timespan of the Time Series function {featureData.Name} does not cover the full period between model start and end time and extrapolation is set to {data.Arguments[0].ExtrapolationType}. The kernel cannot handle this.", featureData);
            }
        }

        private static ValidationIssue CreateMultipleOfModelTimeStepIssue(WaterFlowFMModel waterFlowFmModel, string guiTimeSpanParameter, string outputName)
        {
            var waterFlowFmProperty = waterFlowFmModel.ModelDefinition.GetModelProperty(guiTimeSpanParameter);
            var parameterTimeSpan = (TimeSpan)waterFlowFmProperty.Value;

            if (waterFlowFmModel.TimeStep.Ticks != 0 && parameterTimeSpan.Ticks%waterFlowFmModel.TimeStep.Ticks == 0) // is multiple
                return null;

            var category = waterFlowFmProperty.PropertyDefinition.Category;
            var errorMessage = string.Format("{0} interval must be a multiple of the output timestep.", outputName);
            var validationShortcut = new FmValidationShortcut
            {
                FlowFmModel = (WaterFlowFMModel)waterFlowFmModel,
                TabName = "Output Parameters"
            };
            return new ValidationIssue(category, ValidationSeverity.Error, errorMessage,validationShortcut);
        }
    }
}
