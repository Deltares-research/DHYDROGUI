using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.Plugins.FMSuite.FlowFM;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class SobekToWaterFlowFMRoughnessConverterTest
    {
        private const int RoughnessValueComponentIndex = 0;
        private const int RoughnessTypeComponentIndex = 1;
        private const int ChainageArgumentIndex = 0;
        private const int FunctionArgumentIndex = 1;

        [Test]
        public void WhenChannelFrictionDefinitionsIsNull_ThenShouldThrowArgumentNullException()
        {
            var hydroNetwork = new HydroNetwork();
            var converter = new SobekToWaterFlowFMRoughnessConverter();

            TestDelegate action = () =>
            {
                converter.ConvertSobekRoughnessToWaterFlowFmRoughness(null, new RoughnessSection(new CrossSectionSectionType(), hydroNetwork), hydroNetwork);
            };
            
            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.AreEqual("channelFrictionDefinitions", exception.ParamName);
        }

        [Test]
        public void WhenRoughnessSectionsIsNull_ThenShouldThrowArgumentNullException()
        {
            var converter = new SobekToWaterFlowFMRoughnessConverter();

            TestDelegate action = () => converter.ConvertSobekRoughnessToWaterFlowFmRoughness(Enumerable.Empty<ChannelFrictionDefinition>(), null, new HydroNetwork());
            
            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.AreEqual("defaultRoughnessSection", exception.ParamName);
        }

        [Test]
        public void WhenNetworkIsNull_ThenShouldThrowArgumentNullException()
        {
            var converter = new SobekToWaterFlowFMRoughnessConverter();

            TestDelegate action = () => converter.ConvertSobekRoughnessToWaterFlowFmRoughness(Enumerable.Empty<ChannelFrictionDefinition>(), new RoughnessSection(new CrossSectionSectionType(), new HydroNetwork()), null);

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.AreEqual("network", exception.ParamName);
        }

        [Test]
        public void GivenChannelWithCrossSectionThatOnlyReferencesMainSectionType_WhenConvertingRoughness_ThenChannelFrictionDefinitionSetToConstantChannelFrictionDefinition()
        {
            // Given
            using (var fmModel = new WaterFlowFMModel())
            {
                var channel = new Channel();
                var hydroNetwork = fmModel.Network;
                var mainSectionType = new CrossSectionSectionType
                {
                    Name = "Main"
                };

                var mainRoughnessSection = new RoughnessSection(mainSectionType, hydroNetwork);

                hydroNetwork.Branches.Add(channel);

                channel.BranchFeatures.Add(new CrossSection(new CrossSectionDefinitionYZ
                {
                    Sections =
                    {
                        new CrossSectionSection
                        { 
                            MinY = 0,
                            MaxY = 50,
                            SectionType = mainSectionType
                        },
                        new CrossSectionSection
                        {
                            MinY = 50,
                            MaxY = 100,
                            SectionType = mainSectionType
                        }
                    }
                }));

                // Preconditions
                Assert.AreEqual(ChannelFrictionSpecificationType.ModelSettings, fmModel.ChannelFrictionDefinitions.First().SpecificationType);

                // When
                new SobekToWaterFlowFMRoughnessConverter().ConvertSobekRoughnessToWaterFlowFmRoughness(
                    fmModel.ChannelFrictionDefinitions,
                    mainRoughnessSection,
                    hydroNetwork);

                // Then
                Assert.AreEqual(ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition, fmModel.ChannelFrictionDefinitions.First().SpecificationType);
            }
        }

        [Test]
        public void GivenChannelWithCrossSectionThatReferencesOtherThanMainSectionType_WhenConvertingRoughness_ThenChannelFrictionDefinitionSetToRoughnessSections()
        {
            // Given
            using (var fmModel = new WaterFlowFMModel())
            {
                var channel = new Channel();
                var hydroNetwork = fmModel.Network;
                var mainSectionType = new CrossSectionSectionType
                {
                    Name = "Main"
                };

                var mainRoughnessSection = new RoughnessSection(mainSectionType, hydroNetwork);

                hydroNetwork.Branches.Add(channel);

                channel.BranchFeatures.Add(new CrossSection(new CrossSectionDefinitionYZ
                {
                    Sections =
                    {
                        new CrossSectionSection
                        {
                            MinY = 0,
                            MaxY = 50,
                            SectionType = mainSectionType
                        },
                        new CrossSectionSection
                        {
                            MinY = 50,
                            MaxY = 100,
                            SectionType = new CrossSectionSectionType()
                        }
                    }
                }));

                // Preconditions
                Assert.AreEqual(ChannelFrictionSpecificationType.ModelSettings, fmModel.ChannelFrictionDefinitions.First().SpecificationType);

                // When
                new SobekToWaterFlowFMRoughnessConverter().ConvertSobekRoughnessToWaterFlowFmRoughness(
                    fmModel.ChannelFrictionDefinitions,
                    mainRoughnessSection,
                    hydroNetwork);

                // Then
                Assert.AreEqual(ChannelFrictionSpecificationType.RoughnessSections, fmModel.ChannelFrictionDefinitions.First().SpecificationType);
            }
        }

        [Test]
        public void GivenComplexSituationWithSharedCrossSection_WhenConvertingRoughness_ThenChannelFrictionDefinitionForBothChannelsSetToRoughnessSections()
        {
            // Given
            using (var fmModel = new WaterFlowFMModel())
            {
                var channel1 = new Channel();
                var channel2 = new Channel();
                var hydroNetwork = fmModel.Network;
                var mainSectionType = new CrossSectionSectionType
                {
                    Name = "Main"
                };

                var mainRoughnessSection = new RoughnessSection(mainSectionType, hydroNetwork);

                var crossSection1 = new CrossSection(new CrossSectionDefinitionYZ
                {
                    Sections =
                    {
                        new CrossSectionSection
                        {
                            MinY = 0,
                            MaxY = 50,
                            SectionType = new CrossSectionSectionType()
                        }
                    }
                });

                var crossSection2 = new CrossSection(new CrossSectionDefinitionYZ
                {
                    Sections =
                    {
                        new CrossSectionSection
                        {
                            MinY = 50,
                            MaxY = 100,
                            SectionType = mainSectionType
                        }
                    }
                });

                var crossSection3 = new CrossSection(new CrossSectionDefinitionYZ
                {
                    Sections =
                    {
                        new CrossSectionSection
                        {
                            MinY = 50,
                            MaxY = 100,
                            SectionType = mainSectionType
                        }
                    }
                });

                hydroNetwork.Branches.Add(channel1);
                hydroNetwork.Branches.Add(channel2);
                channel1.BranchFeatures.Add(crossSection1);
                channel1.BranchFeatures.Add(crossSection2);
                channel2.BranchFeatures.Add(crossSection3);
                crossSection2.ShareDefinitionAndChangeToProxy();
                crossSection3.UseSharedDefinition(hydroNetwork.SharedCrossSectionDefinitions.First());

                // Preconditions
                Assert.AreEqual(ChannelFrictionSpecificationType.ModelSettings, fmModel.ChannelFrictionDefinitions.ElementAt(0).SpecificationType);
                Assert.AreEqual(ChannelFrictionSpecificationType.ModelSettings, fmModel.ChannelFrictionDefinitions.ElementAt(1).SpecificationType);

                // When
                new SobekToWaterFlowFMRoughnessConverter().ConvertSobekRoughnessToWaterFlowFmRoughness(
                    fmModel.ChannelFrictionDefinitions,
                    mainRoughnessSection,
                    hydroNetwork);

                // Then
                Assert.AreEqual(ChannelFrictionSpecificationType.RoughnessSections, fmModel.ChannelFrictionDefinitions.ElementAt(0).SpecificationType);
                Assert.AreEqual(ChannelFrictionSpecificationType.RoughnessSections, fmModel.ChannelFrictionDefinitions.ElementAt(1).SpecificationType);
            }
        }

        [Test]
        public void GivenChannelWithoutLocationsInMainSection_WhenConvertingRoughness_ThenChannelFrictionDefinitionSetToConstantChannelFrictionDefinitionWithExpectedValues()
        {
            // Given
            using (var fmModel = new WaterFlowFMModel())
            {
                var channel = new Channel();
                var hydroNetwork = fmModel.Network;
                var mainSectionType = new CrossSectionSectionType
                {
                    Name = "Main"
                };

                var mainRoughnessSection = new RoughnessSection(mainSectionType, hydroNetwork)
                {
                    RoughnessNetworkCoverage =
                    {
                        DefaultRoughnessType = RoughnessType.DeBosBijkerk,
                        DefaultValue = 35.6
                    }
                };

                hydroNetwork.Branches.Add(channel);

                var channelFrictionDefinition = fmModel.ChannelFrictionDefinitions.First();

                // Preconditions
                Assert.AreEqual(ChannelFrictionSpecificationType.ModelSettings, channelFrictionDefinition.SpecificationType);

                // When
                new SobekToWaterFlowFMRoughnessConverter().ConvertSobekRoughnessToWaterFlowFmRoughness(
                    fmModel.ChannelFrictionDefinitions,
                    mainRoughnessSection,
                    hydroNetwork);

                // Then
                Assert.AreEqual(ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition, channelFrictionDefinition.SpecificationType);
                Assert.AreEqual(RoughnessType.DeBosBijkerk, channelFrictionDefinition.ConstantChannelFrictionDefinition.Type);
                Assert.AreEqual(35.6, channelFrictionDefinition.ConstantChannelFrictionDefinition.Value);
            }
        }

        [Test]
        public void GivenChannelWithOneLocationInMainSection_WhenConvertingRoughness_ThenChannelFrictionDefinitionSetToSpatialChannelFrictionDefinitionWithExpectedValues()
        {
            // Given
            using (var fmModel = new WaterFlowFMModel())
            {
                var channel = new Channel();
                var hydroNetwork = fmModel.Network;
                var mainSectionType = new CrossSectionSectionType
                {
                    Name = "Main"
                };

                var mainRoughnessSection = new RoughnessSection(mainSectionType, hydroNetwork)
                {
                    RoughnessNetworkCoverage =
                    {
                        DefaultRoughnessType = RoughnessType.Manning
                    }
                };

                var roughnessNetworkCoverage = mainRoughnessSection.RoughnessNetworkCoverage;
                roughnessNetworkCoverage.Locations.AddValues(new[] {new NetworkLocation(channel, 10)});
                roughnessNetworkCoverage.Components[RoughnessValueComponentIndex].SetValues(new[]
                {
                    12.3
                });
                roughnessNetworkCoverage.Components[RoughnessTypeComponentIndex].SetValues(new[]
                {
                    (int) RoughnessType.DeBosBijkerk
                });

                hydroNetwork.Branches.Add(channel);

                var channelFrictionDefinition = fmModel.ChannelFrictionDefinitions.First();

                // Preconditions
                Assert.AreEqual(ChannelFrictionSpecificationType.ModelSettings, channelFrictionDefinition.SpecificationType);

                // When
                new SobekToWaterFlowFMRoughnessConverter().ConvertSobekRoughnessToWaterFlowFmRoughness(
                    fmModel.ChannelFrictionDefinitions,
                    mainRoughnessSection,
                    hydroNetwork);

                // Then
                Assert.AreEqual(ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition, channelFrictionDefinition.SpecificationType);

                var spatialChannelFrictionDefinition = channelFrictionDefinition.SpatialChannelFrictionDefinition;
                Assert.AreEqual(RoughnessType.DeBosBijkerk, spatialChannelFrictionDefinition.Type);
                Assert.AreEqual(RoughnessFunction.Constant, spatialChannelFrictionDefinition.FunctionType);
                Assert.AreEqual(1, spatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.Count);

                var constantSpatialChannelFrictionDefinition = spatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.ElementAt(0);
                Assert.AreEqual(10, constantSpatialChannelFrictionDefinition.Chainage);
                Assert.AreEqual(12.3, constantSpatialChannelFrictionDefinition.Value);
            }
        }

        [Test]
        public void GivenChannelWithMultipleLocationsInMainSection_WhenConvertingRoughness_ThenChannelFrictionDefinitionsSetToSpatialChannelFrictionDefinitionWithExpectedValues()
        {
            // Given
            using (var fmModel = new WaterFlowFMModel())
            {
                var channel = new Channel();
                var hydroNetwork = fmModel.Network;
                var mainSectionType = new CrossSectionSectionType
                {
                    Name = "Main"
                };

                var mainRoughnessSection = new RoughnessSection(mainSectionType, hydroNetwork)
                {
                    RoughnessNetworkCoverage =
                    {
                        DefaultRoughnessType = RoughnessType.Manning
                    }
                };

                var roughnessNetworkCoverage = mainRoughnessSection.RoughnessNetworkCoverage;
                roughnessNetworkCoverage.Locations.AddValues(new[] { new NetworkLocation(channel, 10) });
                roughnessNetworkCoverage.Locations.AddValues(new[] { new NetworkLocation(channel, 20) });
                roughnessNetworkCoverage.Components[RoughnessValueComponentIndex].SetValues(new[]
                {
                    12.3, 45.6
                });
                roughnessNetworkCoverage.Components[RoughnessTypeComponentIndex].SetValues(new[]
                {
                    (int) RoughnessType.DeBosBijkerk,
                    (int) RoughnessType.DeBosBijkerk
                });

                hydroNetwork.Branches.Add(channel);

                var channelFrictionDefinition = fmModel.ChannelFrictionDefinitions.First();

                // Preconditions
                Assert.AreEqual(ChannelFrictionSpecificationType.ModelSettings, channelFrictionDefinition.SpecificationType);

                // When
                new SobekToWaterFlowFMRoughnessConverter().ConvertSobekRoughnessToWaterFlowFmRoughness(
                    fmModel.ChannelFrictionDefinitions,
                    mainRoughnessSection,
                    hydroNetwork);

                // Then
                Assert.AreEqual(ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition, channelFrictionDefinition.SpecificationType);

                var spatialChannelFrictionDefinition = channelFrictionDefinition.SpatialChannelFrictionDefinition;
                Assert.AreEqual(RoughnessType.DeBosBijkerk, spatialChannelFrictionDefinition.Type);
                Assert.AreEqual(RoughnessFunction.Constant, spatialChannelFrictionDefinition.FunctionType);
                Assert.AreEqual(2, spatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.Count);

                var constantSpatialChannelFrictionDefinition1 = spatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.ElementAt(0);
                Assert.AreEqual(10, constantSpatialChannelFrictionDefinition1.Chainage);
                Assert.AreEqual(12.3, constantSpatialChannelFrictionDefinition1.Value);

                var constantSpatialChannelFrictionDefinition2 = spatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.ElementAt(1);
                Assert.AreEqual(20, constantSpatialChannelFrictionDefinition2.Chainage);
                Assert.AreEqual(45.6, constantSpatialChannelFrictionDefinition2.Value);
            }
        }

        [Test]
        public void GivenChannelWithFunctionOfQInMainSection_WhenConvertingRoughness_ThenChannelFrictionDefinitionSetToSpatialChannelFrictionDefinitionWithExpectedValues()
        {
            // Given
            using (var fmModel = new WaterFlowFMModel())
            {
                var channel = new Channel();
                var hydroNetwork = fmModel.Network;
                var mainSectionType = new CrossSectionSectionType
                {
                    Name = "Main"
                };

                var mainRoughnessSection = new RoughnessSection(mainSectionType, hydroNetwork)
                {
                    RoughnessNetworkCoverage =
                    {
                        DefaultRoughnessType = RoughnessType.Manning
                    }
                };

                var roughnessNetworkCoverage = mainRoughnessSection.RoughnessNetworkCoverage;

                var firstLocation = 1.33;
                var secondLocation = 7.88;
                var functionValues = new[] { 33.2, 44.02 };
                var roughnessValues = new[] { 1.8, 2.77, 3.333, 4.444 };
                var roughnessTypes = new[] { (int) RoughnessType.DeBosBijkerk, (int) RoughnessType.DeBosBijkerk };
                roughnessNetworkCoverage.Locations.AddValues(new[] { new NetworkLocation(channel, firstLocation) });
                roughnessNetworkCoverage.Locations.AddValues(new[] { new NetworkLocation(channel, secondLocation) });
                roughnessNetworkCoverage.Components[ChainageArgumentIndex].SetValues(functionValues);
                roughnessNetworkCoverage.Components[RoughnessTypeComponentIndex].SetValues(roughnessTypes);
                
                var functionOfQ = RoughnessSection.DefineFunctionOfQ();
                functionOfQ.Arguments[ChainageArgumentIndex].SetValues(new[] { firstLocation, secondLocation });
                functionOfQ.Arguments[FunctionArgumentIndex].SetValues(functionValues);
                functionOfQ.Components[RoughnessValueComponentIndex].SetValues(roughnessValues);
                mainRoughnessSection.AddQRoughnessFunctionToBranch(channel, functionOfQ);

                hydroNetwork.Branches.Add(channel);

                var channelFrictionDefinition = fmModel.ChannelFrictionDefinitions.First();

                // Preconditions
                Assert.AreEqual(ChannelFrictionSpecificationType.ModelSettings, channelFrictionDefinition.SpecificationType);

                // When
                new SobekToWaterFlowFMRoughnessConverter().ConvertSobekRoughnessToWaterFlowFmRoughness(
                    fmModel.ChannelFrictionDefinitions,
                    mainRoughnessSection,
                    hydroNetwork);

                // Then
                Assert.AreEqual(ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition, channelFrictionDefinition.SpecificationType);

                var spatialChannelFrictionDefinition = channelFrictionDefinition.SpatialChannelFrictionDefinition;
                Assert.AreEqual(RoughnessType.DeBosBijkerk, spatialChannelFrictionDefinition.Type);
                Assert.AreEqual(RoughnessFunction.FunctionOfQ, spatialChannelFrictionDefinition.FunctionType);

                var function = spatialChannelFrictionDefinition.Function;
                Assert.That(function, Is.Not.Null);
                Assert.That(function.Arguments[ChainageArgumentIndex].GetValues(), Is.EqualTo(new[] { firstLocation, secondLocation }));
                Assert.That(function.Arguments[FunctionArgumentIndex].GetValues(), Is.EqualTo(functionValues));
                Assert.That(function.Components[RoughnessValueComponentIndex].GetValues(), Is.EqualTo(roughnessValues));
            }
        }

        [Test]
        public void GivenChannelWithFunctionOfHInMainSection_WhenConvertingRoughness_ThenChannelFrictionDefinitionSetToSpatialChannelFrictionDefinitionWithExpectedValues()
        {
            // Given
            using (var fmModel = new WaterFlowFMModel())
            {
                var channel = new Channel();
                var hydroNetwork = fmModel.Network;
                var mainSectionType = new CrossSectionSectionType
                {
                    Name = "Main"
                };

                var mainRoughnessSection = new RoughnessSection(mainSectionType, hydroNetwork)
                {
                    RoughnessNetworkCoverage =
                    {
                        DefaultRoughnessType = RoughnessType.Manning
                    }
                };

                var roughnessNetworkCoverage = mainRoughnessSection.RoughnessNetworkCoverage;

                var firstLocation = 1.33;
                var secondLocation = 7.88;
                var functionValues = new[] { 33.2, 44.02 };
                var roughnessValues = new[] { 1.8, 2.77, 3.333, 4.444 };
                var roughnessTypes = new[] { (int) RoughnessType.DeBosBijkerk, (int) RoughnessType.DeBosBijkerk };
                roughnessNetworkCoverage.Locations.AddValues(new[] { new NetworkLocation(channel, firstLocation) });
                roughnessNetworkCoverage.Locations.AddValues(new[] { new NetworkLocation(channel, secondLocation) });
                roughnessNetworkCoverage.Components[ChainageArgumentIndex].SetValues(functionValues);
                roughnessNetworkCoverage.Components[RoughnessTypeComponentIndex].SetValues(roughnessTypes);

                var functionOfH = RoughnessSection.DefineFunctionOfH();
                functionOfH.Arguments[ChainageArgumentIndex].SetValues(new[] { firstLocation, secondLocation });
                functionOfH.Arguments[FunctionArgumentIndex].SetValues(functionValues);
                functionOfH.Components[RoughnessValueComponentIndex].SetValues(roughnessValues);
                mainRoughnessSection.AddHRoughnessFunctionToBranch(channel, functionOfH);

                hydroNetwork.Branches.Add(channel);

                var channelFrictionDefinition = fmModel.ChannelFrictionDefinitions.First();

                // Preconditions
                Assert.AreEqual(ChannelFrictionSpecificationType.ModelSettings, channelFrictionDefinition.SpecificationType);

                // When
                new SobekToWaterFlowFMRoughnessConverter().ConvertSobekRoughnessToWaterFlowFmRoughness(
                    fmModel.ChannelFrictionDefinitions,
                    mainRoughnessSection,
                    hydroNetwork);

                // Then
                Assert.AreEqual(ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition, channelFrictionDefinition.SpecificationType);

                var spatialChannelFrictionDefinition = channelFrictionDefinition.SpatialChannelFrictionDefinition;
                Assert.AreEqual(RoughnessType.DeBosBijkerk, spatialChannelFrictionDefinition.Type);
                Assert.AreEqual(RoughnessFunction.FunctionOfH, spatialChannelFrictionDefinition.FunctionType);

                var function = spatialChannelFrictionDefinition.Function;
                Assert.That(function, Is.Not.Null);
                Assert.That(function.Arguments[ChainageArgumentIndex].GetValues(), Is.EqualTo(new[] { firstLocation, secondLocation }));
                Assert.That(function.Arguments[FunctionArgumentIndex].GetValues(), Is.EqualTo(functionValues));
                Assert.That(function.Components[RoughnessValueComponentIndex].GetValues(), Is.EqualTo(roughnessValues));
            }
        }
    }
}