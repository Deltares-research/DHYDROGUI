using System;
using System.Drawing;
using DelftTools.Hydro;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public class DryAreasLayerProviderTest : GroupableFeature2DPolygonsLayerProviderTest
    {
        protected override ILayerSubProvider GetLayerSubProvider()
        {
            return new DryAreasLayerProvider();
        }

        protected override HydroArea CreateHydroArea()
        {
            var hydroArea = new HydroArea();
            hydroArea.DryAreas.Add(new GroupableFeature2DPolygon());
            hydroArea.DryAreas.Add(new GroupableFeature2DPolygon());

            return hydroArea;
        }

        protected override IEventedList<GroupableFeature2DPolygon> GetStructureCollection(HydroArea hydroArea)
        {
            return hydroArea.DryAreas;
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