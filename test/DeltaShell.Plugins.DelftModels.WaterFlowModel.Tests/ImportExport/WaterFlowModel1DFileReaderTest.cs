using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport
{
    [TestFixture]
    public class WaterFlowModel1DFileReaderTest
    {
        
        [Test]
        public void GivenAMd1dFile_WhenReadingTheAttachedNetworkDefinitionFile_ThenAModelIsReturned()
        {
            var md1dFilePath = TestHelper.GetTestFilePath(@"Md1dReading\Md1dExport.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNotNull(waterFlowModel1D);
        }

        [Test]
        public void GivenAMd1dFile_WhenReadingTheNetworkDefinitionFileWithABadNode_ThenNullIsReturned()
        {
            var md1dFilePath = TestHelper.GetTestFilePath(@"Md1dReading\Md1dExportBadNode.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNull(waterFlowModel1D);
        }

        [Test]
        public void GivenAMd1dFile_WhenReadingTheNetworkDefinitionFileWithABadBranch_ThenNullIsReturned()
        {
            var md1dFilePath = TestHelper.GetTestFilePath(@"Md1dReading\Md1dExportBadBranch.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNull(waterFlowModel1D);
        }

        [Test]
        public void GivenAMd1dFile_WhenReadingTheNetworkDefinitionFileWithABadNetworkDiscretization_ThenNullIsReturned()
        {
            var md1dFilePath = TestHelper.GetTestFilePath(@"Md1dReading\Md1dExportBadNetworkDiscretization.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNull(waterFlowModel1D);
        }



    }
}
