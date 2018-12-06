using System.IO;
using System.Linq;
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
       public void GivenAnMd1dFile_WhenReadingTheAttachedNetworkDefinitionFile_ThenAModelIsReturned()
        {
            var md1dFilePath = Path.Combine(tempFolderPath, "Md1dExport.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNotNull(waterFlowModel1D);
        }

        [Test]
        public void GivenAnMd1dFileWithReversedRoughnessSectionDefined_WhenReadingFlow1DModel_ThenModelUsesReversedRoughness()
        {
            var md1dFilePath = Path.Combine(tempFolderPath, "Md1dExportWithReversedRoughnessSection.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNotNull(waterFlowModel1D);
            Assert.IsTrue(waterFlowModel1D.UseReverseRoughness);
            Assert.IsTrue(waterFlowModel1D.UseReverseRoughnessInCalculation);
        }

        [Test]
        public void GivenAnMd1dFile_WhenReadingTheNetworkDefinitionFileWithABadNode_ThenNullIsReturned()
        {
            var md1dFilePath = Path.Combine(tempFolderPath, "Md1dExportBadNode.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNull(waterFlowModel1D);
        }

        [Test]
        public void GivenAnMd1dFile_WhenReadingTheNetworkDefinitionFileWithABadBranch_ThenNullIsReturned()
        {
            var md1dFilePath = Path.Combine(tempFolderPath, "Md1dExportBadBranch.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNull(waterFlowModel1D);
        }

        [Test]
        public void GivenAnMd1dFile_WhenReadingTheNetworkDefinitionFileWithABadNetworkDiscretization_ThenNullIsReturned()
        {
            var md1dFilePath = Path.Combine(tempFolderPath, "Md1dExportBadNetworkDiscretization.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNull(waterFlowModel1D);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAMd1dFile_WhenReading_ThenAModelIsReturned()
        {
            var md1dFilePath = TestHelper.GetTestFilePath(@"ImportSpatialData\water flow 1d.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNotNull(waterFlowModel1D.Network);

            Assert.AreEqual(267, waterFlowModel1D.Network.Branches.Count);
            Assert.AreEqual(212, waterFlowModel1D.Network.HydroNodes.Count());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnMd1dFile_WhenReadingAnIncorrectSpatialDataFile_ThenNullIsReturned()
        {
            var md1dFilePath = TestHelper.GetTestFilePath(@"ImportSpatialData\water flow 1dIncorrect.md1d");

            var waterFlowModel1D = WaterFlowModel1DFileReader.Read(md1dFilePath);
            Assert.IsNull(waterFlowModel1D);
        }
    }
}
