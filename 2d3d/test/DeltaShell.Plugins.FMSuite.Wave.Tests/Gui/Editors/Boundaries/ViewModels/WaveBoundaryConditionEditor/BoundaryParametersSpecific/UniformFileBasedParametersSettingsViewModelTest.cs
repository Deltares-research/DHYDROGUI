using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParametersSpecific
{
    [TestFixture]
    public class UniformFileBasedParametersSettingsViewModelTest
    {
        private readonly Random random = new Random();

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            string expectedFilePath = "file path " + random.Next();

            var parameters = new FileBasedParameters(expectedFilePath);

            // Call
            var viewModel = new UniformFileBasedParametersSettingsViewModel(parameters);

            // Assert
            Assert.That(viewModel, Is.InstanceOf<FileBasedParametersSettingsViewModel>());
            Assert.That(viewModel.ActiveParametersViewModel, Is.Not.Null);
            Assert.That(viewModel.ActiveParametersViewModel.FilePath, Is.EqualTo(expectedFilePath));

            Assert.That(viewModel.GroupBoxTitle, Is.EqualTo("Uniform File Based Parameters"));
        }

        [Test]
        public void Constructor_ActiveParametersViewModelNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new UniformFileBasedParametersSettingsViewModel(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("parameters"));
        }
    }
}