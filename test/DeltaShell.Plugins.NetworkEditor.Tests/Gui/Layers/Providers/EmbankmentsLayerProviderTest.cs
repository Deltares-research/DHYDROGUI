using System;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers.Editors;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers.Renderers;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Layers.Providers
{
    [TestFixture]
    public class EmbankmentsLayerProviderTest : FeaturesLayerProviderTest<Embankment>
    {
        protected override void AssertLayerProviderSpecificSettings(VectorLayer vectorLayer)
        {
            var featureEditor = vectorLayer.FeatureEditor as HydroAreaFeatureEditor;
            Assert.IsNotNull(featureEditor);

            IFeature feature = featureEditor.CreateNewFeature(Substitute.For<ILayer>());
            Assert.That(feature, Is.TypeOf<Embankment>());

            Assert.That(vectorLayer.CustomRenderers.Single(), Is.TypeOf<EmbankmentRenderer>());
        }

        protected override ILayerSubProvider GetLayerSubProvider()
        {
            return new EmbankmentsLayerProvider();
        }

        protected override HydroArea CreateHydroArea()
        {
            var hydroArea = new HydroArea();
            hydroArea.Embankments.Add(new Embankment());
            hydroArea.Embankments.Add(new Embankment());

            return hydroArea;
        }

        protected override IEventedList<Embankment> GetStructureCollection(HydroArea hydroArea)
        {
            return hydroArea.Embankments;
        }

        protected override Color ExpectedVectorStyleLineColor()
        {
            return Color.SandyBrown;
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