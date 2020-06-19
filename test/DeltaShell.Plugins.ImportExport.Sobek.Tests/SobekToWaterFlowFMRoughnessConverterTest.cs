using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Roughness;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.Plugins.FMSuite.FlowFM;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

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
            var converter = new SobekToWaterFlowFMRoughnessConverter();

            TestDelegate action = () =>
            {
                converter.ConvertSobekRoughnessToWaterFlowFmRoughness(null, new RoughnessSection(new CrossSectionSectionType(), new HydroNetwork()));
            };
            
            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.AreEqual("channelFrictionDefinitions", exception.ParamName);
        }

        [Test]
        public void WhenRoughnessSectionsIsNull_ThenShouldThrowArgumentNullException()
        {
            var converter = new SobekToWaterFlowFMRoughnessConverter();

            TestDelegate action = () => converter.ConvertSobekRoughnessToWaterFlowFmRoughness(Enumerable.Empty<ChannelFrictionDefinition>(), null);
            
            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.AreEqual("defaultRoughnessSection", exception.ParamName);
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