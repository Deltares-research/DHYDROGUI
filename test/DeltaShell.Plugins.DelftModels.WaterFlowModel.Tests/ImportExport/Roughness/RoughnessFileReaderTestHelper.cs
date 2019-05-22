using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Roughness
{
    public class RoughnessFileReaderTestHelper
    {
        protected static void CheckResults(RoughnessSection roughnessSection, IHydroNetwork network)
        {
            //check new defaults:
            Assert.That(roughnessSection.RoughnessNetworkCoverage.DefaultRoughnessType, Is.EqualTo(RoughnessType.Manning));
            Assert.That(roughnessSection.RoughnessNetworkCoverage.DefaultValue, Is.EqualTo(555.0).Within(0.0001));

            //check constant values:
            Assert.That(roughnessSection.RoughnessNetworkCoverage.Locations.Values.Count, Is.EqualTo(3));
            var constFunc =
                ((MultiDimensionalArray<GeoAPI.Extensions.Coverages.INetworkLocation>)
                    roughnessSection.RoughnessNetworkCoverage.Locations.Values).FirstOrDefault();
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
            var waterlevels = (MultiDimensionalArray<double>)roughness.Values;
            Assert.That(waterlevels.Count, Is.EqualTo(4));
            Assert.That(waterlevels[0], Is.EqualTo(55.000).Within(0.0001));
            Assert.That(waterlevels[1], Is.EqualTo(5050.000).Within(0.0001));
            Assert.That(waterlevels[2], Is.EqualTo(5051.000).Within(0.0001));
            Assert.That(waterlevels[3], Is.EqualTo(5052.000).Within(0.0001));

            var chainage =
                roughnessSection.FunctionOfH(expectedBranchWithFunctionOfHForWaterLevel).Arguments.ElementAtOrDefault(0);
            Assert.That(chainage, Is.Not.Null);
            var chainageValue =
                ((MultiDimensionalArray<double>)chainage.Values).FirstOrDefault();
            Assert.That(chainageValue, Is.Not.Null);
            Assert.That(chainageValue, Is.EqualTo(80.000).Within(0.0001));

            Assert.That(
                roughnessSection.EvaluateRoughnessType(new NetworkLocation(expectedBranchWithFunctionOfHForWaterLevel,
                    chainageValue)), Is.EqualTo(RoughnessType.StricklerKs));

            var time =
                roughnessSection.FunctionOfH(expectedBranchWithFunctionOfHForWaterLevel).Arguments.ElementAtOrDefault(1);
            Assert.That(time, Is.Not.Null);
            var timeValues = (MultiDimensionalArray<double>)time.Values;
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
            var waterdischarge = (MultiDimensionalArray<double>)roughness.Values;
            Assert.That(waterdischarge.Count, Is.EqualTo(3));
            Assert.That(waterdischarge[0], Is.EqualTo(0.000).Within(0.0001));
            Assert.That(waterdischarge[1], Is.EqualTo(500.000).Within(0.0001));
            Assert.That(waterdischarge[2], Is.EqualTo(750.000).Within(0.0001));


            chainage =
                roughnessSection.FunctionOfQ(expectedBranchWithFunctionOfQForWaterDischarge)
                    .Arguments.ElementAtOrDefault(0);
            Assert.That(chainage, Is.Not.Null);
            chainageValue =
                ((MultiDimensionalArray<double>)chainage.Values).FirstOrDefault();
            Assert.That(chainageValue, Is.Not.Null);
            Assert.That(chainageValue, Is.EqualTo(90.000).Within(0.0001));

            Assert.That(
                roughnessSection.EvaluateRoughnessType(new NetworkLocation(expectedBranchWithFunctionOfQForWaterDischarge,
                    chainageValue)), Is.EqualTo(RoughnessType.WhiteColebrook));

            time = roughnessSection.FunctionOfQ(expectedBranchWithFunctionOfQForWaterDischarge).Arguments.ElementAtOrDefault(1);
            Assert.That(time, Is.Not.Null);
            timeValues = (MultiDimensionalArray<double>)time.Values;
            Assert.That(timeValues.Count, Is.EqualTo(3));
            Assert.That(timeValues[0], Is.EqualTo(2.000).Within(0.0001));
            Assert.That(timeValues[1], Is.EqualTo(5.000).Within(0.0001));
            Assert.That(timeValues[2], Is.EqualTo(7.000).Within(0.0001));
        }
    }
}
