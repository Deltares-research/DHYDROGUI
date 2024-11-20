using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.OutputData
{
    [TestFixture]
    public class WavmFileFunctionStoreTest
    {
        private const string wavmWavesPath = "./WaveOutputDataHarvesterTest/wavm-Waves.nc";
        private const string wavmWadPath = "./WaveOutputDataHarvesterTest/wavm-wad.nc";

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Constructor_ExpectedResults()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string localNcPath = tempDir.CopyTestDataFileToTempDirectory(wavmWavesPath);

                // Call
                var store = new WavmFileFunctionStore(localNcPath);

                // Assert
                Assert.That(store, Is.InstanceOf<FMNetCdfFileFunctionStore>());

                Assert.That(store.DisableCaching, Is.True);

                Assert.That(store.Grid, Is.Not.Null);
                Assert.That(store.Grid.Size1, Is.EqualTo(3));
                Assert.That(store.Grid.Size2, Is.EqualTo(3));

                Assert.That(store.Functions.Count, Is.EqualTo(27));
                string[] expectedFunctionNames =
                {
                    "hsign",
                    "dir",
                    "pdir",
                    "period",
                    "rtp",
                    "depth",
                    "veloc-x",
                    "veloc-y",
                    "transp-x",
                    "transp-y",
                    "dspr",
                    "dissip",
                    "leak",
                    "qb",
                    "ubot",
                    "steepw",
                    "wlength",
                    "tps",
                    "tm02",
                    "tmm10",
                    "dhsign",
                    "drtm01",
                    "setup",
                    "fx",
                    "fy",
                    "windu",
                    "windv",
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
                string localNcPath = tempDir.CopyTestDataFileToTempDirectory(wavmWavesPath);

                // Call
                var store = new WavmFileFunctionStore(localNcPath);

                // Assert
                foreach (IFunction function in store.Functions)
                {
                    Assert.That(function.Arguments[0].Name, Is.EqualTo("Time"));
                    Assert.That(function.Arguments[0].Attributes["ncName"], Is.EqualTo("time"));
                    Assert.That(function.Arguments[0].Attributes["hasVariable"], Is.EqualTo("true"));
                    Assert.That(function.Arguments[0].Attributes, Contains.Key("ncRefDate"));
                    Assert.That(function.Arguments[0].IsEditable, Is.False);

                    Assert.That(function.Arguments[1].Name, Is.EqualTo("N"));
                    Assert.That(function.Arguments[1].Attributes, Contains.Key("ncName"));
                    Assert.That(function.Arguments[1].Attributes["hasVariable"], Is.EqualTo("false"));
                    Assert.That(function.Arguments[1].IsEditable, Is.False);
        
                    Assert.That(function.Arguments[2].Name, Is.EqualTo("M"));
                    Assert.That(function.Arguments[2].Attributes, Contains.Key("ncName"));
                    Assert.That(function.Arguments[2].Attributes["hasVariable"], Is.EqualTo("false"));
                    Assert.That(function.Arguments[2].IsEditable, Is.False);

                    Assert.That(function.Components[0].Name, Is.Not.Null);
                    Assert.That(function.Components[0].Attributes, Contains.Key("ncName"));
                    Assert.That(function.Components[0].Attributes["hasVariable"], Is.EqualTo("true"));
                    Assert.That(function.Components[0].IsEditable, Is.False);
                }
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void OpenWavmFileAndCheckGridAndFunctions()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                string localNcPath = tempDir.CopyTestDataFileToTempDirectory("output_wavm/wavm-wave.nc");

                var store = new WavmFileFunctionStore(localNcPath);
                
                Assert.AreEqual(63, store.Grid.Size2);
                Assert.AreEqual(28, store.Grid.Size1);
                Assert.AreEqual(27, store.Functions.Count);
            }
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void OpenWavmFileAndCheckGridCoordinatesHasNoFillValues()
        {
            const double fillValue = 9.9692099683868690e+36;
            
            using (var tempDir = new TemporaryDirectory())
            {
                string localNcPath = tempDir.CopyTestDataFileToTempDirectory(wavmWadPath);

                var store = new WavmFileFunctionStore(localNcPath);

                IMultiDimensionalArray<double> xValues = store.Grid.X.Values;
                IMultiDimensionalArray<double> yValues = store.Grid.Y.Values;

                Assert.AreEqual(4450, xValues.Count);
                Assert.AreEqual(4450, yValues.Count);
                Assert.IsFalse(xValues.Any(c => c.Equals(fillValue)));
                Assert.IsFalse(yValues.Any(c => c.Equals(fillValue)));
            }
        }
    }
}