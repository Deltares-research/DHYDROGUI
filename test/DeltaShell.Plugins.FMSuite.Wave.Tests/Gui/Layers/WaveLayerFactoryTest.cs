using System;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using GeoAPI.Extensions.CoordinateSystems;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;
using SharpMap.Layers;

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
        public void CreateObstacleDataLayer_CoordinateSystemNull_ThrowsArgumentNullException()
        {
            // Setup
            var obstacleData = new EventedList<WaveObstacle>();

            // Call
            void Call() => WaveLayerFactory.CreateObstacleDataLayer(obstacleData, null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("coordinateSystem"));
        }

    }
}