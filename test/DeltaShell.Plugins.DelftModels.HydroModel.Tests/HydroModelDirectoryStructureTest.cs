using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Import;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    public class HydroModelDirectoryStructureTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void AfterImportingAndExportingDimrConfig_CorrectDirectoryStructureIsPreserved()
        {
            string testDataDirectory = TestHelper.GetTestFilePath("SimpleModel");

            using (var temp = new TemporaryDirectory())
            {
                string modelDirectory = temp.CopyDirectoryToTempDirectory(testDataDirectory);
                string exportModelDirectory = temp.CreateDirectory("SimpleModel_export");

                List<IDimrModelFileImporter> dimrModelImporters = GetDimrModelFileImporters();

                var importer = new DHydroConfigXmlImporter(() => dimrModelImporters, () => null);
                var exporter = new DHydroConfigXmlExporter();

                string importDimrFilePath = Path.Combine(modelDirectory, "computation", "dimr.xml");
                var hydroModel = (HydroModel)importer.ImportItem(importDimrFilePath);

                string exportDimrFilePath = Path.Combine(exportModelDirectory, "dimr.xml");
                exporter.Export(hydroModel, exportDimrFilePath);

                AssertPathExists(exportModelDirectory, "dimr.xml");
                AssertPathExists(exportModelDirectory, "computation");
                AssertPathExists(exportModelDirectory, "geometry");
                AssertPathExists(exportModelDirectory, "initial_conditions");
                AssertPathExists(exportModelDirectory, "rtc");
            }
        }

        private static List<IDimrModelFileImporter> GetDimrModelFileImporters()
        {
            return new List<IDimrModelFileImporter>
            {
                new WaterFlowFMFileImporter(() => null),
                new RealTimeControlModelImporter()
            };
        }

        private static void AssertPathExists(string root, string relativePath)
        {
            Assert.That(Path.Combine(root, relativePath), Does.Exist);
        }
    }
}