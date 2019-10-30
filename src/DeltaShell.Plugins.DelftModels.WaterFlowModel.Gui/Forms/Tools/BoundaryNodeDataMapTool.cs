using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Properties;
using GeoAPI.Geometries;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.Tools
{
    public class BoundaryNodeDataMapTool : MapTool
    {
        /// <summary>
        /// The boundary node data to apply the map tool logic for/to
        /// </summary>
        public IEnumerable<Model1DBoundaryNodeData> BoundaryNodeData { get; set; }

        public override bool AlwaysActive
        {
            get { return true; }
        }

        public override IEnumerable<MapToolContextMenuItem> GetContextMenuItems(Coordinate worldPosition)
        {
            BoundaryNodeData = MapControl == null || MapControl.SelectedFeatures == null
                ? Enumerable.Empty<Model1DBoundaryNodeData>()
                : MapControl.SelectedFeatures.OfType<Model1DBoundaryNodeData>();

            if (!BoundaryNodeData.Any()) return Enumerable.Empty<MapToolContextMenuItem>();

            return new[]
                {
                    new MapToolContextMenuItem
                        {
                            Priority = 2,
                            MenuItem = GetToolStripMenuItems()
                        }
                };
        }

        protected virtual ToolStripMenuItem GetToolStripMenuItems()
        {
            var enableHTypes = BoundaryNodeData != null && BoundaryNodeData.Any();
            var enableQTypes = enableHTypes && !BoundaryNodeData.Any(bnd => bnd.Feature.IsConnectedToMultipleBranches);

            var convertMenu = new ToolStripMenuItem("Convert boundaries");

            convertMenu.DropDownItems.Add(new ToolStripMenuItem(Resources.BoundaryNodeDataMapTool_AddToolStripMenuItems_Turn_selected_nodes_into_none, Resources.None, TurnSelectedNodesIntoNone) { Enabled = enableHTypes });
            convertMenu.DropDownItems.Add(new ToolStripMenuItem(Resources.BoundaryNodeDataMapTool_AddToolStripMenuItems_Turn_selected_nodes_into_H, Resources.HConst, TurnSelectedNodesIntoHBoundary) { Enabled = enableHTypes });
            convertMenu.DropDownItems.Add(new ToolStripMenuItem(Resources.BoundaryNodeDataMapTool_AddToolStripMenuItems_Turn_selected_nodes_into_H_t_, Resources.HBoundary, TurnSelectedNodesIntoHTimeSeries) { Enabled = enableHTypes });
            convertMenu.DropDownItems.Add(new ToolStripMenuItem(Resources.BoundaryNodeDataMapTool_AddToolStripMenuItems_Turn_selected_nodes_into_Q, Resources.QConst, TurnSelectedNodesIntoQBoundary) { Enabled = enableQTypes });
            convertMenu.DropDownItems.Add(new ToolStripMenuItem(Resources.BoundaryNodeDataMapTool_AddToolStripMenuItems_Turn_selected_nodes_into_Q_t_, Resources.QBoundary, TurnSelectedNodesIntoQTimeSeries) { Enabled = enableQTypes });
            convertMenu.DropDownItems.Add(new ToolStripMenuItem(Resources.BoundaryNodeDataMapTool_AddToolStripMenuItems_Turn_selected_nodes_into_Q_h_, Resources.QHBoundary, TurnSelectedNodesIntoQHTable) { Enabled = enableQTypes });

            return convertMenu;
        }

        private void TurnSelectedNodesIntoNone(object sender, EventArgs e)
        {
            if (BoundaryNodeData == null) return;

            foreach (var boundaryNodeData in BoundaryNodeData)
            {
                boundaryNodeData.DataType = Model1DBoundaryNodeDataType.None;
            }
        }

        private void TurnSelectedNodesIntoHBoundary(object sender, EventArgs e)
        {
            if (BoundaryNodeData == null) return;
            
            foreach (var boundaryNodeData in BoundaryNodeData)
            {
                boundaryNodeData.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
            }
        }

        private void TurnSelectedNodesIntoHTimeSeries(object sender, EventArgs e)
        {
            if (BoundaryNodeData == null) return;
            
            foreach (var boundaryNodeData in BoundaryNodeData)
            {
                boundaryNodeData.DataType = Model1DBoundaryNodeDataType.WaterLevelTimeSeries;
            }
        }

        private void TurnSelectedNodesIntoQBoundary(object sender, EventArgs e)
        {
            if (BoundaryNodeData == null) return;
            
            foreach (var boundaryNodeData in BoundaryNodeData)
            {
                boundaryNodeData.DataType = Model1DBoundaryNodeDataType.FlowConstant;
            }
        }

        private void TurnSelectedNodesIntoQTimeSeries(object sender, EventArgs e)
        {
            if (BoundaryNodeData == null) return;
            
            foreach (var boundaryNodeData in BoundaryNodeData)
            {
                boundaryNodeData.DataType = Model1DBoundaryNodeDataType.FlowTimeSeries;
            }
        }

        private void TurnSelectedNodesIntoQHTable(object sender, EventArgs e)
        {
            if (BoundaryNodeData == null) return;
            
            foreach (var boundaryNodeData in BoundaryNodeData)
            {
                boundaryNodeData.DataType = Model1DBoundaryNodeDataType.FlowWaterLevelTable;
            }
        }
    }
}
