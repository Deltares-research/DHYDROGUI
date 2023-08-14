using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui
{
    public static class FlowFMMapViewDecorator
    {
        internal const string BoundaryToolName = "Boundary tool (2D)";
        internal const string SourceToolName = "Source tool (2D)";
        internal const string SourceAndSinkToolName = "Source & sink tool";
        internal const string LateralPointToolName = "Lateral point (2D)";
        internal const string LateralToolName = "Lateral tool";
        internal const string Reverse2DLineToolName = "Reverse line(s) (2D)";
        private static readonly Bitmap BoundaryIcon = Resources.boundary;
        private static readonly Bitmap SourceSinkIcon = Properties.Resources.SourceSink;
        private static readonly Bitmap SourceIcon = Properties.Resources.LateralSourceMap;
        private static readonly Bitmap LateralPointIcon = Properties.Resources.LateralPoint;
        private static readonly Bitmap LateralPolygonIcon = Properties.Resources.LateralPolygon;

        private static readonly string ModelName = nameof(WaterFlowFMModel);

        public static void AddMapToolsIfMissing(MapView mapView)
        {
            if (mapView.MapControl.Tools.OfType<Feature2DLineTool>().Any(t => t.Name == BoundaryToolName))
            {
                return; // already has them
            }

            var tools = new List<MapTool>();

            tools.Add(new Reverse2DLineTool
            {
                Name = Reverse2DLineToolName,
                LayerFilter = layer => (layer.Name == HydroAreaLayerNames.ObservationCrossSectionsPluralName ||
                                        layer.Name == HydroAreaLayerNames.PumpsPluralName ||
                                        layer.Name == HydroAreaLayerNames.StructuresPluralName ||
                                        layer.Name == FlowFMMapLayerProvider.SourcesAndSinksLayerName) &&
                                       layer.DataSource is Feature2DCollection
            });

            // model
            tools.Add(new Feature2DLineTool(FlowFMMapLayerProvider.BoundariesLayerName, BoundaryToolName, BoundaryIcon));
            tools.Add(new Feature2DLineTool(FlowFMMapLayerProvider.SourcesAndSinksLayerName, SourceAndSinkToolName, SourceSinkIcon));
            tools.Add(new Feature2DPointTool(FlowFMMapLayerProvider.SourcesAndSinksLayerName, SourceToolName, SourceIcon));
            tools.Add(new Feature2DLineTool(LateralMapLayerProvider.LayerName, LateralToolName, LateralPolygonIcon) {CloseLine = true});
            tools.Add(new Feature2DPointTool(LateralMapLayerProvider.LayerName, LateralPointToolName, LateralPointIcon));
            tools.OfType<ITargetLayerTool>().ForEach(t => t.LayerFilter = GetLayerFilter(t));

            mapView.MapControl.Tools.AddRange(tools);
        }

        private static Func<ILayer, bool> GetLayerFilter(ITargetLayerTool tool)
        {
            return l => l.Name == tool.LayerName &&            //expected layer name must match
                        l.DataSource is Feature2DCollection && //and layer must be a 2D layer
                        ((Feature2DCollection)l.DataSource).ModelName == ModelName;
        }
    }
}