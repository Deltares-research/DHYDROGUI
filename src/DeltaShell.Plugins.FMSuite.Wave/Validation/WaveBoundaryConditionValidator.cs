using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    public static class WaveBoundaryConditionValidator
    {
        public static ValidationReport Validate(WaveModel model)
        {
            var subReports = model.BoundaryConditions
                .Select(bc => new ValidationReport(bc.Name, ValidateBoundaryCondition(bc)))
                .ToList();

            return new ValidationReport("Waves Model Boundary Conditions", subReports);
        }

        private static IEnumerable<ValidationIssue> ValidateBoundaryCondition(WaveBoundaryCondition bc)
        {
            if (!bc.DataPointIndices.Any())
            {
                yield return new ValidationIssue(bc.VariableDescription, ValidationSeverity.Error,
                    Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Boundary_has_no_data_defined, bc);
            }
            if (bc.IsHorizontallyUniform && bc.Feature.Geometry.Coordinates.Length > 2)
            {
                yield return
                    new ValidationIssue(bc.VariableDescription, ValidationSeverity.Info,
                        Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Boundary_condition_contains_internal_geometry_points,
                        bc);
            }
            else if (!bc.IsHorizontallyUniform && Enumerable.Range(1, bc.Feature.Geometry.Coordinates.Length - 2).Except(bc.DataPointIndices).Any())

            {
                yield return
                    new ValidationIssue(bc.VariableDescription, ValidationSeverity.Warning,
                        Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Boundary_condition_contains_unactivated_support_points,
                        bc);
            }

            if (bc.DataType == BoundaryConditionDataType.ParametrizedSpectrumTimeseries && bc.PointData.Count > 1 &&
                bc.SpatialDefinitionType != WaveBoundaryConditionSpatialDefinitionType.Uniform)
            {
                var times = bc.PointData[0].Arguments[0].GetValues<DateTime>();
                foreach (var f in bc.PointData.Skip(1))
                {
                    var compareTimes = f.Arguments[0].GetValues<DateTime>().ToList();
                    if (!times.SequenceEqual(compareTimes))
                    {
                        yield return new ValidationIssue(bc.VariableDescription, ValidationSeverity.Error,
                            string.Format(Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Time_points_are_not_synchronized_on_boundary___0_, bc.Name), bc);
                    }
                }
            }
        }
    }
}