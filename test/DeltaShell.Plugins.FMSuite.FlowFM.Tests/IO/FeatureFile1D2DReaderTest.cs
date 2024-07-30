using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using Deltares.Infrastructure.Extensions;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.InitialField;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Coverages;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class FeatureFile1D2DReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void WaterFlowFMModel_RoughnessAndFrictionSpecifications_AreCorrectlySet()
        {
            // Arrange
            var mduFilePath = TestHelper.GetTestFilePath(@"roughness\globalFriction_except_channel4_onlanes\DFM.mdu");
            
            // Call
            using (var model = new WaterFlowFMModel(mduFilePath))
            {
                // Assert
                Assert.That(model.RoughnessSections.Count, Is.EqualTo(2));
                Assert.That(model.RoughnessSections[0].Name, Is.EqualTo(RoughnessDataSet.MainSectionTypeName));

                IMultiDimensionalArray<INetworkLocation> roughnessLocations = model.RoughnessSections[0].RoughnessNetworkCoverage.Locations.Values;
                string[] branchNames = roughnessLocations.Select(v => v.Branch.Name).ToArray();
                Assert.That(branchNames, Has.Exactly(1).EqualTo("Channel_1D_1"));
                Assert.That(branchNames, Has.Exactly(1).EqualTo("Channel_1D_4"));

                Assert.That(model.ChannelFrictionDefinitions, Has.Count.EqualTo(5));
                Assert.That(model.ChannelFrictionDefinitions, Has.Exactly(1).Matches<ChannelFrictionDefinition>(
                                cfd => cfd.SpecificationType == ChannelFrictionSpecificationType.RoughnessSections &&
                                       cfd.Channel.Name.EqualsCaseInsensitive("Channel_1D_4")));
                Assert.That(model.ChannelFrictionDefinitions, Has.Exactly(4).Matches<ChannelFrictionDefinition>(
                                cfd => cfd.SpecificationType == ChannelFrictionSpecificationType.ModelSettings));
            }
        }
    

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
            // Arrange
            var featureFileReader = new FeatureFile1D2DReader(new InitialFieldFile());

            
            // Call
            void Call() => featureFileReader.Read1D2DFeatures(mduFilePath, modelDefinition, network, roughnessSections, channelFrictionDefinitions, channelInitialConditionDefinitions);

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
                                "hydroNetwork");

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