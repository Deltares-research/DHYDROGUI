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
        /// <summary>
        /// Validate all entities that can occur in an Area2D of a WaterFlow Model. The anomalies are returned as messages in the ValidationReport.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>ValidationReport that contains the validationmessages which can be Info, Warning or Error</returns>
        public static ValidationReport Validate(WaterFlowFMModel model)
        {
            var area = model.Area;

            var issues = ValidateThinDams(model, area.ThinDams)
                         .Concat(ValidateSourceAndSinks(model, model.SourcesAndSinks))
                         .Concat(ValidateFixedWeirs(model, area.FixedWeirs))
                         .Concat(ValidateWeirs(model, area.Weirs))
                         .Concat(ValidatePumps(model, area.Pumps));

            return new ValidationReport("Structures", issues);
        }

        /// <summary>
        /// Validate the thin dams and return any issues encountered.
        /// </summary>
        /// <param name="model">The model to which the thinDams belong.</param>
        /// <param name="thinDams">The set of thin dams to be evaluated.</param>
        /// <returns> A set of validation issues encountered. </returns>
        public static IEnumerable<ValidationIssue> ValidateThinDams(WaterFlowFMModel model, IEnumerable<ThinDam2D> thinDams)
        {
            return thinDams.Where(td => !model.SnapsToGrid(td.Geometry))
                           .Select(td => new ValidationIssue(td,
                                                             ValidationSeverity.Warning,
                                                             string.Format(Resources.WaterFlowFMArea2DValidator_Validate_thin_dam___0___not_within_grid_extent, td.Name),
                                                             thinDams));
        }

        /// <summary>
        /// Validate the source and sinks and return any issues encountered.
        /// </summary>
        /// <param name="model">The model to which the source and sinks belong.</param>
        /// <param name="sourcesAndSinks">The the set of sources and sinks to be evaluated.</param>
        /// <returns> A set of validation issues encountered. </returns>
        public static IEnumerable<ValidationIssue> ValidateSourceAndSinks(WaterFlowFMModel model,
                                                                          IEnumerable<FeatureData.SourceAndSink> sourcesAndSinks)
        {
            var issues = new List<ValidationIssue>();
            foreach (var sourceAndSink in sourcesAndSinks)
            {
                if (!model.SnapsToGrid(sourceAndSink.Feature.Geometry))
                {
                    issues.Add(new ValidationIssue(sourceAndSink
                                                 , ValidationSeverity.Warning
                                                 , $"source/sink '{sourceAndSink.Name}' not within grid extent"
                                                 , model.Pipes));
                }

                var timeArgument = sourceAndSink
                                   .Function.Arguments.OfType<IVariable<DateTime>>()
                                   .First();
                if (timeArgument.Values.Any())
                {
                    var startTime = timeArgument.Values.First();
                    var stopTime = timeArgument.Values.Last();

                    if (startTime > model.StartTime || stopTime < model.StopTime)
                    {
                        issues.Add(new ValidationIssue(sourceAndSink
                                                     , ValidationSeverity.Error
                                                     , $"source/sink '{sourceAndSink.Name}': discharge time series does not span the model run interval."
                                                     , sourceAndSink));
                    }
                }
                else
                {
                    issues.Add(new ValidationIssue(sourceAndSink
                                                 , ValidationSeverity.Error
                                                 , $"source/sink '{sourceAndSink.Name}': discharge time series does not contain any values."
                                                 , sourceAndSink));
                }
            }

            return issues;
        }

        /// <summary>
        /// Validate the fixed weirs and return any encountered issues.
        /// </summary>
        /// <param name="model">The model to which the fixed weirs belong.</param>
        /// <param name="fixedWeirs">The set of fixed weirs to be evaluated.</param>
        /// <returns> A set of validation issues encountered. </returns>
        public static IEnumerable<ValidationIssue> ValidateFixedWeirs(WaterFlowFMModel model, IEnumerable<FixedWeir> fixedWeirs)
        {
            var issues = new List<ValidationIssue>();
            foreach (var fixedWeir in fixedWeirs)
            {
                if (!model.SnapsToGrid(fixedWeir.Geometry))
                {
                    issues.Add(new ValidationIssue(fixedWeir
                                                 , ValidationSeverity.Warning
                                                 , $"fixed weir '{fixedWeir.Name}' not within grid extent"
                                                 , fixedWeirs));
                }

                var dataToCheck =
                    model.FixedWeirsProperties.FirstOrDefault(d => d.Feature == fixedWeir);
                var counter = dataToCheck.DataColumns[1].ValueList.Count;
                for (var i = 0; i < counter; i++)
                {
                    if (((double)dataToCheck.DataColumns[1].ValueList[i] <= 0.0) ||
                        ((double)dataToCheck.DataColumns[2].ValueList[i] <= 0.0))
                    {
                        issues.Add(new ValidationIssue(fixedWeir
                                                     , ValidationSeverity.Warning
                                                     , $"fixed weir '{fixedWeir.Name}' has unphysical sill depths, parts will be ignored by dflow-fm"
                                                     , fixedWeirs));
                    }
                }
            }

            return issues;
        }

        /// <summary>
        /// Validate the weirs and return any encountered issues.
        /// </summary>
        /// <param name="model">The model to which the pumps belong.</param>
        /// <param name="weirs">The set of weirs to be evaluated.</param>
        /// <returns> A set of validation issues encountered. </returns>
        public static IEnumerable<ValidationIssue> ValidateWeirs(WaterFlowFMModel model, IEnumerable<Weir2D> weirs)
        {
            var issues = new List<ValidationIssue>();

            foreach (var weir in weirs)
            {
                if (!model.SnapsToGrid(weir.Geometry))
                {
                    issues.Add(new ValidationIssue(weir
                                                 , ValidationSeverity.Warning
                                                 , $"{weir.Name} is not within grid extend."
                                                 , weirs));
                }

                var result = weir.Validate();
                if (!result.IsValid)
                {
                    issues.Add(new ValidationIssue(weir
                                                 , ValidationSeverity.Error
                                                 , $"{weir.Name}: {result.ValidationException.Messages}"
                                                 , weir));
                }

                if (weir.UseCrestLevelTimeSeries)
                {
                    if (weir.CrestLevelTimeSeries.Time.Values.Any())
                    {
                        var startTime = weir.CrestLevelTimeSeries.Time.Values.First();
                        var stopTime = weir.CrestLevelTimeSeries.Time.Values.Last();

                        if (startTime > model.StartTime || stopTime < model.StopTime)
                        {
                            issues.Add(new ValidationIssue(weir
                                                         , ValidationSeverity.Error
                                                         , $"'{weir.Name}': crest level time series does not span the model run interval."
                                                         , weir));
                        }
                    }
                    else
                    {
                        issues.Add(new ValidationIssue(weir
                                                     , ValidationSeverity.Error
                                                     , $"'{weir.Name}': crest level time series does not contain any values."
                                                     , weir));
                    }
                }

                if (weir.WeirFormula is SimpleWeirFormula weirFormula &&
                    weirFormula.LateralContraction < 0.0)
                {
                    issues.Add(new ValidationIssue(weir
                                                 , ValidationSeverity.Error
                                                 , $"'{weir.Name}': lateral contraction coefficient must be greater than or equal to zero."
                                                 , weir));
                }

                if (weir.WeirFormula is IGatedWeirFormula gatedWeirFormula)
                {
                    // DoorHeight
                    if (gatedWeirFormula.DoorHeight < 0.0)
                    {
                        issues.Add(new ValidationIssue(weir
                                                     , ValidationSeverity.Error
                                                     , $"'{weir.Name}': door height must be greater than or equal to 0."
                                                     , weir));
                    }

                    // HorizontalDoorOpeningWidth
                    if (gatedWeirFormula.UseHorizontalDoorOpeningWidthTimeSeries)
                    {
                        var doorOpeningTimeSeries =
                            gatedWeirFormula.HorizontalDoorOpeningWidthTimeSeries;
                        if (doorOpeningTimeSeries.Components[0].Values.Cast<object>()
                                                 .Any(value => (double)value < 0.0))
                        {
                            issues.Add(new ValidationIssue(weir
                                                         , ValidationSeverity.Error
                                                         , $"'{weir.Name}': opening width time series values must be greater than or equal to 0."
                                                         , weir));
                        }

                        if (doorOpeningTimeSeries.Time.Values.Any())
                        {
                            var startTime = doorOpeningTimeSeries.Time.Values.First();
                            var stopTime = doorOpeningTimeSeries.Time.Values.Last();

                            if (startTime > model.StartTime || stopTime < model.StopTime)
                            {
                                issues.Add(new ValidationIssue(weir
                                                             , ValidationSeverity.Error
                                                             , $"'{weir.Name}': opening width time series does not span the model run interval."
                                                             , weir));
                            }
                        }
                        else
                        {
                            issues.Add(new ValidationIssue(weir
                                                         , ValidationSeverity.Error
                                                         , $"'{weir.Name}': opening width time series does not contain any values."
                                                         , weir));
                        }
                    }
                    else if (gatedWeirFormula.HorizontalDoorOpeningWidth < 0.0)
                    {
                        issues.Add(new ValidationIssue(weir
                                                     , ValidationSeverity.Error
                                                     , $"'{weir.Name}': opening width must be greater than or equal to 0."
                                                     , weir));
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
                                issues.Add(new ValidationIssue(weir
                                                             , ValidationSeverity.Error
                                                             , $"'{weir.Name}': lower edge level time series does not span the model run interval."
                                                             , weir));
                            }
                        }
                        else
                        {
                            issues.Add(new ValidationIssue(weir
                                                         , ValidationSeverity.Error
                                                         , $"'{weir.Name}': lower edge level time series does not contain any values."
                                                         , weir));
                        }
                    }
                }

                issues.AddIssueIfInvalidCrestWidthValue(weir, weir.CrestWidth, "Crest Width");

                if (weir.WeirFormula is GeneralStructureWeirFormula generalStructureFormula)
                {
                    if (generalStructureFormula.HorizontalDoorOpeningDirection !=
                        GateOpeningDirection.Symmetric)
                    {
                        issues.Add(new ValidationIssue(weir
                                                     , ValidationSeverity.Error
                                                     , $"'{weir.Name}': only symmetric horizontal door opening direction is supported for general structures."
                                                     , weir));
                    }

                    // CrestWidth
                    issues.AddIssueIfInvalidCrestWidthValue(weir, generalStructureFormula.WidthStructureLeftSide,    "Upstream 2 Width");
                    issues.AddIssueIfInvalidCrestWidthValue(weir, generalStructureFormula.WidthLeftSideOfStructure,  "Upstream 1 Width");
                    issues.AddIssueIfInvalidCrestWidthValue(weir, generalStructureFormula.WidthStructureRightSide,   "Downstream 1 Width");
                    issues.AddIssueIfInvalidCrestWidthValue(weir, generalStructureFormula.WidthRightSideOfStructure, "Downstream 2 Width");
                }
            }

            return issues;
        }

        /// <summary>
        /// Add an issue to this issues if any is encountered for the specified <paramref name="crestWidthValue"/>.
        /// </summary>
        /// <param name="issues">The issues to which any encountered issues is added.</param>
        /// <param name="subjectWeir">The weir to which the crest width property belongs.</param>
        /// <param name="crestWidthValue">The crest width value to be evaluated.</param>
        /// <param name="crestWidthPropertyName">The name of the crest width property to be evaluated.</param>
        /// <remarks> Issues is not null. </remarks>
        private static void AddIssueIfInvalidCrestWidthValue(this ICollection<ValidationIssue> issues,
                                                             IWeir subjectWeir,
                                                             double crestWidthValue,
                                                             string crestWidthPropertyName)
        {
            if (double.IsNaN(crestWidthValue))
                issues.Add(new ValidationIssue(subjectWeir
                                             , ValidationSeverity.Info
                                             , $"{crestWidthPropertyName} for '{subjectWeir.Name}' structure type: {subjectWeir.WeirFormula.GetName2D()}, will be calculated by the computational core."
                                             , subjectWeir));
            else if (crestWidthValue <= 0.0)
                issues.Add(new ValidationIssue(subjectWeir
                                             , ValidationSeverity.Error
                                             , $"{crestWidthPropertyName} for '{subjectWeir.Name}' structure type: {subjectWeir.WeirFormula.GetName2D()}, must be greater than 0."
                                             , subjectWeir));
        }

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
                    issues.Add(new ValidationIssue(sobekPump
                                                 , ValidationSeverity.Warning
                                                 , $"pump '{sobekPump.Name}' not within grid extent"
                                                 , pumps));
                }

                var result = sobekPump.Validate();
                if (!result.IsValid)
                {
                    issues.Add(new ValidationIssue(sobekPump
                                                 , ValidationSeverity.Error
                                                 , $"pump '{sobekPump.Name}': {result.ValidationException.Message}"
                                                 , sobekPump));
                }

                // Capacity must be >= 0
                if (sobekPump.CanBeTimedependent && sobekPump.UseCapacityTimeSeries)
                {
                    if (sobekPump.CapacityTimeSeries.Components[0].Values.Cast<object>()
                                 .Any(value => (double)value < 0.0))
                    {
                        issues.Add(new ValidationIssue(sobekPump
                                                     , ValidationSeverity.Error
                                                     , $"pump '{sobekPump.Name}': capacity time series values must be greater than or equal to 0."
                                                     , sobekPump));
                    }

                    if (sobekPump.CapacityTimeSeries.Time.Values.Any())
                    {
                        var startTime = sobekPump.CapacityTimeSeries.Time.Values.First();
                        var stopTime = sobekPump.CapacityTimeSeries.Time.Values.Last();

                        if (startTime > model.StartTime || stopTime < model.StopTime)
                        {
                            issues.Add(new ValidationIssue(sobekPump
                                                         , ValidationSeverity.Error
                                                         , $"pump '{sobekPump.Name}': capacity time series does not span the model run interval."
                                                         , sobekPump));
                        }
                    }
                    else
                    {
                        issues.Add(new ValidationIssue(sobekPump
                                                     , ValidationSeverity.Error
                                                     , $"pump '{sobekPump.Name}': capacity time series does not contain any values."
                                                     , sobekPump));
                    }
                }
                else
                {
                    if (sobekPump.Capacity < 0)
                    {
                        issues.Add(new ValidationIssue(sobekPump
                                                     , ValidationSeverity.Error
                                                     , $"pump '{sobekPump.Name}': Capacity must be greater than or equal to 0."
                                                     , sobekPump));
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
