using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.NGHS.Common.IO.RestartFiles;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.NGHS.TestUtils.AssertConstraints;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class FMRestartFileImporterTest
    {
        [Test]
        public void Constructor_GetModelsNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new FMRestartFileImporter(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("getModels"));
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var importer = new FMRestartFileImporter(Enumerable.Empty<WaterFlowFMModel>);

            // Assert
            Assert.That(importer.Name, Is.EqualTo("Restart File"));
            Assert.That(importer.Category, Is.EqualTo("NetCdf"));
            Assert.That(importer.Description, Is.EqualTo(string.Empty));
            Assert.That(importer.Image, Is.Not.Null);
            Assert.That(importer.SupportedItemTypes, Collection.OnlyContains(typeof(RestartFile)));
            Assert.That(importer.CanImportOnRootLevel, Is.False);
            Assert.That(importer.FileFilter, Is.EqualTo($"FM restart files|*_rst.nc"));
            Assert.That(importer.TargetDataDirectory, Is.Null);
            Assert.That(importer.ShouldCancel, Is.False);
            Assert.That(importer.OpenViewAfterImport, Is.False);
        }

        private IEnumerable<TestCaseData> CanImportOnTestCases()
        {
            yield return new TestCaseData(new RestartFile(), true);
            yield return new TestCaseData(new object(), false);
        }

        [TestCaseSource(nameof(CanImportOnTestCases))]
        public void CanImportOn(object obj, bool expected)
        {
            // Setup
            var importer = new FMRestartFileImporter(Enumerable.Empty<WaterFlowFMModel>);

            // Call
            bool result = importer.CanImportOn(obj);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ImportItem_TargetNull_ThrowsArgumentNullException()
        {
            // Setup
            var importer = new FMRestartFileImporter(Enumerable.Empty<WaterFlowFMModel>);

            // Call
            void Call() => importer.ImportItem("path/to/the.file", null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("target"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void ImportItem_PathNullOrEmpty_ThrowsArgumentException(string path)
        {
            // Setup
            var importer = new FMRestartFileImporter(Enumerable.Empty<WaterFlowFMModel>);

            // Call
            void Call() => importer.ImportItem(path, new RestartFile());

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.Message, Is.EqualTo("Path cannot be null or empty."));
        }

        [Test]
        public void ImportItem_FileDoesNotExist_ThrowsFileNotFoundException()
        {
            // Setup
            var importer = new FMRestartFileImporter(Enumerable.Empty<WaterFlowFMModel>);

            // Call
            void Call() => importer.ImportItem("path/to/the.file", new RestartFile());

            // Assert
            var e = Assert.Throws<FileNotFoundException>(Call);
            Assert.That(e.Message, Is.EqualTo("Restart file does not exist: path/to/the.file"));
        }

        [Test]
        public void ImportItem_ImportsRestartFile()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                // Setup
                var model = new WaterFlowFMModel();
                var importer = new FMRestartFileImporter(() => new[] {model});
                string filePath = Path.Combine(tempDir.Path, "file_rst.nc");
                File.WriteAllText(filePath, "");

                // Call
                object result = importer.ImportItem(filePath, model.RestartInput);

                // Assert
                Assert.That(result, Is.SameAs(model.RestartInput));
                Assert.That(model.RestartInput.Path, Is.EqualTo(filePath));
                Assert.That(model.UseRestart, Is.True);
            }
        }
    }
}