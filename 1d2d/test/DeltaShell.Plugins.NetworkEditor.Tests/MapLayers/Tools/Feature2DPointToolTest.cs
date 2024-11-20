using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;
using SharpMap;
using SharpMap.Data.Providers;
using SharpMap.Extensions.CoordinateSystems;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.Tools
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class Feature2DPointToolTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void AddFeature()
        {
            var area = new HydroArea();
            var map = new Map();
            var layer = MapLayerProviderHelper.CreateLayersRecursive(area, null,
                                                                     new[] {new NetworkEditorMapLayerProvider()});
            layer.DataSource = new Feature2DCollection().Init(area.ObservationPoints, "ObservationPoint", "MyModelName",
                                                              area.CoordinateSystem);
            map.Layers.Add(layer);

            var mapView = new MapView {Map = map};
            var pointTool = new Feature2DPointTool("", "", null);
            pointTool.LayerFilter = l => l.Name == HydroArea.ObservationPointsPluralName;
            mapView.MapControl.Tools.Add(pointTool);

            Action<Form> formAction = f =>
                {
                    var args = new MouseEventArgs(MouseButtons.Left, 0, 0, 0, 0);
                    var coord = new Coordinate(10, 10);

                    pointTool.OnMouseDown(coord, args);
                    pointTool.OnMouseMove(coord, args);
                    pointTool.OnMouseUp(coord, args);

                    Assert.AreEqual(1, layer.DataSource.Features.Count);
                    Assert.AreEqual(10, layer.DataSource.Features.OfType<Feature2DPoint>().First().Geometry.Coordinate.X);

                    Assert.AreEqual(1, area.ObservationPoints.Count);
                    Assert.AreEqual(10, area.ObservationPoints.First().Geometry.Coordinate.X);
                };
            WindowsFormsTestHelper.ShowModal(mapView, formAction);
        }

        
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void AddFeatureWithMapCoordinateSystemActive()
        {
            var oldFactory = Map.CoordinateSystemFactory;
            Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();

            try
            {
                
                var area = new HydroArea();
                var map = new Map();
                var layer = MapLayerProviderHelper.CreateLayersRecursive(area, null,
                                                                     new[] {new NetworkEditorMapLayerProvider()});
                layer.DataSource = new Feature2DCollection().Init(area.ObservationPoints, "ObservationPoint", "MyModelName",
                                                              area.CoordinateSystem);
                map.Layers.Add(layer);
                map.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(4326); //wgs84
                //area.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(4326); //wgs84

                var mapView = new MapView {Map = map};
                var pointTool = new Feature2DPointTool("", "", null);
                pointTool.LayerFilter = l => l.Name == HydroArea.ObservationPointsPluralName;
                mapView.MapControl.Tools.Add(pointTool);

                Action<Form> formAction = f =>
                    {
                        var args = new MouseEventArgs(MouseButtons.Left, 0, 0, 0, 0);
                        var coord = new Coordinate(10, 10);

                        pointTool.OnMouseDown(coord, args);
                        pointTool.OnMouseMove(coord, args);
                        pointTool.OnMouseUp(coord, args);

                        Assert.AreEqual(1, layer.DataSource.Features.Count);
                        Assert.AreEqual(10, layer.DataSource.Features.OfType<Feature2DPoint>().First().Geometry.Coordinate.X);

                        Assert.AreEqual(1, area.ObservationPoints.Count);
                        Assert.AreEqual(10, area.ObservationPoints.First().Geometry.Coordinate.X);

                        map.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(3857); //web mercator

                        pointTool.OnMouseDown(coord, args);
                        pointTool.OnMouseMove(coord, args);
                        pointTool.OnMouseUp(coord, args);

                        Assert.AreEqual(2, layer.DataSource.Features.Count);
                        Assert.AreEqual(10, ((Feature2DPoint) layer.DataSource.Features[1]).Geometry.Coordinate.X);

                        Assert.AreEqual(2, area.ObservationPoints.Count);
                        Assert.AreEqual(10, area.ObservationPoints[1].Geometry.Coordinate.X);
                    };
                WindowsFormsTestHelper.ShowModal(mapView, formAction);
            }
            finally
            {
                Map.CoordinateSystemFactory = oldFactory;
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void AddFeatureWithAreaCoordinateSystemActive()
        {
            var oldFactory = Map.CoordinateSystemFactory;
            Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();

            try
            {

                var area = new HydroArea();
                var map = new Map();
                var layer = MapLayerProviderHelper.CreateLayersRecursive(area, null,
                                                                     new[] { new NetworkEditorMapLayerProvider() });
                layer.DataSource = new Feature2DCollection().Init(area.ObservationPoints, "ObservationPoint", "MyModelName",
                                                              area.CoordinateSystem);
                map.Layers.Add(layer);
                area.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(4326); //wgs84

                var mapView = new MapView { Map = map };
                var pointTool = new Feature2DPointTool("", "", null);
                pointTool.LayerFilter = l => l.Name == HydroArea.ObservationPointsPluralName;
                mapView.MapControl.Tools.Add(pointTool);

                Action<Form> formAction = f =>
                {
                    var args = new MouseEventArgs(MouseButtons.Left, 0, 0, 0, 0);
                    var coord = new Coordinate(10, 10);

                    pointTool.OnMouseDown(coord, args);
                    pointTool.OnMouseMove(coord, args);
                    pointTool.OnMouseUp(coord, args);

                    Assert.AreEqual(1, layer.DataSource.Features.Count);
                    Assert.AreEqual(10, layer.DataSource.Features.OfType<Feature2DPoint>().First().Geometry.Coordinate.X);

                    Assert.AreEqual(1, area.ObservationPoints.Count);
                    Assert.AreEqual(10, area.ObservationPoints.First().Geometry.Coordinate.X);

                    area.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(3857); //web mercator

                    pointTool.OnMouseDown(coord, args);
                    pointTool.OnMouseMove(coord, args);
                    pointTool.OnMouseUp(coord, args);

                    Assert.AreEqual(2, layer.DataSource.Features.Count);
                    Assert.AreEqual(10, ((Feature2DPoint)layer.DataSource.Features[1]).Geometry.Coordinate.X);

                    Assert.AreEqual(2, area.ObservationPoints.Count);
                    Assert.AreEqual(10, area.ObservationPoints[1].Geometry.Coordinate.X);
                };
                WindowsFormsTestHelper.ShowModal(mapView, formAction);
            }
            finally
            {
                Map.CoordinateSystemFactory = oldFactory;
            }
        }
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void AddFeatureWithDifferentCoordinateSystemsActive()
        {
            var oldFactory = Map.CoordinateSystemFactory;
            Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();

            try
            {

                var area = new HydroArea();
                var map = new Map();
                var layer = MapLayerProviderHelper.CreateLayersRecursive(area, null,
                                                                     new[] { new NetworkEditorMapLayerProvider() });
                layer.DataSource = new Feature2DCollection().Init(area.ObservationPoints, "ObservationPoint", "MyModelName",
                                                              area.CoordinateSystem);
                map.Layers.Add(layer);
                area.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(28992); //Rd new

                var mapView = new MapView { Map = map };
                var pointTool = new Feature2DPointTool("", "", null);
                pointTool.LayerFilter = l => l.Name == HydroArea.ObservationPointsPluralName;
                mapView.MapControl.Tools.Add(pointTool);

                Action<Form> formAction = f =>
                {
                    var args = new MouseEventArgs(MouseButtons.Left, 0, 0, 0, 0);
                    var coord = new Coordinate(10, 10);

                    pointTool.OnMouseDown(coord, args);
                    pointTool.OnMouseMove(coord, args);
                    pointTool.OnMouseUp(coord, args);

                    Assert.AreEqual(1, layer.DataSource.Features.Count);
                    Assert.AreEqual(10, layer.DataSource.Features.OfType<Feature2DPoint>().First().Geometry.Coordinate.X);

                    Assert.AreEqual(1, area.ObservationPoints.Count);
                    Assert.AreEqual(10, area.ObservationPoints.First().Geometry.Coordinate.X);

                    map.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(3857); //web mercator
                    // transform is now set!!

                    pointTool.OnMouseDown(coord, args);
                    pointTool.OnMouseMove(coord, args);
                    pointTool.OnMouseUp(coord, args);

                    Assert.AreEqual(2, layer.DataSource.Features.Count);
                    Assert.AreEqual(-587778.636, ((Feature2DPoint)layer.DataSource.Features[1]).Geometry.Coordinate.X, 0.001);

                    Assert.AreEqual(2, area.ObservationPoints.Count);
                    Assert.AreEqual(-587778.636, area.ObservationPoints[1].Geometry.Coordinate.X,0.001);
                };
                WindowsFormsTestHelper.ShowModal(mapView, formAction);
            }
            finally
            {
                Map.CoordinateSystemFactory = oldFactory;
            }
        }

    }
}