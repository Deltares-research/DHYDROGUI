using System;
using System.Drawing;
using DelftTools.Hydro.GroupableFeatures;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers.Renderers;
using NUnit.Framework;
using SharpMap;
using SharpMap.Data.Providers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.CustomRenderers
{
    [TestFixture]
    internal class EnclosureRendererTest
    {
        [Test]
        public void EnclosureRendererReturnsFalseWithInvalidGeometryTest()
        {
            var model = new WaterFlowFMModel();
            GroupableFeature2DPolygon enclosureFeature =
                FlowFMTestHelper.CreateFeature2DPolygonFromGeometry("Enclosure01",
                                                                    FlowFMTestHelper.GetInvalidGeometryForEnclosureExample());

            model.Area.Enclosures.Add(enclosureFeature);

            var enclosureRenderer = new EnclosureRenderer();
            Feature2DCollection ds = new HydroAreaFeature2DCollection(model.Area).Init(model.Area.Enclosures, "Enclosure", model.Name, model.Area.CoordinateSystem);
            var layer = new VectorLayer()
            {
                Map = new Map(new Size(20, 20)),
                DataSource = ds
            };
            layer.Render();

            Graphics graphics = Graphics.FromImage(layer.Image);

            Assert.NotNull(enclosureRenderer);
            Assert.IsFalse(enclosureRenderer.Render(enclosureFeature, graphics, layer));
        }

        [Test]
        [TestCase(1, 1)]
        [TestCase(5, 5)]
        [TestCase(10, 10)]
        [TestCase(20, 20)]
        [TestCase(200, 200)]
        public void EnclosureRendererReturnsTrueWithValidGeometryRegardlessOfTheMapSize(int mapWidth, int mapHeight)
        {
            var model = new WaterFlowFMModel();

            GroupableFeature2DPolygon enclosureFeature =
                FlowFMTestHelper.CreateFeature2DPolygonFromGeometry("Enclosure01",
                                                                    FlowFMTestHelper.GetValidGeometryForEnclosureExample());

            model.Area.Enclosures.Add(enclosureFeature);

            var enclosureRenderer = new EnclosureRenderer();
            Feature2DCollection ds = new HydroAreaFeature2DCollection(model.Area).Init(model.Area.Enclosures, "Enclosure", model.Name, model.Area.CoordinateSystem);
            var layer = new VectorLayer()
            {
                Map = new Map(new Size(mapWidth, mapHeight)),
                DataSource = ds
            };
            layer.Render();

            Graphics graphics = Graphics.FromImage(layer.Image);

            Assert.NotNull(enclosureRenderer);
            Assert.IsTrue(enclosureRenderer.Render(enclosureFeature, graphics, layer));
        }

        [Test]
        public void EnclosureRendererThrowsExceptionWhenFeatureIsNotFeature2DPolygonTest()
        {
            var model = new WaterFlowFMModel();
            var enclosureRenderer = new EnclosureRenderer();
            Feature2DCollection ds = new HydroAreaFeature2DCollection(model.Area).Init(model.Area.Enclosures, "Enclosure", model.Name, model.Area.CoordinateSystem);
            var layer = new VectorLayer()
            {
                Map = new Map(new Size(200, 200)),
                DataSource = ds
            };
            layer.Render();

            Graphics graphics = Graphics.FromImage(layer.Image);

            Assert.NotNull(enclosureRenderer);
            Assert.Throws<InvalidOperationException>(() => enclosureRenderer.Render(null, graphics, layer));
        }
    }
}