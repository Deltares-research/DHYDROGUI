using System.Linq;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui;
using NUnit.Framework;
using SharpMap.Layers;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI
{
    [TestFixture]
    public class RainfallRunoffMapLayerProviderTest
    {
        [Test]
        public void CreatesCustomLayerForCatchments()
        {
            var model = new RainfallRunoffModel();
            model.OutputSettings.GetEngineParameter(QuantityType.GroundwaterLevel, ElementSet.UnpavedElmSet).IsEnabled = true;

            var layerProvider = new RainfallRunoffMapLayerProvider();

            var outputFolder = layerProvider.ChildLayerObjects(model).Skip(1).First();
            var coverage = model.OutputCoverages.Skip(1).First();

            Assert.IsTrue(layerProvider.ChildLayerObjects(outputFolder).Contains(coverage));
            Assert.IsTrue(layerProvider.CanCreateLayerFor(coverage, outputFolder));

            var createdLayer = (FeatureCoverageLayer) layerProvider.CreateLayer(coverage, outputFolder);
            Assert.IsNotNull(createdLayer.Renderer.GeometryForFeatureDelegate); //asssert it got injected
        }
    }
}