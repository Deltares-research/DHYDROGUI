using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess
{
    [TestFixture]
    public class InputFileImporterServiceTest
    {
        // This value should be the same as DirectoryConstants DirectoryNameConstants.InputDirectoryName
        private const string inputFolderName = "input";

        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var model = Substitute.For<IWaveModel>();

            // Call
            var service = new InputFileImporterService(model);

            // Assert
            Assert.That(service, Is.InstanceOf<IInputFileImporterService>());
        }

        [Test]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException()
        {
            void Call() => new InputFileImporterService(null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveModel"));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void HasFile_NullOrEmpty_ReturnsFalse(string filePath)
        {
            // Setup
            var model = Substitute.For<IWaveModel>();
            var service = new InputFileImporterService(model);

            // Call
            bool result = service.HasFile(filePath);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase("existingFile.txt", true)]
        [TestCase("nonExistingFile.txt", false)]
        public void HasFile_ExpectedResults(string fileName, bool expectedResult)
        {
            // Setup
            var model = Substitute.For<IWaveModel>();
            var service = new InputFileImporterService(model);

            using (var tempDir = new TemporaryDirectory())
            {
                // configure model directory
                model.Name = "Waves";
                string modelPath = tempDir.CreateDirectory(model.Name);

                string inputFolder = Path.Combine(model.Name, inputFolderName);
                string absoluteInputFolderPath = tempDir.CreateDirectory(inputFolder);

                model.Path = modelPath;

                string existingFilePath = Path.Combine(absoluteInputFolderPath, "existingFile.txt");
                File.WriteAllText(existingFilePath, "some text that goes into the file.");

                // Call
                bool result = service.HasFile(fileName);

                // Assert
                Assert.That(result, Is.EqualTo(expectedResult));
            }
        }

        [Test]
        public void HasFile_FileExistsInSubFolder_ReturnsFalse()
        {
            // Setup
            const string fileName = "existingFile.txt";

            var model = Substitute.For<IWaveModel>();
            var service = new InputFileImporterService(model);

            using (var tempDir = new TemporaryDirectory())
            {
                // configure model directory
                model.Name = "Waves";
                string modelPath = tempDir.CreateDirectory(model.Name);
                model.Path = modelPath;

                string inputFolder = Path.Combine(model.Name, inputFolderName, "subFolder");
                string absoluteInputFolderPath = tempDir.CreateDirectory(inputFolder);


                string existingFilePath = Path.Combine(absoluteInputFolderPath, "existingFile.txt");
                File.WriteAllText(existingFilePath, "some text that goes into the file.");

                // Call
                bool result = service.HasFile(fileName);

                // Assert
                Assert.That(result, Is.False);
            }
        }

        [Test]
        public void CopyFile_SourceFilePathNull_ThrowsArgumentNullException()
        {
            // Setup
            var model = Substitute.For<IWaveModel>();
            var service = new InputFileImporterService(model);

            // Call | Assert
            void Call() => service.CopyFile(null);
            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("sourceFilePath"));
        }

        [Test]
        public void CopyFile_ValidSourceFilePath_ExpectedResults()
        {
            // Setup
            const string content = "Content for the content god";
            const string fileName = "TitleForTheTitleThrone";

            var model = Substitute.For<IWaveModel>();
            var service = new InputFileImporterService(model);

            using (var tempDir = new TemporaryDirectory())
            {
                string sourceFilePath = tempDir.CreateFile(fileName, content);

                // configure model directory
                model.Name = "Waves";
                string modelPath = tempDir.CreateDirectory(model.Name);

                string inputFolder = Path.Combine(model.Name, inputFolderName);
                string absoluteInputFolderPath = tempDir.CreateDirectory(inputFolder);

                model.Path = modelPath;

                // Call
                service.CopyFile(sourceFilePath);

                // Assert
                string expectedFilePath = Path.Combine(absoluteInputFolderPath, fileName);
                Assert.That(File.Exists(expectedFilePath), Is.True);
                Assert.That(File.ReadAllText(expectedFilePath), Is.EqualTo(content));
            }
        }

        [Test]
        public void CopyFile_WithFileName_ValidSourceFilePath_ExpectedResults()
        {
            // Setup
            const string content = "Content for the content god";
            const string fileName = "TitleForTheTitleThrone";

            var model = Substitute.For<IWaveModel>();
            var service = new InputFileImporterService(model);

            using (var tempDir = new TemporaryDirectory())
            {
                string sourceFilePath = tempDir.CreateFile(fileName, content);
                const string someOtherFileName = "namesAreHard.txt";

                // configure model directory
                model.Name = "Waves";
                string modelPath = tempDir.CreateDirectory(model.Name);

                string inputFolder = Path.Combine(model.Name, inputFolderName);
                string absoluteInputFolderPath = tempDir.CreateDirectory(inputFolder);

                model.Path = modelPath;

                // Call
                service.CopyFile(sourceFilePath, someOtherFileName);

                // Assert
                string expectedFilePath = Path.Combine(absoluteInputFolderPath, someOtherFileName);
                Assert.That(File.Exists(expectedFilePath), Is.True);
                Assert.That(File.ReadAllText(expectedFilePath), Is.EqualTo(content));
            }
        }

        [Test]
        [TestCase("someFile.txt")]
        [TestCase("subFolder\\someFile.txt")]
        public void IsInputFolder_InFolder_ReturnsTrue(string relativeFilePath)
        {
            // Setup
            var model = Substitute.For<IWaveModel>();
            model.Name = "Waves";

            model.Path = Path.Combine(Path.GetFullPath("."), model.Name);

            string someFilePath = Path.GetFullPath(Path.Combine(model.Path, inputFolderName, relativeFilePath));

            var service = new InputFileImporterService(model);

            // Call
            bool result = service.IsInInputFolder(someFilePath);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase("someFile.txt")]
        [TestCase("subFolder\\someFile.txt")]
        public void IsInputFolder_NotInFolder_ReturnsFalse(string relativeFilePath)
        {
            // Setup
            var model = Substitute.For<IWaveModel>();
            model.Name = "Waves";

            model.Path = Path.Combine(Path.GetFullPath("."), model.Name);

            string someFilePath = Path.GetFullPath(Path.Combine(".", "notTheInputFolder", relativeFilePath));

            var service = new InputFileImporterService(model);

            // Call
            bool result = service.IsInInputFolder(someFilePath);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        [TestCase("someFile.txt")]
        [TestCase("subFolder\\someFile.txt")]
        public void GetAbsolutePath_ExpectedResults(string relativeFilePath)
        {
            // Setup
            var model = Substitute.For<IWaveModel>();
            model.Name = "Waves";
            model.Path = Path.Combine(Path.GetFullPath("."), model.Name);

            var service = new InputFileImporterService(model);

            // Call
            string result = service.GetAbsolutePath(relativeFilePath);

            // Assert
            string expectedPath = Path.GetFullPath(Path.Combine(model.Path, inputFolderName, relativeFilePath));
            Assert.That(result, Is.EqualTo(expectedPath));
        }

        [Test]
        [TestCase("someFile.txt")]
        [TestCase("subFolder\\someFile.txt")]
        public void GetRelativePath_ExpectedResults(string relativeFilePath)
        {
            // Setup
            var model = Substitute.For<IWaveModel>();
            model.Name = "Waves";

            model.Path = Path.Combine(Path.GetFullPath("."), model.Name);
            string absolutePath = Path.GetFullPath(Path.Combine(model.Path, inputFolderName, relativeFilePath));

            var service = new InputFileImporterService(model);

            // Call
            string result = service.GetRelativePath(absolutePath);

            // Assert
            Assert.That(result, Is.EqualTo(relativeFilePath));
        }
    }
}