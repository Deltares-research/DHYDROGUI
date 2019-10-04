using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.IO;
using DeltaShell.Dimr;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class WaterFlowFMFileImporterTest
    {
        private WaterFlowFMFileImporter importer;

        [SetUp]
        public void SetUp()
        {
            string temp = FileUtils.CreateTempDirectory();
            importer = new WaterFlowFMFileImporter(() => temp);
        }

        [Test]
        public void CheckIfWaterFlowFMFileImporterImplementsIDimrModelFileImporterInterface()
        {
            Assert.IsTrue(importer is IDimrModelFileImporter);
        }

        [Test]
        public void CallingMasterFileExtensionShouldReturnTheCorrectExtensionOfFM()
        {
            Assert.AreEqual("mdu", importer.MasterFileExtension);
        }

        [Test]
        public void CallingSubFoldersShouldReturnTheCorrectSubFolderNameForADimrConfigurationOfFM()
        {
            List<string> subFolder = importer.SubFolders.ToList();

            Assert.AreEqual(1, subFolder.Count);
            Assert.AreEqual("dflowfm", subFolder.FirstOrDefault());
        }
    }
}