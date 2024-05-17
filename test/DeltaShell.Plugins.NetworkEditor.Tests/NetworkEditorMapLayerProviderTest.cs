using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.GroupableFeatures;
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
                    })
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

            var pump2DFeature = new Pump
            {
                Capacity = 2.0,
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

            var gate2DFeature = new Structure
            {
                Name = "Gate2D01",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0.0, 0.0),
                    new Coordinate(12.0, 0.0)
                })
            };

            area.Structures.Add(gate2DFeature);

            using (var mapView = new MapView())
            {
                ILayer layer = MapLayerProviderHelper.CreateLayersRecursive(area.Structures, area, new List<IMapLayerProvider> {NetworkEditorMapLayerProviderCreator.CreateMapLayerProvider()});
                mapView.Map.Layers.Add(layer);

                WindowsFormsTestHelper.ShowModal(mapView, delegate { mapView.Map.ZoomToExtents(); });
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowMapLayerWithWeir2D()
        {
            var area = new HydroArea();

            var weir2DFeature = new Structure
            {
                Name = "weir2D01",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0.0, 0.0),
                    new Coordinate(12.0, 0.0)
                })
            };

            area.Structures.Add(weir2DFeature);

            using (var mapView = new MapView())
            {
                ILayer layer = MapLayerProviderHelper.CreateLayersRecursive(area.Structures, area, new List<IMapLayerProvider> {NetworkEditorMapLayerProviderCreator.CreateMapLayerProvider()});
                mapView.Map.Layers.Add(layer);

                WindowsFormsTestHelper.ShowModal(mapView, delegate { mapView.Map.ZoomToExtents(); });
            }
        }
    }
}