using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;

using FlowFMResources = DeltaShell.Plugins.FMSuite.FlowFM.Properties.Resources;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class ChannelFrictionDefinitionFileReaderTest
    {
        [Test]
        public void GivenRoughnessChannelsFile_WhenReading_SetsCorrectChannelFrictionDefinitions()
        {
            var channelsRoughnessFile = TestHelper.GetTestFilePath($"IO\\{FlowFMResources.Roughness_Main_Channels_Filename}");

            using (var fmModel = new WaterFlowFMModel())
            {
                var modelDefinition = fmModel.ModelDefinition;
                var network = fmModel.Network;
                var channelFrictionDefinitions = fmModel.ChannelFrictionDefinitions;

                var numberOfBranches = 14;
                for (int i = 0; i < numberOfBranches; i++)
                {
                    var channel = new Channel() { Name = $"Channel{i}" };
                    network.Branches.Add(channel);
                }
                Assert.That(network.Branches.Count, Is.EqualTo(numberOfBranches));
                Assert.That(fmModel.ChannelFrictionDefinitions.Count, Is.EqualTo(numberOfBranches));

                var expectedChannelFrictionDefinitions = GetExpectedChannelFrictionDefinitions(network.Branches);

                ChannelFrictionDefinitionFileReader.ReadFile(channelsRoughnessFile, modelDefinition, network, channelFrictionDefinitions);

                CompareChannelFrictionDefinitions(expectedChannelFrictionDefinitions.ToList(), channelFrictionDefinitions.ToList());
            }
        }

        private void CompareChannelFrictionDefinitions(List<ChannelFrictionDefinition> expectedChannelFrictionDefinitions, List<ChannelFrictionDefinition> actualChannelFrictionDefinitions)
        {
            Assert.That(expectedChannelFrictionDefinitions.Count, Is.EqualTo(actualChannelFrictionDefinitions.Count));

            var serializer = new JavaScriptSerializer();

            foreach (var expectedChannelFrictionDefinition in expectedChannelFrictionDefinitions)
            {
                var branchName = expectedChannelFrictionDefinition.Channel.Name;
                var actualChannelFrictionDefinition = actualChannelFrictionDefinitions.FirstOrDefault(cfd => cfd.Channel.Name.Equals(branchName));

                Assert.That(actualChannelFrictionDefinition, Is.Not.Null);
                Assert.That(expectedChannelFrictionDefinition.SpecificationType, Is.EqualTo(actualChannelFrictionDefinition.SpecificationType));
                Assert.That(serializer.Serialize(expectedChannelFrictionDefinition.ConstantChannelFrictionDefinition), Is.EqualTo(serializer.Serialize(actualChannelFrictionDefinition.ConstantChannelFrictionDefinition)));

                var expectedSpatialChannelFrictionDefinition = expectedChannelFrictionDefinition.SpatialChannelFrictionDefinition;
                if (expectedSpatialChannelFrictionDefinition != null)
                {
                    var actualSpatialChannelFrictionDefinition = actualChannelFrictionDefinition.SpatialChannelFrictionDefinition;

                    Assert.That(expectedSpatialChannelFrictionDefinition.Type, Is.EqualTo(actualSpatialChannelFrictionDefinition.Type));
                    Assert.That(expectedSpatialChannelFrictionDefinition.FunctionType, Is.EqualTo(actualSpatialChannelFrictionDefinition.FunctionType));
                    Assert.That(serializer.Serialize(expectedSpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions), Is.EqualTo(serializer.Serialize(actualSpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions)));

                    if (expectedSpatialChannelFrictionDefinition.Function != null)
                    {
                        Assert.That(expectedSpatialChannelFrictionDefinition.Function.GetAllComponentValues(), Is.EqualTo(actualSpatialChannelFrictionDefinition.Function.GetAllComponentValues()));
                    }
                }
            }
        }

        private static IEnumerable<ChannelFrictionDefinition> GetExpectedChannelFrictionDefinitions(IEnumerable<IBranch> branches)
        {
            var channelFrictionDefinitions = new List<ChannelFrictionDefinition>();

            var branchesList = branches.ToList();
            var branch0 = branchesList.First(b => b.Name.Equals("Channel0"));
            ChannelFrictionDefinition cfd0 = new ChannelFrictionDefinition(branch0 as Channel);
            cfd0.SpecificationType = ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition;
            cfd0.ConstantChannelFrictionDefinition.Value = 123;
            channelFrictionDefinitions.Add(cfd0);

            var branch1 = branchesList.First(b => b.Name.Equals("Channel1"));
            ChannelFrictionDefinition cfd1 = new ChannelFrictionDefinition(branch1 as Channel);
            cfd1.SpecificationType = ChannelFrictionSpecificationType.ModelSettings;
            channelFrictionDefinitions.Add(cfd1);

            var branch2 = branchesList.First(b => b.Name.Equals("Channel2"));
            ChannelFrictionDefinition cfd2 = new ChannelFrictionDefinition(branch2 as Channel);
            cfd2.SpecificationType = ChannelFrictionSpecificationType.ModelSettings;
            channelFrictionDefinitions.Add(cfd2);

            var branch3 = branchesList.First(b => b.Name.Equals("Channel3"));
            ChannelFrictionDefinition cfd3 = new ChannelFrictionDefinition(branch3 as Channel);
            cfd3.SpecificationType = ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition;
            cfd3.ConstantChannelFrictionDefinition.Value = 3;
            cfd3.ConstantChannelFrictionDefinition.Type = RoughnessType.DeBosAndBijkerk;
            channelFrictionDefinitions.Add(cfd3);

            var branch4 = branchesList.First(b => b.Name.Equals("Channel4"));
            ChannelFrictionDefinition cfd4 = new ChannelFrictionDefinition(branch4 as Channel);
            cfd4.SpecificationType = ChannelFrictionSpecificationType.ModelSettings;
            channelFrictionDefinitions.Add(cfd4);

            var branch5 = branchesList.First(b => b.Name.Equals("Channel5"));
            ChannelFrictionDefinition cfd5 = new ChannelFrictionDefinition(branch5 as Channel);
            cfd5.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd5.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.Constant;
            cfd5.SpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.Add(
                new ConstantSpatialChannelFrictionDefinition() {Chainage = 0, Value = 111});
            cfd5.SpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.Add(
                new ConstantSpatialChannelFrictionDefinition() {Chainage = 50.123, Value = 50123});
            channelFrictionDefinitions.Add(cfd5);

            var branch6 = branchesList.First(b => b.Name.Equals("Channel6"));
            ChannelFrictionDefinition cfd6 = new ChannelFrictionDefinition(branch6 as Channel);
            cfd6.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd6.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfH;
            cfd6.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] { 1, 2 }); // chainage
            cfd6.SpatialChannelFrictionDefinition.Function.Arguments[1].SetValues(new double[] { 3, 4 }); // H or Q value
            cfd6.SpatialChannelFrictionDefinition.Function.Components[0].SetValues(new double[] { 5, 6, 7, 8 }); // friction value
            channelFrictionDefinitions.Add(cfd6);

            var branch7 = branchesList.First(b => b.Name.Equals("Channel7"));
            ChannelFrictionDefinition cfd7 = new ChannelFrictionDefinition(branch7 as Channel);
            cfd7.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd7.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfQ;
            cfd7.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] {1, 2, 3});
            cfd7.SpatialChannelFrictionDefinition.Function.Arguments[1].SetValues(new double[] {4, 5});
            cfd7.SpatialChannelFrictionDefinition.Function.Components[0]
                .SetValues(new double[] {6, 7, 8, 9, 10, 11});
            channelFrictionDefinitions.Add(cfd7);

            var branch8 = branchesList.First(b => b.Name.Equals("Channel8"));
            ChannelFrictionDefinition cfd8 = new ChannelFrictionDefinition(branch8 as Channel);
            cfd8.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd8.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfH;
            cfd8.SpatialChannelFrictionDefinition.Type = RoughnessType.StricklerKn;
            cfd8.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] {1});
            cfd8.SpatialChannelFrictionDefinition.Function.Arguments[1].SetValues(new double[] {2, 3, 4, 5, 6});
            cfd8.SpatialChannelFrictionDefinition.Function.Components[0].SetValues(new double[] {7, 8, 9, 10, 11});
            channelFrictionDefinitions.Add(cfd8);

            var branch9 = branchesList.First(b => b.Name.Equals("Channel9"));
            ChannelFrictionDefinition cfd9 = new ChannelFrictionDefinition(branch9 as Channel);
            cfd9.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd9.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfH;
            cfd9.SpatialChannelFrictionDefinition.Type = RoughnessType.StricklerKs;
            cfd9.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] {1, 2, 3, 4, 5});
            cfd9.SpatialChannelFrictionDefinition.Function.Arguments[1].SetValues(new double[] {6});
            cfd9.SpatialChannelFrictionDefinition.Function.Components[0].SetValues(new double[] {7, 8, 9, 10, 11});
            channelFrictionDefinitions.Add(cfd9);

            var branch10 = branchesList.First(b => b.Name.Equals("Channel10"));
            ChannelFrictionDefinition cfd10 = new ChannelFrictionDefinition(branch10 as Channel);
            cfd10.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd10.SpatialChannelFrictionDefinition.Type = RoughnessType.WallLawNikuradse;
            cfd10.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.Constant;
            cfd10.SpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.Add(
                new ConstantSpatialChannelFrictionDefinition() {Chainage = 0, Value = 111});
            channelFrictionDefinitions.Add(cfd10);

            var branch11 = branchesList.First(b => b.Name.Equals("Channel11"));
            ChannelFrictionDefinition cfd11 = new ChannelFrictionDefinition(branch11 as Channel);
            cfd11.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd11.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfH;
            cfd11.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] { 1 }); // no H value and no friction value is set
            channelFrictionDefinitions.Add(cfd11);

            var branch12 = branchesList.First(b => b.Name.Equals("Channel12"));
            ChannelFrictionDefinition cfd12 = new ChannelFrictionDefinition(branch12 as Channel);
            cfd12.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd12.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfQ;
            cfd12.SpatialChannelFrictionDefinition.Function.Arguments[0].SetValues(new double[] { 1, 2 }); // no H value and no friction value is set
            channelFrictionDefinitions.Add(cfd12);

            var branch13 = branchesList.First(b => b.Name.Equals("Channel13"));
            ChannelFrictionDefinition cfd13 = new ChannelFrictionDefinition(branch13 as Channel);
            cfd13.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            cfd13.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.Constant; // no ConstantSpatialChannelFrictionDefinitions added to list
            channelFrictionDefinitions.Add(cfd13);

            Assert.That(channelFrictionDefinitions.Count, Is.EqualTo(14));
            return channelFrictionDefinitions;
        }
    }
}