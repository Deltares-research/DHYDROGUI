using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;
using SharpMap.Api.Layers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers
{
    [TestFixture]
    public class SamplesMapLayerProviderTest
    {
        [Test]
        public void Create_SamplesNull_ThrowsException()
        {
            // Call
            void Call() => SamplesMapLayerProvider.Create(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Create_ReturnsReadOnlyPointCloudLayer()
        {
            // Setup
            const string name = "randomName";
            var samples = new Samples(name);

            // Call
            ILayer layer = SamplesMapLayerProvider.Create(samples);

            // Assert
            Assert.That(layer, Is.TypeOf<PointCloudLayer>());

            var pointCloudLayer = (PointCloudLayer)layer;
            Assert.That(pointCloudLayer.ReadOnly, Is.True);
            Assert.That(pointCloudLayer.Name, Is.EqualTo(name));
        }
    }
}