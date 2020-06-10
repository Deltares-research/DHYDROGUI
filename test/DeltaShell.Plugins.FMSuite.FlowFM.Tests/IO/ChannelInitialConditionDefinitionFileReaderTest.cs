using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class ChannelInitialConditionDefinitionFileReaderTest
    {
        private const double Epsilon = 0.0001;

        [Test]
        public void GivenInvalidPath_WhenCallingReadFile_ThenThrowsException()
        {
            // Given
            const string invalidPath = "invalidPath";

            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;

                // When
                TestDelegate action = () =>
                    ChannelInitialConditionDefinitionFileReader.ReadFile(invalidPath, modelDefinition, null, null);

                // Then
                var exception = Assert.Throws<FileReadingException>(action);
                Assert.AreEqual($"Could not read file {invalidPath} properly, it doesn't exist.", exception.Message);
            }
        }

        [Test]
        public void GivenFileWithNoCategories_WhenCallingReadFile_ThenThrowsException()
        {
            // Given
            var noCategoriesFile = TestHelper.GetTestFilePath(@"IO\noCategories.ini");

            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;

                // When
                TestDelegate action = () =>
                    ChannelInitialConditionDefinitionFileReader.ReadFile(noCategoriesFile, modelDefinition, null, null);

                // Then
                var exception = Assert.Throws<FileReadingException>(action);
                Assert.AreEqual($"Could not read file {noCategoriesFile} properly, it seems empty.", exception.Message);
            }
        }

        [Test]
        public void GivenFileWithMissingGlobalCategory_WhenCallingReadFile_ThenThrowsException()
        {
            // Given
            var missingGlobalCategoryFile = TestHelper.GetTestFilePath(@"IO\missingGlobalCategory.ini");

            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;

                // When
                TestDelegate action = () =>
                    ChannelInitialConditionDefinitionFileReader.ReadFile(missingGlobalCategoryFile, modelDefinition, null, null);

                // Then
                var exception = Assert.Throws<FileReadingException>(action);
                Assert.AreEqual($"Could not read file {missingGlobalCategoryFile} properly, no global property was found.", exception.Message);
            }
        }

        [Test]
        public void GivenFileWithOnlyInvalidCategories_WhenCallingReadFile_ThenThrowsException()
        {
            // Given
            var invalidCategoriesOnlyFile = TestHelper.GetTestFilePath(@"IO\invalidCategoriesOnly.ini");

            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;

                // When
                TestDelegate action = () =>
                    ChannelInitialConditionDefinitionFileReader.ReadFile(invalidCategoriesOnlyFile, modelDefinition, null, null);

                // Then
                var exception = Assert.Throws<FileReadingException>(action);
                Assert.AreEqual($"Could not read file {invalidCategoriesOnlyFile} properly, no global property was found.", exception.Message);
            }
        }

        [Test]
        public void GivenFileWithChannelThatDoesNotExistOnModel_WhenCallingReadFile_ThenThrowsException()
        {
            // Given
            var filePath = TestHelper.GetTestFilePath(@"IO\InitialWaterDepth_expected.ini");

            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;

                var branchDictionary = new Dictionary<string, IBranch>();

                // When
                TestDelegate action = () => ChannelInitialConditionDefinitionFileReader.ReadFile(filePath,
                    modelDefinition, branchDictionary, fmModel.ChannelInitialConditionDefinitions);

                // Then
                var exception = Assert.Throws<FileReadingException>(action, "");
                Assert.AreEqual("Branch (Channel0) where the initial condition should be put on is not available in the model.", exception.Message);
            }
        }

        [Test]
        [TestCase(InitialConditionQuantity.WaterLevel, 123, "InitialWaterLevel_expected.ini")]
        [TestCase(InitialConditionQuantity.WaterDepth, 456, "InitialWaterDepth_expected.ini")]
        public void GivenInitialConditionFile_WhenCallingReadFile_ThenCorrectlySetsModelGlobalProperties(
            InitialConditionQuantity expectedQuantity,
            double expectedValue,
            string filename)
        {
            // Given
            var filePath = TestHelper.GetTestFilePath($"IO\\{filename}");
            var tempFolder = FileUtils.CreateTempDirectory();

            try
            {
                var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
                using (var fmModel = new WaterFlowFMModel {MduFilePath = mduFilePath})
                {
                    var modelDefinition = fmModel.ModelDefinition;

                    // Fill the branchDictionary with branches listed in the initial conditions file
                    var branchDictionary = new Dictionary<string, IBranch>();
                    for (var i = 0; i < 5; i++)
                    {
                        var channelName = $"Channel{i}";
                        var channel = new Channel {Name = channelName};
                        fmModel.Network.Branches.Add(channel);
                        branchDictionary.Add(channelName, channel);
                    }

                    // When
                    ChannelInitialConditionDefinitionFileReader.ReadFile(filePath, modelDefinition, branchDictionary,
                        fmModel.ChannelInitialConditionDefinitions);

                    // Then
                    var actualGlobalValue = (double) modelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalValue1D).Value;
                    var actualGlobalQuantity = (InitialConditionQuantity) (int) modelDefinition
                        .GetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D).Value;

                    Assert.That(actualGlobalQuantity, Is.EqualTo(expectedQuantity));
                    Assert.That(actualGlobalValue, Is.EqualTo(expectedValue).Within(Epsilon));
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(tempFolder);
            }
        }
    }
}
