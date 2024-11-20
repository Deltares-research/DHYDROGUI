using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Buttons;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Buttons
{
    [TestFixture]
    public class SelectComFileButtonTest
    {
        private const string fileFilter = "Communication files|*_com.nc";
        
        [Test]
        public void Constructor_FileDialogServiceNull_ThrownArgumentNullException()
        {
            // Call
            void Call() => new SelectComFileButton(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Execute_InputNull_ThrowsArgumentNullException()
        {
            // Setup
            var fileDialogService = Substitute.For<IFileDialogService>();
            var buttonBehaviour = new SelectComFileButton(fileDialogService);

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
            var buttonBehaviour = new SelectComFileButton(fileDialogService);
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
            var buttonBehaviour = new SelectComFileButton(fileDialogService);
            var inputObject = new WaveModel();

            var selectedFilePath = $"some{pathSeparator}file.path";

            fileDialogService.ShowOpenFileDialog(Arg.Is<FileDialogOptions>(options => options.FileFilter == fileFilter))
                             .Returns(selectedFilePath);

            // Call
            buttonBehaviour.Execute(inputObject);

            // Assert
            object fileLocation = inputObject.ModelDefinition
                                             .GetModelProperty("Output", "COMFile").Value;
            const string expFileLocation = "some/file.path";
            Assert.That(fileLocation, Is.EqualTo(expFileLocation));
        }

        [Test]
        public void Execute_NoSelectedFileLocation_EmptyFileLocationOnTheWaveModel()
        {
            // Setup
            var fileDialogService = Substitute.For<IFileDialogService>();
            var buttonBehaviour = new SelectComFileButton(fileDialogService);
            var inputObject = new WaveModel();

            fileDialogService.ShowOpenFileDialog(Arg.Is<FileDialogOptions>(options => options.FileFilter == fileFilter))
                             .Returns((string)null);

            // Call
            buttonBehaviour.Execute(inputObject);

            // Assert
            object fileLocation = inputObject.ModelDefinition
                                             .GetModelProperty("Output", "COMFile").Value;
            Assert.That(fileLocation, Is.Empty);
            fileDialogService.Received(1).ShowOpenFileDialog(Arg.Any<FileDialogOptions>());
        }
    }
}