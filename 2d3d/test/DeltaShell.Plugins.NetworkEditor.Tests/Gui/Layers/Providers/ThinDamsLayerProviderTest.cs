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
    public class ThinDamsLayerProviderTest : FeaturesLayerProviderTest<ThinDam2D>
    {
        protected override ILayerSubProvider GetLayerSubProvider()
        {
            return new ThinDamsLayerProvider();
        }

        protected override HydroArea CreateHydroArea()
        {
            var hydroArea = new HydroArea();
            hydroArea.ThinDams.Add(new ThinDam2D());
            hydroArea.ThinDams.Add(new ThinDam2D());

            return hydroArea;
        }

        protected override IEventedList<ThinDam2D> GetStructureCollection(HydroArea hydroArea)
        {
            return hydroArea.ThinDams;
        }

        protected override Color ExpectedVectorStyleLineColor()
        {
            return Color.Red;
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