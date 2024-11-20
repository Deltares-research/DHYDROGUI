using System;
using System.Globalization;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class WaterFlowFMResourcesTest
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
                Assert.NotNull(Resources.area2d);
                Assert.NotNull(Resources.down);
                Assert.NotNull(Resources.dry_point);
                Assert.NotNull(Resources.FunctionGrid2D);
                Assert.NotNull(Resources.GateSmall);
                Assert.NotNull(Resources.hurricane2);
                Assert.NotNull(Resources.Observation);
                Assert.NotNull(Resources.PumpSmall);
                Assert.NotNull(Resources.StructureFeatureSmall);
                Assert.NotNull(Resources.TextDocument);
                Assert.NotNull(Resources.TimeSeries);
                Assert.NotNull(Resources.unstruc);
                Assert.NotNull(Resources.unstrucModel);
                Assert.NotNull(Resources.unstrucWater);
                Assert.NotNull(Resources.WeirSmall);
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
                TryRetreivingMessage(Resources.BcmFileImporter_ImportItem_Morphology_boundary_condition_bcm_file_importer_could_not_import_data_onto_given_target);
                TryRetreivingMessage(Resources.ExportGrid_Cannot_export_in_this_format_if_the_grid_is_not_correct);
                TryRetreivingMessage(Resources.ExtForceFile_StoreUnknownQuantities_Spatial_varying_quantity__0__detected_in_the_external_force_file_and_will_be_passed_to_the_computational_core__This_may_affect_your_simulation_);
                TryRetreivingMessage(Resources.FlowFMApplicationPlugin_Description);
                TryRetreivingMessage(Resources.FMMapFileFunctionStore_ConstructFunctions_Time_dependent_variable___0___has_been_filtered_out);
                TryRetreivingMessage(Resources.FMMapFileFunctionStore_CoordinateSystem_Could_not_set_coordinate_system_in_output_map_because_grid_is_not_set);
                TryRetreivingMessage(Resources.FMMapFileFunctionStore_CreateCoverage_CannotCreateSpatialDataOnVolumeLocation);
                TryRetreivingMessage(Resources.FMMapFileFunctionStore_CreateCoverage_NetlinkDimensionCurrentyNotSupported);
                TryRetreivingMessage(Resources.FMMapFileFunctionStore_CreateCoverage_UnexpectedLocationDimension);
                TryRetreivingMessage(Resources.FMMapFileFunctionStore_CreateCoverageFromNetCdfVariable_FailedToConstructGridSpatialData);
                TryRetreivingMessage(Resources.FMMapFileFunctionStore_CreateCoverageFromNetCdfVariable_NetcdfVariableHasBeenIgnored);
                TryRetreivingMessage(Resources.MduFile_ReadMorphologyProperties_Cannot_read_ibedcond_because_this_is_not_an_integer__number__in_file__0_);
                TryRetreivingMessage(Resources.SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_Cannot_create_xyz_file_for_spatial_varying_initial_condition__0__because_it_is_a_value_spatial_operation__please_interpolate_the_operation_to_the_grid_or);
                TryRetreivingMessage(Resources.SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_No_spatial_operations_of_type_Import__Add_or_Value_found_for_spatially_varying_property__0___Remember_to_interpolate_them_to_generate_the_xyz_file__Otherwise_the_model_might_not_run_as_expected_);
                TryRetreivingMessage(Resources.SedimentFile_WriteXYZIfDirectoryExists_Could_not_get_directory_name_from_file_path__0_);
                TryRetreivingMessage(Resources.WaterFlowFMBoundaryConditionValidator_ValidateBoundaryConditionPointIndex_Time_series_contains_forbidden_negative_values_for__0__at_point__1_);
                TryRetreivingMessage(Resources.WaterFlowFMBoundaryConditionValidator_ValidateFlowBoundaryConditions_A_morphology_boundary_condition_cannot_have_more_than_one_point_with_generated_data_);
                TryRetreivingMessage(Resources.WaterFlowFMBoundaryConditionValidator_ValidateMorphologyBoundaryHaveHydroBoundaries_Morphology_boundary_condition_must_have_a_Hydro_boundary_condition_);
                TryRetreivingMessage(Resources.WaterFlowFMBoundaryConditionValidator_ValidateSedimentConcentrationBoundaryHaveHydroBoundaries_Sediment_concentration_boundary_condition_must_have_a_Hydro_boundary_condition_);
                TryRetreivingMessage(Resources.WaterFlowFMBoundaryConditionValidator_ValidateSupportPointNames_Custom_support_point_names_are_not_supported_by_gui);
                TryRetreivingMessage(Resources.WaterFlowFMModel_SetVar_FM_Model__0__is_part_of_a_1D2D_model_and_can_t_have_morphology_properties_and___or_sediments__Removing_these_properties_from_the_model);
                TryRetreivingMessage(Resources.WaterFlowFMModelDefinition_SelectSpatialOperations_Duplication_of_spatial_operations_for__0___Please_verify_the_model_after_saving_);
                TryRetreivingMessage(Resources.WaterFlowFMModelDefinition_SetMapFormatPropertyValue_MapFormat_property_value_of_FlowFM_model__0__is_changed_to_1__because_it_is_part_of_an_1D2D_integrated_model_);
                TryRetreivingMessage(Resources.WaterFlowFMModelDefinition_SetMapFormatPropertyValue_MapFormat_property_value_of_FlowFM_model__0__is_changed_to_4_due_to_activation_of_Morphology_);
                TryRetreivingMessage(Resources.WaterFlowFMModelDefinitionValidator_Validate_);
                TryRetreivingMessage(Resources.UnstructuredGridCoverageExtensions_ToPointCloud_Converting_time_dependent_spatial_data_to_samples_is_not_supported);
                TryRetreivingMessage(Resources.UnstructuredGridCoverageExtensions_ToPointCloud_Converting_a_non_double_valued_coverage_component_to_a_point_cloud_is_not_supported);
                TryRetreivingMessage(Resources.UnstructuredGridCoverageExtensions_ToPointCloud_Spatial_data_is_not_consistent__number_of_coordinate_does_not_match_number_of_values);
                TryRetreivingMessage(Resources.GridHelper_CreateUnstructuredGridFromNetCdfFor1D2DLinks_It_was_not_possible_to_load_from_file___0___LogFilePath___1_);
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