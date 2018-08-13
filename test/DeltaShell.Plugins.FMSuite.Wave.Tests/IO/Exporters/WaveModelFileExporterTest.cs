using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Wave.IO.Exporters;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Exporters
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class WaveModelFileExporterTest
    {

        private WaveModelFileExporter exporter;

        [Test]
        public void NamePropertyTest()
        {
            var expected = "Waves model";
            exporter = new WaveModelFileExporter();
            Assert.AreEqual(expected, exporter.Name);
        }

        [Test]
        public void ExportWaveModel_WithExistingPath()
        {
            exporter = new WaveModelFileExporter();
            var model = new WaveModel();

            var exportFolder = FileUtils.CreateTempDirectory();
            var fullPath = Path.Combine(exportFolder, model.Name + ".mdw");

            Assert.IsTrue(exporter.Export(model, exportFolder));
            Assert.IsTrue(File.Exists(fullPath));

            FileUtils.DeleteIfExists(exportFolder);
        }

        [Test]
        [Category(TestCategory.WorkInProgress)]
        // File should be exported correctly when path does not exist.
        public void ExportWaveModel_WithoutExistingPath_ShouldStillExport()
        {
            exporter = new WaveModelFileExporter();
            var model = new WaveModel();

            var temp = FileUtils.CreateTempDirectory();
            var exportFolder = Path.Combine(temp, @"non-existent-folder");
            var fullPath = Path.Combine(exportFolder, model.Name + ".mdw");

            Assert.IsTrue(exporter.Export(model, exportFolder));
            Assert.IsTrue(File.Exists(fullPath));

            FileUtils.DeleteIfExists(temp);
        }

        [Test]      
        public void ExportWaveModel_WithInvalidPath_ShouldGiveException()
        {
            exporter = new WaveModelFileExporter();
            var model = new WaveModel();

            var temp = FileUtils.CreateTempDirectory();
            var exportFolder = Path.Combine(temp, @"invalid-folder*");
            var fullPath = Path.Combine(exportFolder, model.Name + ".mdw");

            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => { Assert.IsFalse(exporter.Export(model, exportFolder)); },
                String.Format("Export of Waves model failed to path {0}.", exportFolder)
            );

            Assert.IsFalse(File.Exists(fullPath));

            FileUtils.DeleteIfExists(temp);
        }

        [Test]
        public void SourceTypesPropertyTest()
        {
            var expected = new List<Type> {typeof(WaveModel)};
            exporter = new WaveModelFileExporter();
            Assert.AreEqual(expected, exporter.SourceTypes());
        }

        [Test]
        public void FileFilterProperyTest()
        {
            string expected = "Master Definition WAVE File|*.mdw";
            exporter = new WaveModelFileExporter();
            Assert.AreEqual(expected, exporter.FileFilter);
        }

        [Test]
        public void CanExportForPropertyTest()
        {
            exporter = new WaveModelFileExporter();
            Assert.IsTrue(exporter.CanExportFor(new WaveModel()));
        }
    }
}
