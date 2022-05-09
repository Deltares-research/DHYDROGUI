using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Hydro.Validators;
using DelftTools.Utils.Validation;
using ValidationAspects;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class WaterFlowFMArea2DValidator
    {
        public static ValidationReport Validate(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();
            var area = model.Area;
            
            foreach (var thinDam in area.ThinDams)
            {
                if (!model.SnapsToGrid(thinDam.Geometry))
                {
                    issues.Add(new ValidationIssue(thinDam, ValidationSeverity.Warning,
                                                   "thin dam '" + thinDam.Name + "' not within grid extent", new ValidatedFeatures(area, thinDam)));
                }
            }

            foreach (var sourceAndSink in model.SourcesAndSinks)
            {
                if (!model.SnapsToGrid(sourceAndSink.Feature.Geometry))
                {
                    issues.Add(new ValidationIssue(sourceAndSink, ValidationSeverity.Warning,
                                                   "source/sink '" + sourceAndSink.Name + "' not within grid extent", new ValidatedFeatures(area, sourceAndSink.Feature)));
                }
                var timeArgument = sourceAndSink.Function.Arguments.OfType<IVariable<DateTime>>().First();
                if (timeArgument.Values.Any())
                {
                    var startTime = timeArgument.Values.First();
                    var stopTime =timeArgument.Values.Last();

                    if (startTime > model.StartTime || stopTime < model.StopTime)
                    {
                        issues.Add(new ValidationIssue(sourceAndSink, ValidationSeverity.Error,
                            "source/sink '" + sourceAndSink.Name +
                            "': discharge time series does not span the model run interval.", sourceAndSink));
                    }
                }
                else
                {
                    issues.Add(new ValidationIssue(sourceAndSink, ValidationSeverity.Error,
                        "source/sink '" + sourceAndSink.Name +
                        "': discharge time series does not contain any values.", sourceAndSink));
                }
            }

            foreach (var fixedWeir in area.FixedWeirs)
            {
                if (!model.SnapsToGrid(fixedWeir.Geometry))
                {
                    issues.Add(new ValidationIssue(fixedWeir, ValidationSeverity.Warning,
                                                   "fixed weir '" + fixedWeir.Name + "' not within grid extent", new ValidatedFeatures(area, fixedWeir)));
                }

                var dataToCheck =
                    model.FixedWeirsProperties.FirstOrDefault(d => d.Feature == fixedWeir);
                var counter = dataToCheck.DataColumns[1].ValueList.Count;
                for (int i = 0; i < counter; i++)
                {
                    if (((double)dataToCheck.DataColumns[1].ValueList[i] <= 0.0) ||
                        ((double)dataToCheck.DataColumns[2].ValueList[i] <= 0.0))
                        {
                            issues.Add(new ValidationIssue(fixedWeir, ValidationSeverity.Warning,
                                "fixed weir '" + fixedWeir.Name +
                                "' has unphysical sill depths, parts will be ignored by dflow-fm", fixedWeir));
                        }
                }
            }

            foreach (var weir in area.Weirs)
            {
                if (!model.SnapsToGrid(weir.Geometry))
                {
                    issues.Add(new ValidationIssue(weir, ValidationSeverity.Warning,
                                                   "weir '" + weir.Name + "' not within grid extent", new ValidatedFeatures(area, weir)));
                }
                var result = weir.Validate();
                if (!result.IsValid)
                {
                    issues.Add(new ValidationIssue(weir, ValidationSeverity.Error,
                        "weir '" + weir.Name + "': " + result.ValidationException.Message, weir));
                }

                if (weir.UseCrestLevelTimeSeries)
                {
                    if (weir.CrestLevelTimeSeries.Time.Values.Any())
                    {
                        var startTime = weir.CrestLevelTimeSeries.Time.Values.First();
                        var stopTime = weir.CrestLevelTimeSeries.Time.Values.Last();

                        if (startTime > model.StartTime || stopTime < model.StopTime)
                        {
                            issues.Add(new ValidationIssue(weir, ValidationSeverity.Error,
                                "weir '" + weir.Name +
                                "': crest level time series does not span the model run interval.", weir));
                        }
                    }
                    else
                    {
                        issues.Add(new ValidationIssue(weir, ValidationSeverity.Error,
                            "weir '" + weir.Name +
                            "': crest level time series does not contain any values.", weir));
                    }
                }
                if (weir.WeirFormula is SimpleWeirFormula formula && formula.CorrectionCoefficient < 0.0)
                {
                    issues.Add(new ValidationIssue(weir, ValidationSeverity.Error,
                        "weir '" + weir.Name +
                        "': correction coefficient must be greater than or equal to zero.", weir));
                }
            }

            foreach (var sobekPump in area.Pumps)
            {
                if (!model.SnapsToGrid(sobekPump.Geometry))
                {
                    issues.Add(new ValidationIssue(sobekPump, ValidationSeverity.Warning,
                                                   "pump '" + sobekPump.Name + "' not within grid extent", new ValidatedFeatures(area, sobekPump)));
                }
                var result = sobekPump.Validate();
                if (!result.IsValid)
                {
                    issues.Add(new ValidationIssue(sobekPump, ValidationSeverity.Error,
                        "pump '" + sobekPump.Name + "': " + result.ValidationException.Message, sobekPump));
                }

                // Capacity must be >= 0
                if (sobekPump.CanBeTimedependent && sobekPump.UseCapacityTimeSeries)
                {
                    if(sobekPump.CapacityTimeSeries.Components[0].Values.Cast<object>().Any(value => (double) value < 0.0))
                    {
                        issues.Add(new ValidationIssue(sobekPump, ValidationSeverity.Error,
                            "pump '" + sobekPump.Name +
                            "': capacity time series values must be greater than or equal to 0.", sobekPump));
                    }
                    if (sobekPump.CapacityTimeSeries.Time.Values.Any())
                    {
                        var startTime = sobekPump.CapacityTimeSeries.Time.Values.First();
                        var stopTime = sobekPump.CapacityTimeSeries.Time.Values.Last();

                        if (startTime > model.StartTime || stopTime < model.StopTime)
                        {
                            issues.Add(new ValidationIssue(sobekPump, ValidationSeverity.Error,
                                "pump '" + sobekPump.Name +
                                "': capacity time series does not span the model run interval.", sobekPump));
                        }
                    }
                    else
                    {
                        issues.Add(new ValidationIssue(sobekPump, ValidationSeverity.Error,
                            "pump '" + sobekPump.Name +
                            "': capacity time series does not contain any values.", sobekPump));
                    }
                }
                else
                {
                    if (sobekPump.Capacity < 0)
                    {
                        issues.Add(new ValidationIssue(sobekPump, ValidationSeverity.Error,
                            "pump '" + sobekPump.Name + "': Capacity must be greater than or equal to 0.", sobekPump));
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

            foreach (var gate in area.Gates)
            {
                if (!model.SnapsToGrid(gate.Geometry))
                {
                    issues.Add(new ValidationIssue(gate, ValidationSeverity.Warning,
                                                   "gate '" + gate.Name + "' not within grid extent", new ValidatedFeatures(area, gate)));
                }
                if (gate.DoorHeight < 0.0)
                {
                    issues.Add(new ValidationIssue(gate, ValidationSeverity.Error,
                            "gate '" + gate.Name +
                            "': door height must be greater than or equal to 0.", gate));
                }
                if (gate.UseSillLevelTimeSeries)
                {
                    if (gate.SillLevelTimeSeries.Time.Values.Any())
                    {
                        var startTime = gate.SillLevelTimeSeries.Time.Values.First();
                        var stopTime = gate.SillLevelTimeSeries.Time.Values.Last();

                        if (startTime > model.StartTime || stopTime < model.StopTime)
                        {
                            issues.Add(new ValidationIssue(gate, ValidationSeverity.Error,
                                "gate '" + gate.Name +
                                "': sill level time series does not span the model run interval.", gate));
                        }
                    }
                    else
                    {
                        issues.Add(new ValidationIssue(gate, ValidationSeverity.Error,
                            "gate '" + gate.Name +
                            "': sill level time series does not contain any values.", gate));
                    }
                }
                if (gate.UseOpeningWidthTimeSeries)
                {
                    if (gate.OpeningWidthTimeSeries.Components[0].Values.Cast<object>()
                            .Any(value => (double) value < 0.0))
                    {
                        issues.Add(new ValidationIssue(gate, ValidationSeverity.Error,
                            "gate '" + gate.Name +
                            "': Opening width time series values must be greater than or equal to 0.", gate));
                    }
                    if (gate.OpeningWidthTimeSeries.Time.Values.Any())
                    {
                        var startTime = gate.OpeningWidthTimeSeries.Time.Values.First();
                        var stopTime = gate.OpeningWidthTimeSeries.Time.Values.Last();

                        if (startTime > model.StartTime || stopTime < model.StopTime)
                        {
                            issues.Add(new ValidationIssue(gate, ValidationSeverity.Error,
                                "gate '" + gate.Name +
                                "': opening width time series does not span the model run interval.", gate));
                        }
                    }
                    else
                    {
                        issues.Add(new ValidationIssue(gate, ValidationSeverity.Error,
                            "gate '" + gate.Name +
                            "': opening width time series does not contain any values.", gate));
                    }
                }
                else if (gate.OpeningWidth < 0)
                {
                    issues.Add(new ValidationIssue(gate, ValidationSeverity.Error,
                        "gate '" + gate.Name + "': Opening width must be greater than or equal to 0.", gate));
                }
                if (gate.UseLowerEdgeLevelTimeSeries)
                {
                    if (gate.LowerEdgeLevelTimeSeries.Time.Values.Any())
                    {
                        var startTime = gate.LowerEdgeLevelTimeSeries.Time.Values.First();
                        var stopTime = gate.LowerEdgeLevelTimeSeries.Time.Values.Last();

                        if (startTime > model.StartTime || stopTime < model.StopTime)
                        {
                            issues.Add(new ValidationIssue(gate, ValidationSeverity.Error,
                                "gate '" + gate.Name +
                                "': lower edge level time series does not span the model run interval.", gate));
                        }
                    }
                    else
                    {
                        issues.Add(new ValidationIssue(gate, ValidationSeverity.Error,
                            "gate '" + gate.Name +
                            "': lower edge level time series does not contain any values.", gate));
                    }
                }
            }

            return new ValidationReport("Structures", issues);
        }

        private static void ValidatePumpSuctionSide(IPump sobekPump, ICollection<ValidationIssue> issues)
        {
            if (sobekPump.StartSuction < sobekPump.StopSuction)
            {
                issues.Add(new ValidationIssue(sobekPump, ValidationSeverity.Error,
                    "pump '" + sobekPump.Name +
                    "': Suction start level must be greater than or equal to suction stop level.", sobekPump));
            }
        }

        private static void ValidatePumpDeliverySide(IPump sobekPump, ICollection<ValidationIssue> issues)
        {
            if (sobekPump.StartDelivery > sobekPump.StopDelivery)
            {
                issues.Add(new ValidationIssue(sobekPump, ValidationSeverity.Error,
                    "pump '" + sobekPump.Name +
                    "': Delivery start level must be less than or equal to delivery stop level.", sobekPump));
            }
        }
    }
}
