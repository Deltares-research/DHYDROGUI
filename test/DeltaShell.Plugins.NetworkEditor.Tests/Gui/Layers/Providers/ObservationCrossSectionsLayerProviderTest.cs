using System;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
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
    public class ObservationCrossSectionsLayerProviderTest : Feature2DLayerProviderTest<ObservationCrossSection2D>
    {
        protected override void AssertLayerProviderSpecificSettings(VectorLayer vectorLayer)
        {
            Assert.IsNotNull(vectorLayer.CustomRenderers);
            Assert.That(vectorLayer.CustomRenderers.Single(), Is.TypeOf<ArrowLineStringAdornerRenderer>());
        }

        protected override ILayerSubProvider GetLayerSubProvider()
        {
            return new ObservationCrossSectionsLayerProvider();
        }

        protected override HydroArea CreateHydroArea()
        {
            var hydroArea = new HydroArea();
            hydroArea.ObservationCrossSections.Add(new ObservationCrossSection2D());
            hydroArea.ObservationCrossSections.Add(new ObservationCrossSection2D());

            return hydroArea;

        }

        protected override IEventedList<ObservationCrossSection2D> GetStructureCollection(HydroArea hydroArea)
        {
            return hydroArea.ObservationCrossSections;
        }

        protected override Color ExpectedVectorStyleColor()
        {
            return Color.DeepPink;
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