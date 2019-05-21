using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    public class CatchmentCoverageMaintainerImplTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        public void AddCatchmentToNetworkAdjustsCoverage()
        {
            var rrModel = new RainfallRunoffModel { Name = "Test" };
            var featureCoverage = new FeatureCoverage("Test");

            featureCoverage.Arguments.Add(new Variable<IFeature>("Feature"));
            featureCoverage.Components.Add(new Variable<double>("Value"));

            new CatchmentCoverageMaintainer(rrModel).Initialize(featureCoverage);

            Assert.AreEqual(0, featureCoverage.Features.Count);

            rrModel.Basin.Catchments.Add(new Catchment{CatchmentType = CatchmentType.Paved});

            Assert.AreEqual(1, featureCoverage.Features.Count);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RemoveCatchmentFromNetworkAdjustsCoverage()
        {
            var rrModel = new RainfallRunoffModel { Name = "Test" };
            var featureCoverage = new FeatureCoverage("Test");

            featureCoverage.Arguments.Add(new Variable<IFeature>("Feature"));
            featureCoverage.Components.Add(new Variable<double>("Value"));

            new CatchmentCoverageMaintainer(rrModel).Initialize(featureCoverage);

            var catchment = new Catchment { CatchmentType = CatchmentType.Paved };
            rrModel.Basin.Catchments.Add(catchment);

            Assert.AreEqual(1, featureCoverage.Features.Count);

            rrModel.Basin.Catchments.Remove(catchment);

            Assert.AreEqual(0, featureCoverage.Features.Count);
        }
    }
}
