using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Validators;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.Restart;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

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
                                                ValidateExtraResistance(flowModel1D.Network.Structures.Where(s => s is IExtraResistance)),
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
                             && (bc.DataType == Model1DBoundaryNodeDataType.FlowConstant
                                 || bc.DataType == Model1DBoundaryNodeDataType.FlowTimeSeries
                                 || bc.DataType == Model1DBoundaryNodeDataType.FlowWaterLevelTable));

            foreach (var bc in boundaryConditionsWithMultipleConnectingBranches)
            {
                issues.Add(new ValidationIssue(bc, ValidationSeverity.Error, string.Format(
                    Resources.WaterFlowModel1DModelDataValidator_ValidateBoundaryConditions_The_boundary_condition__0__has_multiple_connecting_branches__This_is_only_possible_for_waterlevel_boundary_conditions_, bc.Name)));
            }

            // SOBEK3-1035: Q(h) boundaries should have values in sequence
            foreach (var bc in model.BoundaryConditions.Where(bc => bc.DataType == Model1DBoundaryNodeDataType.FlowWaterLevelTable && bc.Data != null))
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

        public static ValidationReport ValidateStructures(IHydroNetwork network)
        {
            return StructuresValidator.Validate(network);
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
            return ValidateRoughness(model.Network, model.RoughnessSections);
        }

        public static ValidationReport ValidateRoughness(IHydroNetwork network, IEnumerable<RoughnessSection> roughnessSections)
        {
            return new ValidationReport("Roughness", network.Channels.SelectMany(c => GetRoughnessValidationIssues(roughnessSections, c)));
        }

        private static IEnumerable<ValidationIssue> GetRoughnessValidationIssues(IEnumerable<RoughnessSection> roughnessSections,
            IChannel channel)
        {
            return roughnessSections.SelectMany(rs => GetRoughnessValidationIssuesForSection(rs, channel));
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

        public static ValidationReport ValidateExtraResistance(IEnumerable<IStructure1D> extraResistances)
        {
            return ExtraResistanceValidator.Validate(extraResistances);
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
