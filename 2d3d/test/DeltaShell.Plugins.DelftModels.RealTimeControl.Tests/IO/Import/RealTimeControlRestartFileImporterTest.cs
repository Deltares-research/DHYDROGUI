using System;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Import;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.Import
{
    [TestFixture]
    public class RealTimeControlRestartFileImporterTest
    {
        [Test]
        public void Constructor_GetModelsNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new RealTimeControlRestartFileImporter(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("getModels"));
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var importer = new RealTimeControlRestartFileImporter(Enumerable.Empty<RealTimeControlModel>);

            // Assert
            Assert.That(importer.Name, Is.EqualTo("Restart File"));
            Assert.That(importer.Category, Is.EqualTo("XML"));
            Assert.That(importer.Description, Is.EqualTo(string.Empty));
            Assert.That(importer.Image, Is.Not.Null);
            CollectionContainsOnlyAssert.AssertContainsOnly(importer.SupportedItemTypes, typeof(RealTimeControlRestartFile));
            Assert.That(importer.SupportedItemTypes, Has.Exactly(1).Matches<Type>(x => x == typeof(RealTimeControlRestartFile)));
            Assert.That(importer.CanImportOnRootLevel, Is.False);
            Assert.That(importer.FileFilter, Is.EqualTo("Real Time Control restart files|*.xml"));
            Assert.That(importer.TargetDataDirectory, Is.Null);
            Assert.That(importer.ShouldCancel, Is.False);
            Assert.That(importer.OpenViewAfterImport, Is.False);
        }

        [Test]
        public void ImportItem_TargetNull_ThrowsArgumentNullException()
        {
            // Setup
            var importer = new RealTimeControlRestartFileImporter(Enumerable.Empty<RealTimeControlModel>);

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
            var importer = new RealTimeControlRestartFileImporter(Enumerable.Empty<RealTimeControlModel>);

            // Call
            void Call() => importer.ImportItem("path/to/the.file", new RealTimeControlRestartFile());

            // Assert
            var e = Assert.Throws<FileNotFoundException>(Call);
            Assert.That(e.Message, Is.EqualTo("Restart file does not exist: path/to/the.file"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItem_ImportsRestartFile()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                // Setup
                var model = Substitute.For<IRealTimeControlModel>();
                model.RestartInput = new RealTimeControlRestartFile();
                
                var importer = new RealTimeControlRestartFileImporter(() => new[]
                {
                    model
                });
                string filePath = Path.Combine(tempDir.Path, "file_rst.xml");
                const string fileContent = @"file content here";
                File.WriteAllText(filePath, fileContent);

                // Call
                object result = importer.ImportItem(filePath, model.RestartInput);

                // Assert
                Assert.That(result, Is.SameAs(model.RestartInput));
                Assert.That(model.RestartInput.Name, Is.EqualTo(Path.GetFileName(filePath)));
                Assert.That(model.RestartInput.Content, Is.EqualTo(fileContent));
                model.Received().MarkOutputOutOfSync();
            }
        }

        [Test]
        public void CanImportOn_IsRestartInputForModel_ReturnsTrue()
        {
            var model = new RealTimeControlModel();
            var importer = new RealTimeControlRestartFileImporter(() => new[]
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
            var model = new RealTimeControlModel();
            var importer = new RealTimeControlRestartFileImporter(() => new[]
            {
                model
            });

            // Call
            bool result = importer.CanImportOn(new RealTimeControlRestartFile());

            // Assert
            Assert.That(result, Is.False);
        }

        [TestCase(null)]
        [TestCase("")]
        public void ImportItem_PathNullOrEmpty_ThrowsArgumentException(string path)
        {
            // Setup
            var importer = new RealTimeControlRestartFileImporter(Enumerable.Empty<RealTimeControlModel>);

            // Call
            void Call() => importer.ImportItem(path, new RealTimeControlRestartFile());

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.Message, Is.EqualTo("Path cannot be null or empty."));
        }
    }
}