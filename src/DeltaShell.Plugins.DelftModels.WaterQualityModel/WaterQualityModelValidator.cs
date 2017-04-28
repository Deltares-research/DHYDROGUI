using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.Restart;
using DelftTools.Utils;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel
{
    public class WaterQualityModelValidator : IValidator<WaterQualityModel, WaterQualityModel>
    {
        public ValidationReport Validate(WaterQualityModel model, WaterQualityModel target = null)
        {
            return new ValidationReport("Water Quality Model", new []
                {
                    new ValidationReport("Flow data", ValidateHydroData(model)),
                    new ValidationReport("Model settings", ValidateModelSettings(model)),
                    new ValidationReport("Simulation timer", ValidateSimulationTimer(model)),
                    new ValidationReport("Output timers", ValidateOutputTimers(model, model.ModelSettings)),
                    new ValidationReport("Substance process library", ValidateSubstanceProcessLibrary(model.SubstanceProcessLibrary)),
                    new ValidationReport("Loads", ValidateLoads(model)), 
                    new ValidationReport(@"Observation points / areas", ValidateObservationPointsAndAreas(model)), 
                    new ValidationReport("Input restart state", ValidateRestartInput(model)),
                    new ValidationReport("Output restart state", ValidateRestartOutput(model)),
                    new ValidationReport("Segment function file existance", ValidateExistanceSegmentFiles(model))
                });
        }

        private IEnumerable<ValidationIssue> ValidateExistanceSegmentFiles(WaterQualityModel model)
        {
            var processCoefficientsSegmentFunctionType = model.ProcessCoefficients.OfType<SegmentFileFunction>().ToList();
            foreach (var processCoefficientSegmentFunctionType in processCoefficientsSegmentFunctionType)
            {
                var fileName = processCoefficientSegmentFunctionType.UrlPath;
                
                if (File.Exists(fileName)) continue;

                var message = string.Format("Segmentation file for function: {0} not specified", processCoefficientSegmentFunctionType.Name);
                if (fileName != null)
                {
                    message = string.Format("Could not find segmentation file for function: {0}", processCoefficientSegmentFunctionType.Name);
                }

                yield return new ValidationIssue(processCoefficientSegmentFunctionType, ValidationSeverity.Error, message, new WaterQualityFunctionDataWrapper(model.ProcessCoefficients));
            }
           
        }

        /// <summary>
        /// Check whether <paramref name="substanceProcessLibrary"/> is invalid
        /// </summary>
        private static IEnumerable<ValidationIssue> ValidateSubstanceProcessLibrary(SubstanceProcessLibrary substanceProcessLibrary)
        {
            if (substanceProcessLibrary != null && !File.Exists(substanceProcessLibrary.ProcessDefinitionFilesPath + ".def"))
            {
                var message = string.Format("Could not find process definition files: {0}",
                    substanceProcessLibrary.ProcessDefinitionFilesPath);

                yield return new ValidationIssue(substanceProcessLibrary, ValidationSeverity.Error, message);
            }

            if (substanceProcessLibrary == null || substanceProcessLibrary.Substances.Count == 0)
            {
                yield return new ValidationIssue(substanceProcessLibrary, ValidationSeverity.Error, "At least one substance has to be declared");
            }
        }

        /// <summary>
        /// Check whether one of the output timers of <paramref name="model"/> is invalid
        /// </summary>
        private static IEnumerable<ValidationIssue> ValidateOutputTimers(ITimeDependentModel model, WaterQualityModelSettings settings)
        {
            var referenceTimeStep = model.TimeStep;
            var referenceStartTime = model.StartTime;
            var referenceStopTime = model.StopTime;

            var vi1 = CheckTimers(model, "balance output", settings.BalanceStartTime,
                        settings.BalanceStopTime, settings.BalanceTimeStep,
                        "simulation", referenceStartTime, referenceStopTime, referenceTimeStep,
                        false, false);

            var vi2 = CheckTimers(model, "monitoring locations output", settings.HisStartTime,
                        settings.HisStopTime, settings.HisTimeStep,
                        "simulation", referenceStartTime, referenceStopTime, referenceTimeStep,
                        false, false);

            var vi3 = CheckTimers(model, "cells output", settings.MapStartTime,
                        settings.MapStopTime, settings.MapTimeStep,
                        "simulation", referenceStartTime, referenceStopTime, referenceTimeStep,
                        false, false);

            foreach (var vi in vi1.Concat(vi2).Concat(vi3))
            {
                yield return vi;
            }
        }

        /// <summary>
        /// Validates time settings given reference time settings
        /// </summary>
        /// <param name="model">The model that will be included in the ValidationIssue</param>
        /// <param name="description">A description for the time settings that must be checked</param>
        /// <param name="toBeCheckedStartTime">The start time that must be checked</param>
        /// <param name="toBeCheckedStopTime">The stop time that must be checked</param>
        /// <param name="toBeCheckedTimeStep">The time step that must be checked</param>
        /// <param name="referenceDescription">A description for the reference time settings</param>
        /// <param name="referenceStartTime">The reference start time</param>
        /// <param name="referenceStopTime">The reference stop time</param>
        /// <param name="referenceTimeStep">The reference time step</param>
        /// <param name="referenceIsFromLinkedFlowModel">Indicates if the reference comes from a linked flow model</param>
        /// <param name="timeStepTwoWayMultiple">Indicates if the timestep integer multiple is allowed to be two-way</param>
        private static IEnumerable<ValidationIssue> CheckTimers(IModel model, string description, DateTime toBeCheckedStartTime, DateTime toBeCheckedStopTime, TimeSpan toBeCheckedTimeStep, string referenceDescription, DateTime referenceStartTime, DateTime referenceStopTime, TimeSpan referenceTimeStep, bool referenceIsFromLinkedFlowModel, bool timeStepTwoWayMultiple)
        {
            // 'toBeChecked' cannot have start time after stop time
            if (toBeCheckedStartTime > toBeCheckedStopTime)
            {
                yield return new ValidationIssue(model, ValidationSeverity.Error,
                    String.Format(Resources.WaterQualityModelValidate_CheckTimers_Start_time_cannot_be_after_stop_time, description));
            }

            // 'reference' timestep must be > 0, otherwise modulo calculation does not work properly
            if (referenceTimeStep <= new TimeSpan(0))
            {
                yield return new ValidationIssue(model, ValidationSeverity.Error,
                    String.Format(Resources.WaterQualityModelValidate_CheckTimers_Time_step_must_be_greater_than_0, referenceDescription));
                yield break;
            }

            // 'toBeChecked' time range must be in 'reference' time range
            if (toBeCheckedStartTime < referenceStartTime || toBeCheckedStopTime > referenceStopTime)
            {
                yield return new ValidationIssue(model, ValidationSeverity.Warning, String.Format(
                        Resources.WaterQualityModelValidate_CheckTimers_Period_should_be_correctly_bounded,
                        description, FormatTimeString(toBeCheckedStartTime), FormatTimeString(toBeCheckedStopTime),
                        referenceDescription, referenceIsFromLinkedFlowModel ? Resources.WaterQualityModelValidate_CheckTimers_Linked_flow : "",
                        FormatTimeString(referenceStartTime), FormatTimeString(referenceStopTime)));
            }

            // 'toBeChecked' start time should be aligned with the reference samples
            if (!IsAlignedWith(toBeCheckedStartTime, referenceStartTime, referenceTimeStep))
            {
                yield return new ValidationIssue(model, ValidationSeverity.Error, String.Format(
                        Resources.WaterQualityModelValidate_CheckTimers_Start_time_must_be_aligned_with_sample,
                        description, FormatTimeString(referenceStartTime),
                        new DeltaShellTimeSpanConverter().ConvertTo(referenceTimeStep, typeof(string))));
            }

            // 'toBeChecked' time step cannot be greater than 'toBeChecked' time range
            var differenceTimeSpan = toBeCheckedStopTime - toBeCheckedStartTime;
            if (toBeCheckedTimeStep > differenceTimeSpan)
            {
                yield return new ValidationIssue(model, ValidationSeverity.Error,
                    String.Format(
                        Resources.WaterQualityModelValidate_CheckTimers_Time_step_cannot_be_greater_than_difference_start_and_stop_time,
                        description, new DeltaShellTimeSpanConverter().ConvertTo(differenceTimeSpan, typeof(string))));
            }

            // 'toBeChecked' stop time should be aligned with the reference samples
            if (!IsAlignedWith(toBeCheckedStopTime, referenceStartTime, referenceTimeStep))
            {
                yield return new ValidationIssue(model, ValidationSeverity.Error, String.Format(
                        Resources.WaterQualityModelValidate_CheckTimers_Stop_time_must_be_aligned_with_sample,
                        description, FormatTimeString(referenceStartTime),
                        new DeltaShellTimeSpanConverter().ConvertTo(referenceTimeStep, typeof(string))));
            }

            // 'toBeChecked' timestep must be > 0
            if (toBeCheckedTimeStep.Ticks <= 0)
            {
                yield return new ValidationIssue(model, ValidationSeverity.Error, String.Format(Resources.WaterQualityModelValidate_CheckTimers_Time_step_must_be_greater_than_0, description));
                yield break;
            }

            // 'toBeChecked' timestep must be integer multiple (or if 'timeStepTwoWayMultiple' is true a result of integer division)
            if (!IsIntegerMultipleOfEachOther(toBeCheckedTimeStep, referenceTimeStep, timeStepTwoWayMultiple))
            {
                yield return new ValidationIssue(model, ValidationSeverity.Error, String.Format(
                        Resources.WaterQualityModelValidate_CheckTimers_Time_step_must_be_integer_positive_non_zero_multiple,
                        description, timeStepTwoWayMultiple ? Resources.WaterQualityModelValidate_CheckTimers_Time_step_must_be_result_of_integer_division : "",
                        referenceDescription, new DeltaShellTimeSpanConverter().ConvertTo(referenceTimeStep, typeof(string))));
            }
        }

        /// <summary>
        /// This function checks if <paramref name="checkTime"/> can be expressed as:
        /// <paramref name="startTime"/> + N * <paramref name="deltaTime"/>, for integer N
        /// </summary>
        /// <param name="checkTime">The <see cref="DateTime"/> to be checked</param>
        /// <param name="startTime">The start <see cref="DateTime"/></param>
        /// <param name="deltaTime">The time increment from <paramref name="startTime"/></param>
        /// <returns>If <paramref name="checkTime"/> can be expressed as multiple of <paramref name="deltaTime"/> starting from <paramref name="startTime"/></returns>
        private static bool IsAlignedWith(DateTime checkTime, DateTime startTime, TimeSpan deltaTime)
        {
            return ((checkTime - startTime).Ticks % deltaTime.Ticks) == 0;
        }

        private static string FormatTimeString(DateTime dateTime)
        {
            var culture = CultureInfo.InvariantCulture;
            var format = String.Format("yyyy{0}MM{0}dd HH{1}mm{1}ss", culture.DateTimeFormat.DateSeparator, culture.DateTimeFormat.TimeSeparator);

            return dateTime.ToString(format, culture);
        }

        /// <summary>
        /// Determines if input parameters can be expressed as:
        /// 1) <paramref name="spanOne"/> = N * <paramref name="spanTwo"/> OR 
        /// 2) <paramref name="spanOne"/> = <paramref name="spanTwo"/> / N for integer N > 0
        /// </summary>
        /// <param name="spanOne">A non-zero timespan</param>
        /// <param name="spanTwo">A second non-zero timespan</param>
        /// <param name="twoWay">When set to false, only check expression 1)</param>
        /// <returns>
        /// True if the input arguments can be expressed by expression 1) or - if relevant - expression 2), false otherwise
        /// </returns>
        private static bool IsIntegerMultipleOfEachOther(TimeSpan spanOne, TimeSpan spanTwo, bool twoWay)
        {
            var compareResult = spanOne.CompareTo(spanTwo);

            if (compareResult < 0)
            {
                if (!twoWay) return false; // Expression 1) is false and expression 2) is not relevant
                if (spanTwo.Ticks % spanOne.Ticks != 0) return false; // Expression 1) is false and expression 2) is false
            }
            else
            {
                if (spanOne.Ticks % spanTwo.Ticks != 0) return false; // Expression 1) is false and expression 2) is false (whether relevant or not)         
            }

            return true;
        }

        private IEnumerable<ValidationIssue> ValidateHydroData(WaterQualityModel model)
        {
            if (model.HydroData != null) yield break;

            yield return new ValidationIssue(model, ValidationSeverity.Error, "No flow data available. Import a hyd file.", new HydFileImporter{});
        }

        private IEnumerable<ValidationIssue> ValidateObservationPointsAndAreas(WaterQualityModel model)
        {
            if ((model.ModelSettings.MonitoringOutputLevel == MonitoringOutputLevel.None ||
                model.ModelSettings.MonitoringOutputLevel == MonitoringOutputLevel.Areas) && model.ObservationPoints.Count > 0)
            {
                yield return new ValidationIssue(model, ValidationSeverity.Warning, "There are observation points available, but the monitoring output level excludes them from delwaq. Level: " + model.ModelSettings.MonitoringOutputLevel);
            }

            var obsPoints = ValidateWaterQualityNameableMapFeatures(model, model.ObservationPoints, "Observation point", 
                observationPoint => observationPoint.ObservationPointType == ObservationPointType.SinglePoint, model.ObservationAreas.GetValuesAsLabels(), "Observation area");

            foreach (var validationIssue in obsPoints)
            {
                yield return validationIssue;
            }
        }

        private IEnumerable<ValidationIssue> ValidateSimulationTimer(WaterQualityModel model)
        {
            return CheckTimers(model, "simulation", model.StartTime, model.StopTime,
                model.TimeStep, "simulation", model.StartTime, model.StopTime, model.TimeStep, false, true);
        }

        private IEnumerable<ValidationIssue> ValidateLoads(WaterQualityModel model)
        {
            return ValidateWaterQualityNameableMapFeatures(model, model.Loads, "Load", load => true, Enumerable.Empty<string>(), "");
        }

        private IEnumerable<ValidationIssue> ValidateWaterQualityNameableMapFeatures<T>(WaterQualityModel model, IEnumerable<T> nameablePointFeatures, string pointTypeDescription, Func<T, bool> shouldCheckZFunction, IEnumerable<string> existingNames, string alreadyFoundItemDescription) where T : NameablePointFeature
        {
            var foundNames = existingNames.ToList();

            // If not a ZLayer model, then we are working with a sigma-layered model:
            var min = model.LayerType == LayerType.Sigma ? 0.0 : model.ZBot;
            var max = model.LayerType == LayerType.Sigma ? 1.0 : model.ZTop;

            var mapFeatureStartingIndex = foundNames.Count;
            foreach (var pointFeature in nameablePointFeatures)
            {
                var shouldCheckZ = shouldCheckZFunction(pointFeature);
                if (shouldCheckZ && double.IsNaN(pointFeature.Z))
                {
                    yield return new ValidationIssue(pointFeature, ValidationSeverity.Error,
                        string.Format("{0} '{1}' has an undefined Z.", pointTypeDescription, pointFeature.Name));
                }
                else if (shouldCheckZ && !pointFeature.Z.IsInRange(min, max))
                {
                    yield return new ValidationIssue(pointFeature, ValidationSeverity.Error,
                        string.Format("{0} '{1}' has height of {2}, but is required to be in range [{3}, {4}].",
                            pointTypeDescription, pointFeature.Name, pointFeature.Z, min, max));
                }
                else if (model.HasHydroDataImported)
                {
                    var issue = ValidateNameablePointLocation(model, pointFeature, pointTypeDescription, shouldCheckZ);
                    if (issue != null)
                    {
                        yield return issue;
                    }
                }

                var index = foundNames.IndexOf(pointFeature.Name.ToLowerInvariant());
                if (index >= 0)
                {
                    var typeName = index < mapFeatureStartingIndex ? alreadyFoundItemDescription : pointTypeDescription;
                    yield return new ValidationIssue(pointFeature, ValidationSeverity.Error,
                        string.Format("{0} names should be unique and another {1} with name '{2}' was already found.",
                            pointTypeDescription, typeName.ToLower(), pointFeature.Name));
                }
                else
                {
                    foundNames.Add(pointFeature.Name.ToLowerInvariant());
                }
            }
        }

        private ValidationIssue ValidateNameablePointLocation(WaterQualityModel model, NameablePointFeature nameablePointFeature, string pointTypeDescription, bool is3DFeature)
        {
            try
            {
                if (is3DFeature && !model.IsInsideActiveCell(nameablePointFeature.Geometry.Coordinate))
                {
                    return new ValidationIssue(nameablePointFeature, ValidationSeverity.Warning,
                        string.Format("{0} '{1}' is inside an inactive cell.", pointTypeDescription, nameablePointFeature.Name));
                }
                else if (!is3DFeature && !model.IsInsideActiveCell2D(nameablePointFeature.Geometry.Coordinate))
                {
                    return new ValidationIssue(nameablePointFeature, ValidationSeverity.Warning,
                        string.Format("{0} '{1}' is inside an inactive cell.", pointTypeDescription, nameablePointFeature.Name));
                }
            }
            catch (ArgumentException)
            {
                return new ValidationIssue(nameablePointFeature, ValidationSeverity.Error,
                    string.Format("{0} '{1}' is not within grid or has ambiguous location (on a grid edge or grid vertex).",
                        pointTypeDescription, nameablePointFeature.Name));
            }
            return null;
        }

        private IEnumerable<ValidationIssue> ValidateModelSettings(WaterQualityModel model)
        {
            if (model.ModelSettings.NrOfThreads > Environment.ProcessorCount)
            {
                yield return new ValidationIssue(model, ValidationSeverity.Error, 
                    string.Format("This machine cannot use more threads than {0}.", Environment.ProcessorCount));
            }
        }

        private IEnumerable<ValidationIssue> ValidateRestartOutput(WaterQualityModel model)
        {
            if (!model.WriteRestart) yield break;

            if (model.UseSaveStateTimeRange)
            {
                // cannot write multiple restart states in command line mode
                yield return new ValidationIssue(model, ValidationSeverity.Error, "Cannot use save state time range when running in Command Line mode");
            }

            var report = RestartTimeRangeValidator.ValidateRestartTimeRangeSettings(
                model.UseSaveStateTimeRange,
                model.SaveStateStartTime,
                model.SaveStateStopTime,
                model.SaveStateTimeStep,
                model);

            foreach (var issue in report.GetAllIssuesRecursive().Where(i => i.Severity == ValidationSeverity.Error))
            {
                yield return issue;
            }

            foreach (var issue in report.GetAllIssuesRecursive().Where(i => i.Severity == ValidationSeverity.Warning))
                yield return issue;
        }

        private static IEnumerable<ValidationIssue> ValidateRestartInput(WaterQualityModel model)
        {
            if (!model.UseRestart) yield break;
            
            IEnumerable<string> errors, warnings;
            model.ValidateInputState(out errors, out warnings);

            foreach (string error in errors)
            {
                yield return new ValidationIssue(model, ValidationSeverity.Error, error);
            }

            foreach (string warning in warnings)
            {
                yield return new ValidationIssue(model, ValidationSeverity.Warning, warning);
            }
        }
    }
}