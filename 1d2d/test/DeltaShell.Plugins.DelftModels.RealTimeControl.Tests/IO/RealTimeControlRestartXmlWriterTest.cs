using System.IO;
using System.IO.Abstractions.TestingHelpers;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO
{
    [TestFixture]
    public class RealTimeControlRestartXmlWriterTest
    {
        private const string targetDirectory = "test_dir";

        private MockFileSystem fileSystem;

        [SetUp]
        public void SetUp()
        {
            fileSystem = new MockFileSystem();
        }

        [Test]
        public void Constructor_FileSystemIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => new RealTimeControlRestartXmlWriter(null), Throws.ArgumentNullException);
        }

        [Test]
        public void WriteToXml_ModelIsNull_ThrowsArgumentNullException()
        {
            RealTimeControlRestartXmlWriter writer = CreateWriter();

            Assert.That(() => writer.WriteToXml(null, targetDirectory), Throws.ArgumentNullException);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void WriteToXml_DirectoryIsNullOrEmpty_ThrowsArgumentException(string directory)
        {
            RealTimeControlModel model = CreateModel();
            RealTimeControlRestartXmlWriter writer = CreateWriter();

            Assert.That(() => writer.WriteToXml(model, directory), Throws.ArgumentException);
        }

        [Test]
        public void WriteToXml_DirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
        {
            RealTimeControlModel model = CreateModel();
            RealTimeControlRestartXmlWriter writer = CreateWriter();

            Assert.That(() => writer.WriteToXml(model, targetDirectory), Throws.InstanceOf<DirectoryNotFoundException>());
        }
        
        [Test]
        public void WriteToXml_UseRestartIsTrue_RestartXmlFileIsWritten()
        {
            RealTimeControlModel model = CreateModel();
            RealTimeControlRestartXmlWriter writer = CreateWriter();

            fileSystem.Directory.CreateDirectory(targetDirectory);
            writer.WriteToXml(model, targetDirectory);

            MockFileData restartFile = fileSystem.GetFile($@"{targetDirectory}\{RealTimeControlXmlFiles.XmlImportState}");
            
            Assert.That(restartFile, Is.Not.Null);
            Assert.That(restartFile.TextContents, Is.EqualTo("content"));
        }
        
        [Test]
        public void WriteToXml_UseRestartIsFalse_RestartXmlFileIsNotWritten()
        {
            RealTimeControlModel model = CreateModel();
            RealTimeControlRestartXmlWriter writer = CreateWriter();

            model.RestartInput.Content = null;
            
            fileSystem.Directory.CreateDirectory(targetDirectory);
            writer.WriteToXml(model, targetDirectory);

            bool fileExists = fileSystem.File.Exists($@"{targetDirectory}\{RealTimeControlXmlFiles.XmlImportState}");
            Assert.That(fileExists, Is.False);
        }

        private RealTimeControlRestartXmlWriter CreateWriter()
        {
            return new RealTimeControlRestartXmlWriter(fileSystem);
        }

        private RealTimeControlModel CreateModel()
        {
            var restartFile = new RealTimeControlRestartFile("file.name", "content");
            return new RealTimeControlModel { RestartInput = restartFile };
        }
    }
}