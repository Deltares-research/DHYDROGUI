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
    public class PumpsLayerProviderTest : FeaturesLayerProviderTest<Pump2D>
    {
        protected override void AssertLayerProviderSpecificSettings(VectorLayer vectorLayer)
        {
            var pump = vectorLayer.FeatureEditor.CreateNewFeature(vectorLayer) as Pump2D;
            Assert.IsNotNull(pump);
            Assert.That(pump.CanBeTimedependent, Is.True);

            Assert.That(vectorLayer.CustomRenderers.Single(), Is.TypeOf<ArrowLineStringAdornerRenderer>());
        }

        protected override ILayerSubProvider GetLayerSubProvider()
        {
            return new PumpsLayerProvider();
        }

        protected override HydroArea CreateHydroArea()
        {
            var hydroArea = new HydroArea();
            hydroArea.Pumps.Add(new Pump2D());
            hydroArea.Pumps.Add(new Pump2D());

            return hydroArea;
        }

        protected override IEventedList<Pump2D> GetStructureCollection(HydroArea hydroArea)
        {
            return hydroArea.Pumps;
        }

        protected override Color ExpectedVectorStyleLineColor()
        {
            return Color.Aquamarine;
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