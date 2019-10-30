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
    public class LateralSourceDataMapTool : MapTool
    {
        /// <summary>
        /// The lateral source data to apply the map tool logic for/to
        /// </summary>
        public IEnumerable<Model1DLateralSourceData> LateralSourceData { get; set; }

        public override bool AlwaysActive
        {
            get { return true; }
        }

        public override IEnumerable<MapToolContextMenuItem> GetContextMenuItems(Coordinate worldPosition)
        {
            LateralSourceData = MapControl == null || MapControl.SelectedFeatures == null
                ? Enumerable.Empty<Model1DLateralSourceData>()
                : MapControl.SelectedFeatures.OfType<Model1DLateralSourceData>();

            if (!LateralSourceData.Any()) return Enumerable.Empty<MapToolContextMenuItem>();
            
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
            var enableHTypes = LateralSourceData != null && LateralSourceData.Any();
            
            var convertMenu = new ToolStripMenuItem("Convert laterals");

            convertMenu.DropDownItems.Add(new ToolStripMenuItem(Resources.LateralSourceDataMapTool_AddToolStripMenuItems_Turn_selected_laterals_into_Q, null, TurnSelectedLateralsIntoQBoundary) { Enabled = enableHTypes });
            convertMenu.DropDownItems.Add(new ToolStripMenuItem(Resources.LateralSourceDataMapTool_AddToolStripMenuItems_Turn_selected_laterals_into_Q_t_, null, TurnSelectedLateralsIntoQTimeSeries) { Enabled = enableHTypes });
            convertMenu.DropDownItems.Add(new ToolStripMenuItem(Resources.LateralSourceDataMapTool_AddToolStripMenuItems_Turn_selected_laterals_into_Q_h_, null, TurnSelectedLateralsIntoQHTable) { Enabled = enableHTypes });

            return convertMenu;
        }

        private void TurnSelectedLateralsIntoQBoundary(object sender, EventArgs e)
        {
            if (LateralSourceData == null) return;

            foreach (var lateralSourceData in LateralSourceData)
            {
                lateralSourceData.DataType = Model1DLateralDataType.FlowConstant;
            }
        }

        private void TurnSelectedLateralsIntoQTimeSeries(object sender, EventArgs e)
        {
            if (LateralSourceData == null) return;

            foreach (var lateralSourceData in LateralSourceData)
            {
                lateralSourceData.DataType = Model1DLateralDataType.FlowTimeSeries;
            }
        }

        private void TurnSelectedLateralsIntoQHTable(object sender, EventArgs e)
        {
            if (LateralSourceData == null) return;

            foreach (var lateralSourceData in LateralSourceData)
            {
                lateralSourceData.DataType = Model1DLateralDataType.FlowWaterLevelTable;
            }
        }
    }
}
