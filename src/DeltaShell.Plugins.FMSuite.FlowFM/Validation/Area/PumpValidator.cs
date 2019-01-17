using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation;
using ValidationAspects;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area
{
    public static class PumpValidator
    {
        /// <summary>
        /// Validate the pumps.
        /// </summary>
        /// <param name="model">The model to which the pumps belong.</param>
        /// <param name="pumps">The set of pumps to be evaluated.</param>
        /// <returns> An enumeration of encountered validation issues. </returns>
        public static IEnumerable<ValidationIssue> ValidatePumps(WaterFlowFMModel model, IEnumerable<Pump2D> pumps)
        {
            var issues = new List<ValidationIssue>();
            foreach (var sobekPump in pumps)
            {
                if (!model.SnapsToGrid(sobekPump.Geometry))
                {
                    issues.Add(new ValidationIssue(sobekPump,
                                                   ValidationSeverity.Warning,
                                                   $"pump '{sobekPump.Name}' not within grid extent",
                                                   pumps));
                }

                var result = sobekPump.Validate();
                if (!result.IsValid)
                {
                    issues.Add(new ValidationIssue(sobekPump,
                                                   ValidationSeverity.Error,
                                                   $"pump '{sobekPump.Name}': {result.ValidationException.Message}",
                                                   sobekPump));
                }

                // Capacity must be >= 0
                if (sobekPump.CanBeTimedependent && sobekPump.UseCapacityTimeSeries)
                {
                    if (sobekPump.CapacityTimeSeries.Components[0].Values.Cast<object>()
                                 .Any(value => (double)value < 0.0))
                    {
                        issues.Add(new ValidationIssue(sobekPump,
                                                       ValidationSeverity.Error,
                                                       $"pump '{sobekPump.Name}': capacity time series values must be greater than or equal to 0.",
                                                       sobekPump));
                    }

                    if (sobekPump.CapacityTimeSeries.Time.Values.Any())
                    {
                        var startTime = sobekPump.CapacityTimeSeries.Time.Values.First();
                        var stopTime = sobekPump.CapacityTimeSeries.Time.Values.Last();

                        if (startTime > model.StartTime || stopTime < model.StopTime)
                        {
                            issues.Add(new ValidationIssue(sobekPump,
                                                           ValidationSeverity.Error,
                                                           $"pump '{sobekPump.Name}': capacity time series does not span the model run interval.",
                                                           sobekPump));
                        }
                    }
                    else
                    {
                        issues.Add(new ValidationIssue(sobekPump,
                                                       ValidationSeverity.Error,
                                                       $"pump '{sobekPump.Name}': capacity time series does not contain any values.",
                                                       sobekPump));
                    }
                }
                else
                {
                    if (sobekPump.Capacity < 0)
                    {
                        issues.Add(new ValidationIssue(sobekPump,
                                                       ValidationSeverity.Error,
                                                       $"pump '{sobekPump.Name}': Capacity must be greater than or equal to 0.",
                                                       sobekPump));
                    }
                }

                switch (sobekPump.ControlDirection)
                {
                    case PumpControlDirection.DeliverySideControl:
                        ValidatePumpDeliverySide(sobekPump, issues);
                        break;
                    case PumpControlDirection.SuctionAndDeliverySideControl:
                        ValidatePumpDeliverySide(sobekPump, issues);
                        ValidatePumpSuctionSide(sobekPump, issues);
                        break;
                    case PumpControlDirection.SuctionSideControl:
                        ValidatePumpSuctionSide(sobekPump, issues);
                        break;
                }
            }

            return issues;
        }

        private static void ValidatePumpSuctionSide(IPump sobekPump, ICollection<ValidationIssue> issues)
        {
            if (sobekPump.StartSuction < sobekPump.StopSuction)
            {
                issues.Add(new ValidationIssue(sobekPump,
                                               ValidationSeverity.Error,
                                               $"pump '{sobekPump.Name}': Suction start level must be greater than or equal to suction stop level.",
                                               sobekPump));
            }
        }

        private static void ValidatePumpDeliverySide(IPump sobekPump, ICollection<ValidationIssue> issues)
        {
            if (sobekPump.StartDelivery > sobekPump.StopDelivery)
            {
                issues.Add(new ValidationIssue(sobekPump,
                                               ValidationSeverity.Error,
                                               $"pump '{sobekPump.Name}': Delivery start level must be less than or equal to delivery stop level.",
                                               sobekPump));
            }
        }
    }
}
