using System;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap.Layers;
using SharpMap.Rendering;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public class WeirsLayerProviderTest : FeaturesLayerProviderTest<Weir2D>
    {
        protected override void AssertLayerProviderSpecificSettings(VectorLayer vectorLayer)
        {
            Assert.That(vectorLayer.CustomRenderers.Single(), Is.TypeOf<ArrowLineStringAdornerRenderer>());
        }

        protected override ILayerSubProvider GetLayerSubProvider()
        {
            return new WeirsLayerProvider();
        }

        protected override HydroArea CreateHydroArea()
        {
            var hydroArea = new HydroArea();
            hydroArea.Weirs.Add(new Weir2D());
            hydroArea.Weirs.Add(new Weir2D());

            return hydroArea;
        }

        protected override IEventedList<Weir2D> GetStructureCollection(HydroArea hydroArea)
        {
            return hydroArea.Weirs;
        }

        protected override Color ExpectedVectorStyleLineColor()
        {
            return Color.LightSteelBlue;
        }

        protected override float ExpectedVectorStyleLineWidth()
        {
            return 3f;
        }

        protected override Type ExpectedVectorStyleGeometryType()
        {
            return typeof(ILineString);
        }
    }
}