using System;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Restart;
using NSubstitute;
using NUnit.Framework;
using WaterFlowFMRestartModel = DeltaShell.NGHS.Common.Restart.IRestartModel<DeltaShell.Plugins.FMSuite.FlowFM.Restart.WaterFlowFMRestartFile>;

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
            var importer = new FMRestartFileImporter(Enumerable.Empty<WaterFlowFMRestartModel>);

            // Assert
            Assert.That(importer.Name, Is.EqualTo("Restart File"));
            Assert.That(importer.Category, Is.EqualTo("NetCdf"));
            Assert.That(importer.Description, Is.EqualTo(string.Empty));
            Assert.That(importer.Image, Is.Not.Null);
            CollectionContainsOnlyAssert.AssertContainsOnly(importer.SupportedItemTypes, typeof(WaterFlowFMRestartFile));
            Assert.That(importer.CanImportOnRootLevel, Is.False);
            Assert.That(importer.FileFilter, Is.EqualTo($"FM restart files|*_rst.nc;*_map.nc"));
            Assert.That(importer.TargetDataDirectory, Is.Null);
            Assert.That(importer.ShouldCancel, Is.False);
            Assert.That(importer.OpenViewAfterImport, Is.False);
        }

        [Test]
        public void CanImportOn_IsRestartInputForModel_ReturnsTrue()
        {
            var model = Substitute.For<WaterFlowFMRestartModel>();
            var importer = new FMRestartFileImporter(() => new[]
            {
                model
            });

            // Call
            bool result = importer.CanImportOn(model.RestartInput);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CanImportOn_IsNotRestartInputForModel_ReturnsFalse()
        {
            // Setup
            var model = Substitute.For<WaterFlowFMRestartModel>();
            var importer = new FMRestartFileImporter(() => new[]
            {
                model
            });

            // Call
            bool result = importer.CanImportOn(new WaterFlowFMRestartFile());

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ImportItem_TargetNull_ThrowsArgumentNullException()
        {
            // Setup
            var importer = new FMRestartFileImporter(Enumerable.Empty<WaterFlowFMRestartModel>);

            // Call
            void Call() => importer.ImportItem("path/to/the.file", null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("target"));
        }

        [Test]
        public void ImportItem_FileDoesNotExist_ThrowsFileNotFoundException()
        {
            // Setup
            var importer = new FMRestartFileImporter(Enumerable.Empty<WaterFlowFMRestartModel>);

            // Call
            void Call() => importer.ImportItem("path/to/the.file", new WaterFlowFMRestartFile());

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
                var model = Substitute.For<WaterFlowFMRestartModel, ITimeDependentModel>();
                model.RestartInput = new WaterFlowFMRestartFile();

                var importer = new FMRestartFileImporter(() => new[]
                {
                    model
                });
                string filePath = Path.Combine(tempDir.Path, "file_rst.nc");
                File.WriteAllText(filePath, "");

                // Call
                object result = importer.ImportItem(filePath, model.RestartInput);

                // Assert
                Assert.That(result, Is.SameAs(model.RestartInput));
                Assert.That(model.RestartInput.Path, Is.EqualTo(filePath));
                
                ((ITimeDependentModel)model).Received(1).MarkOutputOutOfSync();
            }
        }

        [TestCase(null)]
        [TestCase("")]
        public void ImportItem_PathNullOrEmpty_ThrowsArgumentException(string path)
        {
            // Setup
            var importer = new FMRestartFileImporter(Enumerable.Empty<WaterFlowFMRestartModel>);

            // Call
            void Call() => importer.ImportItem(path, new WaterFlowFMRestartFile());

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.Message, Is.EqualTo("Path cannot be null or empty."));
        }
    }
}