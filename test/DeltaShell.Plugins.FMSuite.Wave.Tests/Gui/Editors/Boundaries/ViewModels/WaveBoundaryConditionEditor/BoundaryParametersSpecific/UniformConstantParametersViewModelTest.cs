using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParametersSpecific
{
    [TestFixture]
    public class UniformConstantParametersViewModelTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var parameters = new ConstantParameters(0, 0, 0, 0);
            var parametersViewModel = new ConstantParametersViewModel(parameters);

            // Call
            var viewModel = new UniformConstantParametersViewModel(parametersViewModel);

            // Assert
            Assert.That(viewModel.ActiveParametersViewModel, Is.SameAs(parametersViewModel));
        }

        [Test]
        public void Constructor_ActiveParametersViewModelNull_ThrowsArgumentNullException()
        {
            void Call() => new UniformConstantParametersViewModel(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("activeParametersViewModel"));
        }
    }
}