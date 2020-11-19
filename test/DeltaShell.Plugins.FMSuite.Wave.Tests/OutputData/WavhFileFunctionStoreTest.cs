using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using GeoAPI.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.OutputData
{
    [TestFixture]
    public class WavhFileFunctionStoreTest
    {
        private const string ncPath = "./WaveOutputDataHarvesterTest/wavh-Waves.nc";

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Constructor_ExpectedResults()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string localNcPath = tempDir.CopyTestDataFileToTempDirectory(ncPath);

                // Call
                var store = new WavhFileFunctionStore(localNcPath);

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
        public void ConstructedCoverages_ConfiguredCorrectly()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string localNcPath = tempDir.CopyTestDataFileToTempDirectory(ncPath);

                // Call
                var store = new WavhFileFunctionStore(localNcPath);

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

                    IVariable featureVariable = coverage.Arguments.LastOrDefault();
                    Assert.That(featureVariable, Is.Not.Null);

                    Assert.That(featureVariable.Name, Is.EqualTo("stations"));
                    Assert.That(featureVariable.IsEditable, Is.False);

                    Assert.That(featureVariable.Attributes["ncName"], Is.EqualTo("stations"));
                    Assert.That(featureVariable.Attributes["hasVariable"], Is.EqualTo("false"));
                }
            }
        }
    }
}