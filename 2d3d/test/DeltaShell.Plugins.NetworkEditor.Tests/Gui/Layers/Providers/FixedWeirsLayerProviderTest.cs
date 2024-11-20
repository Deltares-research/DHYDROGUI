using System;
using System.Drawing;
using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public class FixedWeirsLayerProviderTest : FeaturesLayerProviderTest<FixedWeir>
    {
        protected override ILayerSubProvider GetLayerSubProvider()
        {
            return new FixedWeirsLayerProvider();
        }

        protected override HydroArea CreateHydroArea()
        {
            var hydroArea = new HydroArea();
            hydroArea.FixedWeirs.Add(new FixedWeir());
            hydroArea.FixedWeirs.Add(new FixedWeir());

            return hydroArea;
        }

        protected override IEventedList<FixedWeir> GetStructureCollection(HydroArea hydroArea)
        {
            return hydroArea.FixedWeirs;
        }

        protected override Color ExpectedVectorStyleLineColor()
        {
            return Color.Purple;
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