using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    public static class WaveTimePointValidator
    {
        public static List<ValidationIssue> Issues { get; private set; }
        public static IList<DateTime> TimePoints { get; private set; }
        public static WaveModel Model { get; private set; }

        /// <summary>
        /// Validation for wave model time point editing.
        /// <param name="model">A wave model entity</param>
        /// <returns>A validation report regarding wave model time points</returns>
        /// </summary>
        public static ValidationReport Validate(WaveModel model)
        {
            Model = model;
            Issues = new List<ValidationIssue>();
            TimePoints = model.TimePointData.TimePoints;

            BoundaryConditionTimePointsPrecedesModelStartTime();
            NoTimePointsDefined();
            ModelStartTimePrecedesReferenceTime();

            return new ValidationReport("Waves Model Time Points", Issues);
        }

        private static void BoundaryConditionTimePointsPrecedesModelStartTime()
        {
            if (Model.BoundaryConditions.Any(b=>b.DataType == BoundaryConditionDataType.ParameterizedSpectrumTimeseries))
            {
                var boundaryConditionPointData = Model.ModelDefinition.BoundaryConditions.SelectMany(bc => bc.PointData).ToList();
                var boundaryConditionTimePoints = boundaryConditionPointData.SelectMany(b => b.Arguments[0].GetValues<DateTime>()).ToList();
                var modelStartTimeIsPreceded = boundaryConditionTimePoints.All(btp => btp.Date < TimePoints.FirstOrDefault());

                if (modelStartTimeIsPreceded)
                {
                    Issues.Add(new ValidationIssue(null, ValidationSeverity.Error,
                        Resources.WaveTimePointValidator_BoundaryConditionTimePointsPrecedesModelStartTime_Model_start_time_does_not_precedes_any_of_Boundary_Condition_time_points_,
                        Model.TimePointData));
                }
            }

        }

        private static void ModelStartTimePrecedesReferenceTime()
        {
            if (TimePoints.Count > 0)
            {
                var hasInvalidTimePoint = TimePoints.Any(tp => tp < Model.ModelDefinition.ModelReferenceDateTime);

                if (hasInvalidTimePoint)
                {
                    Issues.Add(new ValidationIssue(null, ValidationSeverity.Error,
                               Resources.WaveTimePointValidator_Validate_Model_Start_time_precedes_Reference_Time,
                       Model.TimePointData));
                }
            }
        }

        private static void NoTimePointsDefined()
        {
            if (!Model.IsCoupledToFlow && TimePoints.Count == 0)
            {
                Issues.Add(new ValidationIssue(Model, ValidationSeverity.Error,
                    Resources.WaveTimePointValidator_Validate_No_time_points_defined,
                    Model.TimePointData));
            }
        }
    }
}