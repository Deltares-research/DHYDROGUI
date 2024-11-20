using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap;
using SharpMap.Api;
using SharpMap.Layers;
using SharpMap.Rendering.Thematics;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.CustomRenderers
{
    [TestFixture]
    public class StructureRendererTest
    {
        [Test]
        public void GetFeatures_NotInPolygon_InEnvelope_Should_Not_Return_Feature()
        {
            var geometryFeature = new Point(50, 50);
            
            VectorLayer layer = SetUpLayer(geometryFeature);

            var lassoNoCatch = new LinearRing( new[]{
                new Coordinate(0, 0),
                new Coordinate(100, 0),
                new Coordinate(100, 100),
                new Coordinate(75, 100),
                new Coordinate(75, 25),
                new Coordinate(0, 25),
                new Coordinate(0, 0)
            });
            var structureRenderer = new StructureRenderer();
            var featuresCatch = structureRenderer.GetFeatures(lassoNoCatch, layer);
            
            var result = featuresCatch.ToList();
            Assert.AreEqual(0,result.Count());

        }

        [Test]
        public void GetFeatures_InPolygon_Should_Return_Feature()
        {
            var geometryFeature = new Point(50, 50);
            
            VectorLayer layer = SetUpLayer(geometryFeature);

            var lassoWithACatch = new LinearRing( new[]{
                new Coordinate(40, 40),
                new Coordinate(60, 40),
                new Coordinate(60, 60),
                new Coordinate(40, 60),
                new Coordinate(40, 40)
            });
            var structureRenderer = new StructureRenderer();
            var featuresCatch = structureRenderer.GetFeatures(lassoWithACatch, layer);

            var result = featuresCatch.ToList();
            Assert.AreEqual(1,result.Count());
            var feature = result.First();
            Assert.AreEqual(geometryFeature,feature.Geometry); 
        }
        
        private static VectorLayer SetUpLayer(Point geometryFeature)
        {
            var feature = Substitute.For<IPointFeature>();
            feature.Geometry = geometryFeature;
            var layer = Substitute.For<VectorLayer>();
            layer.Theme = new CategorialTheme();
            layer.Map = new Map(new Size(500, 500));
            layer.CoordinateTransformation = null;
            var featureProvider = Substitute.For<IFeatureProvider>();
            layer.DataSource = featureProvider;
            var features = new List<IFeature> { feature };
            featureProvider.Features.Returns(features);
            return layer;
        }
    }
}