using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using NUnit.Framework;
using System;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM;
using NetTopologySuite.Extensions.Coverages;

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

        // [Test]
        // public void GivenABranchThatHasRoughnessDefinedOnMultipleSections_WhenConverting_ThenSpecificationTypeIsSetToRoughnessSections()
        // {
        //     using (var fmModel = new WaterFlowFMModel())
        //     {
        //         // Given
        //         fmModel.Network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
        //         var branch = fmModel.Network.Branches.FirstOrDefault();
        //         Assert.That(branch, Is.Not.Null);
        //
        //         var roughnessSection001 = new RoughnessSection(new CrossSectionSectionType() {Name = "Section001"}, fmModel.Network);
        //         var roughnessSection002 = new RoughnessSection(new CrossSectionSectionType() {Name = "Section002"}, fmModel.Network);
        //         var roughnessSections = fmModel.RoughnessSections;
        //         roughnessSections.Add(roughnessSection001);
        //         roughnessSections.Add(roughnessSection002);
        //
        //         roughnessSection001.RoughnessNetworkCoverage.Locations.AddValues(new[] {new NetworkLocation(branch, 10)});
        //         roughnessSection002.RoughnessNetworkCoverage.Locations.AddValues(new[] {new NetworkLocation(branch, 20)});
        //
        //         // When
        //         var converter = new SobekToWaterFlowFMRoughnessConverter();
        //         converter.ConvertSobekRoughnessToWaterFlowFmRoughness(fmModel.ChannelFrictionDefinitions, roughnessSections, fmModel.Network);
        //
        //         // Then
        //         var channelFrictionDefinition = fmModel.ChannelFrictionDefinitions.FirstOrDefault();
        //         Assert.That(channelFrictionDefinition, Is.Not.Null);
        //         Assert.That(channelFrictionDefinition.SpecificationType, Is.EqualTo(ChannelFrictionSpecificationType.RoughnessSections));
        //
        //         // make sure the definitions are not removed from the roughness sections
        //         Assert.That(roughnessSection001.RoughnessNetworkCoverage.GetLocationsForBranch(branch), Is.Not.Empty);
        //         Assert.That(roughnessSection002.RoughnessNetworkCoverage.GetLocationsForBranch(branch), Is.Not.Empty);
        //     }
        // }
        //
        // [Test]
        // public void GivenABranchDefinedInOneRoughnessSectionWithASingleNetworkLocation_WhenConverting_ThenSpecificationTypeIsSetToBranchConstant()
        // {
        //     using (var fmModel = new WaterFlowFMModel())
        //     {
        //         // Given
        //         fmModel.Network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
        //         var branch = fmModel.Network.Branches.FirstOrDefault();
        //         Assert.That(branch, Is.Not.Null);
        //
        //         var roughnessSection001 = new RoughnessSection(new CrossSectionSectionType() {Name = "Section001"}, fmModel.Network);
        //         fmModel.RoughnessSections.Add(roughnessSection001);
        //         var roughnessNetworkCoverage = roughnessSection001.RoughnessNetworkCoverage;
        //         roughnessNetworkCoverage.Locations.AddValues(new[] {new NetworkLocation(branch, 10)});
        //         roughnessNetworkCoverage.Components[RoughnessValueComponentIndex].SetValues(new[] {123.456});
        //         roughnessNetworkCoverage.Components[RoughnessTypeComponentIndex].SetValues(new[] {(int) RoughnessType.WhiteColebrook});
        //
        //         // When
        //         var converter = new SobekToWaterFlowFMRoughnessConverter();
        //         converter.ConvertSobekRoughnessToWaterFlowFmRoughness(fmModel.ChannelFrictionDefinitions, fmModel.RoughnessSections, fmModel.Network);
        //
        //         // Then
        //         var channelFrictionDefinition = fmModel.ChannelFrictionDefinitions.FirstOrDefault();
        //         Assert.That(channelFrictionDefinition, Is.Not.Null);
        //         Assert.That(channelFrictionDefinition.SpecificationType, Is.EqualTo(ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition));
        //         var constantDefinition = channelFrictionDefinition.ConstantChannelFrictionDefinition;
        //         Assert.That(constantDefinition, Is.Not.Null);
        //         Assert.That(constantDefinition.Value, Is.EqualTo(123.456));
        //         Assert.That(constantDefinition.Type, Is.EqualTo(RoughnessType.WhiteColebrook));
        //
        //         // make sure the definition is removed from the roughness section
        //         Assert.That(roughnessNetworkCoverage.GetLocationsForBranch(branch), Is.Empty);
        //     }
        // }
        //
        // [Test]
        // public void GivenABranchDefinedInOneRoughnessSectionWithMultipleNetworkLocationsAndConstantFunction_WhenConverting_ThenSetsCorrectSpatialData()
        // {
        //     using (var fmModel = new WaterFlowFMModel())
        //     {
        //         // Given
        //         fmModel.Network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
        //         var branch = fmModel.Network.Branches.FirstOrDefault();
        //         Assert.That(branch, Is.Not.Null);
        //
        //         var roughnessSection001 = new RoughnessSection(new CrossSectionSectionType() {Name = "Section001"}, fmModel.Network);
        //         fmModel.RoughnessSections.Add(roughnessSection001);
        //
        //         var roughnessNetworkCoverage = roughnessSection001.RoughnessNetworkCoverage;
        //         roughnessNetworkCoverage.Locations.AddValues(new[] {new NetworkLocation(branch, 10)});
        //         roughnessNetworkCoverage.Locations.AddValues(new[] {new NetworkLocation(branch, 20)});
        //         roughnessNetworkCoverage.Components[RoughnessValueComponentIndex].SetValues(new[] {11.0, 22.0});
        //         roughnessNetworkCoverage.Components[RoughnessTypeComponentIndex].SetValues(new[]
        //             {(int) RoughnessType.DeBosBijkerk, (int) RoughnessType.DeBosBijkerk});
        //
        //         // Precondition
        //         Assert.That(roughnessSection001.GetRoughnessFunctionType(branch), Is.EqualTo(RoughnessFunction.Constant));
        //
        //         // When
        //         var converter = new SobekToWaterFlowFMRoughnessConverter();
        //         converter.ConvertSobekRoughnessToWaterFlowFmRoughness(fmModel.ChannelFrictionDefinitions, fmModel.RoughnessSections, fmModel.Network);
        //
        //         // Then
        //         var channelFrictionDefinition = fmModel.ChannelFrictionDefinitions.FirstOrDefault();
        //         Assert.That(channelFrictionDefinition, Is.Not.Null);
        //         Assert.That(channelFrictionDefinition.SpecificationType, Is.EqualTo(ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition));
        //         var spatialDefinition = channelFrictionDefinition.SpatialChannelFrictionDefinition;
        //         Assert.That(spatialDefinition.FunctionType, Is.EqualTo(RoughnessFunction.Constant));
        //         Assert.That(spatialDefinition.Type, Is.EqualTo(RoughnessType.DeBosBijkerk));
        //         Assert.That(spatialDefinition.Function, Is.EqualTo(null));
        //         var constantSpatialDefinitions = spatialDefinition.ConstantSpatialChannelFrictionDefinitions;
        //         Assert.That(constantSpatialDefinitions, Is.Not.Null);
        //         var firstConstantSpatialDefinition = constantSpatialDefinitions.FirstOrDefault();
        //         Assert.That(firstConstantSpatialDefinition, Is.Not.Null);
        //         Assert.That(firstConstantSpatialDefinition.Chainage, Is.EqualTo(10));
        //         Assert.That(firstConstantSpatialDefinition.Value, Is.EqualTo(11.0));
        //         var secondConstantSpatialDefinition = constantSpatialDefinitions.LastOrDefault();
        //         Assert.That(secondConstantSpatialDefinition, Is.Not.Null);
        //         Assert.That(secondConstantSpatialDefinition.Chainage, Is.EqualTo(20));
        //         Assert.That(secondConstantSpatialDefinition.Value, Is.EqualTo(22.0));
        //
        //         // assert that friction definition for the branch is removed from the RoughnessSection
        //         Assert.That(roughnessNetworkCoverage.GetLocationsForBranch(branch), Is.Empty);
        //
        //     }
        // }
        //
        // [Test]
        // public void GivenABranchDefinedInOneRoughnessSectionWithMultipleNetworkLocationsAndFunctionOfH_WhenConverting_ThenSetsCorrectSpatialData()
        // {
        //     using (var fmModel = new WaterFlowFMModel())
        //     {
        //         // Given
        //         fmModel.Network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
        //         var branch = fmModel.Network.Branches.FirstOrDefault();
        //         Assert.That(branch, Is.Not.Null);
        //
        //         var roughnessSection001 = new RoughnessSection(new CrossSectionSectionType() {Name = "Section001"}, fmModel.Network);
        //         fmModel.RoughnessSections.Add(roughnessSection001);
        //
        //         var roughnessNetworkCoverage = roughnessSection001.RoughnessNetworkCoverage;
        //
        //         var firstLocation = 12.34;
        //         var secondLocation = 56.78;
        //         var functionValues = new[] {11.0, 22.0};
        //         var roughnessValues = new[] {0.1, 2.2, 3.2, 4.2};
        //         var roughnessTypes = new[] {(int) RoughnessType.Strickler, (int) RoughnessType.Strickler};
        //         roughnessNetworkCoverage.Locations.AddValues(new[] {new NetworkLocation(branch, firstLocation)});
        //         roughnessNetworkCoverage.Locations.AddValues(new[] {new NetworkLocation(branch, secondLocation)});
        //         roughnessNetworkCoverage.Components[ChainageArgumentIndex].SetValues(functionValues);
        //         roughnessNetworkCoverage.Components[RoughnessTypeComponentIndex].SetValues(roughnessTypes);
        //
        //         var functionOfH = RoughnessSection.DefineFunctionOfH();
        //         functionOfH.Arguments[ChainageArgumentIndex].SetValues(new[] {firstLocation, secondLocation});
        //         functionOfH.Arguments[FunctionArgumentIndex].SetValues(functionValues);
        //         functionOfH.Components[RoughnessValueComponentIndex].SetValues(roughnessValues);
        //         roughnessSection001.AddHRoughnessFunctionToBranch(branch, functionOfH);
        //
        //         // Precondition
        //         Assert.That(roughnessSection001.GetRoughnessFunctionType(branch), Is.EqualTo(RoughnessFunction.FunctionOfH));
        //
        //         // When
        //         var converter = new SobekToWaterFlowFMRoughnessConverter();
        //         converter.ConvertSobekRoughnessToWaterFlowFmRoughness(fmModel.ChannelFrictionDefinitions, fmModel.RoughnessSections, fmModel.Network);
        //
        //         // Then
        //         var channelFrictionDefinition = fmModel.ChannelFrictionDefinitions.FirstOrDefault();
        //         Assert.That(channelFrictionDefinition, Is.Not.Null);
        //         Assert.That(channelFrictionDefinition.SpecificationType, Is.EqualTo(ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition));
        //         
        //         var spatialDefinition = channelFrictionDefinition.SpatialChannelFrictionDefinition;
        //         Assert.That(spatialDefinition.FunctionType, Is.EqualTo(RoughnessFunction.FunctionOfH));
        //         Assert.That(spatialDefinition.Type, Is.EqualTo((RoughnessType) roughnessTypes[0]));
        //         Assert.That(spatialDefinition.ConstantSpatialChannelFrictionDefinitions, Is.Null);
        //         
        //         var spatialFunction = spatialDefinition.Function;
        //         Assert.That(spatialFunction, Is.Not.Null);
        //         Assert.That(spatialFunction.Arguments[ChainageArgumentIndex].GetValues(), Is.EqualTo(new[] {firstLocation, secondLocation}));
        //         Assert.That(spatialFunction.Arguments[FunctionArgumentIndex].GetValues(), Is.EqualTo(functionValues));
        //         Assert.That(spatialFunction.Components[RoughnessValueComponentIndex].GetValues(), Is.EqualTo(roughnessValues));
        //
        //         // assert that friction definition for the branch is removed from the RoughnessSection
        //         Assert.That(roughnessNetworkCoverage.GetLocationsForBranch(branch), Is.Empty);
        //         Assert.That(roughnessSection001.GetRoughnessFunctionType(branch), Is.EqualTo(RoughnessFunction.Constant));
        //         TestDelegate action = () => roughnessSection001.FunctionOfH(branch);
        //         Assert.Throws<KeyNotFoundException>(action);
        //     }
        // }
        //
        // [Test]
        // public void GivenABranchDefinedInOneRoughnessSectionWithMultipleNetworkLocationsAndFunctionOfQ_WhenConverting_ThenSetsCorrectSpatialData()
        // {
        //     using (var fmModel = new WaterFlowFMModel())
        //     {
        //         // Given
        //         fmModel.Network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
        //         var branch = fmModel.Network.Branches.FirstOrDefault();
        //         Assert.That(branch, Is.Not.Null);
        //
        //         var roughnessSection001 = new RoughnessSection(new CrossSectionSectionType() { Name = "Section001" }, fmModel.Network);
        //         fmModel.RoughnessSections.Add(roughnessSection001);
        //
        //         var roughnessNetworkCoverage = roughnessSection001.RoughnessNetworkCoverage;
        //
        //         var firstLocation = 1.33;
        //         var secondLocation = 7.88;
        //         var functionValues = new[] { 33.2, 44.02 };
        //         var roughnessValues = new[] { 1.8, 2.77, 3.333, 4.444 };
        //         var roughnessTypes = new[] { (int)RoughnessType.StricklerNikuradse, (int)RoughnessType.StricklerNikuradse };
        //         roughnessNetworkCoverage.Locations.AddValues(new[] { new NetworkLocation(branch, firstLocation) });
        //         roughnessNetworkCoverage.Locations.AddValues(new[] { new NetworkLocation(branch, secondLocation) });
        //         roughnessNetworkCoverage.Components[ChainageArgumentIndex].SetValues(functionValues);
        //         roughnessNetworkCoverage.Components[RoughnessTypeComponentIndex].SetValues(roughnessTypes);
        //
        //         var functionOfQ = RoughnessSection.DefineFunctionOfQ();
        //         functionOfQ.Arguments[ChainageArgumentIndex].SetValues(new[] { firstLocation, secondLocation });
        //         functionOfQ.Arguments[FunctionArgumentIndex].SetValues(functionValues);
        //         functionOfQ.Components[RoughnessValueComponentIndex].SetValues(roughnessValues);
        //         roughnessSection001.AddQRoughnessFunctionToBranch(branch, functionOfQ);
        //
        //         // Precondition
        //         Assert.That(roughnessSection001.GetRoughnessFunctionType(branch), Is.EqualTo(RoughnessFunction.FunctionOfQ));
        //
        //         // When
        //         var converter = new SobekToWaterFlowFMRoughnessConverter();
        //         converter.ConvertSobekRoughnessToWaterFlowFmRoughness(fmModel.ChannelFrictionDefinitions, fmModel.RoughnessSections, fmModel.Network);
        //
        //         // Then
        //         var channelFrictionDefinition = fmModel.ChannelFrictionDefinitions.FirstOrDefault();
        //         Assert.That(channelFrictionDefinition, Is.Not.Null);
        //         Assert.That(channelFrictionDefinition.SpecificationType, Is.EqualTo(ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition));
        //
        //         var spatialDefinition = channelFrictionDefinition.SpatialChannelFrictionDefinition;
        //         Assert.That(spatialDefinition.FunctionType, Is.EqualTo(RoughnessFunction.FunctionOfQ));
        //         Assert.That(spatialDefinition.Type, Is.EqualTo((RoughnessType)roughnessTypes[0]));
        //         Assert.That(spatialDefinition.ConstantSpatialChannelFrictionDefinitions, Is.Null);
        //
        //         var spatialFunction = spatialDefinition.Function;
        //         Assert.That(spatialFunction, Is.Not.Null);
        //         Assert.That(spatialFunction.Arguments[ChainageArgumentIndex].GetValues(), Is.EqualTo(new[] { firstLocation, secondLocation }));
        //         Assert.That(spatialFunction.Arguments[FunctionArgumentIndex].GetValues(), Is.EqualTo(functionValues));
        //         Assert.That(spatialFunction.Components[RoughnessValueComponentIndex].GetValues(), Is.EqualTo(roughnessValues));
        //
        //         // assert that friction definition for the branch is removed from the RoughnessSection
        //         Assert.That(roughnessNetworkCoverage.GetLocationsForBranch(branch), Is.Empty);
        //         Assert.That(roughnessSection001.GetRoughnessFunctionType(branch), Is.EqualTo(RoughnessFunction.Constant));
        //         TestDelegate action = () => roughnessSection001.FunctionOfQ(branch);
        //         Assert.Throws<KeyNotFoundException>(action);
        //     }
        // }
    }
}