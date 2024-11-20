using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.MapTools
{
    /// <summary>
    /// Maptool to copy branch features to the clipboard via context menu
    /// </summary>
    public class CopyBranchFeaturesMapTool : MapTool
    {
        public override IEnumerable<MapToolContextMenuItem> GetContextMenuItems(Coordinate worldPosition)
        {
            yield return new MapToolContextMenuItem
                        {
                            Priority = 2,
                            MenuItem = new ToolStripMenuItem("Copy", null, CopyBranchFeatureEventHandler)
                               {
                                   ShortcutKeys = Keys.Control | Keys.C
                               }
                        };
        }

        private void CopyBranchFeatureEventHandler(object sender, EventArgs e)
        {
            Execute();
        }

        public override void Execute()
        {
            var cursor = MapControl.Cursor;
            MapControl.Cursor = Cursors.WaitCursor;

            CopySelectedFeatureToClipBoard();

            MapControl.Cursor = cursor;
            MapControl.Refresh();
        }

        private void CopySelectedFeatureToClipBoard()
        {
            if (!MapControl.SelectedFeatures.Any())
            {
                return;
            }

            var selectedFeature = MapControl.SelectedFeatures.First();
            var branchFeature = selectedFeature as IBranchFeature;
            if (branchFeature != null)
            {
                HydroNetworkCopyAndPasteHelper.SetNetworkFeatureToClipBoard(branchFeature);
            }
        }

        public override bool Enabled
        {
            get
            {
                if (MapControl.SelectedFeatures.Count() == 1 && 
                    !MapControl.SelectedFeatures.OfType<IChannel>().Any() && !MapControl.SelectedFeatures.OfType<INode>().Any())
                {
                    IEnumerable<BranchStructure> features = MapControl.SelectedFeatures.OfType<BranchStructure>();
                    return features.Count() == 1;
                }
                return false;
            }
        }

        public override bool AlwaysActive
        {
            get
            {
                if (MapControl.SelectedFeatures.Count() == 1 &&
                    !MapControl.SelectedFeatures.OfType<IChannel>().Any() && !MapControl.SelectedFeatures.OfType<INode>().Any())
                {
                    return true;
                }
                return false;
            }
        }
    }
}