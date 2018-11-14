using System.IO;
using System.Reflection;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class WaterFlowModel1DFileReaderTest
    {
        private string tempFolderPath;
        
        [TestFixtureSetUp]
        public void FixtureSetup()
        {
           var testFolder = TestHelper.GetTestDataPath(Assembly.GetExecutingAssembly(), @"Md1dReading");
           tempFolderPath = TestHelper.CreateLocalCopy(testFolder);

        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            FileUtils.DeleteIfExists(tempFolderPath);
        }
        
        [Test]
       public void GivenAMd1dFile_WhenReadingTheAttachedNetworkDefinitionFile_ThenAModelIsReturned()
        {
            var md1dFilePath = Path.Combine(tempFolderPath, "Md1dExport.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNotNull(waterFlowModel1D);
        }

        [Test]
        public void GivenAMd1dFile_WhenReadingTheNetworkDefinitionFileWithABadNode_ThenNullIsReturned()
        {
            var md1dFilePath = Path.Combine(tempFolderPath, "Md1dExportBadNode.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNull(waterFlowModel1D);
        }

        [Test]
        public void GivenAMd1dFile_WhenReadingTheNetworkDefinitionFileWithABadBranch_ThenNullIsReturned()
        {
            var md1dFilePath = Path.Combine(tempFolderPath, "Md1dExportBadBranch.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNull(waterFlowModel1D);
        }

        [Test]
        public void GivenAMd1dFile_WhenReadingTheNetworkDefinitionFileWithABadNetworkDiscretization_ThenNullIsReturned()
        {
            var md1dFilePath = Path.Combine(tempFolderPath, "Md1dExportBadNetworkDiscretization.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNull(waterFlowModel1D);
        }



    }
}
