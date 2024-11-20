using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Api.Layers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.MapTools
{
    class ImportBranchesFromSelectionMapTool : MapTool
    {
        public ImportBranchesFromSelectionMapTool(Func<ILayer, bool> layerFilter)
        {
            LayerFilter = layerFilter;
        }
        private ILayer TargetLayer
        {
            get { return Layers.FirstOrDefault(); }
        }

        public override bool Enabled
        {
            get { return TargetLayer != null && MapControl.SelectedFeatures.Any(s => s.Geometry is ILineString); }
        }

        public override bool AlwaysActive
        {
            get { return Enabled; }
        }

        public override void Execute()
        {
            Cursor cursor = MapControl.Cursor;
            MapControl.Cursor = Cursors.WaitCursor;

            var featureProvider = TargetLayer.DataSource;

            foreach (var feature in MapControl.SelectedFeatures)
            {
                if (!(feature.Geometry is LineString))
                {
                    continue;
                }

                if(!featureProvider.Contains(feature))
                {
                    featureProvider.Add(feature.Geometry);    
                }
                
            }
            MapControl.Cursor = cursor;
            MapControl.Refresh();
        }

        public override IEnumerable<MapToolContextMenuItem> GetContextMenuItems(Coordinate worldPosition)
        {
            if (!Enabled) yield break;

            yield return new MapToolContextMenuItem
                {
                    Priority = 5,
                    MenuItem =
                        new ToolStripMenuItem("Import selected features to branch layer", null, ImportBranchEventHandler)
                };
        }

        private void ImportBranchEventHandler(object sender, EventArgs e)
        {
            if (BeforeExecute != null)
            {
                BeforeExecute();
            }

            Execute();

            if (AfterExecute != null)
            {
                AfterExecute();
            }
        }

        public static event Action BeforeExecute;
        
        public static event Action AfterExecute;
    }
}