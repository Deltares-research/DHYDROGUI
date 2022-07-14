using System;
using DeltaShell.NGHS.IO.Properties;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests
{
    [TestFixture]
    public class NGHSIOResourcesTest
    {
        private void TryRetreivingMessage(String message)
        {
            Assert.NotNull(message);
            Assert.IsNotEmpty(message);
            Assert.GreaterOrEqual(message.Length, 1);
        }

        [Test]
        public void GetMessageResourcesAsStringsTest()
        {
            try
            {
                TryRetreivingMessage(Resources
                    .RoughnessDataFileReader_FillFunctionWithTableData_Filling_the_table_of_the_Q_or_H_function_failed_);
                TryRetreivingMessage(Resources
                    .RoughnessDataFileReader_FillFunctionWithTableData_Filling_the_table_of_the_Q_or_H_function_failed__values_count_doesn_t_match_the_defined_levels_count_);
                TryRetreivingMessage(Resources
                    .RoughnessDataFileReader_ReadDefinitionData_Could_not_read_roughness_definition_data);
                TryRetreivingMessage(Resources
                    .RoughnessDataFileReader_ReadDefinitionData_Couldn_t_read_roughness_section_with_a_function_type_of____0_);
                TryRetreivingMessage(Resources.RoughnessDataFileReader_ReadFile_adding_to_network_went_wrong_);
                TryRetreivingMessage(Resources
                    .RoughnessDataFileReader_ReadFile_Could_not_read_content_section__0__properly);
                TryRetreivingMessage(Resources
                    .Could_not_read_file_0_properly_it_doesnt_exist);
                TryRetreivingMessage(Resources
                    .RoughnessDataFileReader_ReadFile_Could_not_read_file__0__properly__it_seems_empty);
                TryRetreivingMessage(Resources.RoughnessDataFileReader_ReadFile_Could_not_set_roughness_data_in_model);
                TryRetreivingMessage(Resources
                    .RoughnessDataFileReader_ReadFile_While_reading_roughness_section_an_error_occured___0___1_);
                TryRetreivingMessage(Resources
                    .RoughnessDataFileReader_ReadRoughnessBranchData_branch___0___where_the_roughness_should_be_put_on_is_not_available_in_the_model);
                TryRetreivingMessage(Resources
                    .RoughnessDataFileReader_ReadRoughnessBranchData_Could_not_read_roughness_branch_data);
                TryRetreivingMessage(Resources
                    .RoughnessDataFileReader_ReadRoughnessBranchData_The_length_of_the_number_of_levels___0___and_the_defined_number_of_levels_of_the_branch_property__1__are_not_the_same_of_branch_properties____2__);
                TryRetreivingMessage(Resources
                    .RoughnessDataFileReader_ReadRoughnessBranchData_While_reading_branches_for_roughness_section_an_error_occured___0___1_);
                TryRetreivingMessage(Resources
                    .RoughnessDataFileReader_ReadRoughnessSection_Could_not_import_calibrated_roughness_section__0__because_the_calibrated_roughness_section_you_want_to_import_doesn_t_exist_in_the_model);
                TryRetreivingMessage(Resources
                    .RoughnessDataFileReader_ReadRoughnessSection_When_reading_reverse_roughness_section___0___the_referring__linked___normal__roughness_section___1___is_not_found__The_normal_section___1___should_be_imported_first_);
                TryRetreivingMessage(Resources
                    .RoughnessDataFileReader_ReadRoughnessSection_While_creating_the_roughnes_section_from_the_roughness_file_the_fisrt_argument_of_the_roughness_network_coverage_is_not_created__used_to_set_the_interpolation_type);
                TryRetreivingMessage(Resources
                    .RoughnessDataFileReader_ReadRoughnessSection_While_creating_the_roughnes_section_from_the_roughness_file_the_roughness_network_coverage_is_not_created);
            }
            catch (Exception e)
            {
                Assert.Fail("Retreiving messages threw an exception: {0}", e.Message);
            }
        }

    }
}
