using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.FMSuite.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Containers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers
{
    [TestFixture]
    public class WaveLayerInstanceCreatorTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var instanceCreator = new WaveLayerInstanceCreator();

            // Assert
            Assert.That(instanceCreator, Is.InstanceOf<IWaveLayerInstanceCreator>());
        }

        [Test]
        public void CreateModelGroupLayer_ValidModel_ReturnsCorrectResults()
        {
            // Setup
            var waveModel = new WaveModel();
            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            ILayer layer = instanceCreator.CreateModelGroupLayer(waveModel);

            // Assert
            Assert.That(layer, Is.InstanceOf<ModelGroupLayer>(),
                        $"Expected the result to be an instance of {nameof(ModelGroupLayer)}");

            var modelGroupLayer = (ModelGroupLayer) layer;
            Assert.That(modelGroupLayer.Model, Is.SameAs(waveModel),
                        "Expected the model of the layer to the same as the provided model.");
            Assert.That(modelGroupLayer.Name, Is.EqualTo(waveModel.Name),
                        "Expected the name of the layer to the equal as the provided model name.");
        }

        [Test]
        public void CreateModelGroupLayer_WaveModelNull_ThrowsArgumentNullException()
        {
            // Setup
            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            void Call() => instanceCreator.CreateModelGroupLayer(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("waveModel"));
        }

        [Test]
        public void CreateWaveDomainDataLayer_ValidDomain_ReturnsCorrectResults()
        {
            // Setup
            const string expectedDomainName = "DomainName";
            var domain = new WaveDomainData(expectedDomainName);
            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            ILayer layer = instanceCreator.CreateWaveDomainDataLayer(domain);

            // Assert
            Assert.That(layer, Is.InstanceOf<GroupLayer>(),
                        $"Expected the result to be an instance of {nameof(GroupLayer)}");
            Assert.That(layer.Name, Is.EqualTo($"Domain ({expectedDomainName})"),
                        "Expected the layer to have a different name.");
        }

        [Test]
        public void CreateWaveDomainDataLayer_DomainNull_ThrowsArgumentNullException()
        {
            // Setup
            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            void Call() => instanceCreator.CreateWaveDomainDataLayer(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("domain"));
        }

        [Test]
        public void CreateObstacleLayer_ValidWaveModel_ReturnsCorrectResults()
        {
            // Setup
            var model = new WaveModel();
            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            ILayer layer = instanceCreator.CreateObstacleLayer(model);

            // Assert
            Assert.That(layer, Is.InstanceOf<VectorLayer>(),
                        $"Expected the result to be an instance of {nameof(VectorLayer)}");
            Assert.That(layer.Name, Is.EqualTo("Obstacles"),
                        "Expected the layer to have a different name.");
        }

        [Test]
        public void CreateObstacleLayer_WaveModelNull_ThrowsArgumentNullException()
        {
            // Setup
            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            void Call() => instanceCreator.CreateObstacleLayer(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("waveModel"));
        }

        [Test]
        public void CreateObservationPointsLayer_ValidWaveModel_ReturnsCorrectResults()
        {
            // Setup
            var model = new WaveModel();
            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            ILayer layer = instanceCreator.CreateObservationPointsLayer(model);

            // Assert
            Assert.That(layer, Is.InstanceOf<VectorLayer>(),
                        $"Expected the result to be an instance of {nameof(VectorLayer)}");
            Assert.That(layer.Name, Is.EqualTo("Observation Points"),
                        "Expected the layer to have a different name.");
        }

        [Test]
        public void CreateObservationPointsLayer_WaveModelNull_ThrowsArgumentNullException()
        {
            // Setup
            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            void Call() => instanceCreator.CreateObservationPointsLayer(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("waveModel"));
        }

        [Test]
        public void CreateObservationCrossSectionsLayer_ValidWaveModel_ReturnsCorrectResults()
        {
            // Setup
            var model = new WaveModel();
            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            ILayer layer = instanceCreator.CreateObservationCrossSectionLayer(model);

            // Assert
            Assert.That(layer, Is.InstanceOf<VectorLayer>(),
                        $"Expected the result to be an instance of {nameof(VectorLayer)}");
            Assert.That(layer.Name, Is.EqualTo("Observation Cross-Sections"),
                        "Expected the layer to have a different name.");
        }

        [Test]
        public void CreateObservationCrossSectionsLayer_WaveModelNull_ThrowsArgumentNullException()
        {
            // Setup
            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            void Call() => instanceCreator.CreateObservationCrossSectionLayer(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("waveModel"));
        }

        [Test]
        public void CreateGridLayer_ValidCurvilinearGrid_ReturnsCorrectResults()
        {
            // Setup
            const string gridName = "gridName";

            IList<double> xCoordinates = new List<double> {0.0};
            IList<double> yCoordinates = new List<double> {0.0};

            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var grid = new CurvilinearGrid(1, 1,
                                           xCoordinates, yCoordinates,
                                           string.Empty) {Name = gridName};

            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            ILayer layer = instanceCreator.CreateGridLayer(grid, coordinateSystem);

            // Assert
            Assert.That(layer, Is.InstanceOf<CurvilinearGridLayer>(),
                        $"Expected the result to be an instance of {nameof(CurvilinearGridLayer)}");
            Assert.That(layer.Name, Is.EqualTo(gridName),
                        "Expected the layer to have a different name.");
        }

        [Test]
        public void CreateGridLayer_ValidIDiscreteGridPointCoverage_ReturnsCorrectResults()
        {
            // Setup
            const string gridName = "gridName";
            IList<double> xCoordinates = new List<double> {0.0};
            IList<double> yCoordinates = new List<double> {0.0};

            var grid = new DiscreteGridPointCoverage(1, 1, xCoordinates, yCoordinates) {Name = gridName};

            var coordinateSystem = Substitute.For<ICoordinateSystem>();

            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            ILayer layer = instanceCreator.CreateGridLayer(grid, coordinateSystem);

            // Assert
            Assert.That(layer, Is.InstanceOf<CurvilinearVertexCoverageLayer>(),
                        $"Expected the result to be an instance of {nameof(CurvilinearVertexCoverageLayer)}");
            Assert.That(layer.Name, Is.EqualTo(gridName),
                        "Expected the layer to have a different name.");
        }

        [Test]
        public void CreateGridLayer_DiscreteGridNull_ThrowsArgumentNullException()
        {
            // Setup
            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            void Call() => instanceCreator.CreateGridLayer(null, coordinateSystem);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("discreteGrid"));
        }

        [Test]
        public void CreateBoundaryLayer_ValidParameters_ReturnsCorrectResults()
        {
            // Setup
            var featureProviderContainer = Substitute.For<IBoundaryMapFeaturesContainer>();
            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            ILayer layer = instanceCreator.CreateBoundaryLayer(featureProviderContainer);

            Assert.That(layer, Is.InstanceOf<GroupLayer>(),
                        $"Expected the result to be an instance of {nameof(GroupLayer)}");
            Assert.That(layer.Name, Is.EqualTo(WaveLayerNames.BoundaryLayerName),
                        "Expected the layer to have a different name.");

            var groupLayer = (GroupLayer) layer;
            Assert.That(groupLayer.Layers.Count, Is.EqualTo(4),
                        "Expected a different number of layers:");

            ILayer lineLayer = groupLayer.Layers.FirstOrDefault(x => x.Name == WaveLayerNames.BoundaryLineLayerName);
            Assert.That(lineLayer, Is.Not.Null,
                        $"Expected the layer with name '{WaveLayerNames.BoundaryLineLayerName}' to exist.");
            Assert.That(lineLayer, Is.InstanceOf(typeof(VectorLayer)),
                        $"Expected the layer with name '{WaveLayerNames.BoundaryLineLayerName}' to be of type {typeof(VectorLayer)}");
            AssertCorrectMapTreeBehaviourSubBoundaryLayer(lineLayer);

            ILayer startPointsLayer =
                groupLayer.Layers.FirstOrDefault(x => x.Name == WaveLayerNames.BoundaryStartPointsLayerName);
            Assert.That(startPointsLayer, Is.Not.Null,
                        $"Expected the layer with name '{WaveLayerNames.BoundaryStartPointsLayerName}' to exist.");
            AssertCorrectPointBoundaryLayer(startPointsLayer, featureProviderContainer.BoundaryStartPointMapFeatureProvider, WaveLayerNames.BoundaryStartPointsLayerName, Color.LightGreen);

            ILayer endPointsLayer =
                groupLayer.Layers.FirstOrDefault(x => x.Name == WaveLayerNames.BoundaryEndPointsLayerName);
            Assert.That(endPointsLayer, Is.Not.Null,
                        $"Expected the layer with name '{WaveLayerNames.BoundaryEndPointsLayerName}' to exist.");
            AssertCorrectPointBoundaryLayer(endPointsLayer, featureProviderContainer.BoundaryEndPointMapFeatureProvider, WaveLayerNames.BoundaryEndPointsLayerName, Color.LightCoral);

            ILayer supportPointsLayer =
                groupLayer.Layers.FirstOrDefault(x => x.Name == WaveLayerNames.BoundarySupportPointsLayerName);
            Assert.That(supportPointsLayer, Is.Not.Null,
                        $"Expected the layer with name '{WaveLayerNames.BoundarySupportPointsLayerName}' to exist.");
            AssertCorrectPointBoundaryLayer(supportPointsLayer, featureProviderContainer.SupportPointMapFeatureProvider, WaveLayerNames.BoundarySupportPointsLayerName, DeltaresColor.LightBlue);
        }

        [Test]
        public void CreateBoundaryLayer_FeaturesProviderContainerNull_ThrowsArgumentNullException()
        {
            // Setup
            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            void Call() => instanceCreator.CreateBoundaryLayer(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("featuresProviderContainer"));
        }

        [Test]
        public void CreateBoundaryLineLayer_ValidParameters_ExpectedResults()
        {
            // Setup
            var featureProvider = Substitute.For<IFeatureProvider>();
            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            ILayer layer = instanceCreator.CreateBoundaryLineLayer(featureProvider);

            // Assert
            Assert.That(layer, Is.InstanceOf(typeof(VectorLayer)),
                        $"Expected the layer with name '{WaveLayerNames.BoundaryLineLayerName}' to be of type {typeof(VectorLayer)}");
            AssertCorrectMapTreeBehaviourSubBoundaryLayer(layer);
        }

        [Test]
        public void CreateBoundaryLineLayer_FeatureProviderNull_ThrowsArgumentNullException()
        {
            var instanceCreator = new WaveLayerInstanceCreator();

            void Call() => instanceCreator.CreateBoundaryLineLayer(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("featureProvider"));
        }

        [Test]
        public void CreateBoundaryStartPointsLayer_ValidParameters_ExpectedResults()
        {
            // Setup
            var featureProvider = Substitute.For<IFeatureProvider>();
            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            ILayer layer = instanceCreator.CreateBoundaryStartPointLayer(featureProvider);

            // Assert
            AssertCorrectPointBoundaryLayer(layer, featureProvider, WaveLayerNames.BoundaryStartPointsLayerName, Color.LightGreen);
        }

        [Test]
        public void CreateBoundaryStartPointsLayer_FeatureProviderNull_ThrowsArgumentNullException()
        {
            var instanceCreator = new WaveLayerInstanceCreator();

            void Call() => instanceCreator.CreateBoundaryStartPointLayer(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("featureProvider"));
        }

        [Test]
        public void CreateBoundaryEndPointsLayer_ValidParameters_ExpectedResults()
        {
            // Setup
            var featureProvider = Substitute.For<IFeatureProvider>();
            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            ILayer layer = instanceCreator.CreateBoundaryEndPointLayer(featureProvider);

            // Assert
            AssertCorrectPointBoundaryLayer(layer, featureProvider, WaveLayerNames.BoundaryEndPointsLayerName, Color.LightCoral);
        }

        [Test]
        public void CreateBoundaryEndPointsLayer_FeatureProviderNull_ThrowsArgumentNullException()
        {
            var instanceCreator = new WaveLayerInstanceCreator();

            void Call() => instanceCreator.CreateBoundaryEndPointLayer(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("featureProvider"));
        }

        [Test]
        public void CreateSupportPointsLayer_ValidParameters_ExpectedResults()
        {
            // Setup
            var featureProvider = Substitute.For<IFeatureProvider>();
            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            ILayer supportPointsLayer = instanceCreator.CreateSupportPointsLayer(featureProvider);

            // Assert
            AssertCorrectPointBoundaryLayer(supportPointsLayer, featureProvider, WaveLayerNames.BoundarySupportPointsLayerName, DeltaresColor.LightBlue);
        }

        [Test]
        public void CreateSupportPointsLayer_FeatureProviderNull_ThrowsArgumentNullException()
        {
            var instanceCreator = new WaveLayerInstanceCreator();

            void Call() => instanceCreator.CreateSupportPointsLayer(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("featureProvider"));
        }

        [Test]
        public void CreateSelectedSupportPointLayer_ValidParameters_ExpectedResults()
        {
            // Setup
            var featureProvider = Substitute.For<IFeatureProvider>();
            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            ILayer layer = instanceCreator.CreateSelectedSupportPointLayer(featureProvider);

            // Assert
            AssertCorrectPointBoundaryLayer(layer, featureProvider, WaveLayerNames.SelectedSupportPointLayerName, Color.PaleVioletRed);
        }

        [Test]
        public void CreateSelectedSupportPointLayer_FeatureProviderNull_ThrowsArgumentNullException()
        {
            var instanceCreator = new WaveLayerInstanceCreator();

            void Call() => instanceCreator.CreateSelectedSupportPointLayer(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("featureProvider"));
        }

        [Test]
        public void CreateActiveSupportPointsLayer_ValidParameters_ExpectedResults()
        {
            // Setup
            var featureProvider = Substitute.For<IFeatureProvider>();
            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            ILayer layer = instanceCreator.CreateActiveSupportPointsLayer(featureProvider);

            // Assert
            AssertCorrectPointBoundaryLayer(layer, featureProvider, WaveLayerNames.ActiveSupportPointsLayerName, Color.Gold);
        }

        [Test]
        public void CreateActiveSupportPointsLayer_FeatureProviderNull_ThrowsArgumentNullException()
        {
            var instanceCreator = new WaveLayerInstanceCreator();

            void Call() => instanceCreator.CreateActiveSupportPointsLayer(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("featureProvider"));
        }

        [Test]
        public void CreateInactiveSupportPointsLayer_ValidParameters_ExpectedResults()
        {
            // Setup
            var featureProvider = Substitute.For<IFeatureProvider>();
            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            ILayer layer = instanceCreator.CreateInactiveSupportPointsLayer(featureProvider);

            // Assert
            AssertCorrectPointBoundaryLayer(layer, featureProvider, WaveLayerNames.InactiveSupportPointsLayerName, Color.LightGray);
        }

        [Test]
        public void CreateInactiveSupportPointsLayer_FeatureProviderNull_ThrowsArgumentNullException()
        {
            var instanceCreator = new WaveLayerInstanceCreator();

            void Call() => instanceCreator.CreateInactiveSupportPointsLayer(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("featureProvider"));
        }

        [Test]
        public void CreateWaveOutputDataLayer_OutputDataNull_ThrowsArgumentNullException()
        {
            var instanceCreator = new WaveLayerInstanceCreator();

            void Call() => instanceCreator.CreateWaveOutputDataLayer(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("outputData"));
        }

        [Test]
        public void CreateWaveOutputDataLayer_ValidParameters_ExpectedResults()
        {
            // Setup
            var outputData = Substitute.For<IWaveOutputData>();
            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            ILayer layer = instanceCreator.CreateWaveOutputDataLayer(outputData);

            // Assert
            Assert.That(layer, Is.InstanceOf<GroupLayer>());
            Assert.That(layer.Name, Is.EqualTo(WaveLayerNames.WaveOutputDataLayerName));
            Assert.That(((GroupLayer) layer).LayersReadOnly, Is.True);
        }

        [Test]
        public void CreateWaveOutputGroupLayer_LayerNameNull_ThrowsArgumentNullException()
        {
            var instanceCreator = new WaveLayerInstanceCreator();

            void Call() => instanceCreator.CreateWaveOutputGroupLayer(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("layerName"));
        }

        [Test]
        public void CreateWaveOutputGroupLayer_LayerNameNotNull_ExpectedResults()
        {
            // Setup
            const string outputName = "Map Files";
            var instanceCreator = new WaveLayerInstanceCreator();

            // Call
            ILayer layer = instanceCreator.CreateWaveOutputGroupLayer(outputName);

            // Assert
            Assert.That(layer, Is.InstanceOf<GroupLayer>());
            Assert.That(layer.Name, Is.EqualTo(outputName));
            Assert.That(((GroupLayer) layer).LayersReadOnly, Is.True);
        }


        private static void AssertCorrectPointBoundaryLayer(ILayer layer,
                                                            IFeatureProvider featureProvider,
                                                            string expectedLayerName,
                                                            Color expectedColor)
        {
            Assert.That(layer.Name, Is.EqualTo(expectedLayerName));
            Assert.That(layer, Is.InstanceOf(typeof(VectorLayer)),
                        $"Expected the layer with name '{expectedLayerName}' to be of type {typeof(VectorLayer)}");
            AssertCorrectMapTreeBehaviourSubBoundaryLayer(layer);

            Assert.That(layer.DataSource,
                        Is.SameAs(featureProvider));
            Assert.That(layer.Selectable, Is.False);

            var vectorLayer = (VectorLayer) layer;
            Assert.That(vectorLayer.Style.GeometryType, Is.EqualTo(typeof(IPoint)));

            var solidBrush = vectorLayer.Style.Fill as SolidBrush;
            Assert.That(solidBrush, Is.Not.Null);
            Assert.That(solidBrush.Color.Equals(expectedColor));
        }

        private static void AssertCorrectMapTreeBehaviourSubBoundaryLayer(ILayer layer)
        {
            Assert.That(layer.ReadOnly, Is.True);
            Assert.That(layer.ShowInTreeView, Is.False);
            Assert.That(layer.ShowInLegend, Is.False);
            Assert.That(layer.NameIsReadOnly, Is.True);
            Assert.That(layer.FeatureEditor, Is.Not.Null);
        }
    }
}