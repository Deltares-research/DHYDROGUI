using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.Restart;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using ValidationAspects;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation
{
    public static class WaterFlowModel1DModelDataValidator
    {
        public static ValidationReport Validate(WaterFlowModel1D flowModel1D)
        {
            return new ValidationReport("Model Data",
                                        new[]
                                            {
                                                ValidateModelSettings(flowModel1D),
                                                ValidateStructures(flowModel1D.Network),
                                                WaterFlowModel1DDiscretizationValidator.Validate(flowModel1D.NetworkDiscretization, flowModel1D),
                                                ValidateRoughness(flowModel1D),
                                                ValidateExtraResistance(flowModel1D),
                                                RestartTimeRangeValidator.ValidateRestartTimeRangeSettings(
                                                    flowModel1D.UseSaveStateTimeRange,
                                                    flowModel1D.SaveStateStartTime,
                                                    flowModel1D.SaveStateStopTime,
                                                    flowModel1D.SaveStateTimeStep, 
                                                    flowModel1D),
                                                ValidateInputRestartState(flowModel1D),
                                                ValidateBoundaryConditions(flowModel1D),
                                                WaterFlowModel1DSalinityValidator.Validate(flowModel1D),
                                                WaterFlowModel1DTemperatureValidator.Validate(flowModel1D)
                                            });
        }
        

        private static ValidationReport ValidateBoundaryConditions(WaterFlowModel1D model)
        {
            var issues = new List<ValidationIssue>();

            var boundaryConditionsWithMultipleConnectingBranches = model.BoundaryConditions
                .Where(bc => bc.Feature.IsConnectedToMultipleBranches
                             && (bc.DataType == WaterFlowModel1DBoundaryNodeDataType.FlowConstant
                                 || bc.DataType == WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries
                                 || bc.DataType == WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable));

            foreach (var bc in boundaryConditionsWithMultipleConnectingBranches)
            {
                issues.Add(new ValidationIssue(bc, ValidationSeverity.Error, string.Format(
                    Resources.WaterFlowModel1DModelDataValidator_ValidateBoundaryConditions_The_boundary_condition__0__has_multiple_connecting_branches__This_is_only_possible_for_waterlevel_boundary_conditions_, bc.Name)));
            }

            // SOBEK3-1035: Q(h) boundaries should have values in sequence
            foreach (var bc in model.BoundaryConditions.Where(bc => bc.DataType == WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable && bc.Data != null))
            {
                var values = bc.Data.GetValues<double>().ToList();

                if (values.GroupBy(n => n).Any(c => c.Count() > 1)) // check for duplicates
                {
                    issues.Add(new ValidationIssue(bc, ValidationSeverity.Warning, string.Format(
                        Resources.WaterFlowModel1DModelDataValidator_ValidateBoundaryConditions_DuplicateValues, bc.Name)));
                    continue;
                }

                if(!IsPositiveSequence(values) && !IsNegativeSequence(values))
                    issues.Add(new ValidationIssue(bc, ValidationSeverity.Warning, string.Format(
                        Resources.WaterFlowModel1DModelDataValidator_ValidateBoundaryConditions_NonSequentialValues, bc.Name)));
            }

            return new ValidationReport(Resources.WaterFlowModel1DModelDataValidator_ValidateBoundaryConditions_Boundary_conditions, issues);
        }

        private static bool IsPositiveSequence(IList<double> values)
        {
            if (values.Count > 1)
            {
                for (var i = 1; i < values.Count; i++)
                {
                    if (values[i - 1] >= values[i]) return false;
                }
            }
            return true;
        }

        private static bool IsNegativeSequence(IList<double> values)
        {
            if (values.Count > 1)
            {
                for (var i = 1; i < values.Count; i++)
                {
                    if (values[i - 1] <= values[i]) return false;
                }
            }
            return true;
        }

        private static ValidationReport ValidateStructures(IHydroNetwork network)
        {
            var issues = new List<ValidationIssue>();

            foreach (var composite in network.CompositeBranchStructures)
            {
                if (composite.Structures.Count == 0)
                {
                    issues.Add(new ValidationIssue(composite, ValidationSeverity.Error, "Does not contain any structures", network));
                }

                foreach (var structure in composite.Structures)
                {
                    //generic validation (using Validation Aspects)
                    var result = structure.Validate();
                    if (!result.IsValid)
                    {
                        issues.Add(new ValidationIssue(structure, ValidationSeverity.Error, result.ValidationException.Message));
                    }

                    if (structure is IWeir) ValidateWeir((IWeir)structure, issues);
                    if (structure is IPump) ValidatePump((IPump)structure, issues);
                }
            }
            return new ValidationReport("Structures", issues);
        }

        private static void ValidatePump(IPump structure, List<ValidationIssue> issues)
        {
            // Capacity must be >= 0
            if (structure.Capacity < 0)
            {
                issues.Add(new ValidationIssue(structure, ValidationSeverity.Error, "pump '" + structure.Name + Resources.WaterFlowModel1DModelDataValidator_ValidatePump____Capacity_must_be_greater_than_or_equal_to_0_));
            }

            switch (structure.ControlDirection)
            {
                case PumpControlDirection.DeliverySideControl:
                    ValidatePumpDeliverySide(structure, issues);
                    break;
                case PumpControlDirection.SuctionAndDeliverySideControl:
                    ValidatePumpDeliverySide(structure, issues);
                    ValidatePumpSuctionSide(structure, issues);
                    break;
                case PumpControlDirection.SuctionSideControl:
                    ValidatePumpSuctionSide(structure, issues);
                    break;
            }
        }

        private static void ValidatePumpSuctionSide(IPump sobekPump, ICollection<ValidationIssue> issues)
        {
            if (sobekPump.StartSuction < sobekPump.StopSuction)
            {
                issues.Add(new ValidationIssue(sobekPump, ValidationSeverity.Error,
                                               "pump '" + sobekPump.Name + Resources.WaterFlowModel1DModelDataValidator_ValidatePumpSuctionSide____Suction_start_level_must_be_greater_than_or_equal_to_suction_stop_level_));
            }
        }

        private static void ValidatePumpDeliverySide(IPump sobekPump, ICollection<ValidationIssue> issues)
        {
            if (sobekPump.StartDelivery > sobekPump.StopDelivery)
            {
                issues.Add(new ValidationIssue(sobekPump, ValidationSeverity.Error,
                                               "pump '" + sobekPump.Name + Resources.WaterFlowModel1DModelDataValidator_ValidatePumpDeliverySide____Delivery_start_level_must_be_less_than_or_equal_to_delivery_stop_level_));
            }
        }

        private static void ValidateWeir(IWeir structure, ICollection<ValidationIssue> issues)
        {
            if (structure.CrestWidth < 0)
            {
                issues.Add(new ValidationIssue(structure, ValidationSeverity.Error, "weir '" + structure.Name + Resources.WaterFlowModel1DModelDataValidator_ValidateWeir____Crest_width_must_be_greater_than_or_equal_to_0_));
            }

            if (structure.WeirFormula is GatedWeirFormula)
            {
                ValidateGatedWeirFormula(structure, issues);
            }
        }

        private static void ValidateGatedWeirFormula(IWeir weir, ICollection<ValidationIssue> issues)
        {
            var gatedWeirFormula = (GatedWeirFormula)weir.WeirFormula;

            if (gatedWeirFormula.GateOpening < 0)
            {
                issues.Add(new ValidationIssue(weir, ValidationSeverity.Error, "weir '" + weir.Name + Resources.WaterFlowModel1DModelDataValidator_ValidateGatedWeirFormula____Gate_opening_must_be_greater_than_or_equal_to_0_));
            }

            if (gatedWeirFormula.UseMaxFlowPos && gatedWeirFormula.MaxFlowPos < 0)
            {
                issues.Add(new ValidationIssue(weir, ValidationSeverity.Error, "weir '" + weir.Name + Resources.WaterFlowModel1DModelDataValidator_ValidateGatedWeirFormula____Maximum_positive_flow_restrictions_must_be_greater_than_or_equal_to_0_));
            }
            if (gatedWeirFormula.UseMaxFlowNeg && gatedWeirFormula.MaxFlowNeg < 0)
            {
                issues.Add(new ValidationIssue(weir, ValidationSeverity.Error, "weir '" + weir.Name + Resources.WaterFlowModel1DModelDataValidator_ValidateGatedWeirFormula____Maximum_negative_flow_restrictions_must_be_greater_than_or_equal_to_0_));
            }
        }

        private static ValidationReport ValidateModelSettings(WaterFlowModel1D model)
        {
            var issues = new List<ValidationIssue>();

            //Numerical Parameter Iadvec1D
            var numericalParameter = model.ParameterSettings.FirstOrDefault(p => p.Name == "Iadvec1D");
            if (numericalParameter != null)
            {
                var parameterValue = Convert.ToInt32(numericalParameter.Value);
                if (parameterValue != 1 && parameterValue != 2 && parameterValue != 5)
                {
                    issues.Add(new ValidationIssue("Iadvec1D", ValidationSeverity.Error, string.Format(Resources.WaterFlowModel1DModelDataValidator_ValidateModelSettings_Numerical_Parameter_Iadvec1D_must_be_1___5__Given_Value_is___0_, parameterValue)));
                }
            }

            //Numerical Parameter Limtyphu1D
            numericalParameter = model.ParameterSettings.FirstOrDefault(p => p.Name == "Limtyphu1D");
            if (numericalParameter != null)
            {
                var parameterValue = Convert.ToInt32(numericalParameter.Value);
                if (parameterValue < 1 || parameterValue > 3)
                {
                    issues.Add(new ValidationIssue("Limtyphu1D", ValidationSeverity.Error, string.Format(Resources.WaterFlowModel1DModelDataValidator_ValidateModelSettings_Numerical_Parameter_Limtyphu1D_must_be_1___3__Given_Value_is___0_, parameterValue)));
                }
            }

            //time settings
            var validator = new ModelTimersValidator {Resolution = new TimeSpan(0, 0, 0, 0, 1)};
            issues.AddRange(validator.ValidateModelTimers(model, model.OutputTimeStep));
            
            //additional time settings
            if (model.OutputSettings.GridOutputTimeStep.TotalSeconds <= 0)
            {
                issues.Add(new ValidationIssue("GridOutputTimeStep", ValidationSeverity.Error, Resources.WaterFlowModel1DModelDataValidator_ValidateModelSettings_Grid_output_time_step_must_be_positive_value_));
            }
            if (model.OutputSettings.StructureOutputTimeStep.TotalSeconds <= 0)
            {
                issues.Add(new ValidationIssue("StructureOutputTimeStep", ValidationSeverity.Error, Resources.WaterFlowModel1DModelDataValidator_ValidateModelSettings_Structures_output_time_step_must_be_positive_value_));
            }
            if (model.OutputSettings.StructureOutputTimeStep.TotalSeconds < model.TimeStep.TotalSeconds ||
                model.TimeStep.TotalSeconds > 0 &&
                (int) model.OutputSettings.StructureOutputTimeStep.TotalMilliseconds%
                (int) model.TimeStep.TotalMilliseconds != 0)
            {
                issues.Add(new ValidationIssue("StructureOutputTimeStep", ValidationSeverity.Error,
                    Resources.WaterFlowModel1DModelDataValidator_ValidateModelSettings_The_structures_output_time_step_should_be_a_multiple_of_the_calculation_time_step_));
            }

            // Morphology setting
            if (model.UseMorphology)
            {
                if (String.IsNullOrEmpty(model.ExplicitWorkingDirectory))
                {
                    issues.Add(new ValidationIssue("UseMorphology", ValidationSeverity.Error, Resources.WaterFlowModel1DModelDataValidator_ValidateModelSettings_No_explicit_working_directory_found__Please_save_model_before_morphology_can_be_run_));
                }
                else
                {
                    if (!File.Exists(model.MorphologyPath))
                    {
                        issues.Add(new ValidationIssue("MorphologyPath", ValidationSeverity.Error, Resources.WaterFlowModel1DModelDataValidator_ValidateModelSettings_Indicated_morphology_file_does_not_exist__));
                    }

                    if (!File.Exists(model.SedimentPath))
                    {
                        issues.Add(new ValidationIssue("SedimentPath", ValidationSeverity.Error, Resources.WaterFlowModel1DModelDataValidator_ValidateModelSettings_Indicated_sediment_file_does_not_exist__));
                    }
                }
            }

            return new ValidationReport("Model settings", issues);
        }

        private static ValidationReport ValidateRoughness(WaterFlowModel1D model)
        {
            return new ValidationReport("Roughness", model.Network.Channels.SelectMany(c => GetRoughnessValidationIssues(model, c)));
        }

        private static IEnumerable<ValidationIssue> GetRoughnessValidationIssues(WaterFlowModel1D model,
            IChannel channel)
        {
            return model.RoughnessSections.SelectMany(rs => GetRoughnessValidationIssuesForSection(rs, channel));
        }

        private static IEnumerable<ValidationIssue> GetRoughnessValidationIssuesForSection(RoughnessSection roughnessSection, IChannel channel)
        {
            var roughnessFunctionType = roughnessSection.GetRoughnessFunctionType(channel);
            if (roughnessFunctionType == RoughnessFunction.FunctionOfH || roughnessFunctionType == RoughnessFunction.FunctionOfQ)
            {
                var crossSection = channel.CrossSections.FirstOrDefault();
                if (crossSection != null && crossSection.CrossSectionType != CrossSectionType.ZW)
                {
                    var message = string.Format(
                            Resources.WaterFlowModel1DModelDataValidator_GetRoughnessValidationIssuesForSection_Branch___0___has_Q_H_dependent_roughness_defined_on_section,
                            channel.Name, roughnessSection.Name);
                    yield return new ValidationIssue(channel, ValidationSeverity.Error, message);
                }

                int numRows;
                if (roughnessFunctionType == RoughnessFunction.FunctionOfH)
                {
                    // The function (of H) has two arguments (chainage, H), and one component (the roughness value). 
                    // The number of rows is equal to the number of H values (NOT to the number of values in the roughness component). 
                    numRows = roughnessSection.FunctionOfH(channel).Arguments[1].Values.Count;
                }
                else // function of Q
                {
                    // The number of rows is equal to the number of Q values. 
                    numRows = roughnessSection.FunctionOfQ(channel).Arguments[1].Values.Count;
                }
                if (numRows < 2)
                {
                    var message = string.Format(
                            Resources.WaterFlowModel1DModelDataValidator_GetRoughnessValidationIssuesForSection_Branch___0___has_Q_H_dependent_roughness_defined_on_section_2,
                            channel.Name, roughnessSection.Name);
                    yield return new ValidationIssue(channel, ValidationSeverity.Error, message);
                }
            }
        }

        private static ValidationReport ValidateExtraResistance(WaterFlowModel1D model)
        {
            var issues = new List<ValidationIssue>();

            var extraResistances = model.Network.Structures.Where(s => s is IExtraResistance);
            foreach (IExtraResistance extraResistance in extraResistances)
            {
                int count = extraResistance.FrictionTable.Arguments[0].Values.Count;

                if (count == 0)
                {
                    issues.Add(new ValidationIssue(extraResistance, ValidationSeverity.Error, Resources.WaterFlowModel1DModelDataValidator_ValidateExtraResistance_Empty_roughness_table));
                }
            }
            return new ValidationReport(Resources.WaterFlowModel1DModelDataValidator_ValidateExtraResistance_Extra_resistance, issues);
        }

        private static ValidationReport ValidateInputRestartState(WaterFlowModel1D flowModel1D)
        {
            if (!flowModel1D.UseRestart) return new ValidationReport(Resources.WaterFlowModel1DModelDataValidator_ValidateInputRestartState_Input_restart_state, Enumerable.Empty<ValidationReport>());

            IEnumerable<string> errors, warnings;
            flowModel1D.ValidateInputState(out errors, out warnings);

            var issues = errors.Select(error => new ValidationIssue(Resources.WaterFlowModel1DModelDataValidator_ValidateInputRestartState_Input_restart_state, ValidationSeverity.Error, error)).ToList();
            issues.AddRange(warnings.Select(warning => new ValidationIssue(Resources.WaterFlowModel1DModelDataValidator_ValidateInputRestartState_Input_restart_state, ValidationSeverity.Warning, warning)));

            return new ValidationReport(Resources.WaterFlowModel1DModelDataValidator_ValidateInputRestartState_Input_restart_state, issues);
        }

    }
}
