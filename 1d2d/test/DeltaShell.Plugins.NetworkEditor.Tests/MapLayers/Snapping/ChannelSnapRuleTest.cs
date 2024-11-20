using System;
using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Snapping;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Api.Layers;
using SharpMap.Editors.Snapping;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.Snapping
{
    [TestFixture]
    public class ChannelSnapRuleTest
    {
        [Test]
        public void BranchSnap()
        {
            var channel = new Channel { Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(20, 0) }) };

            var candidates = new[] { new Tuple<IFeature, ILayer>(channel, new VectorLayer()) };

            var branchSnapRule = new BranchSnapRule
            {
                SnapRole = SnapRole.AllTrackers,
                PixelGravity = 40
            };

            var snapResult = branchSnapRule.Execute(null, candidates, channel.Geometry, null, new Coordinate(0, 0), 
                new Envelope(new Coordinate(0, 0)), 0);

            Assert.AreEqual(new Coordinate(0, 0), snapResult.Location);
            Assert.AreEqual(0, snapResult.SnapIndexNext);
            Assert.AreEqual(0, snapResult.SnapIndexPrevious);
            Assert.AreEqual(channel.Geometry, snapResult.NearestTarget);
            Assert.AreEqual(null, snapResult.SnappedFeature);

            snapResult = branchSnapRule.Execute(channel, candidates, channel.Geometry, null, new Coordinate(0, 0), 
                new Envelope(new Coordinate(0, 0)), 0);
            Assert.AreEqual(new Coordinate(0, 0), snapResult.Location);
            Assert.AreEqual(0, snapResult.SnapIndexNext);
            Assert.AreEqual(0, snapResult.SnapIndexPrevious);
            Assert.AreEqual(channel.Geometry, snapResult.NearestTarget);
            Assert.AreEqual(null, snapResult.SnappedFeature);

            /*                                             SourceLayer = HydroNetworkMapLayer.ChannelLayer,
                                             TargetLayer = HydroNetworkMapLayer.NodeLayer,
                                             SnapRole = SnapRole.AllTrackers,
                                             Obligatory = false,
                                             PixelGravity = 40
*/
        }
    }
}
