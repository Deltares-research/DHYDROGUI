using System;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Snapping;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;
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
            var mocks = new MockRepository();

            var hydroLinkingTarget1 = mocks.StrictMock<IHydroObject>();
            var hydroLinkingTarget1Geometry = new Point(0, 0);
            hydroLinkingTarget1.Expect(hlt1 => hlt1.Geometry).Return(hydroLinkingTarget1Geometry).Repeat.Twice();

            var hydroLinkingTarget1Layer = mocks.StrictMock<ILayer>();

            var hydroLinkingSource1 = mocks.StrictMock<IHydroObject>();
            hydroLinkingSource1.Expect(hsl1 => hsl1.Links).Return(new EventedList<HydroLink>()); // No hydro links
            hydroLinkingSource1.Expect(hsl1 => hsl1.CanLinkTo(hydroLinkingTarget1)).Return(true);

            hydroLinkingTarget1Layer.Expect(l => l.CoordinateTransformation).Return(null).Repeat.Any();

            var hydroLinkingSource2 = mocks.StrictMock<IHydroObject>();
            hydroLinkingSource2.Expect(hsl2 => hsl2.Links).Return(new EventedList<HydroLink>
            {
                new HydroLink()
                {
                    Source = hydroLinkingSource2,
                    Target = hydroLinkingTarget1
                }
            }); // Has hydro links

            mocks.ReplayAll();

            var candidates = new[]
            {
                new Tuple<IFeature, ILayer>(hydroLinkingTarget1, hydroLinkingTarget1Layer)
            };
            var hydrolinkSnapTool = new HydroLinkSnapRule();
            SnapResult snapResult = hydrolinkSnapTool.Execute(hydroLinkingSource1, candidates, null, null, null, null, -1);
            var expectedSnapResult = new SnapResult(hydroLinkingTarget1Geometry.Coordinate, hydroLinkingTarget1,
                                                    hydroLinkingTarget1Layer, hydroLinkingTarget1Geometry, 0, 0);
            AssertSnapResult(expectedSnapResult, snapResult);

            snapResult = hydrolinkSnapTool.Execute(hydroLinkingSource2, candidates, null, null, null, null, -1);
            AssertSnapResult(null, snapResult);

            mocks.VerifyAll();
        }

        /// <summary>
        /// Assert call to check two <see cref="SnapResult"/>s.
        /// </summary>
        /// <remarks>TODO: Remove when <see cref="SnapResult"/> implements Equals.</remarks>
        private static void AssertSnapResult(SnapResult expected, SnapResult actual)
        {
            if (expected == null && actual == null)
            {
                return; // both null: ok
            }

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