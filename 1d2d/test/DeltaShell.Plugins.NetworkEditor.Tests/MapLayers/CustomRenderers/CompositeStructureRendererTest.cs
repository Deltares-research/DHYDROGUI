using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Media;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.MapLayers.CustomRenderers;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap;
using SharpMap.Api;
using SharpMap.Layers;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.CustomRenderers
{
    [TestFixture]
    public class CompositeStructureRendererTest
    {
        [Test]
        public void GetFeatures_NotInPolygon_InEnvelope_Should_Not_Return_Feature()
        {
            var geometryFeature = new Point(0, 200);
            
            VectorLayer layer = SetUpLayer(geometryFeature);

            var lassoNoCatch = new LinearRing( new[]{
                new Coordinate(0, 0),
                new Coordinate(200, 0),
                new Coordinate(200, 200),
                new Coordinate(150, 200),
                new Coordinate(150, 25),
                new Coordinate(0, 25),
                new Coordinate(0, 0)
            });
            var compositeStructureRenderer = new CompositeStructureRenderer(new StructureRenderer());
            var featuresCatch = compositeStructureRenderer.GetFeatures(lassoNoCatch, layer);
            
            var result = featuresCatch.ToList();
            Assert.AreEqual(0,result.Count());

        }

        [Test]
        public void GetFeatures_InPolygon_Should_Return_Feature()
        {
            var geometryFeature = new Point(50, 50);
            
            VectorLayer layer = SetUpLayer(geometryFeature);

            var lassoWithACatch = new LinearRing( new[]{
                new Coordinate(20, 20),
                new Coordinate(60, 20),
                new Coordinate(60, 60),
                new Coordinate(20, 60),
                new Coordinate(20, 20)
            });
            var compositeStructureRenderer = new CompositeStructureRenderer(new StructureRenderer());
            var featuresCatch = compositeStructureRenderer.GetFeatures(lassoWithACatch, layer);

            var result = featuresCatch.ToList();
            Assert.AreEqual(1,result.Count());
            var feature = result.First();
            Assert.AreEqual(geometryFeature,feature.Geometry); 
        }
        
        private static VectorLayer SetUpLayer(Point geometryFeature)
        {
            var composite = Substitute.For<ICompositeNetworkPointFeature>();
            var feature = Substitute.For<ICompositeBranchStructure>();
            feature.Geometry = geometryFeature;
            composite.GetPointFeatures().Returns(new List<IFeature> { feature });
            var layer = Substitute.For<VectorLayer>();
            layer.Theme = new CategorialTheme();
            layer.Map = new Map(new Size(500, 500));
            layer.CoordinateTransformation = null;
            layer.Style = new VectorStyle(new SolidBrush(Color.Black), new Pen(Color.Black), true, new Pen(Color.Black), 40.0f, typeof(Point), ShapeType.Ellipse, 20);
            layer.Style.Symbol = new Bitmap(40, 40);
            var featureProvider = Substitute.For<IFeatureProvider>();
            layer.DataSource = featureProvider;
            var features = new List<IFeature> { feature };
            featureProvider.Features.Returns(features);
            return layer;
        }
    }
}