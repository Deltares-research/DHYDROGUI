using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.TestUtils;
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

            using (var temp = new TemporaryDirectory())
            {
                string fullPath = Path.Combine(temp.Path, model.Name + ".mdw");

                Assert.IsTrue(exporter.Export(model, temp.Path));
                Assert.IsTrue(File.Exists(fullPath));
            }
        }

        [Test]
        [Category(TestCategory.WorkInProgress)]
        // File should be exported correctly when path does not exist.
        public void ExportWaveModel_WithoutExistingPath_ShouldStillExport()
        {
            exporter = new WaveModelFileExporter();
            var model = new WaveModel();

            using (var temp = new TemporaryDirectory())
            {
                string exportFolder = Path.Combine(temp.Path, @"non-existent-folder");
                string fullPath = Path.Combine(exportFolder, model.Name + ".mdw");

                Assert.IsTrue(exporter.Export(model, exportFolder));
                Assert.IsTrue(File.Exists(fullPath));
            }
        }

        [Test]
        public void ExportWaveModel_WithInvalidPath_ShouldGiveException()
        {
            exporter = new WaveModelFileExporter();
            var model = new WaveModel();

            using (var temp = new TemporaryDirectory())
            {
                string exportFolder = Path.Combine(temp.Path, @"invalid-folder*");
                string fullPath = Path.Combine(exportFolder, model.Name + ".mdw");

                TestHelper.AssertAtLeastOneLogMessagesContains(
                    () => { Assert.IsFalse(exporter.Export(model, exportFolder)); },
                    string.Format("Export of Waves model failed to path {0}.", exportFolder)
                );

                Assert.IsFalse(File.Exists(fullPath));
            }
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
            var expected = "Master Definition WAVE File|*.mdw";
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