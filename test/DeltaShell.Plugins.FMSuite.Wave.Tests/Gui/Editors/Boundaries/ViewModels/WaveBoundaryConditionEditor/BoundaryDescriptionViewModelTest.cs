using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    [TestFixture]
    public class BoundaryDescriptionViewModelTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            const string expectedName = "aBoundaryName";

            var boundary = Substitute.For<IWaveBoundary>();
            boundary.Name = expectedName;

            // Call
            var viewModel = new BoundaryDescriptionViewModel(boundary);

            // Assert
            Assert.That(viewModel.Name, Is.EqualTo(expectedName), 
                        "Expected a different Name:");
        }

        [Test]
        public void Constructor_ObservedBoundaryNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new BoundaryDescriptionViewModel(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo("observedBoundary"),
                        "Expected a different ParamName:");
        }

        [Test]
        public void GivenABoundaryDescriptionViewModelWithABoundary_WhenNameIsSet_ThenTheNameInTheBoundaryIsAdjusted()
        {
            // Setup
            const string expectedName = "aBoundaryName";
            
            var boundary = Substitute.For<IWaveBoundary>();
            var viewModel = new BoundaryDescriptionViewModel(boundary);

            // Call
            viewModel.Name = expectedName;


            // Assert
            boundary.Received(1).Name = expectedName;
        }
    }
}