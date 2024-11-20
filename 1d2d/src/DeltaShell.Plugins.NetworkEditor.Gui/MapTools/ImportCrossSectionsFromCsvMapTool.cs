using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using GeoAPI.Geometries;
using SharpMap.Api.Layers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.MapTools
{
    public class ImportCrossSectionsFromCsvMapTool: MapTool
    {
        public ImportCrossSectionsFromCsvMapTool(Func<ILayer, bool> layerFilter)
        {
            LayerFilter = layerFilter;
        }

        private HydroRegionMapLayer HydroNetworkMapLayer
        {
            get { return (HydroRegionMapLayer)Layers.FirstOrDefault(); }
        }

        public override bool AlwaysActive
        {
            get { return true; }
        }

        public override bool Enabled { get { return HydroNetworkMapLayer != null; } }

        public override IEnumerable<MapToolContextMenuItem> GetContextMenuItems(Coordinate worldPosition)
        {
            if (!Enabled) yield break;

            yield return new MapToolContextMenuItem
                {
                    Priority = 5,
                    MenuItem = new ToolStripMenuItem("Import cross section(s) from csv", null, ImportCrossSectionsEventHandler)
                };
        }

        private void ImportCrossSectionsEventHandler(object sender, EventArgs e)
        {
            try
            {
                Execute();
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message, "Import failed", MessageBoxButtons.OK);
            }
        }

        public override void Execute()
        {
            var cursor = MapControl.Cursor;
            MapControl.Cursor = Cursors.WaitCursor;
            try
            {
                var dataItem = new DataItem { Value = HydroNetworkMapLayer.Region };
                var gui = NetworkEditorGuiPlugin.Instance.Gui;

                gui.CommandHandler.ImportToDataItem(dataItem);
                while (gui.Application.IsActivityRunning())
                {
                    Application.DoEvents(); // wait until import finishes
                }

            }
            finally
            {
                MapControl.Cursor = cursor;
            }
            MapControl.Refresh();
        }
    }
}
