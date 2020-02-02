using System;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    [TestFixture]
    public class BoundaryGeometryViewModelTest
    {
        private static SupportPointDataComponentViewModel GetDefaultDataComponentViewModel()
        {
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.DataComponent = 
                new SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>();
            var mediator = Substitute.For<IAnnounceSelectedSupportPointDataChanged>();

            return new SupportPointDataComponentViewModel(conditionDefinition,
                                                          Substitute.For<IBoundaryParametersFactory>(),
                                                          mediator);
        }

        [Test]
        public void Constructor_SetsCorrectValues()
        {
            // Setup
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.SupportPoints.Returns(new EventedList<SupportPoint>()
            {
                new SupportPoint(0, geometricDefinition),
                new SupportPoint(1, geometricDefinition)
            });

            var boundary = Substitute.For<IWaveBoundary>();
            boundary.GeometricDefinition.Returns(geometricDefinition);
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            SupportPointDataComponentViewModel dataComponentViewModel = GetDefaultDataComponentViewModel();


            // Call
            var viewModel = new BoundaryGeometryViewModel(boundary,
                                                          geometryFactory,
                                                          dataComponentViewModel);

            // Assert
            Assert.That(viewModel.SupportPointEditorViewModel, Is.Not.Null,
                        "Expected SupportPointEditorViewModel to be set.");
            Assert.That(viewModel.GeometryPreviewViewModel, Is.Not.Null,
                        "Expected GeometryPreviewViewModel to be set.");
        }

        [Test]
        public void Constructor_WaveBoundaryNull_ThrowsArgumentNullException()
        {
            // Setup
            var factory = Substitute.For<IWaveBoundaryGeometryFactory>();
            SupportPointDataComponentViewModel dataComponentViewModel = GetDefaultDataComponentViewModel();

            // Call
            void Call() => new BoundaryGeometryViewModel(null, factory, dataComponentViewModel);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveBoundary"));
        }

        [Test]
        public void Constructor_GeometryFactoryNull_ThrowsArgumentNullException()
        {
            // Setup
            var waveBoundary = Substitute.For<IWaveBoundary>();
            SupportPointDataComponentViewModel dataComponentViewModel = GetDefaultDataComponentViewModel();

            // Call
            void Call() => new BoundaryGeometryViewModel(waveBoundary, null, dataComponentViewModel);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("geometryFactory"));
        }

        [Test]
        public void Constructor_SupportPointDataComponentViewModelNull_ThrowsArgumentNullException()
        {
            // Setup
            var waveBoundary = Substitute.For<IWaveBoundary>();
            var factory = Substitute.For<IWaveBoundaryGeometryFactory>();

            // Call
            void Call() => new BoundaryGeometryViewModel(waveBoundary, factory, null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("supportPointDataComponentViewModel"));
        }
    }
}