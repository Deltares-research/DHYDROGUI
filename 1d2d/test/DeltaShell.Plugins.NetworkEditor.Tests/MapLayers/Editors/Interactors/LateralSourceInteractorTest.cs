using System;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.Editors.Interactors
{
    [TestFixture]
    public class LateralSourceInteractorTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Setup
            var layer = Substitute.For<ILayer>();
            var feature = Substitute.For<IFeature>();
            feature.Geometry = new Point(0, 0);
            var vectorStyle = new VectorStyle();
            var editableObject = Substitute.For<IEditableObject>();

            // Call
            var interactor = new LateralSourceInteractor(layer, feature, vectorStyle, editableObject);

            // Assert
            Assert.That(interactor.Layer, Is.SameAs(layer));
            Assert.That(interactor.Network, Is.Null);
            Assert.That(interactor.EditableObject, Is.SameAs(editableObject));
            Assert.That(interactor.VectorStyle, Is.SameAs(vectorStyle));
            Assert.That(interactor.SourceFeature, Is.SameAs(feature));
            Assert.That(interactor.TargetFeature, Is.SameAs(null));
        }

        [TestCase(100, 0, 0, 0)]
        [TestCase(200, 0, 0, 100)]
        [TestCase(300, 0, 1, 100)]
        public void Stop_SetsCorrectPipeAndChainage(double x, double y, int expPipe, double expChainage)
        {
            // Setup
            var layer = Substitute.For<ILayer>();
            var feature = Substitute.For<IBranchFeature>();
            feature.Geometry.Returns(new Point(x, y));
            var vectorStyle = new VectorStyle();
            var network = Substitute.For<IHydroNetwork>();

            IPipe[] pipes =
            {
                CreatePipe(100, 0, 200, 0),
                CreatePipe(200, 0, 300, 0)
            };
            network.Pipes.Returns(pipes);

            var interactor = new LateralSourceInteractor(layer, feature, vectorStyle, network) {Network = network};

            // Call
            interactor.Stop(null);

            // Assert
            Assert.That(feature.Chainage, Is.EqualTo(expChainage));
            Assert.That(feature.Branch, Is.SameAs(pipes[expPipe]));
        }

        private static IPipe CreatePipe(double x1, double y1, double x2, double y2)
        {
            var c1 = new Coordinate(x1, y1);
            var c2 = new Coordinate(x2, y2);

            var geometry = new LineString(new[]
            {
                c1,
                c2
            });

            double length = Math.Sqrt(((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1)));

            return new Pipe
            {
                Length = length,
                Geometry = geometry
            };
        }
    }
}