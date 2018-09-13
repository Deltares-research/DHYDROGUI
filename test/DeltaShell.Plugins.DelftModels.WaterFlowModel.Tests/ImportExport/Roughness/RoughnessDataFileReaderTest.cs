using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Roughness;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Roughness
{
    [TestFixture]
    public class RoughnessDataFileReaderTest
    {
        [Test]
        public void TestRoughnessDataFileReader_With_Calibrated_RoughnessSectionFile()
        {

            var network = HydroNetworkHelper.GetSnakeHydroNetwork(3);
            var crossSectionSectionType = new CrossSectionSectionType {Name = "Main"};
            var roughnessSection = new RoughnessSection(crossSectionSectionType, network);

            var roughnessFile = TestHelper.GetTestFilePath(@"FileReaders/roughness-Main.ini");
            
            //check original defaults:
            roughnessSection.SetDefaults(RoughnessType.DeBosAndBijkerk, 801.0d);
            Assert.That(roughnessSection.RoughnessNetworkCoverage.DefaultRoughnessType, Is.EqualTo(RoughnessType.DeBosAndBijkerk));
            Assert.That(roughnessSection.RoughnessNetworkCoverage.DefaultValue, Is.EqualTo(801.0).Within(0.0001));

            //check constant values:
            Assert.That(roughnessSection.RoughnessNetworkCoverage.Locations.Values.Count, Is.EqualTo(0));

            //no functions are set for the roughness section
            foreach (var branch in network.Branches)
            {
                //roughnessSection.FunctionOfH(branch);
                Assert.That(() => roughnessSection.FunctionOfH(branch), Throws.Exception.TypeOf<KeyNotFoundException>());
                Assert.That(() => roughnessSection.FunctionOfQ(branch), Throws.Exception.TypeOf<KeyNotFoundException>());
            }
            
            RoughnessDataFileReader.ReadFile(roughnessFile, network, new[] {roughnessSection}, true);
            CheckResults(roughnessSection, network);
            
            //re-read file & check to see if no duplicates are created
            RoughnessDataFileReader.ReadFile(roughnessFile, network, new[] { roughnessSection }, true);
            CheckResults(roughnessSection, network);
        }

        [Test]
        public void ReadReverseRoughnessFile()
        {
            var network = (INetwork)MockRepository.GenerateStrictMock(typeof(INetwork), new[] { typeof(INotifyPropertyChanged), typeof(INotifyCollectionChanged) });

            var branches = new EventedList<IBranch>{ new Branch{Name = "branch1"}};
            network.Expect(n => n.Branches).Return(branches).Repeat.Any();
            network.Expect(n => n.CoordinateSystem).Return(null).Repeat.Any();
            ((INotifyCollectionChanged)network).Expect(n => n.CollectionChanged += null).IgnoreArguments().Repeat.Twice();

            network.Replay();

            var path = TestHelper.GetTestFilePath(@"FileReaders\ReverseRoughness.ini");

            var orginalSection = new RoughnessSection(new CrossSectionSectionType{Name = "Test"}, network);

            var roughnessSections = new List<RoughnessSection> {orginalSection};

            RoughnessDataFileReader.ReadFile(path, network, roughnessSections);

            Assert.AreEqual(2,roughnessSections.Count);
            var reversedSection = roughnessSections[1] as ReverseRoughnessSection;
            Assert.NotNull(reversedSection);

            Assert.AreEqual("Test (Reversed)", reversedSection.Name);
            Assert.AreEqual(true, reversedSection.Reversed);
            Assert.AreEqual(false, reversedSection.UseNormalRoughness);
            Assert.AreEqual(RoughnessType.Manning, reversedSection.GetDefaultRoughnessType());
            Assert.AreEqual(41, reversedSection.GetDefaultRoughnessValue());

            var coverage = reversedSection.RoughnessNetworkCoverage;
            Assert.AreEqual(InterpolationType.Linear, coverage.Arguments[0].InterpolationType);
            Assert.AreEqual(1, coverage.Locations.Values.Count);
        }

        [Test]
        public void ReadReverseRoughnessFileWithUseNormalRoughness()
        {
            var network = (INetwork)MockRepository.GenerateStrictMock(typeof(INetwork), new[] { typeof(INotifyPropertyChanged), typeof(INotifyCollectionChanged) });

            network.Expect(n => n.Branches).Return(new EventedList<IBranch>()).Repeat.Any();
            network.Expect(n => n.CoordinateSystem).Return(null).Repeat.Any();
            ((INotifyCollectionChanged)network).Expect(n => n.CollectionChanged += null).IgnoreArguments().Repeat.Twice();

            network.Replay();

            var path = TestHelper.GetTestFilePath(@"FileReaders\ReverseRoughnessUseNormalRoughness.ini");

            var orginalSection = new RoughnessSection(new CrossSectionSectionType { Name = "Test" }, network);
            orginalSection.SetDefaults(RoughnessType.WhiteColebrook, 2.2);
            var roughnessSections = new List<RoughnessSection> { orginalSection };

            RoughnessDataFileReader.ReadFile(path, network, roughnessSections);

            Assert.AreEqual(2, roughnessSections.Count);
            var reversedSection = roughnessSections[1] as ReverseRoughnessSection;
            Assert.NotNull(reversedSection);

            Assert.AreEqual("Test (Reversed)", reversedSection.Name);
            Assert.AreEqual(true, reversedSection.Reversed);
            Assert.AreEqual(true, reversedSection.UseNormalRoughness);
            Assert.AreEqual(RoughnessType.WhiteColebrook, reversedSection.GetDefaultRoughnessType());
            Assert.AreEqual(2.2, reversedSection.GetDefaultRoughnessValue());
        }

        private static void CheckResults(RoughnessSection roughnessSection, IHydroNetwork network)
        {
            //check new defaults:
            Assert.That(roughnessSection.RoughnessNetworkCoverage.DefaultRoughnessType, Is.EqualTo(RoughnessType.Manning));
            Assert.That(roughnessSection.RoughnessNetworkCoverage.DefaultValue, Is.EqualTo(555.0).Within(0.0001));

            //check constant values:
            Assert.That(roughnessSection.RoughnessNetworkCoverage.Locations.Values.Count, Is.EqualTo(3));
            var constFunc =
                ((DelftTools.Functions.Generic.MultiDimensionalArray<GeoAPI.Extensions.Coverages.INetworkLocation>)
                    (roughnessSection.RoughnessNetworkCoverage.Locations.Values)).FirstOrDefault();
            Assert.That(constFunc, Is.Not.Null);
            var firstBranch = network.Branches.ElementAtOrDefault(0);
            Assert.That(firstBranch, Is.Not.Null);
            Assert.That(constFunc.Branch.Name, Is.EqualTo(firstBranch.Name));
            Assert.That(constFunc.Chainage, Is.EqualTo(75.000).Within(0.0001));
            Assert.That(roughnessSection.RoughnessNetworkCoverage[constFunc], Is.EqualTo(66.000).Within(0.0001));
            Assert.That(roughnessSection.EvaluateRoughnessType(constFunc), Is.EqualTo(RoughnessType.StricklerKn));

            //check waterlevel values:
            var expectedBranchWithFunctionOfHForWaterLevel = network.Branches.ElementAtOrDefault(1);
            Assert.That(expectedBranchWithFunctionOfHForWaterLevel, Is.Not.Null);
            Assert.That(() => roughnessSection.FunctionOfH(expectedBranchWithFunctionOfHForWaterLevel), Throws.Nothing);
            var roughness =
                roughnessSection.FunctionOfH(expectedBranchWithFunctionOfHForWaterLevel).Components.FirstOrDefault();
            Assert.That(roughness, Is.Not.Null);
            var waterlevels = ((DelftTools.Functions.Generic.MultiDimensionalArray<double>) (roughness.Values));
            Assert.That(waterlevels.Count, Is.EqualTo(4));
            Assert.That(waterlevels[0], Is.EqualTo(55.000).Within(0.0001));
            Assert.That(waterlevels[1], Is.EqualTo(5050.000).Within(0.0001));
            Assert.That(waterlevels[2], Is.EqualTo(5051.000).Within(0.0001));
            Assert.That(waterlevels[3], Is.EqualTo(5052.000).Within(0.0001));

            var chainage =
                roughnessSection.FunctionOfH(expectedBranchWithFunctionOfHForWaterLevel).Arguments.ElementAtOrDefault(0);
            Assert.That(chainage, Is.Not.Null);
            var chainageValue =
                ((DelftTools.Functions.Generic.MultiDimensionalArray<double>) chainage.Values).FirstOrDefault();
            Assert.That(chainageValue, Is.Not.Null);
            Assert.That(chainageValue, Is.EqualTo(80.000).Within(0.0001));

            Assert.That(
                roughnessSection.EvaluateRoughnessType(new NetworkLocation(expectedBranchWithFunctionOfHForWaterLevel,
                    chainageValue)), Is.EqualTo(RoughnessType.StricklerKs));

            var time =
                roughnessSection.FunctionOfH(expectedBranchWithFunctionOfHForWaterLevel).Arguments.ElementAtOrDefault(1);
            Assert.That(time, Is.Not.Null);
            var timeValues = ((DelftTools.Functions.Generic.MultiDimensionalArray<double>) (time.Values));
            Assert.That(timeValues.Count, Is.EqualTo(4));
            Assert.That(timeValues[0], Is.EqualTo(11.000).Within(0.0001));
            Assert.That(timeValues[1], Is.EqualTo(110.000).Within(0.0001));
            Assert.That(timeValues[2], Is.EqualTo(111.000).Within(0.0001));
            Assert.That(timeValues[3], Is.EqualTo(112.000).Within(0.0001));


            //check waterdischarge
            var expectedBranchWithFunctionOfQForWaterDischarge = network.Branches.ElementAtOrDefault(2);
            Assert.That(expectedBranchWithFunctionOfQForWaterDischarge, Is.Not.Null);
            Assert.That(() => roughnessSection.FunctionOfQ(expectedBranchWithFunctionOfQForWaterDischarge),
                Throws.Nothing);
            roughness =
                roughnessSection.FunctionOfQ(expectedBranchWithFunctionOfQForWaterDischarge).Components.FirstOrDefault();
            Assert.That(roughness, Is.Not.Null);
            var waterdischarge = ((DelftTools.Functions.Generic.MultiDimensionalArray<double>) (roughness.Values));
            Assert.That(waterdischarge.Count, Is.EqualTo(3));
            Assert.That(waterdischarge[0], Is.EqualTo(0.000).Within(0.0001));
            Assert.That(waterdischarge[1], Is.EqualTo(500.000).Within(0.0001));
            Assert.That(waterdischarge[2], Is.EqualTo(750.000).Within(0.0001));


            chainage =
                roughnessSection.FunctionOfQ(expectedBranchWithFunctionOfQForWaterDischarge)
                    .Arguments.ElementAtOrDefault(0);
            Assert.That(chainage, Is.Not.Null);
            chainageValue =
                ((DelftTools.Functions.Generic.MultiDimensionalArray<double>) chainage.Values).FirstOrDefault();
            Assert.That(chainageValue, Is.Not.Null);
            Assert.That(chainageValue, Is.EqualTo(90.000).Within(0.0001));

            Assert.That(
                roughnessSection.EvaluateRoughnessType(new NetworkLocation(expectedBranchWithFunctionOfQForWaterDischarge,
                    chainageValue)), Is.EqualTo(RoughnessType.WhiteColebrook));

            time = roughnessSection.FunctionOfQ(expectedBranchWithFunctionOfQForWaterDischarge).Arguments.ElementAtOrDefault(1);
            Assert.That(time, Is.Not.Null);
            timeValues = ((DelftTools.Functions.Generic.MultiDimensionalArray<double>) (time.Values));
            Assert.That(timeValues.Count, Is.EqualTo(3));
            Assert.That(timeValues[0], Is.EqualTo(2.000).Within(0.0001));
            Assert.That(timeValues[1], Is.EqualTo(5.000).Within(0.0001));
            Assert.That(timeValues[2], Is.EqualTo(7.000).Within(0.0001));
        }
    }
}