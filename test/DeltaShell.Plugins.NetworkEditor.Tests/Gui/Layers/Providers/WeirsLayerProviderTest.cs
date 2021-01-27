using System;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
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
    public class WeirsLayerProviderTest : FeaturesLayerProviderTest<Structure>
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
            hydroArea.Weirs.Add(new Structure());
            hydroArea.Weirs.Add(new Structure());

            return hydroArea;
        }

        protected override IEventedList<Structure> GetStructureCollection(HydroArea hydroArea)
        {
            return new EventedList<Structure>(hydroArea.Weirs.Cast<Structure>());
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