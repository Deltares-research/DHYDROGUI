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
            // Given
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
                    const int numberOfBranches = 6;

                    for (var i = 0; i < numberOfBranches; i++)
                    {
                        network.Branches.Add(new Channel { Name = $"Channel{i}" });
                    }

                    // Preconditions
                    Assert.That(fmModel.ChannelInitialConditionDefinitions.Count, Is.EqualTo(numberOfBranches));

                    IEventedList<ChannelInitialConditionDefinition> channelInitialConditionDefinitions = fmModel.ChannelInitialConditionDefinitions;

                    EditChannelInitialConditionDefinitions(channelInitialConditionDefinitions, globalQuantity);

                    // When
                    FeatureFile1D2DWriter.Write1D2DFeatures(mduFilePath, fmModel.ModelDefinition,
                        network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, channelInitialConditionDefinitions);

                    // Then
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

        private static void SetGlobalValue(WaterFlowFMModelDefinition modelDefinition, double globalValue)
        {
            modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalValue1D, globalValue.ToString(CultureInfo.InvariantCulture));
        }

        private static void SetGlobalQuantity(WaterFlowFMModelDefinition modelDefinition, InitialConditionQuantity globalQuantity)
        {
            modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D, $"{(int)globalQuantity}");
        }

        private static void EditChannelInitialConditionDefinitions(
            IEventedList<ChannelInitialConditionDefinition> channelInitialConditionDefinitions,
            InitialConditionQuantity globalQuantity)
        {
            // Channel0 --> Use constant value: 789
            ChannelInitialConditionDefinition definition0 = channelInitialConditionDefinitions.First(definition =>
                definition.Channel.Name.Equals("Channel0", StringComparison.InvariantCultureIgnoreCase));
            definition0.SpecificationType = ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition;
            definition0.ConstantChannelInitialConditionDefinition.Value = 789;
            definition0.ConstantChannelInitialConditionDefinition.Quantity = globalQuantity;

            // Channel1 --> Use global value

            // Channel2 --> Use branch chainage: [0, 2.33, 1] [11, 12.22, 4]
            ChannelInitialConditionDefinition definition2 = channelInitialConditionDefinitions.First(definition =>
                definition.Channel.Name.Equals("Channel2", StringComparison.InvariantCultureIgnoreCase));
            definition2.SpecificationType = ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition;
            definition2.SpatialChannelInitialConditionDefinition.Quantity = globalQuantity;
            definition2.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(
                new ConstantSpatialChannelInitialConditionDefinition { Chainage = 0, Value = 11 });
            definition2.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(
                new ConstantSpatialChannelInitialConditionDefinition { Chainage = 2.33, Value = 12.22 });
            definition2.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(
                new ConstantSpatialChannelInitialConditionDefinition { Chainage = 1, Value = 4 });

            // Channel3 --> Use branch chainage: [88.12345] [99.98765]
            ChannelInitialConditionDefinition definition3 = channelInitialConditionDefinitions.First(definition =>
                definition.Channel.Name.Equals("Channel3", StringComparison.InvariantCultureIgnoreCase));
            definition3.SpecificationType = ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition;
            definition3.SpatialChannelInitialConditionDefinition.Quantity = globalQuantity;
            definition3.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(
                new ConstantSpatialChannelInitialConditionDefinition() { Chainage = 88.12345, Value = 99.98765 });

            // Channel4 --> Use branch chainage: <no definitions>
            ChannelInitialConditionDefinition definition4 = channelInitialConditionDefinitions.First(definition =>
                definition.Channel.Name.Equals("Channel4", StringComparison.InvariantCultureIgnoreCase));
            definition4.SpecificationType = ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition;
            definition4.SpatialChannelInitialConditionDefinition.Quantity = globalQuantity;
        }
    }
}