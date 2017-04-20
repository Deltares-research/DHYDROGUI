using System.Diagnostics;
using DeltaShell.Plugins.DeveloperTools.VisualStudioExtensions.DebuggerVisualizers;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Geometries;
using Microsoft.VisualStudio.DebuggerVisualizers;
using NetTopologySuite.Geometries;
using SharpMap;
using SharpMap.Data.Providers;
using SharpMap.Layers;

// TODO: how to pass only IGeometry?
[assembly: DebuggerVisualizer(typeof (GeometryVisualizer), typeof (VisualizerObjectSource), Target = typeof (Point), Description = "OGC Geometry Visualizer")]
[assembly: DebuggerVisualizer(typeof(GeometryVisualizer), typeof(VisualizerObjectSource), Target = typeof(Polygon), Description = "OGC Geometry Visualizer")]
[assembly: DebuggerVisualizer(typeof(GeometryVisualizer), typeof(VisualizerObjectSource), Target = typeof(LineString), Description = "OGC Geometry Visualizer")]
[assembly: DebuggerVisualizer(typeof(GeometryVisualizer), typeof(VisualizerObjectSource), Target = typeof(LinearRing), Description = "OGC Geometry Visualizer")]
[assembly: DebuggerVisualizer(typeof(GeometryVisualizer), typeof(VisualizerObjectSource), Target = typeof(MultiPoint), Description = "OGC Geometry Visualizer")]
[assembly: DebuggerVisualizer(typeof(GeometryVisualizer), typeof(VisualizerObjectSource), Target = typeof(MultiLineString), Description = "OGC Geometry Visualizer")]
[assembly: DebuggerVisualizer(typeof(GeometryVisualizer), typeof(VisualizerObjectSource), Target = typeof(MultiPolygon), Description = "OGC Geometry Visualizer")]
[assembly: DebuggerVisualizer(typeof(GeometryVisualizer), typeof(VisualizerObjectSource), Target = typeof(GeometryCollection), Description = "OGC Geometry Visualizer")]

namespace DeltaShell.Plugins.DeveloperTools.VisualStudioExtensions.DebuggerVisualizers
{
    public class GeometryVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            var o = objectProvider.GetObject();

            if (!(o is IGeometry))
            {
                return;
            }

            var geometry = (IGeometry) objectProvider.GetObject();

            Show(geometry, windowService);
        }

        public void Show(IGeometry geometry, IDialogVisualizerService windowService)
        {
            var geometryCollection = new GeometryCollection(new[] { geometry });
            var dataTableFeatureProvider = new DataTableFeatureProvider(geometryCollection);
            var geometryLayer = new VectorLayer { DataSource = dataTableFeatureProvider };
            var map = new Map { Layers = { geometryLayer } };
            var mapView = new MapView { Width = 300, Height = 300, Map = map };

            windowService.ShowDialog(mapView);
        }
    }
}
