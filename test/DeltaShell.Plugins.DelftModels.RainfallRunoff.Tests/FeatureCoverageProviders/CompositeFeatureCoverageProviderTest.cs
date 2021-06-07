using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.FeatureCoverageProviders;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FeatureCoverageProviders;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.FeatureCoverageProviders
{
    [TestFixture]
    public class CompositeFeatureCoverageProviderTest
    {
        private static IEnumerable<TestCaseData> ConstructorArgumentNullCases()
        {
            yield return new TestCaseData(Enumerable.Empty<IFeatureCoverageProvider>(), null, "model");
            yield return new TestCaseData(null, Substitute.For<IRainfallRunoffModel>(), "providers");
        }
        
        [Test]
        [TestCaseSource(nameof(ConstructorArgumentNullCases))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(IEnumerable<IFeatureCoverageProvider> providers, IRainfallRunoffModel model, string expParamName)
        {
            // Call
            void Call() => new CompositeFeatureCoverageProvider(providers, model);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParamName));
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Setup
            var model = Substitute.For<IRainfallRunoffModel>();
            
            // Call
            var coverageProvider = new CompositeFeatureCoverageProvider(Enumerable.Empty<IFeatureCoverageProvider>(), model);
            
            // Assert
            Assert.That(coverageProvider.Model, Is.SameAs(model));
        }
        
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
                                                                    }, model);

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
                                                                    }, model);

            var name = modelOutputProvider.FeatureCoverageNames.First();

            Assert.AreSame(modelOutputProvider.GetFeatureCoverageByName(name),
                           compositeProvider.GetFeatureCoverageByName(name));
        }
    }
}