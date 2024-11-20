using System;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Snapping;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.Editors.Snapping
{
    [TestFixture]
    public class HydroLinkSnapRuleTest
    {
        [Test]
        public void DoNotSnapIfThereIsAlreadyALink()
        {
            var hydroLinkingTarget1 = Substitute.For<IHydroObject>();
            var hydroLinkingTarget1Geometry = new Point(0, 0);
            hydroLinkingTarget1.Geometry.Returns(hydroLinkingTarget1Geometry);
            
            var hydroLinkingTarget1Layer = Substitute.For<ILayer>();

            var hydroLinkingSource1 = Substitute.For<IHydroObject>();
            hydroLinkingSource1.Links = new EventedList<HydroLink>(); // No hydro links
            hydroLinkingSource1.CanLinkTo(hydroLinkingTarget1).Returns(true);

            hydroLinkingTarget1Layer.CoordinateTransformation.Returns((ICoordinateTransformation)null);

            var hydroLinkingSource2 = Substitute.For<IHydroObject>();
            hydroLinkingSource2.Links = new EventedList<HydroLink> { new HydroLink(hydroLinkingSource2, hydroLinkingTarget1) }; // Has hydro links

            var candidates = new[] { new Tuple<IFeature, ILayer>(hydroLinkingTarget1, hydroLinkingTarget1Layer) };
            var hydrolinkSnapTool = new HydroLinkSnapRule();
            var snapResult = hydrolinkSnapTool.Execute(hydroLinkingSource1, candidates, null, null, null, null, -1);
            var expectedSnapResult = new SnapResult(hydroLinkingTarget1Geometry.Coordinate, hydroLinkingTarget1,
                                                    hydroLinkingTarget1Layer, hydroLinkingTarget1Geometry, 0, 0);
            AssertSnapResult(expectedSnapResult, snapResult);

            snapResult = hydrolinkSnapTool.Execute(hydroLinkingSource2, candidates, null, null, null, null, -1);
            AssertSnapResult(null, snapResult);
        }

        /// <summary>
        /// Assert call to check two <see cref="SnapResult"/>s.
        /// </summary>
        /// <remarks>TODO: Remove when <see cref="SnapResult"/> implements Equals.</remarks>
        private static void AssertSnapResult(SnapResult expected, SnapResult actual)
        {
            if (expected == null && actual == null) return; // both null: ok
            if (expected == null || actual == null)
            {
                Assert.Fail("Expected {0}, but was {1}.", expected, actual); // Either null, not ok
            }

            Assert.AreEqual(expected.SnapIndexPrevious, actual.SnapIndexPrevious);
            Assert.AreEqual(expected.SnapIndexNext, actual.SnapIndexNext);
            Assert.AreEqual(expected.NearestTarget, actual.NearestTarget);
            Assert.AreEqual(expected.SnappedFeature, actual.SnappedFeature);
            Assert.AreEqual(expected.NewFeatureLayer, actual.NewFeatureLayer);
            Assert.AreEqual(expected.Location, actual.Location);
            Assert.AreEqual(expected.VisibleSnaps, actual.VisibleSnaps);
        }
    }
}