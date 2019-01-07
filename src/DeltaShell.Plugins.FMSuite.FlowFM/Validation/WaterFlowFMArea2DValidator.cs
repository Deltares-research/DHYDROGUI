using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
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
                        string.Format(Resources.WaterFlowFMArea2DValidator_Validate_thin_dam___0___not_within_grid_extent, thinDam.Name), area.ThinDams));
                }
            }

            foreach (var sourceAndSink in model.SourcesAndSinks)
            {
                if (!model.SnapsToGrid(sourceAndSink.Feature.Geometry))
                {
                    issues.Add(new ValidationIssue(sourceAndSink, ValidationSeverity.Warning,
                                                   "source/sink '" + sourceAndSink.Name + "' not within grid extent", model.Pipes));
                }
                var timeArgument = sourceAndSink.Function.Arguments.OfType<IVariable<DateTime>>().First();
                if (timeArgument.Values.Any())
                {
                    var startTime = timeArgument.Values.First();
                    var stopTime = timeArgument.Values.Last();

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
                                                   "fixed weir '" + fixedWeir.Name + "' not within grid extent", area.FixedWeirs));
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
                                "' has unphysical sill depths, parts will be ignored by dflow-fm", area.FixedWeirs));
                        }
                }
            }

            foreach (var weir in area.Weirs)
            {
                var weirName = $"{weir.Name}";
                var weirType = $"{weir.WeirFormula.Name}";
                if (!model.SnapsToGrid(weir.Geometry))
                {
                    var msg = String.Format("{0} is not within grid extend.", weirName);
                    issues.Add(new ValidationIssue(weir, ValidationSeverity.Warning, msg, area.Weirs)); 
                }
                var result = weir.Validate();
                if (!result.IsValid)
                {
                    var msg = String.Format("{0}: {1}", weirName, result.ValidationException.Messages);
                    issues.Add(new ValidationIssue(weir, ValidationSeverity.Error, msg, weir));
                }

                if (weir.UseCrestLevelTimeSeries)
                {
                    if (weir.CrestLevelTimeSeries.Time.Values.Any())
                    {
                        var startTime = weir.CrestLevelTimeSeries.Time.Values.First();
                        var stopTime = weir.CrestLevelTimeSeries.Time.Values.Last();

                        if (startTime > model.StartTime || stopTime < model.StopTime)
                        {
                            var msg = $"'{weirName}': crest level time series does not span the model run interval.";
                            issues.Add(new ValidationIssue(weir, ValidationSeverity.Error, msg, weir));
                        }
                    }
                    else
                    {
                        var msg = $"'{weirName}': crest level time series does not contain any values.";
                        issues.Add(new ValidationIssue(weir, ValidationSeverity.Error, msg, weir));
                    }
                }
                if (weir.WeirFormula is SimpleWeirFormula)
                {
                    if (((SimpleWeirFormula) weir.WeirFormula).LateralContraction < 0.0)
                    {
                        var msg = $"'{weirName}': lateral contraction coefficient must be greater than or equal to zero.";
                        issues.Add(new ValidationIssue(weir, ValidationSeverity.Error, msg, weir));
                    }
                }

                var gatedWeirFormula = weir.WeirFormula as IGatedWeirFormula;
                if (gatedWeirFormula != null)
                {
                    // DoorHeight
                    if (gatedWeirFormula.DoorHeight < 0.0)
                    {
                        var msg = $"'{weirName}': door height must be greater than or equal to 0.";
                        issues.Add(new ValidationIssue(weir, ValidationSeverity.Error, msg, weir));
                    }

                    // HorizontalDoorOpeningWidth
                    if (gatedWeirFormula.UseHorizontalDoorOpeningWidthTimeSeries)
                    {
                        var doorOpeningTimeSeries = gatedWeirFormula.HorizontalDoorOpeningWidthTimeSeries;
                        if (doorOpeningTimeSeries.Components[0].Values.Cast<object>()
                                .Any(value => (double)value < 0.0))
                        {
                            var msg =
                                $"'{weirName}': opening width time series values must be greater than or equal to 0.";
                            issues.Add(new ValidationIssue(weir, ValidationSeverity.Error, msg, weir));
                        }
                        if (doorOpeningTimeSeries.Time.Values.Any())
                        {
                            var startTime = doorOpeningTimeSeries.Time.Values.First();
                            var stopTime = doorOpeningTimeSeries.Time.Values.Last();

                            if (startTime > model.StartTime || stopTime < model.StopTime)
                            {
                                var msg =
                                    $"'{weirName}': opening width time series does not span the model run interval.";
                                issues.Add(new ValidationIssue(weir, ValidationSeverity.Error, msg, weir));
                            }
                        }
                        else
                        {
                            var msg = $"'{weirName}': opening width time series does not contain any values.";
                            issues.Add(new ValidationIssue(weir, ValidationSeverity.Error, msg, weir));
                        }
                    }
                    else if (gatedWeirFormula.HorizontalDoorOpeningWidth < 0.0)
                    {
                        var msg = $"'{weirName}': opening width must be greater than or equal to 0.";
                        issues.Add(new ValidationIssue(weir, ValidationSeverity.Error, msg, weir));
                    }

                    // LowerEdgeLevel
                    if (gatedWeirFormula.UseLowerEdgeLevelTimeSeries)
                    {
                        var lowerEdgeLevelTimeSeries = gatedWeirFormula.LowerEdgeLevelTimeSeries;
                        if (lowerEdgeLevelTimeSeries.Time.Values.Any())
                        {
                            var startTime = lowerEdgeLevelTimeSeries.Time.Values.First();
                            var stopTime = lowerEdgeLevelTimeSeries.Time.Values.Last();

                            if (startTime > model.StartTime || stopTime < model.StopTime)
                            {
                                var msg =
                                    $"'{weirName}': lower edge level time series does not span the model run interval.";
                                issues.Add(new ValidationIssue(weir, ValidationSeverity.Error, msg, weir));
                            }
                        }
                        else
                        {
                            var msg = $"'{weirName}': lower edge level time series does not contain any values.";
                            issues.Add(new ValidationIssue(weir, ValidationSeverity.Error, msg, weir));
                        }
                    }
                }

                if (weir.CrestWidth <= 0.0)
                {
                    var msg = $"Crest Width for '{weirName}' structure type: {weirType}, must be greater than 0.";
                    issues.Add(new ValidationIssue(weir, ValidationSeverity.Error, msg, weir));
                }

                var generalStructureFormula = weir.WeirFormula as GeneralStructureWeirFormula;
                if (generalStructureFormula != null)
                {
                    if (generalStructureFormula.HorizontalDoorOpeningDirection != GateOpeningDirection.Symmetric)
                    {
                        var msg = $"'{weirName}': only symmetric horizontal door opening direction is supported for general structures.";
                        issues.Add(new ValidationIssue(weir, ValidationSeverity.Error, msg, weir));
                    }

                    if (generalStructureFormula.WidthStructureLeftSide <= 0.0)
                    {
                        var msg = $"Upstream 2 Crest Width for '{weirName}', structure type {weirType} must be greater than 0.";
                        issues.Add(new ValidationIssue(weir, ValidationSeverity.Error, msg, weir));
                    }

                    if (generalStructureFormula.WidthLeftSideOfStructure <= 0.0)
                    {
                        var msg = $"Upstream 1 Crest Width for '{weirName}', structure type {weirType} must be greater than 0.";
                        issues.Add(new ValidationIssue(weir, ValidationSeverity.Error, msg, weir));
                    }

                    if (generalStructureFormula.WidthStructureRightSide <= 0.0)
                    {
                        var msg = $"Downstream 1 Crest Width for '{weirName}', structure type {weirType} must be greater than 0.";
                        issues.Add(new ValidationIssue(weir, ValidationSeverity.Error, msg, weir));
                    }

                    if (generalStructureFormula.WidthRightSideOfStructure <= 0.0)
                    {
                        var msg = $"Downstream 2 Crest Width for '{weirName}', structure type {weirType} must be greater than 0.";
                        issues.Add(new ValidationIssue(weir, ValidationSeverity.Error, msg, weir));
                    }
                }
            }

            foreach (var sobekPump in area.Pumps)
            {
                if (!model.SnapsToGrid(sobekPump.Geometry))
                {
                    issues.Add(new ValidationIssue(sobekPump, ValidationSeverity.Warning,
                                                   "pump '" + sobekPump.Name + "' not within grid extent", area.Pumps));
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
                            "pump '" + sobekPump.Name + "': Capacity must be greater than or equal to 0."));
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
