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
    public class BridgePillarsLayerProviderTest : FeaturesLayerProviderTest<BridgePillar>
    {
        protected override ILayerSubProvider GetLayerSubProvider()
        {
            return new BridgePillarsLayerProvider();
        }

        protected override HydroArea CreateHydroArea()
        {
            var hydroArea = new HydroArea();
            hydroArea.BridgePillars.Add(new BridgePillar());
            hydroArea.BridgePillars.Add(new BridgePillar());

            return hydroArea;
        }

        protected override IEventedList<BridgePillar> GetStructureCollection(HydroArea hydroArea)
        {
            return hydroArea.BridgePillars;
        }

        protected override Color ExpectedVectorStyleLineColor()
        {
            return Color.LightSeaGreen;
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