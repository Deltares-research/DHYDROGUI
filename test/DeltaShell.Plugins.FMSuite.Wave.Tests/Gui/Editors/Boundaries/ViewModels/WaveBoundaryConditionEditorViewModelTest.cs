using System;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
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
            WaveBoundary boundary = CreateBoundary();
            var configurator = Substitute.For<IGeometryPreviewMapConfigurator>();
            var referenceTimeProvider = Substitute.For<IReferenceDateTimeProvider>();

            boundary.Name = "A Boundary Name";

            // Call
            var viewModel = new WaveBoundaryConditionEditorViewModel(boundary, configurator, referenceTimeProvider);

            // Assert
            Assert.That(viewModel.Name, Is.EqualTo(boundary.Name),
                        "Expected a different Name:");
            Assert.That(viewModel.DescriptionViewModel, Is.Not.Null,
                        "Expected DescriptionViewModel to be set.");
            Assert.That(viewModel.BoundaryWideParametersViewModel, Is.Not.Null,
                        "Expected BoundaryWideParametersViewModel to be set.");
            Assert.That(viewModel.GeometryViewModel, Is.Not.Null,
                        "Expected GeometryViewModel to be set.");
            Assert.That(viewModel.BoundarySpecificParametersSettingsViewModel, Is.Not.Null,
                        "Expected BoundarySpecificParameters");
        }

        [Test]
        public void Constructor_ObservedBoundaryNull_ThrowsArgumentNullException()
        {
            // Setup
            var configurator = Substitute.For<IGeometryPreviewMapConfigurator>();
            var referenceTimeProvider = Substitute.For<IReferenceDateTimeProvider>();

            // Call
            void Call() => new WaveBoundaryConditionEditorViewModel(null, configurator, referenceTimeProvider);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("observedBoundary"));
        }

        [Test]
        public void Constructor_PreviewMapConfiguratorNull_ThrowsArgumentNullException()
        {
            // Setup
            var referenceTimeProvider = Substitute.For<IReferenceDateTimeProvider>();

            // Call
            void Call() => new WaveBoundaryConditionEditorViewModel(CreateBoundary(), null, referenceTimeProvider);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("previewMapConfigurator"));
        }

        [Test]
        public void Constructor_ReferenceDateTimeProviderNull_ThrowsArgumentNullException()
        {
            // Setup
            var configurator = Substitute.For<IGeometryPreviewMapConfigurator>();

            void Call() => new WaveBoundaryConditionEditorViewModel(CreateBoundary(), configurator, null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("referenceDateTimeProvider"));
        }

        private static WaveBoundary CreateBoundary()
        {
            var shape = new GaussShape();
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.SupportPoints.Returns(new EventedList<SupportPoint>()
            {
                new SupportPoint(0, geometricDefinition),
                new SupportPoint(1, geometricDefinition)
            });

            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.DataComponent =
                new UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>(
                    new ConstantParameters<PowerDefinedSpreading>(0.0,
                                                                  0.0,
                                                                  0.0,
                                                                  new PowerDefinedSpreading()));

            conditionDefinition.Shape = shape;

            var boundary = new WaveBoundary("boundary", geometricDefinition, conditionDefinition);
            return boundary;
        }
    }
}