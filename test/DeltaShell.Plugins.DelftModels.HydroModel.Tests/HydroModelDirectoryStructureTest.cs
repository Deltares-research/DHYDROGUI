using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Services;
using DelftTools.TestUtils;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Core.Services;
using DeltaShell.Dimr.DimrXsd;
using DeltaShell.NGHS.IO.FileReaders;
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

                exporter.ExportDirectoryPath = exportModelDirectory;

                string importDimrFilePath = Path.Combine(modelDirectory, "computation", "dimr.xml");
                var hydroModel = (HydroModel)importer.ImportItem(importDimrFilePath);

                exporter.Export(hydroModel, null);

                AssertPathExists(exportModelDirectory, "computation");
                AssertPathExists(exportModelDirectory, "computation/dimr.xml");
                AssertPathExists(exportModelDirectory, "computation/RMM-simple.mdu");
                AssertPathExists(exportModelDirectory, "geometry");
                AssertPathExists(exportModelDirectory, "initial_conditions");
                AssertPathExists(exportModelDirectory, "rtc");

                dimrXML dimrXml = GetDimrXML(Path.Combine(exportModelDirectory, "computation/dimr.xml"));
                AssertCorrectComponent(dimrXml, "real-time control", "../rtc", ".");
                AssertCorrectComponent(dimrXml, "RMM-simple", ".", "RMM-simple.mdu");
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

        private static dimrXML GetDimrXML(string dimrFilePath)
        {
            var delftConfigXmlParser = new DelftConfigXmlFileParser(Substitute.For<ILogHandler>());
            return delftConfigXmlParser.Read<dimrXML>(dimrFilePath);
        }

        private static void AssertCorrectComponent(dimrXML dimrXml, string name, string expWorkingDir, string expInputFile)
        {
            dimrComponentXML component = dimrXml.component.SingleOrDefault(c => c.name == name);
            Assert.That(component, Is.Not.Null);
            Assert.That(component.workingDir, Is.EqualTo(expWorkingDir));
            Assert.That(component.inputFile, Is.EqualTo(expInputFile));
        }
    }
}