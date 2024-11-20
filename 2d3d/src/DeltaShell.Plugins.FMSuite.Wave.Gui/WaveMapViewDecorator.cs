using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.MapTools;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Properties;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui
{
    public static class WaveMapViewDecorator
    {
        internal const string ObstacleToolName = "Obstacle Tool (Waves)";
        internal const string BoundaryToolName = "Boundary Tool (Waves)";

        internal const string ObservationPointToolName = "Observation Point Tool (Waves)";
        internal const string ObservationCrossSectionToolName = "Observation Cross-Section Tool (Waves)";
        private static readonly Bitmap ObstacleIcon = Resources.wall_brick;
        private static readonly Bitmap BoundaryIcon = Common.Gui.Properties.Resources.boundary;
        private static readonly Bitmap ObservationPointIcon = Common.Gui.Properties.Resources.Observation;
        private static readonly Bitmap ObservationCrossSectionIcon = Common.Gui.Properties.Resources.ObservationCS;

        private static readonly string ModelName = nameof(WaveModel);

        public static void AddMapToolsIfMissing(MapView mapView)
        {
            if (mapView.MapControl.Tools.OfType<Feature2DLineTool>().Any(t => t.Name == ObstacleToolName))
            {
                return; // already has them
            }

            var tools = new List<MapTool>();

            // tools:
            tools.Add(
                new Feature2DLineTool(WaveLayerNames.ObstacleLayerName, ObstacleToolName, ObstacleIcon));
            tools.Add(new Feature2DPointTool(WaveLayerNames.ObservationPointLayerName,
                                             ObservationPointToolName, ObservationPointIcon));
            tools.Add(new Feature2DLineTool(WaveLayerNames.ObservationCrossSectionLayerName,
                                            ObservationCrossSectionToolName, ObservationCrossSectionIcon));

            tools.Cast<ITargetLayerTool>().ForEach(t => t.LayerFilter = GetLayerFilter(t));

            var boundaryTool = new GroupedLayerFeature2DLineTool(WaveLayerNames.BoundaryLayerName,
                                                                 WaveLayerNames.BoundaryLineLayerName,
                                                                 BoundaryToolName,
                                                                 BoundaryIcon);

            bool layerFilterBoundary(ILayer layer) =>
                layer.Name == boundaryTool.LayerName &&
                layer is IGroupLayer groupLayer &&
                groupLayer.Layers.Any(x => x.DataSource is BoundaryLineMapFeatureProvider);

            boundaryTool.LayerFilter = layerFilterBoundary;
            tools.Add(boundaryTool);

            mapView.MapControl.Tools.AddRange(tools);
        }

        private static Func<ILayer, bool> GetLayerFilter(ITargetLayerTool tool)
        {
            return l => l.Name == tool.LayerName &&            //expected layer name must match
                        l.DataSource is Feature2DCollection && //and layer must be a 2D layer
                        ((Feature2DCollection) l.DataSource).ModelName == ModelName;
        }
    }
}