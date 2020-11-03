using System.Linq;
using System.Runtime.Remoting;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.OutputData
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class WavmFileFunctionStoreTest
    {
        private const string ncPath = "./WaveOutputDataHarvesterTest/wavm-Waves.nc";

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Constructor_ExpectedResults()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string localNcPath = tempDir.CopyTestDataFileToTempDirectory(ncPath);

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
        public void OpenWavmFileAndCheckGridAndFunctions()
        {
            string wavmPath = TestHelper.GetTestFilePath("output_wavm/wavm-wave.nc");
            var store = new WavmFileFunctionStore(wavmPath);
            Assert.AreEqual(63, store.Grid.Size2);
            Assert.AreEqual(28, store.Grid.Size1);
            Assert.AreEqual(27, store.Functions.Count);
        }
    }
}