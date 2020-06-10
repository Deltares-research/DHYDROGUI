using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using DelftTools.Functions;
using DelftTools.Hydro;
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
        public void GivenFileWithNoCategories_WhenCallingReadFile_ThenThrowsException()
        {
            // Given
            var noCategoriesFile = TestHelper.GetTestFilePath(@"IO\noCategories.ini");

            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;

                // When
                TestDelegate action = () =>
                    ChannelFrictionDefinitionFileReader.ReadFile(noCategoriesFile, modelDefinition, null, null);

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
                    ChannelFrictionDefinitionFileReader.ReadFile(missingGlobalCategoryFile, modelDefinition, null, null);

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
                    ChannelFrictionDefinitionFileReader.ReadFile(invalidCategoriesOnlyFile, modelDefinition, null, null);

                // Then
                var exception = Assert.Throws<FileReadingException>(action);
                Assert.AreEqual($"Could not read file {invalidCategoriesOnlyFile} properly, no global property was found.", exception.Message);
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
                Assert.AreEqual("Branch (Channel0) where the roughness should be put on is not available in the model.", exception.Message);
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

                    for (var i = 0; i < 14; i++)
                    {
                        fmModel.Network.Branches.Add(new Channel { Name = $"Channel{i}" });
                    }

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
                var network = fmModel.Network;
                var channelFrictionDefinitions = fmModel.ChannelFrictionDefinitions;

                const int numberOfChannels = 14;
                
                for (var i = 0; i < numberOfChannels; i++)
                {
                    network.Branches.Add(new Channel { Name = $"Channel{i}" });
                }

                // Preconditions
                Assert.That(fmModel.ChannelFrictionDefinitions.Count, Is.EqualTo(numberOfChannels));

                var expectedChannelFrictionDefinitions = GetExpectedChannelFrictionDefinitions(network.Branches);

                // When
                ChannelFrictionDefinitionFileReader.ReadFile(filePath, modelDefinition, network, channelFrictionDefinitions);

                // Then
                CompareChannelFrictionDefinitions(expectedChannelFrictionDefinitions.ToList(), channelFrictionDefinitions.ToList());
            }
        }

        private static IEnumerable<ChannelFrictionDefinition> GetExpectedChannelFrictionDefinitions(IEnumerable<IBranch> branches)
        {
            var channelFrictionDefinitions = new List<ChannelFrictionDefinition>();

            var branchesList = branches.ToList();
            var branch0 = branchesList.First(b => b.Name.Equals("Channel0"));
            var cfd0 = new ChannelFrictionDefinition((Channel) branch0);
            cfd0.SpecificationType = ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition;
            cfd0.ConstantChannelFrictionDefinition.Value = 123;
            channelFrictionDefinitions.Add(cfd0);

            var branch1 = branchesList.First(b => b.Name.Equals("Channel1"));
            var cfd1 = new ChannelFrictionDefinition((Channel) branch1);
            cfd1.SpecificationType = ChannelFrictionSpecificationType.ModelSettings;
            channelFrictionDefinitions.Add(cfd1);

            var branch2 = branchesList.First(b => b.Name.Equals("Channel2"));
            var cfd2 = new ChannelFrictionDefinition((Channel) branch2);
            cfd2.SpecificationType = ChannelFrictionSpecificationType.ModelSettings;
            channelFrictionDefinitions.Add(cfd2);

            var branch3 = branchesList.First(b => b.Name.Equals("Channel3"));
            var cfd3 = new ChannelFrictionDefinition((Channel) branch3);
            cfd3.SpecificationType = ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition;
            cfd3.ConstantChannelFrictionDefinition.Value = 3;
            cfd3.ConstantChannelFrictionDefinition.Type = RoughnessType.DeBosBijkerk;
            channelFrictionDefinitions.Add(cfd3);

            var branch4 = branchesList.First(b => b.Name.Equals("Channel4"));
            var cfd4 = new ChannelFrictionDefinition((Channel) branch4);
            cfd4.SpecificationType = ChannelFrictionSpecificationType.ModelSettings;
            channelFrictionDefinitions.Add(cfd4);

            var branch5 = branchesList.First(b => b.Name.Equals("Channel5"));
            var cfd5 = new ChannelFrictionDefinition((Channel) branch5);
            cfd5.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd5.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.Constant;
            cfd5.SpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.Add(
                new ConstantSpatialChannelFrictionDefinition {Chainage = 0, Value = 111});
            cfd5.SpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.Add(
                new ConstantSpatialChannelFrictionDefinition {Chainage = 50.123, Value = 50123});
            channelFrictionDefinitions.Add(cfd5);

            var branch6 = branchesList.First(b => b.Name.Equals("Channel6"));
            var cfd6 = new ChannelFrictionDefinition((Channel) branch6);
            cfd6.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd6.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfH;
            cfd6.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] { 1, 2 }); // chainage
            cfd6.SpatialChannelFrictionDefinition.Function.Arguments[1].SetValues(new double[] { 3, 4 }); // H or Q value
            cfd6.SpatialChannelFrictionDefinition.Function.Components[0].SetValues(new double[] { 5, 6, 7, 8 }); // friction value
            channelFrictionDefinitions.Add(cfd6);

            var branch7 = branchesList.First(b => b.Name.Equals("Channel7"));
            var cfd7 = new ChannelFrictionDefinition((Channel) branch7);
            cfd7.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd7.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfQ;
            cfd7.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] {1, 2, 3});
            cfd7.SpatialChannelFrictionDefinition.Function.Arguments[1].SetValues(new double[] {4, 5});
            cfd7.SpatialChannelFrictionDefinition.Function.Components[0]
                .SetValues(new double[] {6, 7, 8, 9, 10, 11});
            channelFrictionDefinitions.Add(cfd7);

            var branch8 = branchesList.First(b => b.Name.Equals("Channel8"));
            var cfd8 = new ChannelFrictionDefinition((Channel) branch8);
            cfd8.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd8.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfH;
            cfd8.SpatialChannelFrictionDefinition.Type = RoughnessType.StricklerNikuradse;
            cfd8.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] {1});
            cfd8.SpatialChannelFrictionDefinition.Function.Arguments[1].SetValues(new double[] {2, 3, 4, 5, 6});
            cfd8.SpatialChannelFrictionDefinition.Function.Components[0].SetValues(new double[] {7, 8, 9, 10, 11});
            channelFrictionDefinitions.Add(cfd8);

            var branch9 = branchesList.First(b => b.Name.Equals("Channel9"));
            var cfd9 = new ChannelFrictionDefinition((Channel) branch9);
            cfd9.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd9.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfH;
            cfd9.SpatialChannelFrictionDefinition.Type = RoughnessType.Strickler;
            cfd9.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] {1, 2, 3, 4, 5});
            cfd9.SpatialChannelFrictionDefinition.Function.Arguments[1].SetValues(new double[] {6});
            cfd9.SpatialChannelFrictionDefinition.Function.Components[0].SetValues(new double[] {7, 8, 9, 10, 11});
            channelFrictionDefinitions.Add(cfd9);

            var branch10 = branchesList.First(b => b.Name.Equals("Channel10"));
            var cfd10 = new ChannelFrictionDefinition((Channel) branch10);
            cfd10.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd10.SpatialChannelFrictionDefinition.Type = RoughnessType.WallLawNikuradse;
            cfd10.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.Constant;
            cfd10.SpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.Add(
                new ConstantSpatialChannelFrictionDefinition {Chainage = 0, Value = 111});
            channelFrictionDefinitions.Add(cfd10);

            var branch11 = branchesList.First(b => b.Name.Equals("Channel11"));
            var cfd11 = new ChannelFrictionDefinition((Channel) branch11);
            cfd11.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd11.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfH;
            cfd11.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] { 1 }); // no H value and no friction value is set
            channelFrictionDefinitions.Add(cfd11);

            var branch12 = branchesList.First(b => b.Name.Equals("Channel12"));
            var cfd12 = new ChannelFrictionDefinition((Channel) branch12);
            cfd12.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd12.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfQ;
            cfd12.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] { 1, 2 }); // no H value and no friction value is set
            channelFrictionDefinitions.Add(cfd12);

            var branch13 = branchesList.First(b => b.Name.Equals("Channel13"));
            var cfd13 = new ChannelFrictionDefinition((Channel) branch13);
            cfd13.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd13.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.Constant; // no ConstantSpatialChannelFrictionDefinitions added to list
            channelFrictionDefinitions.Add(cfd13);

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