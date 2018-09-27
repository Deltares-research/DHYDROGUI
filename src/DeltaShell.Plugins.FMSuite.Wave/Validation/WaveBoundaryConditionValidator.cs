using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;

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
                    "Boundary has no data defined", bc);
            }
            if (bc.IsHorizontallyUniform && bc.Feature.Geometry.Coordinates.Count() > 2)
            {
                yield return
                    new ValidationIssue(bc.VariableDescription, ValidationSeverity.Warning,
                        "Boundary condition contains internal geometry points. These points will be discarded upon saving, exporting or running",
                        bc);
            }
            else if (!bc.IsHorizontallyUniform && Enumerable.Range(1, bc.Feature.Geometry.Coordinates.Count() - 2).Except(bc.DataPointIndices).Any())

            {
                yield return
                    new ValidationIssue(bc.VariableDescription, ValidationSeverity.Warning,
                        "Boundary condition contains unactivated support points. These points will be discarded upon saving, exporting or running",
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
                            string.Format("Time points are not synchronized on boundary: {0}", bc.Name), bc);
                    }
                }
            }
        }
    }
}