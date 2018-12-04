using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class SpatialFileDataReaderTest
    {
        [Test]
        public void GivenAMd1dFile_WhenReading_ThenAModelIsReturned()
        {
            var md1dFilePath = TestHelper.GetTestFilePath(@"ImportSpatialData\water flow 1d.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNotNull(waterFlowModel1D.Network);

            Assert.AreEqual(267, waterFlowModel1D.Network.Branches.Count);
            Assert.AreEqual(212, waterFlowModel1D.Network.HydroNodes.Count());
        }

        [Test]
        public void GivenAnMd1dFile_WhenReadingAnIncorrectSpatialDataFile_ThenNullIsReturned()
        {
            var md1dFilePath = TestHelper.GetTestFilePath(@"ImportSpatialData\water flow 1dIncorrect.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNull(waterFlowModel1D);
        }

    }
}