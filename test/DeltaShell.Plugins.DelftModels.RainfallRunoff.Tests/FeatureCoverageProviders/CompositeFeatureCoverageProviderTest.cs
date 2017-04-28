using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.FeatureCoverageProviders;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FeatureCoverageProviders;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.FeatureCoverageProviders
{
    [TestFixture]
    public class CompositeFeatureCoverageProviderTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        public void CreateCompositeProvider()
        {
            var model = new RainfallRunoffModel();
            var unpavedProvider = new UnpavedFeatureCoverageProvider(model);
            var modelOutputProvider = new ModelOutputFeatureCoverageProvider(model);

            var compositeProvider = new CompositeFeatureCoverageProvider(new IFeatureCoverageProvider[]
                                                                    {
                                                                        unpavedProvider,
                                                                        modelOutputProvider
                                                                    });

            Assert.IsNotNull(compositeProvider.FeatureCoverageNames);
            var numCompositeCoverages = compositeProvider.FeatureCoverageNames.Count();

            var numPolderCoverages = unpavedProvider.FeatureCoverageNames.Count();
            var numModelOutputCoverages = modelOutputProvider.FeatureCoverageNames.Count();

            Assert.AreEqual(numPolderCoverages + numModelOutputCoverages, numCompositeCoverages);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetCoverageThroughCompositeProvider()
        {
            var model = new RainfallRunoffModel();
            var unpavedProvider = new UnpavedFeatureCoverageProvider(model);
            var modelOutputProvider = new ModelOutputFeatureCoverageProvider(model);

            var compositeProvider = new CompositeFeatureCoverageProvider(new IFeatureCoverageProvider[]
                                                                    {
                                                                        unpavedProvider,
                                                                        modelOutputProvider
                                                                    });

            var name = modelOutputProvider.FeatureCoverageNames.First();

            Assert.AreSame(modelOutputProvider.GetFeatureCoverageByName(name),
                           compositeProvider.GetFeatureCoverageByName(name));
        }
    }
}