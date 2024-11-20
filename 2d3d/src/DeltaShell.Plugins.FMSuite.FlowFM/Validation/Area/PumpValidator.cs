using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using GeoAPI.Geometries;
using ValidationAspects;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area
{
    public static class PumpValidator
    {
        /// <summary>
        /// Validates the pumps.
        /// </summary>
        /// <param name="pumps"> The set of pumps to be evaluated. </param>
        /// <param name="gridExtent"> The <see cref="Envelope"/> object that describes the extent of the FM model grid. </param>
        /// <param name="modelStartTime"> The model start time. </param>
        /// <param name="modelStopTime"> The model stop time. </param>
        /// <returns> An enumeration of encountered validation issues. </returns>
        public static IEnumerable<ValidationIssue> Validate(this IEnumerable<IPump> pumps, Envelope gridExtent,
                                                            DateTime modelStartTime, DateTime modelStopTime)
        {
            var issues = new List<ValidationIssue>();
            foreach (IPump pump in pumps)
            {
                issues.AddRange(pump.ValidateSnapping(gridExtent));
                issues.AddRange(pump.ValidatePumpObject());

                issues.AddRange(pump.UseCapacityTimeSeries 
                                    ? pump.ValidatePumpCapacityTimeSeries(modelStartTime, modelStopTime) 
                                    : pump.ValidateCapacityValue());
            }

            return issues;
        }

        private static IEnumerable<ValidationIssue> ValidateSnapping(this IPump pump, Envelope gridExtent)
        {
            if (!pump.Geometry.SnapsToFlowFmGrid(gridExtent))
            {
                yield return new ValidationIssue(pump, ValidationSeverity.Warning,
                                                 $"pump '{pump.Name}' not within grid extent",
                                                 pump);
            }
        }

        private static IEnumerable<ValidationIssue> ValidatePumpObject(this IPump pump)
        {
            ValidationResult result = pump.Validate();
            if (!result.IsValid)
            {
                yield return new ValidationIssue(pump,
                                                 ValidationSeverity.Error,
                                                 $"pump '{pump.Name}': {result.ValidationException.Message}",
                                                 pump);
            }
        }

        private static IEnumerable<ValidationIssue> ValidatePumpCapacityTimeSeries(
            this IPump pump, DateTime modelStartTime,
            DateTime modelStopTime)
        {
            if (pump.CapacityTimeSeries.Components[0].Values.Cast<object>().Any(value => (double) value < 0.0))
            {
                yield return new ValidationIssue(pump,
                                                 ValidationSeverity.Error,
                                                 $"pump '{pump.Name}': capacity time series values must be greater than or equal to 0.",
                                                 pump);
            }

            if (pump.CapacityTimeSeries.Time.Values.Any())
            {
                DateTime startTime = pump.CapacityTimeSeries.Time.Values.First();
                DateTime stopTime = pump.CapacityTimeSeries.Time.Values.Last();

                if (startTime > modelStartTime || stopTime < modelStopTime)
                {
                    yield return new ValidationIssue(pump,
                                                     ValidationSeverity.Error,
                                                     $"pump '{pump.Name}': capacity time series does not span the model run interval.",
                                                     pump);
                }
            }
            else
            {
                yield return new ValidationIssue(pump,
                                                 ValidationSeverity.Error,
                                                 $"pump '{pump.Name}': capacity time series does not contain any values.",
                                                 pump);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateCapacityValue(this IPump pump)
        {
            if (pump.Capacity < 0)
            {
                yield return new ValidationIssue(pump,
                                                 ValidationSeverity.Error,
                                                 $"pump '{pump.Name}': Capacity must be greater than or equal to 0.",
                                                 pump);
            }
        }
    }
}