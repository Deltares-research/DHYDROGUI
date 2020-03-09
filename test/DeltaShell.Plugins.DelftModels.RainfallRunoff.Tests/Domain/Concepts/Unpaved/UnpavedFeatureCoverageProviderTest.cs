using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.FeatureCoverageProviders;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts.Unpaved
{
    [TestFixture]
    public class UnpavedFeatureCoverageProviderTest
    {
        private RainfallRunoffModel model;

        [SetUp]
        public void SetUp()
        {
            model = new RainfallRunoffModel();
            model.Basin.Catchments.Add(new Catchment {CatchmentType = CatchmentType.Unpaved});
            model.Basin.Catchments.Add(new Catchment {CatchmentType = CatchmentType.Unpaved});
            model.Basin.Catchments.Add(new Catchment {CatchmentType = CatchmentType.None});
        }

        [Test]
        public void GetCoverageNames()
        {
            var unpavedFeatureCoverageProvider = new UnpavedFeatureCoverageProvider(model);
            var names = unpavedFeatureCoverageProvider.FeatureCoverageNames.ToList();

            Assert.GreaterOrEqual(names.Count, 9); //number may grow in future
            Assert.AreEqual("Unpaved: Groundwater layer thickness", names.First()); //check description is used
        }

        [Test]
        [Category("Quarantine")]
        public void GetCoverages()
        {
            var unpavedFeatureCoverageProvider = new UnpavedFeatureCoverageProvider(model);
            var names = unpavedFeatureCoverageProvider.FeatureCoverageNames.ToList();
            var coverages = names.Select(unpavedFeatureCoverageProvider.GetFeatureCoverageByName).ToList();
            var featureCoverage = coverages.First();

            Assert.GreaterOrEqual(coverages.Count, 9);
            Assert.AreEqual("Unpaved: Groundwater layer thickness", featureCoverage.Name); //check description is used
            Assert.AreEqual(1, featureCoverage.Arguments.Count);
            Assert.AreEqual(typeof (IFeature), featureCoverage.FeatureVariable.ValueType);
            Assert.AreEqual(1, featureCoverage.Components.Count);
            Assert.AreEqual(2, featureCoverage.Features.Count);             //#unpaved catchments
            Assert.AreEqual(2, featureCoverage.Components[0].Values.Count); //one value for each unpaved catchments
            Assert.AreEqual(2, featureCoverage.Arguments[0].Values.Count);  //#unpaved catchments
        }
    }
}
