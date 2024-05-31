using System.IO;
using DelftTools.Shell.Core.Services;
using DelftTools.TestUtils;
using DeltaShell.Core.Services;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Import;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Exporters;
using NSubstitute;
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

                DHydroConfigXmlImporter importer = CreateImporter();
                DHydroConfigXmlExporter exporter = CreateExporter();

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

        private static DHydroConfigXmlImporter CreateImporter()
        {
            var fileImportService = new FileImportService(Substitute.For<IHybridProjectRepository>());
            var hydroModelReader = new HydroModelReader(fileImportService);

            fileImportService.RegisterFileImporter(new WaterFlowFMFileImporter(() => null));
            fileImportService.RegisterFileImporter(new RealTimeControlModelImporter
            {
                XmlReaders =
                {
                    new RealTimeControlModelXmlReader()
                }
            });

            return new DHydroConfigXmlImporter(fileImportService, hydroModelReader, () => null);
        }

        private static DHydroConfigXmlExporter CreateExporter()
        {
            var fileExportService = new FileExportService();

            fileExportService.RegisterFileExporter(new FMModelFileExporter());
            fileExportService.RegisterFileExporter(new WaveModelFileExporter());
            fileExportService.RegisterFileExporter(new RealTimeControlModelExporter
            {
                XmlWriters =
                {
                    new RealTimeControlXmlWriter(),
                    new RealTimeControlRestartXmlWriter()
                }
            });

            return new DHydroConfigXmlExporter(fileExportService);
        }

        private static void AssertPathExists(string root, string relativePath)
        {
            Assert.That(Path.Combine(root, relativePath), Does.Exist);
        }
    }
}