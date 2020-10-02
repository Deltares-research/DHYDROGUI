using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.OutputData
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class WavmFileFunctionStoreTest
    {
        [Test]
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