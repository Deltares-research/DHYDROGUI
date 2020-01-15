using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
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
            var shape = new GaussShape();
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();

            conditionDefinition.Shape = shape;

            var boundary = new WaveBoundary("boundary", geometricDefinition, conditionDefinition);
            var factory = Substitute.For<IWaveBoundaryGeometryFactory>();

            boundary.Name = "A Boundary Name";

            // Call
            var viewModel = new WaveBoundaryConditionEditorViewModel(boundary, factory);

            // Assert
            Assert.That(viewModel.Name, Is.EqualTo(boundary.Name), 
                        "Expected a different Name:");
            Assert.That(viewModel.DescriptionViewModel, Is.Not.Null,
                        "Expected DescriptionViewModel to be set.");
            Assert.That(viewModel.BoundaryWideParametersViewModel, Is.Not.Null,
                        "Expected BoundaryWideParametersViewModel to be set.");
            Assert.That(viewModel.GeometryViewModel, Is.Not.Null,
                        "Expected GeometryViewModel to be set.");
        }

        [Test]
        public void Constructor_ObservedBoundaryNull_ThrowsArgumentNullException()
        {
            // Setup
            var factory = Substitute.For<IWaveBoundaryGeometryFactory>();

            // Call
            void Call() => new WaveBoundaryConditionEditorViewModel(null, factory);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("observedBoundary"));
        }
    }
}