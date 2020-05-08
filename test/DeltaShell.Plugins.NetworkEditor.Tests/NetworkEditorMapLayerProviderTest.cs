using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture]
    public class NetworkEditorMapLayerProviderTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowMapLayerWithEnclosure()
        {
            var area = new HydroArea();

            var enclosureFeature = new GroupableFeature2DPolygon
            {
                Name = "Enclosure01",
                Geometry = new MultiLineString(new ILineString[]
                {
                    /* 
                           (2.0, 10.0) O----------O (10.0, 10.0)             
                                      /           |
                                     /            |
                         (0.0, 5.0) O             O (10.0, 5.0)
                                    |              \
                                    |               \
                         (0.0, 0.0) O----------------O (12.0, 0.0)
                    */

                    new LineString(new[]
                    {
                        new Coordinate(0.0, 0.0),
                        new Coordinate(12.0, 0.0)
                    }),
                    new LineString(new[]
                    {
                        new Coordinate(12.0, 0.0),
                        new Coordinate(10.0, 5.0)
                    }),
                    new LineString(new[]
                    {
                        new Coordinate(10.0, 5.0),
                        new Coordinate(10.0, 10.0)
                    }),
                    new LineString(new[]
                    {
                        new Coordinate(10.0, 10.0),
                        new Coordinate(2.0, 10.0)
                    }),
                    new LineString(new[]
                    {
                        new Coordinate(2.0, 10.0),
                        new Coordinate(0.0, 5.0)
                    }),
                    new LineString(new[]
                    {
                        new Coordinate(0.0, 5.0),
                        new Coordinate(0.0, 0.0)
                    }),
                })
            };

            area.Enclosures.Add(enclosureFeature);

            using (var mapView = new MapView())
            {
                ILayer layer = MapLayerProviderHelper.CreateLayersRecursive(area.Enclosures, area, new List<IMapLayerProvider> {NetworkEditorMapLayerProviderCreator.CreateMapLayerProvider()});
                mapView.Map.Layers.Add(layer);

                WindowsFormsTestHelper.ShowModal(mapView, delegate { mapView.Map.ZoomToExtents(); });
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowMapLayerWithPump2D()
        {
            var area = new HydroArea();

            var pump2DFeature = new Pump2D
            {
                Capacity = 2.0,
                StartDelivery = 0.0,
                StopDelivery = 0.0,
                StartSuction = 0.001,
                StopSuction = 0.0,
                DirectionIsPositive = true,
                Name = "pump2D01",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0.0, 0.0),
                    new Coordinate(12.0, 0.0)
                })
            };

            area.Pumps.Add(pump2DFeature);

            using (var mapView = new MapView())
            {
                ILayer layer = MapLayerProviderHelper.CreateLayersRecursive(area.Pumps, area, new List<IMapLayerProvider> {NetworkEditorMapLayerProviderCreator.CreateMapLayerProvider()});
                mapView.Map.Layers.Add(layer);

                WindowsFormsTestHelper.ShowModal(mapView, delegate { mapView.Map.ZoomToExtents(); });
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowMapLayerWithGate2D()
        {
            var area = new HydroArea();

            var gate2DFeature = new Weir2D
            {
                Name = "Gate2D01",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0.0, 0.0),
                    new Coordinate(12.0, 0.0)
                })
            };

            area.Weirs.Add(gate2DFeature);

            using (var mapView = new MapView())
            {
                ILayer layer = MapLayerProviderHelper.CreateLayersRecursive(area.Weirs, area, new List<IMapLayerProvider> {NetworkEditorMapLayerProviderCreator.CreateMapLayerProvider()});
                mapView.Map.Layers.Add(layer);

                WindowsFormsTestHelper.ShowModal(mapView, delegate { mapView.Map.ZoomToExtents(); });
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowMapLayerWithWeir2D()
        {
            var area = new HydroArea();

            var weir2DFeature = new Weir2D
            {
                Name = "weir2D01",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0.0, 0.0),
                    new Coordinate(12.0, 0.0)
                })
            };

            area.Weirs.Add(weir2DFeature);

            using (var mapView = new MapView())
            {
                ILayer layer = MapLayerProviderHelper.CreateLayersRecursive(area.Weirs, area, new List<IMapLayerProvider> {NetworkEditorMapLayerProviderCreator.CreateMapLayerProvider()});
                mapView.Map.Layers.Add(layer);

                WindowsFormsTestHelper.ShowModal(mapView, delegate { mapView.Map.ZoomToExtents(); });
            }
        }
    }
}