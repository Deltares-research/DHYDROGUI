using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
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
        public void GivenValidFile_WhenCallingReadFile_ThenCorrectlySetsModelGlobalProperties(
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

                    var branchDictionary = new Dictionary<string, IBranch>();
                    for (var i = 0; i < 5; i++)
                    {
                        var channelName = $"Channel{i}";
                        var channel = new Channel {Name = channelName};
                        fmModel.Network.Branches.Add(channel);
                        branchDictionary.Add(channelName, channel);
                    }

                    // When
                    ChannelInitialConditionDefinitionFileReader.ReadFile(filePath, modelDefinition, branchDictionary, fmModel.ChannelInitialConditionDefinitions);

                    // Then
                    var actualGlobalValue = (double) modelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalValue1D).Value;
                    var actualGlobalQuantity = (InitialConditionQuantity) (int) modelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D).Value;

                    Assert.That(actualGlobalQuantity, Is.EqualTo(expectedQuantity));
                    Assert.That(actualGlobalValue, Is.EqualTo(expectedValue).Within(Epsilon));
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(tempFolder);
            }
        }

        [Test]
        [TestCase(InitialConditionQuantity.WaterLevel, "InitialWaterLevel_expected.ini")]
        [TestCase(InitialConditionQuantity.WaterDepth, "InitialWaterDepth_expected.ini")]
        public void GivenValidFile_WhenCallingReadFile_ThenCorrectlySetsChannelInitialConditionDefinitions(
            InitialConditionQuantity expectedQuantity,
            string filename)
        {
            // Given
            var filePath = TestHelper.GetTestFilePath($"IO\\{filename}");

            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;
                var network = fmModel.Network;
                var channelInitialConditionDefinitions = fmModel.ChannelInitialConditionDefinitions;

                const int numberOfChannels = 5;

                var branchDictionary = new Dictionary<string, IBranch>();
                for (var i = 0; i < numberOfChannels; i++)
                {
                    var channelName = $"Channel{i}";
                    var channel = new Channel { Name = channelName };
                    fmModel.Network.Branches.Add(channel);
                    branchDictionary.Add(channelName, channel);
                }

                // Preconditions
                Assert.That(fmModel.ChannelInitialConditionDefinitions.Count, Is.EqualTo(numberOfChannels));

                var expectedChannelInitialConditionDefinitions = GetExpectedChannelInitialConditionDefinitions(network.Branches, expectedQuantity);

                // When
                ChannelInitialConditionDefinitionFileReader.ReadFile(filePath, modelDefinition, branchDictionary, channelInitialConditionDefinitions);

                // Then
                CompareChannelInitialConditionDefinitions(expectedChannelInitialConditionDefinitions.ToList(), channelInitialConditionDefinitions.ToList());
            }
        }

        private static IEnumerable<ChannelInitialConditionDefinition> GetExpectedChannelInitialConditionDefinitions(IEnumerable<IBranch> branches, InitialConditionQuantity expectedQuantity)
        {
            var channelInitialConditionDefinitions = new List<ChannelInitialConditionDefinition>();

            var branchesList = branches.ToList();
            var branch0 = branchesList.First(b => b.Name.Equals("Channel0"));
            var cfd0 = new ChannelInitialConditionDefinition((Channel) branch0);
            cfd0.SpecificationType = ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition;
            cfd0.ConstantChannelInitialConditionDefinition.Quantity = expectedQuantity;
            cfd0.ConstantChannelInitialConditionDefinition.Value = 123;
            channelInitialConditionDefinitions.Add(cfd0);

            var branch1 = branchesList.First(b => b.Name.Equals("Channel1"));
            var cfd1 = new ChannelInitialConditionDefinition((Channel) branch1);
            cfd1.SpecificationType = ChannelInitialConditionSpecificationType.ModelSettings;
            channelInitialConditionDefinitions.Add(cfd1);

            var branch2 = branchesList.First(b => b.Name.Equals("Channel2"));
            var cfd2 = new ChannelInitialConditionDefinition((Channel) branch2);
            cfd2.SpecificationType = ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition;
            cfd2.SpatialChannelInitialConditionDefinition.Quantity = expectedQuantity;
            cfd2.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(
                new ConstantSpatialChannelInitialConditionDefinition
                {
                    Chainage = 0.0,
                    Value = 11.0
                });
            cfd2.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(
                new ConstantSpatialChannelInitialConditionDefinition
                {
                    Chainage = 2.33,
                    Value = 12.22
                });
            cfd2.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(
                new ConstantSpatialChannelInitialConditionDefinition
                {
                    Chainage = 1.0,
                    Value = 4.0
                });
            channelInitialConditionDefinitions.Add(cfd2);

            var branch3 = branchesList.First(b => b.Name.Equals("Channel3"));
            var cfd3 = new ChannelInitialConditionDefinition((Channel) branch3);
            cfd3.SpecificationType = ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition;
            cfd3.SpatialChannelInitialConditionDefinition.Quantity = expectedQuantity;
            cfd3.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(
                new ConstantSpatialChannelInitialConditionDefinition
                {
                    Chainage = 88.123,
                    Value = 99.98765
                });
            channelInitialConditionDefinitions.Add(cfd3);

            var branch4 = branchesList.First(b => b.Name.Equals("Channel4"));
            var cfd4 = new ChannelInitialConditionDefinition(branch4 as Channel);
            cfd4.SpecificationType = ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition;
            cfd4.SpatialChannelInitialConditionDefinition.Quantity = expectedQuantity;
            channelInitialConditionDefinitions.Add(cfd4);

            return channelInitialConditionDefinitions;
        }

        private static void CompareChannelInitialConditionDefinitions(
            IReadOnlyCollection<ChannelInitialConditionDefinition> expectedChannelInitialConditionDefinitions,
            IReadOnlyCollection<ChannelInitialConditionDefinition> actualChannelInitialConditionDefinitions)
        {
            Assert.That(actualChannelInitialConditionDefinitions.Count, Is.EqualTo(expectedChannelInitialConditionDefinitions.Count));

            var serializer = new JavaScriptSerializer();

            foreach (var expectedChannelInitialConditionDefinition in expectedChannelInitialConditionDefinitions)
            {
                var branchName = expectedChannelInitialConditionDefinition.Channel.Name;
                var actualChannelInitialConditionDefinition = actualChannelInitialConditionDefinitions.FirstOrDefault(cfd => cfd.Channel.Name.Equals(branchName));

                Assert.That(actualChannelInitialConditionDefinition, Is.Not.Null);
                Assert.That(actualChannelInitialConditionDefinition.SpecificationType, Is.EqualTo(expectedChannelInitialConditionDefinition.SpecificationType));
                Assert.That(serializer.Serialize(actualChannelInitialConditionDefinition.ConstantChannelInitialConditionDefinition), Is.EqualTo(serializer.Serialize(expectedChannelInitialConditionDefinition.ConstantChannelInitialConditionDefinition)));

                var expectedSpatialChannelInitialConditionDefinition = expectedChannelInitialConditionDefinition.SpatialChannelInitialConditionDefinition;
                if (expectedSpatialChannelInitialConditionDefinition != null)
                {
                    var actualSpatialChannelInitialConditionDefinition = actualChannelInitialConditionDefinition.SpatialChannelInitialConditionDefinition;

                    Assert.That(actualSpatialChannelInitialConditionDefinition.Quantity, Is.EqualTo(expectedSpatialChannelInitialConditionDefinition.Quantity));
                    Assert.That(serializer.Serialize(actualSpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions),
                        Is.EqualTo(serializer.Serialize(expectedSpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions)));
                }
            }
        }
    }
}
