using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
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
                Action action = () =>
                    ChannelInitialConditionDefinitionFileReader.ReadFile(invalidPath, modelDefinition, null, null);

                // Then
                TestHelper.AssertAtLeastOneLogMessagesContains(action, string.Format(Properties.Resources.FeatureFile1D2DReader_ReadInitialConditionFiles_No_Initial_Quantity_ini_file_found_, invalidPath));
            }
        }

        [Test]
        public void GivenFileWithNoIniSections_WhenCallingReadFile_ThenThrowsException()
        {
            // Given
            var noIniSectionsFile = TestHelper.GetTestFilePath(@"IO\noSections.ini");

            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;

                // When
                TestDelegate action = () =>
                    ChannelInitialConditionDefinitionFileReader.ReadFile(noIniSectionsFile, modelDefinition, null, null);

                // Then
                var exception = Assert.Throws<FileReadingException>(action);
                Assert.AreEqual($"Could not read file {noIniSectionsFile} properly, it seems empty.", exception.Message);
            }
        }

        [Test]
        public void GivenFileWithMissingGlobalIniSection_WhenCallingReadFile_ThenThrowsException()
        {
            // Given
            var missingGlobalIniSectionFile = TestHelper.GetTestFilePath(@"IO\missingGlobalSection.ini");

            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;

                // When
                TestDelegate action = () =>
                    ChannelInitialConditionDefinitionFileReader.ReadFile(missingGlobalIniSectionFile, modelDefinition, null, null);

                // Then
                var exception = Assert.Throws<FileReadingException>(action);
                Assert.AreEqual($"Could not read file {missingGlobalIniSectionFile} properly, no global property was found.", exception.Message);
            }
        }

        [Test]
        public void GivenFileWithOnlyInvalidIniSections_WhenCallingReadFile_ThenThrowsException()
        {
            // Given
            var invalidIniSectionsOnlyFile = TestHelper.GetTestFilePath(@"IO\invalidSectionsOnly.ini");

            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;

                // When
                TestDelegate action = () =>
                    ChannelInitialConditionDefinitionFileReader.ReadFile(invalidIniSectionsOnlyFile, modelDefinition, null, null);

                // Then
                var exception = Assert.Throws<FileReadingException>(action);
                Assert.AreEqual($"Could not read file {invalidIniSectionsOnlyFile} properly, no global property was found.", exception.Message);
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
                Assert.AreEqual("Branch (branch1) where the initial condition should be put on is not available in the model.", exception.Message);
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

                    fmModel.Network = HydroNetworkHelper.GetSnakeHydroNetwork(5);
                    var branchDictionary = fmModel.Network.Branches.ToDictionary(b => b.Name);
                    
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
                
                var channelInitialConditionDefinitions = fmModel.ChannelInitialConditionDefinitions;

                const int numberOfChannels = 5;
                fmModel.Network = HydroNetworkHelper.GetSnakeHydroNetwork(numberOfChannels);
                var branchDictionary = fmModel.Network.Branches.ToDictionary(b => b.Name);
                // Preconditions
                Assert.That(fmModel.ChannelInitialConditionDefinitions.Count, Is.EqualTo(numberOfChannels));

                var expectedChannelInitialConditionDefinitions = GetExpectedChannelInitialConditionDefinitions(fmModel.Network.Branches, expectedQuantity);

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
            var branch1 = branchesList.FirstOrDefault(b => b.Name.Equals("branch1", StringComparison.InvariantCultureIgnoreCase));
            Assert.That(branch1, Is.Not.Null, "branch1 is not in the network");
            Assert.That(branch1, Is.InstanceOf<Channel>(), "branch1 is not of type Channel");
            var cfd1 = new ChannelInitialConditionDefinition((Channel) branch1);
            cfd1.SpecificationType = ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition;
            cfd1.ConstantChannelInitialConditionDefinition.Quantity = expectedQuantity;
            cfd1.ConstantChannelInitialConditionDefinition.Value = 789;
            channelInitialConditionDefinitions.Add(cfd1);

            var branch2 = branchesList.FirstOrDefault(b => b.Name.Equals("branch2", StringComparison.InvariantCultureIgnoreCase));
            Assert.That(branch2, Is.Not.Null, "branch2 is not in the network");
            Assert.That(branch2, Is.InstanceOf<Channel>(), "branch2 is not of type Channel");
            var cfd2 = new ChannelInitialConditionDefinition((Channel) branch2);
            cfd2.SpecificationType = ChannelInitialConditionSpecificationType.ModelSettings;
            channelInitialConditionDefinitions.Add(cfd2);

            var branch3 = branchesList.FirstOrDefault(b => b.Name.Equals("branch3", StringComparison.InvariantCultureIgnoreCase));
            Assert.That(branch3, Is.Not.Null, "branch3 is not in the network");
            Assert.That(branch3, Is.InstanceOf<Channel>(), "branch3 is not of type Channel");
            var cfd3 = new ChannelInitialConditionDefinition((Channel) branch3);
            cfd3.SpecificationType = ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition;
            cfd3.SpatialChannelInitialConditionDefinition.Quantity = expectedQuantity;
            cfd3.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(
                new ConstantSpatialChannelInitialConditionDefinition
                {
                    Chainage = 0.0,
                    Value = 11.0
                });
            cfd3.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(
                new ConstantSpatialChannelInitialConditionDefinition
                {
                    Chainage = 2.33,
                    Value = 12.22
                });
            cfd3.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(
                new ConstantSpatialChannelInitialConditionDefinition
                {
                    Chainage = 1.0,
                    Value = 4.0
                });
            channelInitialConditionDefinitions.Add(cfd3);

            var branch4 = branchesList.FirstOrDefault(b => b.Name.Equals("branch4", StringComparison.InvariantCultureIgnoreCase));
            Assert.That(branch4, Is.Not.Null, "branch4 is not in the network");
            Assert.That(branch4, Is.InstanceOf<Channel>(), "branch4 is not of type Channel");
            var cfd4 = new ChannelInitialConditionDefinition((Channel) branch4);
            cfd4.SpecificationType = ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition;
            cfd4.SpatialChannelInitialConditionDefinition.Quantity = expectedQuantity;
            cfd4.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(
                new ConstantSpatialChannelInitialConditionDefinition
                {
                    Chainage = 88.123,
                    Value = 99.98765
                });
            channelInitialConditionDefinitions.Add(cfd4);

            var branch5 = branchesList.FirstOrDefault(b => b.Name.Equals("branch5", StringComparison.InvariantCultureIgnoreCase));
            Assert.That(branch5, Is.Not.Null, "branch5 is not in the network");
            Assert.That(branch5, Is.InstanceOf<Channel>(), "branch5 is not of type Channel");
            var cfd5 = new ChannelInitialConditionDefinition(branch5 as Channel);
            cfd5.SpecificationType = ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition;
            cfd5.SpatialChannelInitialConditionDefinition.Quantity = expectedQuantity;
            channelInitialConditionDefinitions.Add(cfd5);

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
