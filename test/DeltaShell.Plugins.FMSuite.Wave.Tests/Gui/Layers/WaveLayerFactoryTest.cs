using System;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using NUnit.Framework;
using SharpMap.Api.Layers;

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
    }
}