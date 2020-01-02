using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels
{
    [TestFixture]
    public class WaveBoundaryConditionEditorViewModelTest
    {
        [Test]
        public void Constructor_ObservedBoundaryValid_SetsCorrectValues()
        {
            // Setup
            var boundary = Substitute.For<IWaveBoundary>();
            boundary.Name = "A Boundary Name";

            // Call
            var viewModel = new WaveBoundaryConditionEditorViewModel(boundary);

            // Assert
            Assert.That(viewModel.Name, Is.EqualTo(boundary.Name), "Expected a different Name:");
        }

        [Test]
        public void Constructor_ObservedBoundaryNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new WaveBoundaryConditionEditorViewModel(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("observedBoundary"));
        }
    }
}