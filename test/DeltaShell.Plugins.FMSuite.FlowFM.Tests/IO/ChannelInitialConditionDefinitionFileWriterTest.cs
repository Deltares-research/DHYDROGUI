using System;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class ChannelInitialConditionDefinitionFileWriterTest
    {
        [Test]
        [TestCase(InitialConditionQuantity.WaterLevel, 123)]
        [TestCase(InitialConditionQuantity.WaterDepth, 456)]
        public void GivenCollectionOfChannelInitialConditionDefinitions_WhenWritingToFile_ThenIsSameAsExpectedFile(
            InitialConditionQuantity globalQuantity,
            double globalValue)
        {
            var expectedQuantityFile = TestHelper.GetTestFilePath($"IO\\Initial{globalQuantity}_expected.ini");
            var expectedFieldsFile = TestHelper.GetTestFilePath($"IO\\initialFields{globalQuantity}_expected.ini");
            var tempFolder = FileUtils.CreateTempDirectory();
            var actualQuantityFile = Path.Combine(tempFolder, $"Initial{globalQuantity}.ini");
            var actualFieldsFile = Path.Combine(tempFolder, "initialFields.ini");

            try
            {
                var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
                using (var fmModel = new WaterFlowFMModel() {MduFilePath = mduFilePath})
                {
                    SetGlobalQuantity(fmModel.ModelDefinition, globalQuantity);
                    SetGlobalValue(fmModel.ModelDefinition, globalValue);

                    var network = fmModel.Network;
                    var numberOfBranches = 5;
                    for (int i = 0; i < numberOfBranches; i++)
                    {
                        var channel = new Channel() { Name = $"Channel{i}" };
                        network.Branches.Add(channel);
                    }
                    Assert.That(network.Branches.Count, Is.EqualTo(numberOfBranches));
                    Assert.That(fmModel.ChannelInitialConditionDefinitions.Count, Is.EqualTo(numberOfBranches));

                    IEventedList<ChannelInitialConditionDefinition> channelInitialConditionDefinitions =
                        fmModel.ChannelInitialConditionDefinitions;

                    EditChannelInitialConditionDefinitions(channelInitialConditionDefinitions,globalQuantity, globalValue);

                    // Call
                    FeatureFile1D2DWriter.Write1D2DFeatures(mduFilePath, fmModel.ModelDefinition,
                        network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, channelInitialConditionDefinitions);

                    // Assert
                    Assert.That(File.Exists(actualFieldsFile), Is.True);
                    Assert.That(File.Exists(actualQuantityFile), Is.True);
                    FileAssert.AreEqual(actualFieldsFile, expectedFieldsFile);
                    FileAssert.AreEqual(actualQuantityFile, expectedQuantityFile);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(tempFolder);
            }
        }

        // [Test]
        // [Category("Build.Integration")]
        // //todo: Make sure the original quantity file is deleted when a new one is created
        // public void GivenASavedModel_WhenSavingWithADifferentGlobalInitialConditionQuantity_ThenOriginalQuantityFileIsDeleted()
        // {
        //     var tempFolder = FileUtils.CreateTempDirectory();
        //     try
        //     {
        //         var initialConditionWaterLevelFile = Path.Combine(tempFolder, "InitialWaterLevel.ini");
        //         var initialConditionWaterDepthFile = Path.Combine(tempFolder, "InitialWaterDepth.ini");
        //
        //         var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
        //
        //         using (var fmModel = new WaterFlowFMModel() {MduFilePath = mduFilePath})
        //         {
        //             fmModel.Network.Branches.Add(new Channel());
        //             var modelDefinition = fmModel.ModelDefinition;
        //
        //             var globalInitialConditionQuantity = (InitialConditionQuantity)(int)modelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D).Value;
        //             Assert.That(globalInitialConditionQuantity, Is.EqualTo(InitialConditionQuantity.WaterLevel));
        //
        //             fmModel.ExportTo(mduFilePath, true, false, false);
        //             Assert.That(File.Exists(initialConditionWaterLevelFile), Is.True);
        //             Assert.That(File.Exists(initialConditionWaterDepthFile), Is.False);
        //
        //             // change globalQuantity to WaterDepth
        //             modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D, $"{(int)InitialConditionQuantity.WaterDepth}");
        //
        //             // resave
        //             fmModel.ExportTo(mduFilePath, true, false, false);
        //             Assert.That(File.Exists(initialConditionWaterLevelFile), Is.False);
        //             Assert.That(File.Exists(initialConditionWaterDepthFile), Is.True);
        //         }
        //     }
        //     finally
        //     {
        //         FileUtils.DeleteIfExists(tempFolder);
        //     }
        // }

        private void SetGlobalValue(WaterFlowFMModelDefinition modelDefinition, double globalValue)
        {
            modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalValue1D, globalValue.ToString(CultureInfo.InvariantCulture));
        }

        private void SetGlobalQuantity(WaterFlowFMModelDefinition modelDefinition, InitialConditionQuantity globalQuantity)
        {
            modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D, $"{(int)globalQuantity}");
        }

        private void EditChannelInitialConditionDefinitions(
            IEventedList<ChannelInitialConditionDefinition> channelInitialConditionDefinitions,
            InitialConditionQuantity globalQuantity,
            double globalValue)
        {
            // Channel0 --> Use constant value: 111
            ChannelInitialConditionDefinition definition0 = channelInitialConditionDefinitions.First(definition =>
                definition.Channel.Name.Equals("Channel0", StringComparison.InvariantCultureIgnoreCase));
            definition0.SpecificationType = ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition;
            definition0.ConstantChannelInitialConditionDefinition.Value = 111;
            definition0.ConstantChannelInitialConditionDefinition.Quantity = globalQuantity;

            // Channel1 --> Use constant value: 333
            ChannelInitialConditionDefinition definition1 = channelInitialConditionDefinitions.First(definition =>
                definition.Channel.Name.Equals("Channel1", StringComparison.InvariantCultureIgnoreCase));
            definition1.SpecificationType = ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition;
            definition1.ConstantChannelInitialConditionDefinition.Value = 456;
            definition1.ConstantChannelInitialConditionDefinition.Quantity = globalQuantity;

            // Channel2 --> Use global value

            // Channel3 --> Use branch chainage: [0, 2.33, 1] [11, 12.22, 4]
            ChannelInitialConditionDefinition definition3 = channelInitialConditionDefinitions.First(definition =>
                definition.Channel.Name.Equals("Channel3", StringComparison.InvariantCultureIgnoreCase));
            definition3.SpecificationType = ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition;
            definition3.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(
                new ConstantSpatialChannelInitialConditionDefinition() { Chainage = 0, Value = 11 });
            definition3.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(
                new ConstantSpatialChannelInitialConditionDefinition() { Chainage = 2.33, Value = 12.22 });
            definition3.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(
                new ConstantSpatialChannelInitialConditionDefinition() { Chainage = 1, Value = 4 });

            // Channel4 --> Use branch chainage: [88.12345] [99.98765]
            ChannelInitialConditionDefinition definition4 = channelInitialConditionDefinitions.First(definition =>
                definition.Channel.Name.Equals("Channel4", StringComparison.InvariantCultureIgnoreCase));
            definition4.SpecificationType = ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition;
            definition4.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(
                new ConstantSpatialChannelInitialConditionDefinition() { Chainage = 88.12345, Value = 99.98765 });
        }
    }
}