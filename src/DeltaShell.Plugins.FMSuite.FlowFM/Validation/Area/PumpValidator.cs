using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
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
        public static IEnumerable<ValidationIssue> Validate(this IEnumerable<Pump2D> pumps, Envelope gridExtent,
                                                            DateTime modelStartTime, DateTime modelStopTime)
        {
            var issues = new List<ValidationIssue>();
            foreach (Pump2D pump in pumps)
            {
                issues.AddRange(pump.ValidateSnapping(gridExtent));
                issues.AddRange(pump.ValidatePumpObject());

                if (pump.CanBeTimedependent && pump.UseCapacityTimeSeries)
                {
                    issues.AddRange(pump.ValidatePumpCapacityTimeSeries(modelStartTime, modelStopTime));
                }
                else
                {
                    issues.AddRange(pump.ValidateCapacityValue());
                }

                issues.AddRange(pump.ValidateControlSettings());
            }

            return issues;
        }

        private static IEnumerable<ValidationIssue> ValidateSnapping(this Pump2D pump, Envelope gridExtent)
        {
            if (!pump.Geometry.SnapsToFlowFmGrid(gridExtent))
            {
                yield return new ValidationIssue(pump, ValidationSeverity.Warning,
                                                 $"pump '{pump.Name}' not within grid extent",
                                                 pump);
            }
        }

        private static IEnumerable<ValidationIssue> ValidatePumpObject(this Pump2D pump)
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
            this Pump2D pump, DateTime modelStartTime,
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

        private static IEnumerable<ValidationIssue> ValidateCapacityValue(this Pump2D pump)
        {
            if (pump.Capacity < 0)
            {
                yield return new ValidationIssue(pump,
                                                 ValidationSeverity.Error,
                                                 $"pump '{pump.Name}': Capacity must be greater than or equal to 0.",
                                                 pump);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateControlSettings(this Pump2D pump)
        {
            switch (pump.ControlDirection)
            {
                case PumpControlDirection.DeliverySideControl:
                    return pump.ValidatePumpDeliverySide();
                case PumpControlDirection.SuctionAndDeliverySideControl:
                    return pump.ValidatePumpDeliverySide()
                               .Concat(pump.ValidatePumpSuctionSide());
                case PumpControlDirection.SuctionSideControl:
                    return pump.ValidatePumpSuctionSide();
                default:
                    throw new NotImplementedException();
            }
        }

        private static IEnumerable<ValidationIssue> ValidatePumpDeliverySide(this IPump pump)
        {
            if (pump.StartDelivery > pump.StopDelivery)
            {
                yield return new ValidationIssue(pump, ValidationSeverity.Error,
                                                 $"pump '{pump.Name}': Delivery start level must be less than or equal to delivery stop level.",
                                                 pump);
            }
        }

        private static IEnumerable<ValidationIssue> ValidatePumpSuctionSide(this IPump pump)
        {
            if (pump.StartSuction < pump.StopSuction)
            {
                yield return new ValidationIssue(pump, ValidationSeverity.Error,
                                                 $"pump '{pump.Name}': Suction start level must be greater than or equal to suction stop level.",
                                                 pump);
            }
        }
    }
}