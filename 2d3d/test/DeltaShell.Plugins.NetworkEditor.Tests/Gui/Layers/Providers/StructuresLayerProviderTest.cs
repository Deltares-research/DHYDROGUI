using System;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
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
    public class StructuresLayerProviderTest : FeaturesLayerProviderTest<Structure>
    {
        protected override void AssertLayerProviderSpecificSettings(VectorLayer vectorLayer) =>
            Assert.That(vectorLayer.CustomRenderers.Single(), Is.TypeOf<ArrowLineStringAdornerRenderer>());

        protected override ILayerSubProvider GetLayerSubProvider() =>
            new StructuresLayerProvider();

        protected override HydroArea CreateHydroArea()
        {
            var hydroArea = new HydroArea();
            hydroArea.Structures.Add(new Structure());
            hydroArea.Structures.Add(new Structure());

            return hydroArea;
        }

        protected override IEventedList<Structure> GetStructureCollection(HydroArea hydroArea) =>
            new EventedList<Structure>(hydroArea.Structures.Cast<Structure>());

        protected override Color ExpectedVectorStyleLineColor() => Color.LightSteelBlue;

        protected override float ExpectedVectorStyleLineWidth() => 3f;

        protected override Type ExpectedVectorStyleGeometryType() => typeof(ILineString);
    }
}