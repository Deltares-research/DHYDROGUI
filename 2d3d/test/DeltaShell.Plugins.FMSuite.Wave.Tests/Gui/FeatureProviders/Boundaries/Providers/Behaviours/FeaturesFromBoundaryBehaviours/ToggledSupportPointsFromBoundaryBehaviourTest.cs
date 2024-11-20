using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
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
    public class ToggledSupportPointsFromBoundaryBehaviourTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            SupportPointDataComponentViewModel viewModel = GetViewModel();
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();

            // Call
            var behaviour =
                new ToggledSupportPointsFromBoundaryBehaviour(true,
                                                              viewModel,
                                                              geometryFactory);

            // Assert
            Assert.That(behaviour, Is.InstanceOf<IFeaturesFromBoundaryBehaviour>());
        }

        [Test]
        public void Constructor_SupportPointDataComponentViewModelNull_ThrowsArgumentNullException()
        {
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();

            void Call() => new ToggledSupportPointsFromBoundaryBehaviour(true,
                                                                         null,
                                                                         geometryFactory);

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
        public void Execute_SelectsOnlyExpectedStateSupportPoints()
        {
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var parametersFactory = Substitute.For<IForcingTypeDefinedParametersFactory>();
            var announceChanged = Substitute.For<IAnnounceSupportPointDataChanged>();

            var data = new SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>>();
            conditionDefinition.DataComponent = data;

            var viewModel = new SupportPointDataComponentViewModel(conditionDefinition,
                                                                   parametersFactory,
                                                                   announceChanged);

            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            geometryFactory.ConstructBoundarySupportPoint(null).ReturnsForAnyArgs((IPoint) null);

            var behaviour = new ToggledSupportPointsFromBoundaryBehaviour(true,
                                                                          viewModel,
                                                                          geometryFactory);

            var boundary = Substitute.For<IWaveBoundary>();
            boundary.GeometricDefinition.Length.Returns(50.0);

            var supportPoints = new EventedList<SupportPoint>();
            supportPoints.AddRange(Enumerable.Range(0, 10).Select(x => new SupportPoint(x + 10.0, boundary.GeometricDefinition)));

            var supportPointWithData = new SupportPoint(30.0, boundary.GeometricDefinition);
            supportPoints.Add(supportPointWithData);
            data.AddParameters(supportPointWithData, new ConstantParameters<DegreesDefinedSpreading>(0, 0, 0, new DegreesDefinedSpreading()));

            var pointWithData = Substitute.For<IPoint>();
            geometryFactory.ConstructBoundarySupportPoint(supportPointWithData).Returns(pointWithData);

            boundary.GeometricDefinition.SupportPoints.Returns(supportPoints);

            // Call
            IEnumerable<IFeature> result = behaviour.Execute(boundary);

            // Assert
            List<IFeature> resultsList = result.ToList();
            Assert.That(resultsList.Count, Is.EqualTo(1));

            IFeature feature = resultsList.First();
            Assert.That(feature.Geometry, Is.SameAs(pointWithData));

            geometryFactory.Received(1).ConstructBoundarySupportPoint(supportPointWithData);
            geometryFactory.ReceivedWithAnyArgs(1).ConstructBoundarySupportPoint(supportPointWithData);
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

            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            geometryFactory.ConstructBoundarySupportPoint(null).ReturnsForAnyArgs((IPoint) null);

            var behaviour = new ToggledSupportPointsFromBoundaryBehaviour(false,
                                                                          viewModel,
                                                                          geometryFactory);

            var boundary = Substitute.For<IWaveBoundary>();
            boundary.GeometricDefinition.Length.Returns(50.0);

            var supportPoints = new EventedList<SupportPoint>();
            supportPoints.AddRange(Enumerable.Range(0, 10).Select(x => new SupportPoint(x + 10.0, boundary.GeometricDefinition)));

            boundary.GeometricDefinition.SupportPoints.Returns(supportPoints);

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

            var behaviour = new ToggledSupportPointsFromBoundaryBehaviour(false,
                                                                          viewModel,
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