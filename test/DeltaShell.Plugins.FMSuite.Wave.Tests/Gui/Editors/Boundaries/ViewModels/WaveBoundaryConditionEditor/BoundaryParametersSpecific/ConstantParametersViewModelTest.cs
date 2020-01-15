using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParametersSpecific
{
    [TestFixture]
    public class ConstantParametersViewModelTest
    {
        private readonly Random random = new Random();

        private ConstantParameters GetRandomConstantParameters()
        {
            return new ConstantParameters(random.NextDouble() * 100.0,
                                          random.NextDouble() * 100.0,
                                          random.NextDouble() * 100.0,
                                          random.NextDouble() * 100.0);
        }

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            ConstantParameters parameters = GetRandomConstantParameters();

            // Call
            var viewModel = new ConstantParametersViewModel(parameters);

            // Assert
            Assert.That(viewModel.Height,    Is.EqualTo(parameters.Height));
            Assert.That(viewModel.Period,    Is.EqualTo(parameters.Period));
            Assert.That(viewModel.Direction, Is.EqualTo(parameters.Direction));
            Assert.That(viewModel.Spreading, Is.EqualTo(parameters.Spreading));
        }

        [Test]
        public void Constructor_ParametersNull_ThrowsArgumentNullException()
        {
            void Call() => new ConstantParametersViewModel(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("parameters"));
        }

        [Test]
        public void SetHeight_ExpectedValues()
        {
            // Setup
            ConstantParameters parameters = GetRandomConstantParameters();
            var viewModel = new ConstantParametersViewModel(parameters);
            const double expectedHeight = 50.0;

            // Call
            viewModel.Height = expectedHeight;

            // Assert
            Assert.That(viewModel.Height,    Is.EqualTo(expectedHeight));
        }

        [Test]
        public void SetPeriod_ExpectedValues()
        {
            // Setup
            ConstantParameters parameters = GetRandomConstantParameters();
            var viewModel = new ConstantParametersViewModel(parameters);
            const double expectedPeriod = 50.0;

            // Call
            viewModel.Period = expectedPeriod;

            // Assert
            Assert.That(viewModel.Period,    Is.EqualTo(expectedPeriod));
        }

        [Test]
        public void SetDirection_ExpectedValues()
        {
            // Setup
            ConstantParameters parameters = GetRandomConstantParameters();
            var viewModel = new ConstantParametersViewModel(parameters);
            const double expectedDirection = 50.0;

            // Call
            viewModel.Direction = expectedDirection;

            // Assert
            Assert.That(viewModel.Direction,    Is.EqualTo(expectedDirection));
        }

        [Test]
        public void SetSpreading_ExpectedValues()
        {
            // Setup
            ConstantParameters parameters = GetRandomConstantParameters();
            var viewModel = new ConstantParametersViewModel(parameters);
            const double expectedSpreading = 50.0;

            // Call
            viewModel.Spreading = expectedSpreading;

            // Assert
            Assert.That(viewModel.Spreading,    Is.EqualTo(expectedSpreading));
        }
    }
}
