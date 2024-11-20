using System;
using DelftTools.Controls.Wpf.Commands;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParametersSpecific
{
    public class FileBasedParametersViewModelTest
    {
        private readonly Random random = new Random();

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            FileBasedParameters parameters = GetRandomFileBasedParameters();

            // Call
            var viewModel = new FileBasedParametersViewModel(parameters);

            // Assert
            Assert.That(viewModel.ObservedParameters, Is.SameAs(parameters));
            Assert.That(viewModel.FilePath, Is.EqualTo(parameters.FilePath));
            Assert.That(viewModel.SelectFileCommand, Is.InstanceOf<RelayCommand>());
        }

        [Test]
        public void Constructor_ParametersNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new FileBasedParametersViewModel(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("parameters"));
        }

        [Test]
        public void SetFilePath_ExpectedValues()
        {
            // Setup
            FileBasedParameters parameters = GetRandomFileBasedParameters();
            var viewModel = new FileBasedParametersViewModel(parameters);
            const string expectedFilePath = "new file path";

            // Call
            viewModel.FilePath = expectedFilePath;

            // Assert
            Assert.That(viewModel.FilePath, Is.EqualTo(expectedFilePath));
        }

        private FileBasedParameters GetRandomFileBasedParameters() => new FileBasedParameters("file path " + random.Next());
    }
}