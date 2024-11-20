using System.Drawing;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap;
using SharpMap.Api;
using SharpMap.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.CustomRenderers
{
    [TestFixture]
    class CrossSectionRendererTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        public void GivenDefaultThemedCrossSectionTypeStyleWithCrossSectionZWWhenChangeThemeStyleFromGrayToDeepPinkThenStyleMustBeRenderedCorrectlyInDeepPink()
        {
            var feature = Substitute.For<ICrossSection>();
            feature.Geometry = new LineString(new[]
            {
                new Coordinate(0, 0),
                new Coordinate(0, 100)
            });
            feature.CrossSectionType.Returns(CrossSectionType.ZW);

            var layer = new VectorLayer();
            layer.Map = new Map(new Size(200, 200));
            layer.DataSource = Substitute.For<IFeatureProvider>();
            layer.CoordinateTransformation = null;
            layer.Theme = NetworkLayerThemeFactory.CreateTheme(Enumerable.Repeat(Substitute.For<ICrossSection>(), 1));
            layer.Render();

            var renderer = new CrossSectionRenderer();
            Graphics g = Graphics.FromImage(layer.Image);

            renderer.Render(feature, g, layer);

            if (!SearchPixel(Color.Gray, new Bitmap(layer.Image)))
                Assert.Fail("CrossSection ZW with default colour 'Gray' is not drawn");

            if (layer.Theme.ThemeItems.SingleOrDefault(ti => ti.Label.Equals(CrossSectionType.ZW.GetDescription()))?.Style is VectorStyle style)
            {
                style.Line.Color = Color.DeepPink;
            }

            renderer.Render(feature, g, layer);

            if (!SearchPixel(Color.DeepPink, new Bitmap(layer.Image)))
                Assert.Fail("CrossSection ZW with custom colour 'DeepPink' is not drawn");
        }

        private bool SearchPixel(Color desiredPixelColor, Bitmap bitmap)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color currentPixelColor = bitmap.GetPixel(x, y);


                    if (desiredPixelColor.ToArgb().Equals(currentPixelColor.ToArgb()))
                    {

                        return true;

                    }
                }
            }

            return false;

        }
    }
}
