using System.Drawing;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.CustomRenderers;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.Layers;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.CustomRenderers
{
    [TestFixture]
    public class GdalRendererTest
    {
        [Test]
        public void BoundaryRendererShouldRenderSymbolForFeatures()
        {
            var layer = new VectorLayer()
            {
                Map = new Map(new Size(200, 200)),
                DataSource = new WaqModelFeatureCollection(new WaterQualityModel())
            };
            layer.Render();
            Graphics graphics = Graphics.FromImage(layer.Image);

            var boundaryRenderer = new BoundaryRenderer();

            var feature = (IFeature) new WaterQualityBoundary()
            {
                Name = "PointFeature",
                Geometry = new Point(0.1, 0.1, double.NaN)
            };

            Assert.IsTrue(boundaryRenderer.Render(feature, graphics, layer),
                          "BoundaryRenderer should render symbol for features with 1 coordinate");

            feature = (IFeature) new WaterQualityBoundary()
            {
                Name = "MultiLineFeature__2Points_Equal",
                Geometry = new MultiLineString(new ILineString[]
                {
                    new LineString(new[]
                    {
                        new Coordinate(0.1, 0.1, double.NaN),
                        new Coordinate(0.1, 0.1, double.NaN)
                    })
                })
            };

            Assert.IsTrue(boundaryRenderer.Render(feature, graphics, layer),
                          "BoundaryRenderer should render symbol for features with 2 coordinates that are equal");

            feature = (IFeature) new WaterQualityBoundary()
            {
                Name = "MultiLineFeature__2Points_NotEqual",
                Geometry = new MultiLineString(new ILineString[]
                {
                    new LineString(new[]
                    {
                        new Coordinate(0.1, 0.1, double.NaN),
                        new Coordinate(0.2, 0.2, double.NaN)
                    })
                })
            };

            Assert.IsFalse(boundaryRenderer.Render(feature, graphics, layer),
                           "BoundaryRenderer should not render symbol for features with 2 coordinates that are not equal");

            feature = (IFeature) new WaterQualityBoundary()
            {
                Name = "MultiLineFeature_MoreThan2Points_FirstAndLastEqual",
                Geometry = new MultiLineString(new ILineString[]
                {
                    new LineString(new[]
                    {
                        new Coordinate(0.1, 0.1, double.NaN),
                        new Coordinate(0.2, 0.1, double.NaN),
                        new Coordinate(0.3, 0.1, double.NaN),
                        new Coordinate(0.3, 0.2, double.NaN),
                        new Coordinate(0.2, 0.2, double.NaN),
                        new Coordinate(0.1, 0.1, double.NaN)
                    })
                })
            };

            Assert.IsFalse(boundaryRenderer.Render(feature, graphics, layer),
                           "BoundaryRenderer should not render symbol for features with more than 2 coordinates");
        }

        [Test]
        public void BoundaryRendererShouldCheckForCoordinateTransformation()
        {
            var layer = MockRepository.GenerateStrictMock<ILayer>();
            var graphics = MockRepository.GenerateStub<Graphics>();
            var map = MockRepository.GenerateStub<IMap>();
            var transformation = MockRepository.GenerateStub<ICoordinateTransformation>();
            var mathTransform = MockRepository.GenerateStub<IMathTransform>();

            // expect the transformation calls to be done
            mathTransform.Expect(m => m.Transform(new[]
            {
                0.1,
                0.1
            })).Return(new[]
            {
                10.0,
                10.0
            });
            transformation.Expect(t => t.MathTransform).Return(mathTransform);
            layer.Expect(l => l.CoordinateTransformation).Return(transformation).Repeat.Twice();

            layer.Expect(l => l.Map).Return(map).Repeat.Any();

            layer.Replay();
            graphics.Replay();
            map.Replay();
            transformation.Replay();
            mathTransform.Replay();

            var boundaryRenderer = new BoundaryRenderer();

            var feature = (IFeature) new WaterQualityBoundary
            {
                Name = "PointFeature",
                Geometry = new Point(0.1, 0.1, double.NaN)
            };

            Assert.IsTrue(boundaryRenderer.Render(feature, graphics, layer), "BoundaryRenderer should render symbol for features with 1 coordinate");

            layer.VerifyAllExpectations();
            transformation.VerifyAllExpectations();
            mathTransform.VerifyAllExpectations();
        }
    }
}