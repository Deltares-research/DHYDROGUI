using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NUnit.Framework;
using System;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class InitialConditionInitialFieldsFileReaderTest
    {
        [Test]
        public void GivenInvalidPath_WhenCallingReadFile_ThenThrowsException()
        {
            string invalidPath = "invalidPath";
            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;

                TestDelegate action = () =>
                    InitialConditionInitialFieldsFileReader.ReadFile(invalidPath, modelDefinition);

                Assert.Throws<FileReadingException>(action);
            }
        }

        [Test]
        public void GivenFileWithNoCategories_WhenCallingReadFile_ThenThrowsException()
        {
            var noCategoriesFile = TestHelper.GetTestFilePath(@"IO\noCategories.ini");
            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;

                TestDelegate action = () =>
                    InitialConditionInitialFieldsFileReader.ReadFile(noCategoriesFile, modelDefinition);

                Assert.Throws<FileReadingException>(action);
            }
        }

        [Test]
        public void GivenFileWithOnlyInvalidCategories_WhenCallingReadFile_ThenThrowsException()
        {
            var noCategoriesFile = TestHelper.GetTestFilePath(@"IO\invalidCategoriesOnly.ini");
            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;

                TestDelegate action = () =>
                    InitialConditionInitialFieldsFileReader.ReadFile(noCategoriesFile, modelDefinition);

                Assert.Throws<FileReadingException>(action);
            }
        }

        [Test]
        public void GivenInitialFieldsFile_WhenCallingReadFile_ThenReturnsExpectedTuple()
        {
            var multipleValidCategoriesFile = TestHelper.GetTestFilePath(@"IO\initialFieldsWaterLevel_expected.ini");
            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;

                (InitialConditionQuantity, string) expectedReturnValue =
                    (InitialConditionQuantity.WaterLevel, "InitialWaterLevel.ini");

                (InitialConditionQuantity, string) actualReturnValue =
                    InitialConditionInitialFieldsFileReader.ReadFile(multipleValidCategoriesFile, modelDefinition);

                Assert.That(actualReturnValue, Is.EqualTo(expectedReturnValue)); 
            }
        }

        [Test]
        public void GivenFileWithMultipleValidCategories_WhenCallingReadFile_ThenReturnsDataFromFirstCategoryAndLogsWarning()
        {
            var multipleValidCategoriesFile = TestHelper.GetTestFilePath(@"IO\multipleValidCategories.ini");
            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;

                (InitialConditionQuantity, string) expectedReturnValue =
                    (InitialConditionQuantity.WaterDepth, "InitialWaterDepth.ini");


                (InitialConditionQuantity, string) actualReturnValue =
                    InitialConditionInitialFieldsFileReader.ReadFile(multipleValidCategoriesFile, modelDefinition);

                Action action = () =>
                    InitialConditionInitialFieldsFileReader.ReadFile(multipleValidCategoriesFile, modelDefinition);
                TestHelper.AssertLogMessageIsGenerated(action, Properties.Resources.Initial_Condition_Warning_Only_one_quantity_type_is_currently_supported_reading_the_first_and_ignoring_all_others, 1);


                Assert.That(actualReturnValue, Is.EqualTo(expectedReturnValue)); 
            }
        }

    }
}