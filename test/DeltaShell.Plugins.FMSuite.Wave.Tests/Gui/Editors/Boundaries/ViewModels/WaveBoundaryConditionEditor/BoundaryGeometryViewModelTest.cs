using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    [TestFixture]
    public class BoundaryGeometryViewModelTest
    {
        [Test]
        public void Constructor_SetsCorrectValues()
        {
            // Setup
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();

            // Call
            var viewModel = new BoundaryGeometryViewModel(geometricDefinition);

            // Assert
            Assert.That(viewModel.SupportPointEditorViewModel, Is.Not.Null,
                        "Expected SupportPointEditorViewModel to be set.");
        }

        [Test]
        public void Constructor_GeometricDefinitionNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new BoundaryGeometryViewModel(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("observedGeometricDefinition"));
        }
    }
}