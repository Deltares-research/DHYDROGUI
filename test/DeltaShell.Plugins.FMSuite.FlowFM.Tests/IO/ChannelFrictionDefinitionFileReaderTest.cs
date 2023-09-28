using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Roughness;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;
using FlowFMResources = DeltaShell.Plugins.FMSuite.FlowFM.Properties.Resources;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class ChannelFrictionDefinitionFileReaderTest
    {
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
                    ChannelFrictionDefinitionFileReader.ReadFile(invalidPath, modelDefinition, null, null);

                // Then
                var exception = Assert.Throws<FileReadingException>(action);
                Assert.AreEqual($"Could not read file {invalidPath} properly, it doesn't exist.", exception.Message);
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
                    ChannelFrictionDefinitionFileReader.ReadFile(noIniSectionsFile, modelDefinition, null, null);

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
                    ChannelFrictionDefinitionFileReader.ReadFile(missingGlobalIniSectionFile, modelDefinition, null, null);

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
                    ChannelFrictionDefinitionFileReader.ReadFile(invalidIniSectionsOnlyFile, modelDefinition, null, null);

                // Then
                var exception = Assert.Throws<FileReadingException>(action);
                Assert.AreEqual($"Could not read file {invalidIniSectionsOnlyFile} properly, no global property was found.", exception.Message);
            }
        }

        [Test]
        public void GivenFileWithChannelThatDoesNotExistOnModel_WhenCallingReadFile_ThenThrowsException()
        {
            // Given
            var filePath = TestHelper.GetTestFilePath($"IO\\{FlowFMResources.Roughness_Main_Channels_Filename}");

            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;

                var network = new HydroNetwork();

                // When
                TestDelegate action = () => ChannelFrictionDefinitionFileReader.ReadFile(filePath,
                    modelDefinition, network, fmModel.ChannelFrictionDefinitions);

                // Then
                var exception = Assert.Throws<FileReadingException>(action, "");
                Assert.AreEqual("Branch (branch1) where the roughness should be put on is not available in the model.", exception.Message);
            }
        }

        [Test]
        public void GivenValidFile_WhenCallingReadFile_ThenCorrectlySetsModelGlobalProperties()
        {
            // Given
            var filePath = TestHelper.GetTestFilePath($"IO\\{FlowFMResources.Roughness_Main_Channels_Filename}");
            var tempFolder = FileUtils.CreateTempDirectory();

            try
            {
                var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
                using (var fmModel = new WaterFlowFMModel { MduFilePath = mduFilePath })
                {
                    var modelDefinition = fmModel.ModelDefinition;
                    fmModel.Network = HydroNetworkHelper.GetSnakeHydroNetwork(14);
                    
                    // When
                    ChannelFrictionDefinitionFileReader.ReadFile(filePath, modelDefinition, fmModel.Network,
                        fmModel.ChannelFrictionDefinitions);

                    // Then
                    var actualGlobalValue = (double) modelDefinition.GetModelProperty(GuiProperties.UnifFrictCoefChannels).Value;
                    var actualGlobalType = (RoughnessType) (int) modelDefinition.GetModelProperty(GuiProperties.UnifFrictTypeChannels).Value;

                    Assert.That(actualGlobalType, Is.EqualTo(RoughnessType.WallLawNikuradse));
                    Assert.That(actualGlobalValue, Is.EqualTo(1.2));
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(tempFolder);
            }
        }

        [Test]
        public void GivenValidFile_WhenCallingReadFile_ThenCorrectlySetsChannelFrictionDefinitions()
        {
            // Given
            var filePath = TestHelper.GetTestFilePath($"IO\\{FlowFMResources.Roughness_Main_Channels_Filename}");

            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;
                var channelFrictionDefinitions = fmModel.ChannelFrictionDefinitions;

                const int numberOfChannels = 14;
                fmModel.Network = HydroNetworkHelper.GetSnakeHydroNetwork(numberOfChannels);
                
                // Preconditions
                Assert.That(fmModel.ChannelFrictionDefinitions.Count, Is.EqualTo(numberOfChannels));

                var expectedChannelFrictionDefinitions = GetExpectedChannelFrictionDefinitions(fmModel.Network.Branches);

                // When
                ChannelFrictionDefinitionFileReader.ReadFile(filePath, modelDefinition, fmModel.Network, channelFrictionDefinitions);

                // Then
                CompareChannelFrictionDefinitions(expectedChannelFrictionDefinitions.ToList(), channelFrictionDefinitions.ToList());
            }
        }

        private static IEnumerable<ChannelFrictionDefinition> GetExpectedChannelFrictionDefinitions(IEnumerable<IBranch> branches)
        {
            var channelFrictionDefinitions = new List<ChannelFrictionDefinition>();

            var branchesList = branches.ToList();
            var branch1 = branchesList.FirstOrDefault(b => b.Name.Equals("branch1", StringComparison.InvariantCultureIgnoreCase));
            Assert.That(branch1, Is.Not.Null, "branch1 is not found in the network");
            Assert.That(branch1, Is.InstanceOf<Channel>(), "branch1 is not channel type");
            var cfd1 = new ChannelFrictionDefinition((Channel) branch1);
            cfd1.SpecificationType = ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition;
            cfd1.ConstantChannelFrictionDefinition.Value = 123;
            channelFrictionDefinitions.Add(cfd1);

            var branch2 = branchesList.FirstOrDefault(b => b.Name.Equals("branch2", StringComparison.InvariantCultureIgnoreCase));
            Assert.That(branch2, Is.Not.Null, "branch2 is not found in the network");
            Assert.That(branch2, Is.InstanceOf<Channel>(), "branch2 is not channel type");
            var cfd2 = new ChannelFrictionDefinition((Channel) branch2);
            cfd2.SpecificationType = ChannelFrictionSpecificationType.ModelSettings;
            channelFrictionDefinitions.Add(cfd2);

            var branch3 = branchesList.FirstOrDefault(b => b.Name.Equals("branch3", StringComparison.InvariantCultureIgnoreCase));
            Assert.That(branch3, Is.Not.Null, "branch3 is not found in the network");
            Assert.That(branch3, Is.InstanceOf<Channel>(), "branch3 is not channel type");
            var cfd3 = new ChannelFrictionDefinition((Channel) branch3);
            cfd3.SpecificationType = ChannelFrictionSpecificationType.ModelSettings;
            channelFrictionDefinitions.Add(cfd3);

            var branch4 = branchesList.FirstOrDefault(b => b.Name.Equals("branch4", StringComparison.InvariantCultureIgnoreCase));
            Assert.That(branch4, Is.Not.Null, "branch4 is not found in the network");
            Assert.That(branch4, Is.InstanceOf<Channel>(), "branch4 is not channel type");
            var cfd4 = new ChannelFrictionDefinition((Channel) branch4);
            cfd4.SpecificationType = ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition;
            cfd4.ConstantChannelFrictionDefinition.Value = 3;
            cfd4.ConstantChannelFrictionDefinition.Type = RoughnessType.DeBosBijkerk;
            channelFrictionDefinitions.Add(cfd4);

            var branch5 = branchesList.FirstOrDefault(b => b.Name.Equals("branch5", StringComparison.InvariantCultureIgnoreCase));
            Assert.That(branch5, Is.Not.Null, "branch5 is not found in the network");
            Assert.That(branch5, Is.InstanceOf<Channel>(), "branch5 is not channel type");
            var cfd5 = new ChannelFrictionDefinition((Channel) branch5);
            cfd5.SpecificationType = ChannelFrictionSpecificationType.ModelSettings;
            channelFrictionDefinitions.Add(cfd5);

            var branch6 = branchesList.FirstOrDefault(b => b.Name.Equals("branch6", StringComparison.InvariantCultureIgnoreCase));
            Assert.That(branch6, Is.Not.Null, "branch6 is not found in the network");
            Assert.That(branch6, Is.InstanceOf<Channel>(), "branch6 is not channel type");
            var cfd6 = new ChannelFrictionDefinition((Channel) branch6);
            cfd6.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd6.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.Constant;
            cfd6.SpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.Add(
                new ConstantSpatialChannelFrictionDefinition {Chainage = 0, Value = 111});
            cfd6.SpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.Add(
                new ConstantSpatialChannelFrictionDefinition {Chainage = 50.123, Value = 50123});
            channelFrictionDefinitions.Add(cfd6);

            var branch7 = branchesList.FirstOrDefault(b => b.Name.Equals("branch7", StringComparison.InvariantCultureIgnoreCase));
            Assert.That(branch7, Is.Not.Null, "branch7 is not found in the network");
            Assert.That(branch7, Is.InstanceOf<Channel>(), "branch7 is not channel type");
            var cfd7 = new ChannelFrictionDefinition((Channel) branch7);
            cfd7.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd7.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfH;
            cfd7.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] { 1, 2 }); // chainage
            cfd7.SpatialChannelFrictionDefinition.Function.Arguments[1].SetValues(new double[] { 3, 4 }); // H or Q value
            cfd7.SpatialChannelFrictionDefinition.Function.Components[0].SetValues(new double[] { 5, 6, 7, 8 }); // friction value
            channelFrictionDefinitions.Add(cfd7);

            var branch8 = branchesList.FirstOrDefault(b => b.Name.Equals("branch8", StringComparison.InvariantCultureIgnoreCase));
            Assert.That(branch8, Is.Not.Null, "branch8 is not found in the network");
            Assert.That(branch8, Is.InstanceOf<Channel>(), "branch8 is not channel type");
            var cfd8 = new ChannelFrictionDefinition((Channel) branch8);
            cfd8.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd8.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfQ;
            cfd8.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] {1, 2, 3});
            cfd8.SpatialChannelFrictionDefinition.Function.Arguments[1].SetValues(new double[] {4, 5});
            cfd8.SpatialChannelFrictionDefinition.Function.Components[0]
                .SetValues(new double[] {6, 7, 8, 9, 10, 11});
            channelFrictionDefinitions.Add(cfd8);

            var branch9 = branchesList.FirstOrDefault(b => b.Name.Equals("branch9", StringComparison.InvariantCultureIgnoreCase));
            Assert.That(branch9, Is.Not.Null, "branch9 is not found in the network");
            Assert.That(branch9, Is.InstanceOf<Channel>(), "branch9 is not channel type");
            var cfd9 = new ChannelFrictionDefinition((Channel) branch9);
            cfd9.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd9.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfH;
            cfd9.SpatialChannelFrictionDefinition.Type = RoughnessType.StricklerNikuradse;
            cfd9.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] {1});
            cfd9.SpatialChannelFrictionDefinition.Function.Arguments[1].SetValues(new double[] {2, 3, 4, 5, 6});
            cfd9.SpatialChannelFrictionDefinition.Function.Components[0].SetValues(new double[] {7, 8, 9, 10, 11});
            channelFrictionDefinitions.Add(cfd9);

            var branch10 = branchesList.FirstOrDefault(b => b.Name.Equals("branch10", StringComparison.InvariantCultureIgnoreCase));
            Assert.That(branch10, Is.Not.Null, "branch10 is not found in the network");
            Assert.That(branch10, Is.InstanceOf<Channel>(), "branch10 is not channel type");
            var cfd10 = new ChannelFrictionDefinition((Channel) branch10);
            cfd10.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd10.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfH;
            cfd10.SpatialChannelFrictionDefinition.Type = RoughnessType.Strickler;
            cfd10.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] {1, 2, 3, 4, 5});
            cfd10.SpatialChannelFrictionDefinition.Function.Arguments[1].SetValues(new double[] {6});
            cfd10.SpatialChannelFrictionDefinition.Function.Components[0].SetValues(new double[] {7, 8, 9, 10, 11});
            channelFrictionDefinitions.Add(cfd10);

            var branch11 = branchesList.FirstOrDefault(b => b.Name.Equals("branch11", StringComparison.InvariantCultureIgnoreCase));
            Assert.That(branch11, Is.Not.Null, "branch11 is not found in the network");
            Assert.That(branch11, Is.InstanceOf<Channel>(), "branch11 is not channel type");
            var cfd11 = new ChannelFrictionDefinition((Channel) branch11);
            cfd11.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd11.SpatialChannelFrictionDefinition.Type = RoughnessType.WallLawNikuradse;
            cfd11.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.Constant;
            cfd11.SpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.Add(
                new ConstantSpatialChannelFrictionDefinition {Chainage = 0, Value = 111});
            channelFrictionDefinitions.Add(cfd11);

            var branch12 = branchesList.FirstOrDefault(b => b.Name.Equals("branch12", StringComparison.InvariantCultureIgnoreCase));
            Assert.That(branch12, Is.Not.Null, "branch12 is not found in the network");
            Assert.That(branch12, Is.InstanceOf<Channel>(), "branch12 is not channel type");
            var cfd12 = new ChannelFrictionDefinition((Channel) branch12);
            cfd12.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd12.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfH;
            cfd12.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] { 1 }); // no H value and no friction value is set
            channelFrictionDefinitions.Add(cfd12);

            var branch13 = branchesList.FirstOrDefault(b => b.Name.Equals("branch13", StringComparison.InvariantCultureIgnoreCase));
            Assert.That(branch13, Is.Not.Null, "branch13 is not found in the network");
            Assert.That(branch13, Is.InstanceOf<Channel>(), "branch13 is not channel type");
            var cfd13 = new ChannelFrictionDefinition((Channel) branch13);
            cfd13.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd13.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfQ;
            cfd13.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] { 1, 2 }); // no H value and no friction value is set
            channelFrictionDefinitions.Add(cfd13);

            var branch14 = branchesList.FirstOrDefault(b => b.Name.Equals("branch14", StringComparison.InvariantCultureIgnoreCase));
            Assert.That(branch14, Is.Not.Null, "branch14 is not found in the network");
            Assert.That(branch14, Is.InstanceOf<Channel>(), "branch14 is not channel type");
            var cfd14 = new ChannelFrictionDefinition((Channel) branch14);
            cfd14.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd14.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.Constant; // no ConstantSpatialChannelFrictionDefinitions added to list
            channelFrictionDefinitions.Add(cfd14);

            return channelFrictionDefinitions;
        }

        private static void CompareChannelFrictionDefinitions(
            IReadOnlyCollection<ChannelFrictionDefinition> expectedChannelFrictionDefinitions,
            IReadOnlyCollection<ChannelFrictionDefinition> actualChannelFrictionDefinitions)
        {
            Assert.That(actualChannelFrictionDefinitions.Count, Is.EqualTo(expectedChannelFrictionDefinitions.Count));

            var serializer = new JavaScriptSerializer();

            foreach (var expectedChannelFrictionDefinition in expectedChannelFrictionDefinitions)
            {
                var branchName = expectedChannelFrictionDefinition.Channel.Name;
                var actualChannelFrictionDefinition = actualChannelFrictionDefinitions.FirstOrDefault(cfd => cfd.Channel.Name.Equals(branchName));

                Assert.That(actualChannelFrictionDefinition, Is.Not.Null);
                Assert.That(actualChannelFrictionDefinition.SpecificationType, Is.EqualTo(expectedChannelFrictionDefinition.SpecificationType));
                Assert.That(serializer.Serialize(actualChannelFrictionDefinition.ConstantChannelFrictionDefinition), Is.EqualTo(serializer.Serialize(expectedChannelFrictionDefinition.ConstantChannelFrictionDefinition)));

                var expectedSpatialChannelFrictionDefinition = expectedChannelFrictionDefinition.SpatialChannelFrictionDefinition;
                if (expectedSpatialChannelFrictionDefinition != null)
                {
                    var actualSpatialChannelFrictionDefinition = actualChannelFrictionDefinition.SpatialChannelFrictionDefinition;

                    Assert.That(actualSpatialChannelFrictionDefinition.Type, Is.EqualTo(expectedSpatialChannelFrictionDefinition.Type));
                    Assert.That(actualSpatialChannelFrictionDefinition.FunctionType, Is.EqualTo(expectedSpatialChannelFrictionDefinition.FunctionType));
                    Assert.That(serializer.Serialize(actualSpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions), Is.EqualTo(serializer.Serialize(expectedSpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions)));

                    if (expectedSpatialChannelFrictionDefinition.Function != null)
                    {
                        Assert.That(actualSpatialChannelFrictionDefinition.Function.GetAllComponentValues(), Is.EqualTo(expectedSpatialChannelFrictionDefinition.Function.GetAllComponentValues()));
                    }
                }
            }
        }
    }
}