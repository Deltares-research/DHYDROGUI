using System;
using System.Drawing;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public class LandBoundariesLayerProviderTest : GroupableFeaturesLayerProviderTest<LandBoundary2D>
    {
        protected override ILayerSubProvider GetLayerSubProvider()
        {
            return new LandBoundariesLayerProvider();
        }

        protected override HydroArea CreateHydroArea()
        {
            var hydroArea = new HydroArea();
            hydroArea.LandBoundaries.Add(new LandBoundary2D());
            hydroArea.LandBoundaries.Add(new LandBoundary2D());

            return hydroArea;
        }

        protected override IEventedList<LandBoundary2D> GetStructureCollection(HydroArea hydroArea)
        {
            return hydroArea.LandBoundaries;
        }

        protected override Color ExpectedVectorStyleColor()
        {
            return Color.Black;
        }

        protected override float ExpectedVectorStyleLineWidth()
        {
            return 1f;
        }

        protected override Type ExpectedVectorStyleGeometryType()
        {
            return typeof(ILineString);
        }
    }
}