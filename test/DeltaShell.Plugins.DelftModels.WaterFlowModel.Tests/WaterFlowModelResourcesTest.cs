using System;
using System.Globalization;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModelResourcesTest
    {
        [Test]
        public void WaterFlowFMCultureTest()
        {
            Assert.Null(Resources.Culture);
            var currentCulture = CultureInfo.CurrentCulture;
            try
            {
                TryNewCulture(CultureInfo.CurrentCulture);
                TryNewCulture(CultureInfo.InvariantCulture);
                TryNewCulture(CultureInfo.CurrentUICulture);
                TryNewCulture(CultureInfo.InstalledUICulture);
            }
            catch (Exception e)
            {
                Assert.Fail("Changing culture threw an exception: {0}", e.Message);
            }
            finally
            {
                TryNewCulture(currentCulture);
                Assert.NotNull(Resources.Culture);
            }
        }

        private void TryNewCulture(CultureInfo newCulture)
        {
            Resources.Culture = newCulture;
            Assert.AreEqual(Resources.Culture, newCulture);
        }

        [Test]
        public void GetImageResourcesAsObjectsTest()
        {
            try
            {
                Assert.NotNull(Resources.Boundary);
                Assert.NotNull(Resources.feedback);
                Assert.NotNull(Resources.folder_with_data);
                Assert.NotNull(Resources.generateDataInSeriesToolStripMenuItem_Image);
                Assert.NotNull(Resources.HBoundary);
                Assert.NotNull(Resources.HConst);
                Assert.NotNull(Resources.None);
                Assert.NotNull(Resources.Observation);
                Assert.NotNull(Resources.QBoundary);
                Assert.NotNull(Resources.QConst);
                Assert.NotNull(Resources.QHBoundary);
                Assert.NotNull(Resources.ReverseRoughnessSection);
                Assert.NotNull(Resources.RoughnessSection);
                Assert.NotNull(Resources.unstruc);
                Assert.NotNull(Resources.validation);
                Assert.NotNull(Resources.Wind);
            }
            catch (Exception e)
            {
                Assert.Fail("Retreiving drawings threw an exception: {0}", e.Message);
            }
        }

        [Test]
        public void GetMessageResourcesAsStringsTest()
        {
            try
            {
                TryRetreivingMessage(Resources.WaterFlowModel1D_AddDispersionF3CoverageDataItem_Dispersion_F3_coefficient);
                TryRetreivingMessage(Resources.WaterFlowModel1D_AddDispersionF4CoverageDataItem_Dispersion_F4_coefficient);
                TryRetreivingMessage(Resources.WaterFlowModel1D_EnableSalt_Dispersion_F1_coefficient);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_GetRoughnessValidationIssuesForSection_Branch___0___has_Q_H_dependent_roughness_defined_on_section);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_GetRoughnessValidationIssuesForSection_Branch___0___has_Q_H_dependent_roughness_defined_on_section_2);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidateBoundaryConditions_Boundary_conditions);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidateBoundaryConditions_DuplicateValues);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidateBoundaryConditions_NonSequentialValues);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidateBoundaryConditions_The_boundary_condition__0__has_a_salinity_type_of_None__All_open_boundaries_must_specify_salinity_values_);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidateBoundaryConditions_The_boundary_condition__0__has_multiple_connecting_branches__This_is_only_possible_for_waterlevel_boundary_conditions_);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidateExtraResistance_Empty_roughness_table);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidateExtraResistance_Extra_resistance);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidateGatedWeirFormula____Gate_opening_must_be_greater_than_or_equal_to_0_);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidateGatedWeirFormula____Maximum_negative_flow_restrictions_must_be_greater_than_or_equal_to_0_);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidateInputRestartState_Input_restart_state);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidateModelSettings_Grid_output_time_step_must_be_positive_value_);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidateModelSettings_Numerical_Parameter_Iadvec1D_must_be_1___5__Given_Value_is___0_);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidateModelSettings_Numerical_Parameter_Limtyphu1D_must_be_1___3__Given_Value_is___0_);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidateModelSettings_Numerical_Parameter_Momdilution1D_must_be_1___3__Given_Value_is___0_);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidateModelSettings_Structures_output_time_step_must_be_positive_value_);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidateModelSettings_The_structures_output_time_step_should_be_a_multiple_of_the_calculation_time_step_);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidatePump____Capacity_must_be_greater_than_or_equal_to_0_);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidatePumpDeliverySide____Delivery_start_level_must_be_less_than_or_equal_to_delivery_stop_level_);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidatePumpSuctionSide____Suction_start_level_must_be_greater_than_or_equal_to_suction_stop_level_);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidateSalinity_Salinity);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidateWeir____Crest_width_must_be_greater_than_or_equal_to_0_);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelTemperatureValidator_ValidateModelParameters_Temperature_parameters);
                TryRetreivingMessage(Resources.WaterFlowModel1DOutputFileReader_ReadMetaData_ErrorReadingNetCdfFile);
                TryRetreivingMessage(Resources.WaterFlowModel1DOutputFileValidator_Validate_OutputFileContainsMoreThan1LocationIdVariable);
                TryRetreivingMessage(Resources.WaterFlowModel1DOutputFileValidator_Validate_OutputFileDoesNotContainAnyTimeDependentVariables);
                TryRetreivingMessage(Resources.WaterFlowModel1DOutputFileValidator_Validate_OutputFileDoesNotContainRequiredLocationIdVariable);
                TryRetreivingMessage(Resources.WaterFlowModel1DOutputFileValidator_Validate_OutputFileDoesNotContainRequiredTimeDimension);
                TryRetreivingMessage(Resources.WaterFlowModel1DOutputFileValidator_Validate_OutputFileDoesNotContainRequiredTimeVariable);
                TryRetreivingMessage(Resources.WaterFlowModel1DOutputFileValidator_Validate_OutputFileDoesNotExist);
                TryRetreivingMessage(Resources.WaterFlowModel1DOutputFileValidator_Validate_TimeVariableDoesNotContainUnitAttribute);
                TryRetreivingMessage(Resources.WaterFlowModel1DOutputFileValidator_Validate_TimeVariableDoesNotContainValidUnitInRequiredFormat);
                TryRetreivingMessage(Resources.WaterFlowModel1DTemperatureValidator_ValidateInitialTemperature_Initial_temperature_);
                TryRetreivingMessage(Resources.BoundaryNodeDataMapTool_AddToolStripMenuItems_Turn_selected_nodes_into_H);
                TryRetreivingMessage(Resources.BoundaryNodeDataMapTool_AddToolStripMenuItems_Turn_selected_nodes_into_H_t_);
                TryRetreivingMessage(Resources.BoundaryNodeDataMapTool_AddToolStripMenuItems_Turn_selected_nodes_into_none);
                TryRetreivingMessage(Resources.BoundaryNodeDataMapTool_AddToolStripMenuItems_Turn_selected_nodes_into_Q);
                TryRetreivingMessage(Resources.BoundaryNodeDataMapTool_AddToolStripMenuItems_Turn_selected_nodes_into_Q_h_);
                TryRetreivingMessage(Resources.BoundaryNodeDataMapTool_AddToolStripMenuItems_Turn_selected_nodes_into_Q_t_);
                TryRetreivingMessage(Resources.LateralSourceDataMapTool_AddToolStripMenuItems_Turn_selected_laterals_into_Q);
                TryRetreivingMessage(Resources.LateralSourceDataMapTool_AddToolStripMenuItems_Turn_selected_laterals_into_Q_h_);
                TryRetreivingMessage(Resources.LateralSourceDataMapTool_AddToolStripMenuItems_Turn_selected_laterals_into_Q_t_);
                TryRetreivingMessage(Resources.RoughnessDataFileReader_FillFunctionWithTableData_Filling_the_table_of_the_Q_or_H_function_failed_);
                TryRetreivingMessage(Resources.RoughnessDataFileReader_FillFunctionWithTableData_Filling_the_table_of_the_Q_or_H_function_failed__values_count_doesn_t_match_the_defined_levels_count_);
                TryRetreivingMessage(Resources.RoughnessDataFileReader_ReadDefinitionData_Could_not_read_roughness_definition_data);
                TryRetreivingMessage(Resources.RoughnessDataFileReader_ReadDefinitionData_Couldn_t_read_roughness_section_with_a_function_type_of____0_);
                TryRetreivingMessage(Resources.RoughnessDataFileReader_ReadFile_adding_to_network_went_wrong_);
                TryRetreivingMessage(Resources.RoughnessDataFileReader_ReadFile_Could_not_read_content_section__0__properly);
                TryRetreivingMessage(Resources.RoughnessDataFileReader_ReadFile_Could_not_read_file__0__properly__it_doesn_t_exist_);
                TryRetreivingMessage(Resources.RoughnessDataFileReader_ReadFile_Could_not_read_file__0__properly__it_seems_empty);
                TryRetreivingMessage(Resources.RoughnessDataFileReader_ReadFile_Could_not_set_roughness_data_in_model);
                TryRetreivingMessage(Resources.RoughnessDataFileReader_ReadFile_While_reading_roughness_section_an_error_occured___0___1_);
                TryRetreivingMessage(Resources.RoughnessDataFileReader_ReadRoughnessBranchData_branch___0___where_the_roughness_should_be_put_on_is_not_available_in_the_model);
                TryRetreivingMessage(Resources.RoughnessDataFileReader_ReadRoughnessBranchData_Could_not_read_roughness_branch_data);
                TryRetreivingMessage(Resources.RoughnessDataFileReader_ReadRoughnessBranchData_The_length_of_the_number_of_levels___0___and_the_defined_number_of_levels_of_the_branch_property__1__are_not_the_same_of_branch_properties____2__);
                TryRetreivingMessage(Resources.RoughnessDataFileReader_ReadRoughnessBranchData_While_reading_branches_for_roughness_section_an_error_occured___0___1_);
                TryRetreivingMessage(Resources.RoughnessDataFileReader_ReadRoughnessSection_Could_not_import_calibrated_roughness_section__0__because_the_calibrated_roughness_section_you_want_to_import_doesn_t_exist_in_the_model);
                TryRetreivingMessage(Resources.RoughnessDataFileReader_ReadRoughnessSection_When_reading_reverse_roughness_section___0___the_referring__linked___normal__roughness_section___1___is_not_found__The_normal_section___1___should_be_imported_first_);
                TryRetreivingMessage(Resources.RoughnessDataFileReader_ReadRoughnessSection_While_creating_the_roughnes_section_from_the_roughness_file_the_fisrt_argument_of_the_roughness_network_coverage_is_not_created__used_to_set_the_interpolation_type);
                TryRetreivingMessage(Resources.RoughnessDataFileReader_ReadRoughnessSection_While_creating_the_roughnes_section_from_the_roughness_file_the_roughness_network_coverage_is_not_created);
                TryRetreivingMessage(Resources.WaterFlowModel1DApplicationPlugin_Description);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidateModelSettings_Indicated_morphology_file_does_not_exist__);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidateModelSettings_Indicated_sediment_file_does_not_exist__);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelDataValidator_ValidateModelSettings_No_explicit_working_directory_found__Please_save_model_before_morphology_can_be_run_);
                TryRetreivingMessage(Resources.WaterFlowModel1DModelOutputSettingsValidator_ValidateAggregationOptions_Only_allowed_values__Current__and__None__for__0_);
                TryRetreivingMessage(Resources.WaterFlowModel1DOutputFileReader_ParseReferenceTime_UnableToParseDateTimeFromFile);
                TryRetreivingMessage(Resources.WaterFlowModel1DTemperatureValidator_ValidateBoundaryConditions_The_boundary_condition__0__has_a_temperature_type_of_None__All_open_boundaries_must_specify_temperature_values_);
                TryRetreivingMessage(Resources.WaterFlowModel1DTemperatureValidator_ValidateModelParameters_Values_should_be_in_double_format_);
                TryRetreivingMessage(Resources.WaterFlowModel1DSalinityValidator_ValidateSalinityForKuijperVanRijnPrismaticIsValid_No_Estuary_mouth_node_specified_);
                TryRetreivingMessage(Resources.WaterFlowModel1DSalinityValidator_ValidateSalinityForKuijperVanRijnPrismaticIsValid_Can_not_find_specified_estuary_mouth_node__0__);
                TryRetreivingMessage(Resources.WaterFlowModel1DSalinityValidator_ValidateSalinityForKuijperVanRijnPrismaticIsValid_Estuary_mouth_node__0__is_not_a_boundary_node_);
                TryRetreivingMessage(Resources.WaterFlowModel1DSalinityValidator_ValidateSalinityForKuijperVanRijnPrismaticIsValid_F4_Coverage_values_cannot_all_be_set_to_0__Either_remove_them_or_set_a_valid_value_);
            }
            catch (Exception e)
            {
                Assert.Fail("Retreiving messages threw an exception: {0}", e.Message);
            }
        }
        private void TryRetreivingMessage(String message)
        {
            Assert.NotNull(message);
            Assert.IsNotEmpty(message);
            Assert.GreaterOrEqual(message.Length, 1);
        }
    }
}