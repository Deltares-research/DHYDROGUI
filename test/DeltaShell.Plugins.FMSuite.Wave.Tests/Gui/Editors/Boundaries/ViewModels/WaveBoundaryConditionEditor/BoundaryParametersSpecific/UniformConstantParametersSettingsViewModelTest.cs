using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParametersSpecific
{
    [TestFixture]
    public class UniformConstantParametersSettingsViewModelTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var parameters = new ConstantParameters(0, 0, 0, 0);

            // Call
            var viewModel = new UniformConstantParametersSettingsViewModel(parameters);

            // Assert
            Assert.That(viewModel, Is.InstanceOf<IConstantParametersSettingsViewModel>());
            Assert.That(viewModel.ActiveParametersViewModel, Is.Not.Null);
        }

        [Test]
        public void Constructor_ActiveParametersViewModelNull_ThrowsArgumentNullException()
        {
            void Call() => new UniformConstantParametersSettingsViewModel(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("parameters"));
        }
    }
}