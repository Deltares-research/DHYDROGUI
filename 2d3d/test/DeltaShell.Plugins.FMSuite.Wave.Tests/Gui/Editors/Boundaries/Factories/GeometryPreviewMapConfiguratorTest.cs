using System;
using System.Collections.Generic;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using GeoAPI.Extensions.CoordinateSystems;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.Factories
{
    [TestFixture]
    public class GeometryPreviewMapConfiguratorTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();

            // Call
            var configurator = new GeometryPreviewMapConfigurator(geometryFactory,
                                                                  instanceCreator,
                                                                  coordinateSystem);

            // Assert
            Assert.That(configurator, Is.InstanceOf<IGeometryPreviewMapConfigurator>());
        }

        [Test]
        public void Constructor_GeometryFactoryNull_ThrowsArgumentNullException()
        {
            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();

            void Call() => new GeometryPreviewMapConfigurator(null,
                                                              instanceCreator,
                                                              coordinateSystem);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("geometryFactory"));
        }

        [Test]
        public void Constructor_LayerFactoryNull_ThrowsArgumentNullException()
        {
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();

            void Call() => new GeometryPreviewMapConfigurator(geometryFactory,
                                                              null,
                                                              coordinateSystem);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("layerInstanceCreator"));
        }

        [Test]
        public void ConfigureMap_ExpectedResults()
        {
            // Setup
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            var instanceCreator = Substitute.For<IWaveLayerInstanceCreator>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();

            var lineLayer = Substitute.For<ILayer>();
            instanceCreator.CreateBoundaryLineLayer(Arg.Is<IFeatureProvider>(x => x != null))
                        .Returns(lineLayer);

            var startLayer = Substitute.For<ILayer>();
            instanceCreator.CreateBoundaryStartPointLayer(Arg.Is<IFeatureProvider>(x => x != null))
                        .Returns(startLayer);

            var endLayer = Substitute.For<ILayer>();
            instanceCreator.CreateBoundaryEndPointLayer(Arg.Is<IFeatureProvider>(x => x != null))
                        .Returns(endLayer);

            var supportPointsLayer = Substitute.For<ILayer>();
            instanceCreator.CreateSupportPointsLayer(Arg.Is<IFeatureProvider>(x => x != null))
                        .Returns(supportPointsLayer);

            var activeSupportPointsLayer = Substitute.For<ILayer>();
            instanceCreator.CreateActiveSupportPointsLayer(Arg.Is<IFeatureProvider>(x => x != null))
                        .Returns(activeSupportPointsLayer);

            var inactiveSupportPointsLayer = Substitute.For<ILayer>();
            instanceCreator.CreateInactiveSupportPointsLayer(Arg.Is<IFeatureProvider>(x => x != null))
                        .Returns(inactiveSupportPointsLayer);

            var selectedSupportPointsLayer = Substitute.For<ILayer>();
            instanceCreator.CreateSelectedSupportPointLayer(Arg.Is<IFeatureProvider>(x => x != null))
                        .Returns(selectedSupportPointsLayer);

            var configurator = new GeometryPreviewMapConfigurator(geometryFactory,
                                                                  instanceCreator,
                                                                  coordinateSystem);

            var map = Substitute.For<IMap>();
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            SupportPointDataComponentViewModel viewModel = GetViewModel();
            var refreshGeometryView = Substitute.For<IRefreshGeometryView>();

            // Call
            configurator.ConfigureMap(map, boundaryProvider, viewModel, refreshGeometryView);

            // Assert
            instanceCreator.Received(1).CreateBoundaryLineLayer(Arg.Is<IFeatureProvider>(x => x != null));
            instanceCreator.Received(1).CreateBoundaryStartPointLayer(Arg.Is<IFeatureProvider>(x => x != null));
            instanceCreator.Received(1).CreateBoundaryEndPointLayer(Arg.Is<IFeatureProvider>(x => x != null));
            instanceCreator.Received(1).CreateSupportPointsLayer(Arg.Is<IFeatureProvider>(x => x != null));
            instanceCreator.Received(1).CreateActiveSupportPointsLayer(Arg.Is<IFeatureProvider>(x => x != null));
            instanceCreator.Received(1).CreateInactiveSupportPointsLayer(Arg.Is<IFeatureProvider>(x => x != null));
            instanceCreator.Received(1).CreateSelectedSupportPointLayer(Arg.Is<IFeatureProvider>(x => x != null));

            lineLayer.Received(1).RenderOrder = 5;
            startLayer.Received(1).RenderOrder = 1;
            endLayer.Received(1).RenderOrder = 1;
            supportPointsLayer.Received(1).RenderOrder = 2;
            activeSupportPointsLayer.Received(1).RenderOrder = 3;
            inactiveSupportPointsLayer.Received(1).RenderOrder = 4;
            selectedSupportPointsLayer.Received(1).RenderOrder = 6;

            map.Received(1).ZoomToExtents();
        }

        [Test]
        [TestCaseSource(nameof(GetParamNullData))]
        public void ConfigureMap_ParameterNull_ThrowsArgumentNullException(IMap map,
                                                                           IBoundaryProvider boundaryProvider,
                                                                           SupportPointDataComponentViewModel viewModel,
                                                                           IRefreshGeometryView geometryView,
                                                                           string expectedParamName)
        {
            // Setup
            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            var layerInstanceCreator = Substitute.For<IWaveLayerInstanceCreator>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();

            var configurator = new GeometryPreviewMapConfigurator(geometryFactory,
                                                                  layerInstanceCreator,
                                                                  coordinateSystem);

            // Call | Assert
            void Call() => configurator.ConfigureMap(map, boundaryProvider, viewModel, geometryView);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParamName));
        }

        /// <summary>
        /// GIVEN a GeometryPreviewMapConfigurator
        /// AND a map
        /// AND a BoundaryProvider
        /// AND a RefreshGeometryView
        /// WHEN the map is configured by the configurator
        /// AND a support point is added to the boundary
        /// THEN the refresh geometry view is called
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAGeometryPreviewMapConfigurator_WhenAMapIsConfigured_ThenAddingSupportPointsToTheBoundaryWillRefreshTheMap()
        {
            // Given
            var map = Substitute.For<IMap>();

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var supportPoints = new EventedList<SupportPoint>();
            geometricDefinition.SupportPoints.Returns(supportPoints);

            var boundary = Substitute.For<IWaveBoundary>();
            boundary.GeometricDefinition.Returns(geometricDefinition);

            var boundaryList = new EventedList<IWaveBoundary> {boundary};

            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            boundaryProvider.Boundaries.Returns(boundaryList);

            var geometryFactory = Substitute.For<IWaveBoundaryGeometryFactory>();
            var layerInstanceCreator = Substitute.For<IWaveLayerInstanceCreator>();

            var mapConfigurator = new GeometryPreviewMapConfigurator(geometryFactory, layerInstanceCreator, null);
            var refreshGeometryView = Substitute.For<IRefreshGeometryView>();

            var supportPoint = new SupportPoint(10.0, geometricDefinition);

            // When
            mapConfigurator.ConfigureMap(map, boundaryProvider, GetViewModel(), refreshGeometryView);
            supportPoints.Add(supportPoint);

            // Then
            refreshGeometryView.Received(1).RefreshGeometryView();
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

        private static IEnumerable<TestCaseData> GetParamNullData()
        {
            var map = Substitute.For<IMap>();
            var boundaryProvider = Substitute.For<IBoundaryProvider>();
            SupportPointDataComponentViewModel viewModel = GetViewModel();
            var refreshGeometryView = Substitute.For<IRefreshGeometryView>();

            yield return new TestCaseData(null, boundaryProvider, viewModel, refreshGeometryView, "map");
            yield return new TestCaseData(map, null, viewModel, refreshGeometryView, "boundaryProvider");
            yield return new TestCaseData(map, boundaryProvider, null, refreshGeometryView, "supportPointDataComponentViewModel");
            yield return new TestCaseData(map, boundaryProvider, viewModel, null, "refreshGeometryView");
        }
    }
}