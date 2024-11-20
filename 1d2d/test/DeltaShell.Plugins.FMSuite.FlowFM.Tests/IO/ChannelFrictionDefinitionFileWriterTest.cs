using System;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.InitialField;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class ChannelFrictionDefinitionFileWriterTest
    {
        [Test]
        public void GivenCollectionOfChannelFrictionDefinitions_WhenWritingToFile_ThenIsSameAsExpectedFile()
        {
            var expectedFile = TestHelper.GetTestFilePath(@"IO\ChannelFrictionDefinitions_expected.ini");
            var tempFolder = FileUtils.CreateTempDirectory();
            var actualFilePath = Path.Combine(tempFolder, Resources.Roughness_Main_Channels_Filename);
            try
            {
                var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
                var featureFileWriter = new FeatureFile1D2DWriter(new InitialFieldFile());

                using (var fmModel = new WaterFlowFMModel() { MduFilePath = mduFilePath })
                {
                    // Setup
                    var network = fmModel.Network;

                    var numberOfBranches = 14;
                    for (int i = 0; i < numberOfBranches; i++)
                    {
                        var channel = new Channel() { Name = $"Channel{i}" };
                        network.Branches.Add(channel);
                    }
                    Assert.That(network.Branches.Count, Is.EqualTo(numberOfBranches));
                    Assert.That(fmModel.ChannelFrictionDefinitions.Count, Is.EqualTo(numberOfBranches));
                    Assert.That(fmModel.RoughnessSections.Count, Is.EqualTo(2));

                    IEventedList<RoughnessSection> roughnessSections = fmModel.RoughnessSections;
                    IEventedList<ChannelFrictionDefinition> channelFrictionDefinitions =
                        fmModel.ChannelFrictionDefinitions;

                    EditChannelFrictionDefinitions(channelFrictionDefinitions);

                    // Call
                    featureFileWriter.Write1D2DFeatures(mduFilePath, fmModel.ModelDefinition,
                        network, fmModel.Area, roughnessSections, channelFrictionDefinitions, fmModel.ChannelInitialConditionDefinitions);

                    // Assert
                    Assert.That(File.Exists(actualFilePath), Is.True);
                    FileAssert.AreEqual(actualFilePath, expectedFile);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(tempFolder);
            }
        }

        /// <summary>
        /// Creates several different kinds of ChannelFrictionDefinitions.
        /// </summary>
        /// <param name="channelFrictionDefinitions"></param>
        private static void EditChannelFrictionDefinitions(IEventedList<ChannelFrictionDefinition> channelFrictionDefinitions)
        {
            ChannelFrictionDefinition cfd0 = channelFrictionDefinitions.First(cfd =>
                cfd.Channel.Name.Equals("Channel0", StringComparison.InvariantCultureIgnoreCase));
            cfd0.SpecificationType = ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition;
            cfd0.ConstantChannelFrictionDefinition.Value = 123;

            ChannelFrictionDefinition cfd1 = channelFrictionDefinitions.First(cfd =>
                cfd.Channel.Name.Equals("Channel1", StringComparison.InvariantCultureIgnoreCase));
            cfd1.SpecificationType = ChannelFrictionSpecificationType.RoughnessSections;

            ChannelFrictionDefinition cfd2 = channelFrictionDefinitions.First(cfd =>
                cfd.Channel.Name.Equals("Channel2", StringComparison.InvariantCultureIgnoreCase));
            cfd2.SpecificationType = ChannelFrictionSpecificationType.CrossSectionFrictionDefinitions;

            ChannelFrictionDefinition cfd3 = channelFrictionDefinitions.First(cfd =>
                cfd.Channel.Name.Equals("Channel3", StringComparison.InvariantCultureIgnoreCase));
            cfd3.SpecificationType = ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition;
            cfd3.ConstantChannelFrictionDefinition.Value = 3;
            cfd3.ConstantChannelFrictionDefinition.Type = RoughnessType.DeBosBijkerk;

            ChannelFrictionDefinition cfd4 = channelFrictionDefinitions.First(cfd =>
                cfd.Channel.Name.Equals("Channel4", StringComparison.InvariantCultureIgnoreCase));
            cfd4.SpecificationType = ChannelFrictionSpecificationType.ModelSettings;

            ChannelFrictionDefinition cfd5 = channelFrictionDefinitions.First(cfd =>
                cfd.Channel.Name.Equals("Channel5", StringComparison.InvariantCultureIgnoreCase));
            cfd5.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd5.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.Constant;
            cfd5.SpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.Add(
                new ConstantSpatialChannelFrictionDefinition() {Chainage = 0, Value = 111});
            cfd5.SpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.Add(
                new ConstantSpatialChannelFrictionDefinition() {Chainage = 50.123, Value = 50123});

            ChannelFrictionDefinition cfd6 = channelFrictionDefinitions.First(cfd =>
                cfd.Channel.Name.Equals("Channel6", StringComparison.InvariantCultureIgnoreCase));
            cfd6.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd6.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfH;
            cfd6.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] {1, 2}); // chainage
            cfd6.SpatialChannelFrictionDefinition.Function.Arguments[1].SetValues(new double[] {3, 4}); // H or Q value
            cfd6.SpatialChannelFrictionDefinition.Function.Components[0].SetValues(new double[] {5, 6, 7, 8}); // friction value

            ChannelFrictionDefinition cfd7 = channelFrictionDefinitions.First(cfd =>
                cfd.Channel.Name.Equals("Channel7", StringComparison.InvariantCultureIgnoreCase));
            cfd7.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd7.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfQ;
            cfd7.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] {1, 2, 3});
            cfd7.SpatialChannelFrictionDefinition.Function.Arguments[1].SetValues(new double[] {4, 5});
            cfd7.SpatialChannelFrictionDefinition.Function.Components[0].SetValues(new double[] {6, 7, 8, 9, 10, 11});

            ChannelFrictionDefinition cfd8 = channelFrictionDefinitions.First(cfd =>
                cfd.Channel.Name.Equals("Channel8", StringComparison.InvariantCultureIgnoreCase));
            cfd8.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd8.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfH;
            cfd8.SpatialChannelFrictionDefinition.Type = RoughnessType.StricklerNikuradse;
            cfd8.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] {1});
            cfd8.SpatialChannelFrictionDefinition.Function.Arguments[1].SetValues(new double[] {2, 3, 4, 5, 6});
            cfd8.SpatialChannelFrictionDefinition.Function.Components[0].SetValues(new double[] {7, 8, 9, 10, 11});

            ChannelFrictionDefinition cfd9 = channelFrictionDefinitions.First(cfd =>
                cfd.Channel.Name.Equals("Channel9", StringComparison.InvariantCultureIgnoreCase));
            cfd9.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd9.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfQ;
            cfd9.SpatialChannelFrictionDefinition.Type = RoughnessType.Strickler;
            cfd9.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] {1, 2, 3, 4, 5});
            cfd9.SpatialChannelFrictionDefinition.Function.Arguments[1].SetValues(new double[] {6});
            cfd9.SpatialChannelFrictionDefinition.Function.Components[0].SetValues(new double[] {7, 8, 9, 10, 11});

            ChannelFrictionDefinition cfd10 = channelFrictionDefinitions.First(cfd =>
                cfd.Channel.Name.Equals("Channel10", StringComparison.InvariantCultureIgnoreCase));
            cfd10.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd10.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.Constant;
            cfd10.SpatialChannelFrictionDefinition.Type = RoughnessType.WallLawNikuradse;
            cfd10.SpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.Add(
                new ConstantSpatialChannelFrictionDefinition() {Chainage = 0, Value = 111});

            ChannelFrictionDefinition cfd11 = channelFrictionDefinitions.First(cfd =>
                cfd.Channel.Name.Equals("Channel11", StringComparison.InvariantCultureIgnoreCase));
            cfd11.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd11.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfH;
            cfd11.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] { 1 }); // no H value and no friction value is set

            ChannelFrictionDefinition cfd12 = channelFrictionDefinitions.First(cfd =>
                cfd.Channel.Name.Equals("Channel12", StringComparison.InvariantCultureIgnoreCase));
            cfd12.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd12.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfQ;
            cfd12.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] { 1, 2 }); // no H value and no friction value is set
            
            ChannelFrictionDefinition cfd13 = channelFrictionDefinitions.First(cfd =>
                cfd.Channel.Name.Equals("Channel13", StringComparison.InvariantCultureIgnoreCase));
            cfd13.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd13.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.Constant; // no ConstantSpatialChannelFrictionDefinitions added to list


        }
    }

    
}