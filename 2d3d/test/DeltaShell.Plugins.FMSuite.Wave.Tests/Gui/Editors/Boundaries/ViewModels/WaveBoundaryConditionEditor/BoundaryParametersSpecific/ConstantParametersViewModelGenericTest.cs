using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParametersSpecific
{
    [TestFixture(typeof(PowerDefinedSpreading))]
    [TestFixture(typeof(DegreesDefinedSpreading))]
    public class ConstantParametersViewModelTest<TSpreading> where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        private readonly Random random = new Random();

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            ConstantParameters<TSpreading> parameters = GetRandomConstantParameters();

            // Call
            var viewModel = new ConstantParametersViewModelGeneric<TSpreading>(parameters);

            // Assert
            Assert.That(viewModel.ObservedParameters, Is.SameAs(parameters));
            Assert.That(viewModel.Height, Is.EqualTo(parameters.Height));
            Assert.That(viewModel.Period, Is.EqualTo(parameters.Period));
            Assert.That(viewModel.Direction, Is.EqualTo(parameters.Direction));
            Assert.That(viewModel.Spreading, Is.EqualTo(GetSpreadingValue(parameters)));
        }

        [Test]
        public void Constructor_ParametersNull_ThrowsArgumentNullException()
        {
            void Call() => new ConstantParametersViewModelGeneric<TSpreading>(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("parameters"));
        }

        [Test]
        public void SetHeight_ExpectedValues()
        {
            // Setup
            ConstantParameters<TSpreading> parameters = GetRandomConstantParameters();
            var viewModel = new ConstantParametersViewModelGeneric<TSpreading>(parameters);
            const double expectedHeight = 50.0;

            // Call
            viewModel.Height = expectedHeight;

            // Assert
            Assert.That(viewModel.Height, Is.EqualTo(expectedHeight));
        }

        [Test]
        public void SetPeriod_ExpectedValues()
        {
            // Setup
            ConstantParameters<TSpreading> parameters = GetRandomConstantParameters();
            var viewModel = new ConstantParametersViewModelGeneric<TSpreading>(parameters);
            const double expectedPeriod = 50.0;

            // Call
            viewModel.Period = expectedPeriod;

            // Assert
            Assert.That(viewModel.Period, Is.EqualTo(expectedPeriod));
        }

        [Test]
        public void SetDirection_ExpectedValues()
        {
            // Setup
            ConstantParameters<TSpreading> parameters = GetRandomConstantParameters();
            var viewModel = new ConstantParametersViewModelGeneric<TSpreading>(parameters);
            const double expectedDirection = 50.0;

            // Call
            viewModel.Direction = expectedDirection;

            // Assert
            Assert.That(viewModel.Direction, Is.EqualTo(expectedDirection));
        }

        [Test]
        public void SetSpreading_ExpectedValues()
        {
            // Setup
            ConstantParameters<TSpreading> parameters = GetRandomConstantParameters();
            var viewModel = new ConstantParametersViewModelGeneric<TSpreading>(parameters);
            const double expectedSpreading = 50.0;

            // Call
            viewModel.Spreading = expectedSpreading;

            // Assert
            Assert.That(viewModel.Spreading, Is.EqualTo(expectedSpreading));
        }

        private ConstantParameters<TSpreading> GetRandomConstantParameters() => new ConstantParameters<TSpreading>(random.NextDouble() * 100.0,
                                                                                                                   random.NextDouble() * 100.0,
                                                                                                                   random.NextDouble() * 100.0,
                                                                                                                   new TSpreading());

        private static double GetSpreadingValue(ConstantParameters<TSpreading> param)
        {
            object spreading = param.Spreading;
            if (spreading is PowerDefinedSpreading pds)
            {
                return pds.SpreadingPower;
            }

            if (spreading is DegreesDefinedSpreading dds)
            {
                return dds.DegreesSpreading;
            }

            Assert.Fail("Unsupported Spreading type.");
            return double.NaN;
        }
    }
}