using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Buttons;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Buttons
{
    [TestFixture]
    public class SelectInputTemplateFileButtonTest
    {
        private const string fileFilter = "SWAN input file|*.*";

        [Test]
        public void Constructor_FileDialogServiceNull_ThrownArgumentNullException()
        {
            // Call
            void Call() => new SelectInputTemplateFileButton(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Execute_InputNull_ThrowsArgumentNullException()
        {
            // Setup
            var fileDialogService = Substitute.For<IFileDialogService>();
            var buttonBehaviour = new SelectInputTemplateFileButton(fileDialogService);

            // Call
            void Call() => buttonBehaviour.Execute(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Execute_InputNotWaveModel_ThrowsArgumentException()
        {
            // Setup
            var fileDialogService = Substitute.For<IFileDialogService>();
            var buttonBehaviour = new SelectInputTemplateFileButton(fileDialogService);
            var inputObject = new object();

            // Call
            void Call() => buttonBehaviour.Execute(inputObject);

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        [TestCase("/")]
        [TestCase("\\")]
        public void Execute_SelectedFileLocationIsSetOnTheWaveModel(string pathSeparator)
        {
            // Setup
            var fileDialogService = Substitute.For<IFileDialogService>();
            var buttonBehaviour = new SelectInputTemplateFileButton(fileDialogService);
            var inputObject = new WaveModel();

            var selectedFilePath = $"some{pathSeparator}file.path";
            fileDialogService.SelectFile(fileFilter).Returns(selectedFilePath);

            // Call
            buttonBehaviour.Execute(inputObject);

            // Assert
            object fileLocation = inputObject.ModelDefinition
                                             .GetModelProperty("General", "INPUTTemplateFile").Value;
            const string expFileLocation = "some/file.path";
            Assert.That(fileLocation, Is.EqualTo(expFileLocation));
        }

        [Test]
        public void Execute_NoSelectedFileLocation_EmptyFileLocationOnTheWaveModel()
        {
            // Setup
            var fileDialogService = Substitute.For<IFileDialogService>();
            var buttonBehaviour = new SelectInputTemplateFileButton(fileDialogService);
            var inputObject = new WaveModel();

            fileDialogService.SelectFile(fileFilter).Returns((string)null);

            // Call
            buttonBehaviour.Execute(inputObject);

            // Assert
            object fileLocation = inputObject.ModelDefinition
                                             .GetModelProperty("General", "INPUTTemplateFile").Value;
            Assert.That(fileLocation, Is.Empty);
            fileDialogService.Received(1).SelectFile(fileFilter);
        }
    }
}