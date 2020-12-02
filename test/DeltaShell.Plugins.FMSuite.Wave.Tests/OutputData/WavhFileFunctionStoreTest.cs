using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.OutputData
{
    [TestFixture]
    public class WavhFileFunctionStoreTest
    {
        private const string ncPath = "./WaveOutputDataHarvesterTest/wavh-Waves.nc";

        [Test]
        public void Constructor_FeatureProviderNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new WavhFileFunctionStore("the_path", null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("featureContainer"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Constructor_ExpectedResults()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string localNcPath = tempDir.CopyTestDataFileToTempDirectory(ncPath);
                var featureContainer = Substitute.For<IWaveFeatureContainer>();

                // Call
                var store = new WavhFileFunctionStore(localNcPath, featureContainer);

                // Assert
                Assert.That(store, Is.InstanceOf<FMNetCdfFileFunctionStore>());

                Assert.That(store.DisableCaching, Is.True);

                Assert.That(store.Functions.Count, Is.EqualTo(11));
                string[] expectedFunctionNames =
                {
                    "Water depth (Depth)",
                    "Significant wave height (Hsig)",
                    "Mean wave direction (Dir)",
                    "Peak period (RTpeak)",
                    "Mean absolute wave period (Tm01)",
                    "Directional spreading of the waves (Dspr)",
                    "Rms-value of the maxima of the orbital velocity near the bottom (Ubot)",
                    "X component of the wind velocity (X-Windv)",
                    "Y component of the wind velocity (Y-Windv)",
                    "X component of the current velocity (X-Vel)",
                    "Y component of the current velocity (Y-Vel)",
                };

                string[] storeFunctionNames = store.Functions.Select(x => x.Name).ToArray();
                Assert.That(storeFunctionNames, Is.EquivalentTo(expectedFunctionNames));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCaseSource(nameof(ConstructedCoveragesCases))]
        public void ConstructedCoverages_ConfiguredCorrectly(IEventedList<Feature2DPoint> observationPoints, string expFeatureName)
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string localNcPath = tempDir.CopyTestDataFileToTempDirectory(ncPath);
                var featureContainer = Substitute.For<IWaveFeatureContainer>();

                featureContainer.ObservationPoints.Returns(observationPoints);

                // Call
                var store = new WavhFileFunctionStore(localNcPath, featureContainer);

                // Assert
                foreach (IFunction function in store.Functions)
                {
                    Assert.That(function.Store, Is.SameAs(store));

                    var coverage = function as IFeatureCoverage;
                    Assert.That(coverage, Is.Not.Null);

                    Assert.That(coverage.IsEditable, Is.False);
                    Assert.That(coverage.IsTimeDependent, Is.True);
                    Assert.That(coverage.Time.InterpolationType, Is.EqualTo(InterpolationType.Linear));

                    Assert.That(coverage.Time.Name, Is.EqualTo("Time"));
                    Assert.That(coverage.Time.Attributes["ncName"], Is.EqualTo("time"));
                    Assert.That(coverage.Time.Attributes["hasVariable"], Is.EqualTo("true"));
                    Assert.That(coverage.Time.Attributes, Contains.Key("ncRefDate"));
                    Assert.That(coverage.Time.IsEditable, Is.False);

                    var feature = (Feature2D) coverage.Features.Single();
                    Assert.That(feature.Name, Is.EqualTo(expFeatureName));
                    Assert.That(feature.Geometry.EqualsExact(new Point(3296.9479015919, 3694.42836468886), 1E-7));

                    IVariable featureVariable = coverage.Arguments.LastOrDefault();
                    Assert.That(featureVariable, Is.Not.Null);

                    Assert.That(featureVariable.Name, Is.EqualTo("stations"));
                    Assert.That(featureVariable.IsEditable, Is.False);

                    Assert.That(featureVariable.Attributes["ncName"], Is.EqualTo("stations"));
                    Assert.That(featureVariable.Attributes["hasVariable"], Is.EqualTo("false"));
                }
            }
        }

        private static IEnumerable<TestCaseData> ConstructedCoveragesCases()
        {
            const double x = 3296.9479015919;
            const double y = 3694.42836468886;

            yield return new TestCaseData(GetFeatures(new Point(x, y)),
                                          "model_feature");
            yield return new TestCaseData(GetFeatures(new Point(x - 5E-8, y - 5E-8)),
                                          "model_feature");
            yield return new TestCaseData(GetFeatures(new Point(x + 5E-8, y + 5E-8)),
                                          "model_feature");
            yield return new TestCaseData(GetFeatures(new Point(x - 1E-7, y - 1E-7)),
                                          "Station");
            yield return new TestCaseData(GetFeatures(new Point(x + 1E-7, y + 1E-7)),
                                          "Station");
            yield return new TestCaseData(GetFeatures(new Point((int) x, (int) y)),
                                          "Station");
            yield return new TestCaseData(new EventedList<Feature2DPoint>(),
                                          "Station");
        }

        private static IEventedList<Feature2DPoint> GetFeatures(IGeometry geom)
        {
            return new EventedList<Feature2DPoint>()
            {
                new Feature2DPoint
                {
                    Name = "model_feature",
                    Geometry = geom
                }
            };
        }
    }
}