using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.MapTools;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui
{
    public class FlowFMMapViewDecorator
    {
        private static readonly Bitmap BoundaryIcon = Common.Gui.Properties.Resources.boundary;
        private static readonly Bitmap SourceSinkIcon = Resources.SourceSink;
        private static readonly Bitmap SourceIcon = Resources.LateralSourceMap;

        internal const string BoundaryToolName = "Boundary tool (2D)";
        internal const string SourceToolName = "Source tool (2D)";
        internal const string SourceAndSinkToolName = "Source & sink tool";
        internal const string Reverse2DLineToolName = "Reverse line(s) (2D)";
        internal const string GenerateEmbankmentsToolName = "Generate embankments (2D)";
        internal const string MergeEmbankmentsToolName = "Merge embankments";
        internal const string GridWizardToolName = "Grid wizard";
        internal const string GenerateLinksToolName = "Generate links (1D2D)";
        internal const string AddLinksToolName = "Add link (1D2D)";

        private static readonly string ModelName = typeof (WaterFlowFMModel).Name;



        public static void AddMapToolsIfMissing(MapView mapView)
        {
            if (mapView.MapControl.Tools.OfType<Feature2DLineTool>().Any(t => t.Name == BoundaryToolName))
                return; // already has them

            var tools = new List<MapTool>();
            
            tools.Add(new Reverse2DLineTool
                {
                    Name = Reverse2DLineToolName,
                    LayerFilter = layer => (layer.Name == HydroArea.ObservationCrossSectionsPluralName ||
                                            layer.Name == HydroArea.PumpsPluralName ||
                                            layer.Name == HydroArea.WeirsPluralName ||
                                            layer.Name == HydroArea.GatesPluralName ||
                                            layer.Name == FlowFMLayerNames.SourcesAndSinksLayerName) &&
                                           layer.DataSource is Feature2DCollection
                });

            // model
            tools.Add(new Feature2DLineTool(FlowFMLayerNames.BoundariesLayerName, BoundaryToolName, BoundaryIcon));
            tools.Add(new Feature2DLineTool(FlowFMLayerNames.SourcesAndSinksLayerName, SourceAndSinkToolName, SourceSinkIcon));
            tools.Add(new Feature2DPointTool(FlowFMLayerNames.SourcesAndSinksLayerName, SourceToolName, SourceIcon));
            tools.Add(new GenerateEmbankmentsMapTool());
            tools.Add(new MergeEmbankmentsMapTool());
            tools.Add(new GridWizardMapTool());
            tools.Add(new GenerateLinksMapTool());
            tools.Add(new Add1D2DLinkMapTool());
            tools.OfType<ITargetLayerTool>().ForEach(t => t.LayerFilter = GetLayerFilter(t));

            mapView.MapControl.Tools.AddRange(tools);
        }

        private static Func<ILayer, bool> GetLayerFilter(ITargetLayerTool tool)
        {
            return l => l.Name == tool.LayerName && //expected layer name must match
                        l.DataSource is Feature2DCollection && //and layer must be a 2D layer
                        ((Feature2DCollection) l.DataSource).ModelName == ModelName;
        }
    }
}