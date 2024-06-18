using System;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
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
                    
                    const int numberOfBranches = 6;
                    fmModel.Network = HydroNetworkHelper.GetSnakeHydroNetwork(numberOfBranches);
                    // Preconditions
                    Assert.That(fmModel.ChannelInitialConditionDefinitions.Count, Is.EqualTo(numberOfBranches));

                    IEventedList<ChannelInitialConditionDefinition> channelInitialConditionDefinitions = fmModel.ChannelInitialConditionDefinitions;

                    EditChannelInitialConditionDefinitions(channelInitialConditionDefinitions, globalQuantity);

                    // When
                    FeatureFile1D2DWriter.Write1D2DFeatures(mduFilePath, fmModel.ModelDefinition,
                        fmModel.Network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, channelInitialConditionDefinitions);

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
            // branch1 --> Use constant value: 789
            ChannelInitialConditionDefinition definition1 = channelInitialConditionDefinitions.FirstOrDefault(definition =>
                definition.Channel.Name.Equals("branch1", StringComparison.InvariantCultureIgnoreCase));
            Assert.That(definition1, Is.Not.Null, "branch1 is not in the network");
            definition1.SpecificationType = ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition;
            definition1.ConstantChannelInitialConditionDefinition.Value = 789;
            definition1.ConstantChannelInitialConditionDefinition.Quantity = globalQuantity;

            // branch2 --> Use global value

            // branch3 --> Use branch chainage: [0, 2.33, 1] [11, 12.22, 4]
            ChannelInitialConditionDefinition definition3 = channelInitialConditionDefinitions.FirstOrDefault(definition =>
                definition.Channel.Name.Equals("branch3", StringComparison.InvariantCultureIgnoreCase));
            Assert.That(definition3, Is.Not.Null, "branch3 is not in the network");
            definition3.SpecificationType = ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition;
            definition3.SpatialChannelInitialConditionDefinition.Quantity = globalQuantity;
            definition3.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(
                new ConstantSpatialChannelInitialConditionDefinition { Chainage = 0, Value = 11 });
            definition3.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(
                new ConstantSpatialChannelInitialConditionDefinition { Chainage = 2.33, Value = 12.22 });
            definition3.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(
                new ConstantSpatialChannelInitialConditionDefinition { Chainage = 1, Value = 4 });

            // branch4 --> Use branch chainage: [88.12345] [99.98765]
            ChannelInitialConditionDefinition definition4 = channelInitialConditionDefinitions.FirstOrDefault(definition =>
                definition.Channel.Name.Equals("branch4", StringComparison.InvariantCultureIgnoreCase));
            Assert.That(definition4, Is.Not.Null, "branch4 is not in the network");

            definition4.SpecificationType = ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition;
            definition4.SpatialChannelInitialConditionDefinition.Quantity = globalQuantity;
            definition4.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(
                new ConstantSpatialChannelInitialConditionDefinition() { Chainage = 88.12345, Value = 99.98765 });

            // branch5 --> Use branch chainage: <no definitions>
            ChannelInitialConditionDefinition definition5 = channelInitialConditionDefinitions.FirstOrDefault(definition =>
                definition.Channel.Name.Equals("branch5", StringComparison.InvariantCultureIgnoreCase));
            Assert.That(definition5, Is.Not.Null, "branch5 is not in the network");
            definition5.SpecificationType = ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition;
            definition5.SpatialChannelInitialConditionDefinition.Quantity = globalQuantity;
        }
    }
}