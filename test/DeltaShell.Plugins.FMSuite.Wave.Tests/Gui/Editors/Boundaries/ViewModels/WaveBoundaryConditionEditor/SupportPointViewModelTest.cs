using System;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    [TestFixture]
    public class SupportPointViewModelTest
    {
        private readonly Random random = new Random();
        private SupportPointViewModel viewModel;
        private SupportPoint supportPoint;

        [SetUp]
        public void SetUp()
        {
            supportPoint = new SupportPoint(0, Substitute.For<IWaveBoundaryGeometricDefinition>());
            viewModel = new SupportPointViewModel(supportPoint);
        }

        [Test]
        public void Constructor_SupportPointNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new SupportPointViewModel(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("supportPoint"));
        }

        [Test]
        public void Constructor_SetsCorrectValue()
        {
            // Assert
            Assert.That(viewModel.SupportPoint, Is.SameAs(supportPoint));
        }

        [TestCase(false, false, 0)]
        [TestCase(false, true, 1)]
        [TestCase(true, false, 1)]
        [TestCase(true, true, 0)]
        public void SetEnabled_PropertyChangedFiredOnce(bool originalValue, bool setValue, int expectedPropChangedCount)
        {
            // Setup
            viewModel.IsEnabled = originalValue;

            // Call
            void Call() => viewModel.IsEnabled = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropChangedCount, nameof(viewModel.IsEnabled));
        }

        [TestCase(0, 0)]
        [TestCase(1E-15, 0)]
        [TestCase(1E-14, 1)]
        [TestCase(1, 1)]
        public void SetDistance_CorrectDistanceIsSetOnModelAndPropertyChangedFiredOnce(double setValueDifference,
                                                                                       int expectedPropChangedCount)
        {
            // Setup
            double originalValue = random.NextDouble();
            double setValue = originalValue + setValueDifference;
            viewModel.Distance = originalValue;

            // Call
            void Call() => viewModel.Distance = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropChangedCount, nameof(viewModel.Distance));
            Assert.That(supportPoint.Distance, Is.EqualTo(setValue).Within(1E-15));
        }
    }
}