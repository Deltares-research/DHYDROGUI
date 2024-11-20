using System;
using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Snapping;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Editors.Snapping;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.Snapping
{
    [TestFixture]
    public class CrossSectionSnapRuleTest
    {
        [Test]
        public void SnapTest()
        {
            var channel = new Channel { Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(20, 0) }) };
            var branches = new[] { channel };
            
            var branchLayer = new VectorLayer("") {DataSource = new FeatureCollection {Features = branches}};

            var branchSnapRule = new CrossSectionSnapRule
            {
                NewFeatureLayer = new VectorLayer(),
                SnapRole = SnapRole.AllTrackers,
                PixelGravity = 40
            };
            var candidates = new[] { new Tuple<IFeature, ILayer>(channel, branchLayer) };
            var snapResult = branchSnapRule.Execute(null, candidates, channel.Geometry, null, new Coordinate(0, 0), new Envelope(new Coordinate(0, 0)), 0);

        }
    }
}
