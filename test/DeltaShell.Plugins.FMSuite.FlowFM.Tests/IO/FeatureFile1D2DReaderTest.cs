using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class FeatureFile1D2DReaderTest
    {
        [Test]
        [TestCaseSource(nameof(Read1D2DFeatures_ArgNull_Cases))]
        public void Read1D2DFeatures_ArgNull_ThrowsArgumentNullException(string mduFilePath,
                                                                         WaterFlowFMModelDefinition modelDefinition,
                                                                         IHydroNetwork network,
                                                                         IEventedList<RoughnessSection> roughnessSections,
                                                                         IEventedList<ChannelFrictionDefinition> channelFrictionDefinitions,
                                                                         IEventedList<ChannelInitialConditionDefinition> channelInitialConditionDefinitions,
                                                                         string expParamName)
        {
            // Call
            void Call() => FeatureFile1D2DReader.Read1D2DFeatures(mduFilePath, modelDefinition, network, roughnessSections, channelFrictionDefinitions, channelInitialConditionDefinitions);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParamName));
        }

        private static IEnumerable<TestCaseData> Read1D2DFeatures_ArgNull_Cases()
        {
            yield return ToData(null,
                                new WaterFlowFMModelDefinition(),
                                Substitute.For<IHydroNetwork>(),
                                Substitute.For<IEventedList<RoughnessSection>>(),
                                Substitute.For<IEventedList<ChannelFrictionDefinition>>(),
                                Substitute.For<IEventedList<ChannelInitialConditionDefinition>>(),
                                "mduFilePath");

            yield return ToData("mdu_file_path",
                                null,
                                Substitute.For<IHydroNetwork>(),
                                Substitute.For<IEventedList<RoughnessSection>>(),
                                Substitute.For<IEventedList<ChannelFrictionDefinition>>(),
                                Substitute.For<IEventedList<ChannelInitialConditionDefinition>>(),
                                "modelDefinition");

            yield return ToData("mdu_file_path",
                                new WaterFlowFMModelDefinition(),
                                null,
                                Substitute.For<IEventedList<RoughnessSection>>(),
                                Substitute.For<IEventedList<ChannelFrictionDefinition>>(),
                                Substitute.For<IEventedList<ChannelInitialConditionDefinition>>(),
                                "network");

            yield return ToData("mdu_file_path",
                                new WaterFlowFMModelDefinition(),
                                Substitute.For<IHydroNetwork>(),
                                null,
                                Substitute.For<IEventedList<ChannelFrictionDefinition>>(),
                                Substitute.For<IEventedList<ChannelInitialConditionDefinition>>(),
                                "roughnessSections");

            yield return ToData("mdu_file_path",
                                new WaterFlowFMModelDefinition(),
                                Substitute.For<IHydroNetwork>(),
                                Substitute.For<IEventedList<RoughnessSection>>(),
                                null,
                                Substitute.For<IEventedList<ChannelInitialConditionDefinition>>(),
                                "channelFrictionDefinitions");

            yield return ToData("mdu_file_path",
                                new WaterFlowFMModelDefinition(),
                                Substitute.For<IHydroNetwork>(),
                                Substitute.For<IEventedList<RoughnessSection>>(),
                                Substitute.For<IEventedList<ChannelFrictionDefinition>>(),
                                null,
                                "channelInitialConditionDefinitions");

            TestCaseData ToData(string mduFilePath,
                                WaterFlowFMModelDefinition modelDefinition,
                                IHydroNetwork network,
                                IEventedList<RoughnessSection> roughnessSections,
                                IEventedList<ChannelFrictionDefinition> channelFrictionDefinitions,
                                IEventedList<ChannelInitialConditionDefinition> channelInitialConditionDefinitions,
                                string expParamName) =>
                new TestCaseData(mduFilePath,
                                 modelDefinition,
                                 network,
                                 roughnessSections,
                                 channelFrictionDefinitions,
                                 channelInitialConditionDefinitions,
                                 expParamName)
                    .SetName(expParamName);
        }
    }
}