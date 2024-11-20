using System;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers.Renderers;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public class EnclosuresLayerProviderTest : GroupableFeature2DPolygonsLayerProviderTest
    {
        protected override void AssertLayerProviderSpecificSettings(VectorLayer vectorLayer)
        {
            Assert.That(vectorLayer.Opacity, Is.EqualTo(0.25f));
            Assert.That(vectorLayer.CustomRenderers.Single(), Is.TypeOf<EnclosureRenderer>());
        }

        protected override ILayerSubProvider GetLayerSubProvider()
        {
            return new EnclosuresLayerProvider();
        }

        protected override HydroArea CreateHydroArea()
        {
            var hydroArea = new HydroArea();
            hydroArea.Enclosures.Add(new GroupableFeature2DPolygon());
            hydroArea.Enclosures.Add(new GroupableFeature2DPolygon());

            return hydroArea;
        }

        protected override IEventedList<GroupableFeature2DPolygon> GetStructureCollection(HydroArea hydroArea)
        {
            return hydroArea.Enclosures;
        }

        protected override Color ExpectedVectorStyleLineColor()
        {
            return Color.FromArgb(255, 138, 43, 226);
        }

        protected override float ExpectedVectorStyleLineWidth()
        {
            return 1f;
        }

        protected override Type ExpectedVectorStyleGeometryType()
        {
            return typeof(IPolygon);
        }
    }
}