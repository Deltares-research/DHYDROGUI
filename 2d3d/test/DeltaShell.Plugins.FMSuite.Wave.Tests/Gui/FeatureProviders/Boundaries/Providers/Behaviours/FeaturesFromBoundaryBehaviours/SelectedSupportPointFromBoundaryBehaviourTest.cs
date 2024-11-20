using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers.Behaviours.FeaturesFromBoundaryBehaviours;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.FeatureProviders.Boundaries.Providers.Behaviours.FeaturesFromBoundaryBehaviours
{
    [TestFixture]
    public class SelectedSupportPointFromBoundaryBehaviourTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            SupportPointDataComponentViewModel viewModel = GetViewModel();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();

            // Call
            var behaviour = new SelectedSupportPointFromBoundaryBehaviour(viewModel,
                                                                          geometryFactory);

            // Assert
            Assert.That(behaviour, Is.InstanceOf<IFeaturesFromBoundaryBehaviour>());
        }

        [Test]
        public void Constructor_SupportPointDataComponentViewModelNull_ThrowsArgumentNullException()
        {
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            void Call() => new SelectedSupportPointFromBoundaryBehaviour(null, geometryFactory);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("supportPointDataComponentViewModel"));
        }

        [Test]
        public void Constructor_GeometryFactoryNull_ThrowsArgumentNullException()
        {
            SupportPointDataComponentViewModel viewModel = GetViewModel();
            void Call() => new SelectedSupportPointFromBoundaryBehaviour(viewModel, null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            Assert.That(exception.ParamName, Is.EqualTo("geometryFactory"));
        }

        [Test]
        public void Execute_IsEnabled_ReturnsExpectedFeature()
        {
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var announceChanged = Substitute.For<IAnnounceSupportPointDataChanged>();

            conditionDefinition.DataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>>();

            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition,
                                                                   parametersFactory,
                                                                   announceChanged);
            viewModel.SelectedSupportPoint = new SupportPoint(10.0,
                                                              Substitute.For<IWaveBoundaryGeometricDefinition>());

            var point = Substitute.For<IPoint>();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            geometryFactory.ConstructBoundarySupportPoint(viewModel.SelectedSupportPoint)
                           .Returns(point);

            var behaviour = new SelectedSupportPointFromBoundaryBehaviour(viewModel,
                                                                          geometryFactory);

            var boundary = Substitute.For<IWaveBoundary>();

            // Call
            IEnumerable<IFeature> result = behaviour.Execute(boundary);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            IFeature selectedFeature = result.First();
            Assert.That(selectedFeature, Is.Not.Null);
            Assert.That(selectedFeature.Geometry, Is.SameAs(point));
        }

        [Test]
        public void Execute_NoGeometry_ReturnsEmptyCollection()
        {
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var announceChanged = Substitute.For<IAnnounceSupportPointDataChanged>();

            conditionDefinition.DataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>>();

            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition,
                                                                   parametersFactory,
                                                                   announceChanged);
            viewModel.SelectedSupportPoint = new SupportPoint(10.0,
                                                              Substitute.For<IWaveBoundaryGeometricDefinition>());

            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            geometryFactory.ConstructBoundarySupportPoint(viewModel.SelectedSupportPoint)
                           .Returns((IPoint) null);

            var behaviour = new SelectedSupportPointFromBoundaryBehaviour(viewModel,
                                                                          geometryFactory);

            var boundary = Substitute.For<IWaveBoundary>();

            // Call
            IEnumerable<IFeature> result = behaviour.Execute(boundary);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Execute_NotEnabled_ReturnsEmptyCollection()
        {
            // Setup
            SupportPointDataComponentViewModel viewModel = GetViewModel();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();

            var behaviour = new SelectedSupportPointFromBoundaryBehaviour(viewModel,
                                                                          geometryFactory);

            var boundary = Substitute.For<IWaveBoundary>();

            // Call
            IEnumerable<IFeature> result = behaviour.Execute(boundary);

            // Assert
            Assert.That(result, Is.Empty);
        }

        private static SupportPointDataComponentViewModel GetViewModel()
        {
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var announceChanged = Substitute.For<IAnnounceSupportPointDataChanged>();

            return new SupportPointDataComponentViewModel(conditionDefinition,
                                                          parametersFactory,
                                                          announceChanged);
        }
    }
}