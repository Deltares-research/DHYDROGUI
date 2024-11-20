using System.Collections.Generic;
using System.Windows.Forms;
using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using GeoAPI.Geometries;
using SharpMap.UI.Forms;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.MapTools
{
    public class HydroRegionEditorMapTool : MapTool
    {
        public const string ThinDamToolName = "Thin dam tool (2D)";
        public const string FixedWeirToolName = "Fixed weir tool (2D)";
        public const string ObservationPointToolName = "Observation point tool (2D)";
        public const string ObservationCrossSectionToolName = "Observation cross section tool (2D)";
        public const string PumpToolName = "Pump tool (2D)";
        public const string WeirToolName = "Weir tool (2D)";
        public const string LandBoundaryToolName = "Land boundary tool";
        public const string DryPointToolName = "Dry point tool";
        public const string DryAreaToolName = "Dry area tool";
        public const string EnclosureToolName = "Enclosure tool";
        public const string BridgePillarToolName = "Bridge pillar tool";

        private static bool topologyRulesEnabledState;

        private readonly List<IMapTool> mapTools = new List<IMapTool>();

        public HydroRegionEditorMapTool()
        {
            Tolerance = 1;
        }

        public override IMapControl MapControl
        {
            get => base.MapControl;
            set
            {
                if (MapControl != null)
                {
                    RemoveNetworkEditorTools();

                    var control = (MapControl)MapControl;
                    control.MouseUp -= MapControlMouseUp;
                    control.KeyDown -= MapControlKeyDown;
                    control.KeyUp -= MapControlKeyUp;
                }

                base.MapControl = value;

                if (null != MapControl)
                {
                    AddNetworkEditorTools();

                    var control = (MapControl)MapControl;
                    control.MouseUp += MapControlMouseUp;
                    control.KeyDown += MapControlKeyDown;
                    control.KeyUp += MapControlKeyUp;
                }
            }
        }

        public override bool IsActive
        {
            get => true;
            set { }
        }

        /// <summary>
        /// All topology rules work only when user is editing data (currently it is between mouse down and mouse up).
        /// </summary>
        public bool TopologyRulesEnabled { get; set; }

        public virtual float Tolerance { get; set; }

        public override void OnMouseDown(Coordinate worldPosition, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TopologyRulesEnabled = true;
            }

            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            // select the nearest object, maybe this is redundant and select tool should always do it?
            MapControl.SelectTool.OnMouseDown(worldPosition, e);
        }

        private void AddNetworkEditorTools()
        {
            AddMapTool(new Feature2DLineTool(HydroAreaLayerNames.ThinDamsPluralName, ThinDamToolName, Resources.thindam));
            AddMapTool(new Feature2DLineTool(HydroAreaLayerNames.FixedWeirsPluralName, FixedWeirToolName, Resources.fixedweir));
            AddMapTool(new Feature2DPointTool(HydroAreaLayerNames.ObservationPointsPluralName, ObservationPointToolName, Resources.Observation));
            AddMapTool(new Feature2DLineTool(HydroAreaLayerNames.ObservationCrossSectionsPluralName, ObservationCrossSectionToolName, Resources.observationcs2d));
            AddMapTool(new Feature2DLineTool(HydroAreaLayerNames.PumpsPluralName, PumpToolName, Resources.pump));
            AddMapTool(new Feature2DLineTool(HydroAreaLayerNames.StructuresPluralName, WeirToolName, Resources.Weir) { MaxPoints = 2 });
            AddMapTool(new Feature2DLineTool(HydroAreaLayerNames.LandBoundariesPluralName, LandBoundaryToolName, Resources.landboundary));
            AddMapTool(new Feature2DPointTool(HydroAreaLayerNames.DryPointsPluralName, DryPointToolName, Resources.dry_point));
            AddMapTool(new Feature2DLineTool(HydroAreaLayerNames.DryAreasPluralName, DryAreaToolName, Resources.dry_area) { CloseLine = true });
            AddMapTool(new SingleFeature2DLineTool(HydroAreaLayerNames.EnclosureName, EnclosureToolName, Resources.enclosure) { CloseLine = true });
            AddMapTool(new Feature2DLineTool(HydroAreaLayerNames.BridgePillarsPluralName, BridgePillarToolName, Resources.BridgeSmall));

            MapControl.ActivateTool(MapControl.SelectTool);
        }

        private void MapControlKeyDown(object sender, KeyEventArgs e)
        {
            topologyRulesEnabledState = TopologyRulesEnabled;
            TopologyRulesEnabled = true;
        }

        private void MapControlKeyUp(object sender, KeyEventArgs e)
        {
            // remember topologyrule state; to reset properly. 
            // When tool is active (eg drawing branch) keydown/up should not reset TopologyRulesEnabled
            TopologyRulesEnabled = topologyRulesEnabledState;
        }

        private void MapControlMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TopologyRulesEnabled = false;
            }
        }

        private void RemoveNetworkEditorTools()
        {
            foreach (IMapTool mapTool in mapTools)
            {
                MapControl.Tools.Remove(mapTool);
            }

            mapTools.Clear();
        }

        private void AddMapTool(IMapTool mapTool)
        {
            mapTools.Add(mapTool);
            MapControl.Tools.Add(mapTool);
        }
    }
}