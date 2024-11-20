using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using GeoAPI.CoordinateSystems;
using NUnit.Framework;
using SharpMap;
using SharpMap.Api.Layers;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui
{
    [TestFixture]
    public class WaveModelGroupLayerTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void TransformModelCoordinatesTest()
        {
            var factory = new OgrCoordinateSystemFactory();
            GeoAPI.Extensions.CoordinateSystems.ICoordinateSystem UTM16CS = factory.CreateFromEPSG(32616); // UTM16N
            GeoAPI.Extensions.CoordinateSystems.ICoordinateSystem webMercatorCS = factory.CreateFromEPSG(3857);

            string mdwPath = TestHelper.GetTestFilePath(@"coordinateBasedBoundary\obw.mdw");
            string localPath = WaveTestHelper.CreateLocalCopy(mdwPath);
            var model = new WaveModel(localPath) {CoordinateSystem = UTM16CS};

            IMapLayerProvider provider = WaveMapLayerProviderFactory.ConstructMapLayerProvider(() => new[]
            {
                model
            });

            var modelLayer = (ModelGroupLayer) MapLayerProviderHelper.CreateLayersRecursive(model, null, new[]
            {
                provider
            });
            modelLayer.Layers.ForEach(l => l.Visible = true);

            Map.CoordinateSystemFactory = factory;
            var map = new Map {Layers = {modelLayer}};
            map.CoordinateSystem = UTM16CS;

            List<ILayer> layers = modelLayer.GetAllLayers(false).ToList();
            ConfirmLayerCoordinateSystems(layers, model.CoordinateSystem);

            // update map cs -> updates transform
            map.CoordinateSystem = webMercatorCS;
            ConfirmLayerTransformations(layers, UTM16CS, webMercatorCS);

            // convert model cs -> updates transform
            modelLayer.UpdateCoordinateSystem(UTM16CS, webMercatorCS);
            model.TransformCoordinates(new OgrCoordinateTransformation((OgrCoordinateSystem) UTM16CS, (OgrCoordinateSystem) webMercatorCS));
            ConfirmLayerTransformations(layers, webMercatorCS, webMercatorCS);
        }

        private static void ConfirmLayerCoordinateSystems(List<ILayer> layers, ICoordinateSystem coordinateSystem)
        {
            layers.ForEach(l =>
            {
                Assert.AreEqual(coordinateSystem, l.DataSource.CoordinateSystem);
                if (l.ShowLabels)
                {
                    Assert.AreEqual(coordinateSystem, l.LabelLayer.DataSource.CoordinateSystem);
                }
            });
        }

        private static void ConfirmLayerTransformations(List<ILayer> layers, ICoordinateSystem source, ICoordinateSystem target)
        {
            layers.ForEach(l =>
            {
                Assert.AreEqual(target, l.CoordinateTransformation.TargetCS);
                Assert.AreEqual(source, l.CoordinateTransformation.SourceCS);
                if (l.ShowLabels)
                {
                    Assert.AreEqual(target, l.LabelLayer.CoordinateTransformation.TargetCS);
                    Assert.AreEqual(source, l.LabelLayer.CoordinateTransformation.SourceCS);
                }
            });
        }
    }
}