using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Layers;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;
using SharpMap.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Layers
{
    [TestFixture]
    public class WaveLayerFactoryTest
    {
        [Test]
        public void CreateModelGroupLayer_ValidModel_ReturnsCorrectResults()
        {
            // Setup
            var waveModel = new WaveModel();

            // Call
            ILayer layer = WaveLayerFactory.CreateModelGroupLayer(waveModel);

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
            // Call
            void Call() => WaveLayerFactory.CreateModelGroupLayer(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("waveModel"));
        }

        [Test]
        public void CreateWaveDomainDataLayer_ValidDomain_ReturnsCorrectResults()
        {
            // Setup
            var expectedDomainName = "DomainName";
            var domain = new WaveDomainData(expectedDomainName);

            // Call
            ILayer layer = WaveLayerFactory.CreateWaveDomainDataLayer(domain);

            // Assert
            Assert.That(layer, Is.InstanceOf<GroupLayer>(),
                        $"Expected the result to be an instance of {nameof(GroupLayer)}");
            Assert.That(layer.Name, Is.EqualTo($"Domain ({expectedDomainName})"),
                        "Expected the layer to have a different name.");
        }

        [Test]
        public void CreateWaveDomainDataLayer_DomainNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => WaveLayerFactory.CreateWaveDomainDataLayer(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("domain"));
        }

        [Test]
        public void CreateObstacleLayer_ValidWaveModel_ReturnsCorrectResults()
        {
            // Setup
            var model = new WaveModel();

            // Call
            ILayer layer = WaveLayerFactory.CreateObstacleLayer(model);

            // Assert
            Assert.That(layer, Is.InstanceOf<VectorLayer>(),
                        $"Expected the result to be an instance of {nameof(VectorLayer)}");
            Assert.That(layer.Name, Is.EqualTo("Obstacles"),
                        "Expected the layer to have a different name.");
        }

        [Test]
        public void CreateObstacleLayer_WaveModelNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => WaveLayerFactory.CreateObstacleLayer(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("waveModel"));
        }

        [Test]
        public void CreateObservationPointsLayer_ValidWaveModel_ReturnsCorrectResults()
        {
            // Setup
            var model = new WaveModel();

            // Call
            ILayer layer = WaveLayerFactory.CreateObservationPointsLayer(model);

            // Assert
            Assert.That(layer, Is.InstanceOf<VectorLayer>(),
                        $"Expected the result to be an instance of {nameof(VectorLayer)}");
            Assert.That(layer.Name, Is.EqualTo("Observation Points"),
                        "Expected the layer to have a different name.");
        }

        [Test]
        public void CreateObservationPointsLayer_WaveModelNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => WaveLayerFactory.CreateObservationPointsLayer(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("waveModel"));
        }

        [Test]
        public void CreateObstacleDataLayer_ValidArguments_ReturnsCorrectResults()
        {
            // Setup
            var obstacleData = new EventedList<WaveObstacle>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();

            // Call
            ILayer layer = WaveLayerFactory.CreateObstacleDataLayer(obstacleData, 
                                                                    coordinateSystem);

            // Assert
            Assert.That(layer, Is.InstanceOf<VectorLayer>(),
                        $"Expected the result to be an instance of {nameof(VectorLayer)}");
            Assert.That(layer.Name, Is.EqualTo("Obstacle Data"),
                        "Expected the layer to have a different name.");
        }

        [Test]
        public void CreateObstacleDataLayer_ObstacleData_ThrowsArgumentNullException()
        {
            // Setup
            var coordinateSystem = Substitute.For<ICoordinateSystem>();

            // Call
            void Call() => WaveLayerFactory.CreateObstacleDataLayer(null, coordinateSystem);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("obstacleData"));
        }

        [Test]
        public void CreateObservationCrossSectionsLayer_ValidWaveModel_ReturnsCorrectResults()
        {
            // Setup
            var model = new WaveModel();

            // Call
            ILayer layer = WaveLayerFactory.CreateObservationCrossSectionLayer(model);

            // Assert
            Assert.That(layer, Is.InstanceOf<VectorLayer>(),
                        $"Expected the result to be an instance of {nameof(VectorLayer)}");
            Assert.That(layer.Name, Is.EqualTo("Observation Cross-Sections"),
                        "Expected the layer to have a different name.");
        }

        [Test]
        public void CreateObservationCrosSectionsLayer_WaveModelNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => WaveLayerFactory.CreateObservationCrossSectionLayer(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("waveModel"));
        }

        [Test]
        public void CreateSnappedFeaturesLayer_ValidSnappedFeatures_ReturnsCorrectResults()
        {
            // Setup
            var model = new WaveModel();
            var waveSnappedFeatures = new WaveSnappedFeaturesGroupLayerData(model);

            // Call
            ILayer layer = WaveLayerFactory.CreateSnappedFeaturesLayer(waveSnappedFeatures);

            // Assert
            Assert.That(layer, Is.InstanceOf<GroupLayer>(),
                        $"Expected the result to be an instance of {nameof(GroupLayer)}");
            Assert.That(layer.Name, Is.EqualTo("Estimated Grid-snapped features"),
                        "Expected the layer to have a different name.");

            var groupLayer = (GroupLayer) layer;
            Assert.That(groupLayer.Layers.Count, Is.EqualTo(waveSnappedFeatures.ChildData.Count()),
                        "Expected a different number of layers:");
        }

        [Test]
        public void CreateSnappedFeaturesLayer_SnappedFeaturesNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => WaveLayerFactory.CreateSnappedFeaturesLayer(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("snappedFeatures"));
        }

        [Test]
        [TestCase(true, "domainName", "domainName")]
        [TestCase(false, "domainName", "Output (domainName)")]
        public void CreateOutputLayer_ValidDomainName_ReturnsCorrectResults(bool overrideLayerName, 
                                                                            string domainName, 
                                                                            string expectedName)
        {
            // Setup
            var model = new WaveModel();
            var waveSnappedFeatures = new WaveSnappedFeaturesGroupLayerData(model);

            // Call
            ILayer layer = WaveLayerFactory.CreateSnappedFeaturesLayer(waveSnappedFeatures);

            // Assert
            Assert.That(layer, Is.InstanceOf<GroupLayer>(),
                        $"Expected the result to be an instance of {nameof(GroupLayer)}");
            Assert.That(layer.Name, Is.EqualTo("Estimated Grid-snapped features"),
                        "Expected the layer to have a different name.");

            var groupLayer = (GroupLayer) layer;
            Assert.That(groupLayer.Layers.Count, Is.EqualTo(waveSnappedFeatures.ChildData.Count()),
                        "Expected a different number of layers:");
        }

        [Test]
        public void CreateOutputLayer_DomainNameNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => WaveLayerFactory.CreateOutputLayer(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("domainName"));
        }

        [Test]
        public void CreateGridLayer_ValidCurvilinearGrid_ReturnsCorrectResults()
        {
            // Setup
            const string gridName = "gridName";

            IList<double> xCoordinates = new List<double> { 0.0 };
            IList<double> yCoordinates = new List<double> { 0.0 };

            var coordinateSystem = Substitute.For<ICoordinateSystem>();
            var grid = new CurvilinearGrid(1, 1,
                                           xCoordinates, yCoordinates,
                                           string.Empty)
            {
                Name = gridName,
            };

            // Call
            ILayer layer = WaveLayerFactory.CreateGridLayer(grid, coordinateSystem);

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
            IList<double> xCoordinates = new List<double> { 0.0 };
            IList<double> yCoordinates = new List<double> { 0.0 };

            var grid = new DiscreteGridPointCoverage(1, 1, xCoordinates, yCoordinates) {Name = gridName};

            var coordinateSystem = Substitute.For<ICoordinateSystem>();

            // Call
            ILayer layer = WaveLayerFactory.CreateGridLayer(grid, coordinateSystem);

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

            // Call
            void Call() => WaveLayerFactory.CreateGridLayer(null, coordinateSystem);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("discreteGrid"));
        }

        [Test]
        public void CreateBoundaryLayer_ValidParameters_ReturnsCorrectResults()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();

            var featureProviderContainer = new BoundaryMapFeaturesContainer(boundaryContainer,
                                                                            coordinateSystem);
            var model = Substitute.For<IWaveModel>();

            // Call
            ILayer layer = WaveLayerFactory.CreateBoundaryLayer(featureProviderContainer, 
                                                                model);

            Assert.That(layer, Is.InstanceOf<GroupLayer>(),
                        $"Expected the result to be an instance of {nameof(GroupLayer)}");
            Assert.That(layer.Name, Is.EqualTo(WaveLayerNames.SpatiallyVaryingBoundaryLayerName),
                        "Expected the layer to have a different name.");

            var groupLayer = (GroupLayer) layer;
            Assert.That(groupLayer.Layers.Count, Is.EqualTo(3),
                        "Expected a different number of layers:");

            ILayer lineLayer = groupLayer.Layers.FirstOrDefault(x => x.Name == WaveLayerNames.BoundaryLineLayerName);
            Assert.That(lineLayer, Is.Not.Null,
                        $"Expected the layer with name '{WaveLayerNames.BoundaryLineLayerName}' to exist.");
            Assert.That(lineLayer, Is.InstanceOf(typeof(VectorLayer)),
                        $"Expected the layer with name '{WaveLayerNames.BoundaryLineLayerName}' to be of type {typeof(VectorLayer)}");

            ILayer endPointsLayer =
                groupLayer.Layers.FirstOrDefault(x => x.Name == WaveLayerNames.BoundaryEndPointsLayerName);
            Assert.That(endPointsLayer, Is.Not.Null,
                        $"Expected the layer with name '{WaveLayerNames.BoundaryEndPointsLayerName}' to exist.");
            Assert.That(endPointsLayer, Is.InstanceOf(typeof(VectorLayer)),
                        $"Expected the layer with name '{WaveLayerNames.BoundaryEndPointsLayerName}' to be of type {typeof(VectorLayer)}");

            ILayer supportPointsLayer =
                groupLayer.Layers.FirstOrDefault(x => x.Name == WaveLayerNames.BoundarySupportPointsLayerName);
            Assert.That(supportPointsLayer, Is.Not.Null,
                        $"Expected the layer with name '{WaveLayerNames.BoundarySupportPointsLayerName}' to exist.");
            AssertCorrectSupportPointsLayer(supportPointsLayer, featureProviderContainer);
        }

        [Test]
        public void CreateBoundaryLayer_FeaturesProviderContainerNull_ThrowsArgumentNullException()
        {
            // Setup
            var model = Substitute.For<IWaveModel>();

            // Call
            void Call() => WaveLayerFactory.CreateBoundaryLayer(null, model);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("featuresProviderContainer"));
        }

        [Test]
        public void CreateBoundaryLayer_ModelNull_ThrowsArgumentNullException()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            var coordinateSystem = Substitute.For<ICoordinateSystem>();

            var featureProviderContainer = new BoundaryMapFeaturesContainer(boundaryContainer,
                                                                            coordinateSystem);

            // Call
            void Call() => WaveLayerFactory.CreateBoundaryLayer(featureProviderContainer, null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("model"));
        }

        private static void AssertCorrectSupportPointsLayer(ILayer supportPointsLayer,
                                                            BoundaryMapFeaturesContainer featureProviderContainer)
        {
            Assert.That(supportPointsLayer, Is.InstanceOf(typeof(VectorLayer)),
                        $"Expected the layer with name '{WaveLayerNames.BoundaryEndPointsLayerName}' to be of type {typeof(VectorLayer)}");

            Assert.That(supportPointsLayer.DataSource,
                        Is.EqualTo(featureProviderContainer.SupportPointMapFeatureProvider));
            Assert.That(supportPointsLayer.ReadOnly, Is.True);
            Assert.That(supportPointsLayer.Selectable, Is.False);
            Assert.That(supportPointsLayer.NameIsReadOnly, Is.True);
            Assert.That(supportPointsLayer.FeatureEditor, Is.Not.Null);

            var vectorLayer = supportPointsLayer as VectorLayer;
            Assert.That(vectorLayer, Is.Not.Null);
            Assert.That(vectorLayer.Style.GeometryType, Is.EqualTo(typeof(IPoint)));

            var solidBrush = vectorLayer.Style.Fill as SolidBrush;
            Assert.That(solidBrush, Is.Not.Null);
            Assert.That(solidBrush.Color.Equals(DeltaresColor.LightBlue));
        }
    }
}