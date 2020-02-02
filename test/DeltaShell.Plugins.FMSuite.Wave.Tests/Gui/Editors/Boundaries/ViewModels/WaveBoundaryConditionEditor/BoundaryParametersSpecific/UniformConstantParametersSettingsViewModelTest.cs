using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParametersSpecific
{
    [TestFixture]
    [TestFixture(typeof(PowerDefinedSpreading))]
    [TestFixture(typeof(DegreesDefinedSpreading))]
    public class UniformConstantParametersSettingsViewModelTest<TSpreading> where TSpreading: class, IBoundaryConditionSpreading, new()
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var parameters = new ConstantParameters<TSpreading>(0, 0, 0, new TSpreading());

            // Call
            var viewModel = new UniformConstantParametersSettingsViewModel<TSpreading>(parameters);

            // Assert
            Assert.That(viewModel, Is.InstanceOf<ConstantParametersSettingsViewModel>());
            Assert.That(viewModel.ActiveParametersViewModel, Is.Not.Null, 
                        "Expected the ActiveParametersViewModel to not be null:");
            Assert.That(viewModel.GroupBoxTitle, Is.EqualTo("Uniform Constant Parameters"), 
                        "Expected the GroupBoxTitle to be different:");
        }

        [Test]
        public void Constructor_ActiveParametersViewModelNull_ThrowsArgumentNullException()
        {
            void Call() => new UniformConstantParametersSettingsViewModel<TSpreading>(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("parameters"));
        }
    }
}