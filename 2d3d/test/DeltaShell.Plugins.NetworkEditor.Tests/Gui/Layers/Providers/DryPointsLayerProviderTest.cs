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
    public class DryPointsLayerProviderTest : FeaturesLayerProviderTest<GroupablePointFeature>
    {
        protected override ILayerSubProvider GetLayerSubProvider()
        {
            return new DryPointsLayerProvider();
        }

        protected override HydroArea CreateHydroArea()
        {
            var hydroArea = new HydroArea();
            hydroArea.DryPoints.Add(new GroupablePointFeature());
            hydroArea.DryPoints.Add(new GroupablePointFeature());

            return hydroArea;
        }

        protected override IEventedList<GroupablePointFeature> GetStructureCollection(HydroArea hydroArea)
        {
            return hydroArea.DryPoints;
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
            return typeof(IPoint);
        }
    }
}