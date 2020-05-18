using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.TestUtils;
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
            string invalidPath = "invalidPath";
            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;

                TestDelegate action = () =>
                    ChannelInitialConditionDefinitionFileReader.ReadFile(invalidPath, modelDefinition, null, null);

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
                    ChannelInitialConditionDefinitionFileReader.ReadFile(noCategoriesFile, modelDefinition, null, null);

                Assert.Throws<FileReadingException>(action);
            }
        }

        [Test]
        public void GivenFileWithMissingGlobalCategory_WhenCallingReadFile_ThenThrowsException()
        {
            var noCategoriesFile = TestHelper.GetTestFilePath(@"IO\missingGlobalCategory.ini");
            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;

                TestDelegate action = () =>
                    ChannelInitialConditionDefinitionFileReader.ReadFile(noCategoriesFile, modelDefinition, null, null);

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
                    ChannelInitialConditionDefinitionFileReader.ReadFile(noCategoriesFile, modelDefinition, null, null);

                Assert.Throws<FileReadingException>(action);
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
            var filePath = TestHelper.GetTestFilePath($"IO\\{filename}");

            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;

                // Fill the branchDictionary with branches listed in the initial conditions file
                var branchDictionary = new Dictionary<string, IBranch>();
                var channelCount = 5;
                for (int i = 0; i < channelCount; i++)
                {
                    var channelName = $"Channel{i}";
                    var channel = new Channel() {Name = channelName};
                    fmModel.Network.Branches.Add(channel);
                    branchDictionary.Add(channelName, channel);
                }

                ChannelInitialConditionDefinitionFileReader.ReadFile(filePath, modelDefinition, branchDictionary,
                    fmModel.ChannelInitialConditionDefinitions);

                var actualGlobalValue =
                    (double) modelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalValue1D).Value;
                var actualGlobalQuantity = (InitialConditionQuantity) (int) modelDefinition
                    .GetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D).Value;

                Assert.That(actualGlobalQuantity, Is.EqualTo(expectedQuantity));
                Assert.That(actualGlobalValue, Is.EqualTo(expectedValue).Within(Epsilon));


            }
        }

        [Test]
        public void GivenInitialConditionQuantityFileWithBranchThatDoesNotExistOnModel_WhenCallingReadFile_ThenThrowsException()
        {
            var filePath = TestHelper.GetTestFilePath(@"IO\InitialWaterDepth_expected.ini");

            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;

                var branchDictionary = new Dictionary<string, IBranch>();

                TestDelegate action = () => ChannelInitialConditionDefinitionFileReader.ReadFile(filePath,
                    modelDefinition, branchDictionary, fmModel.ChannelInitialConditionDefinitions);

                Assert.Throws<FileReadingException>(action);
            }
        }
    }
}
