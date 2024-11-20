using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using SharpMap.Api.Layers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.MapTools
{
    public class PasteIntoBranchFeaturesMapTool : MapTool
    {
        public PasteIntoBranchFeaturesMapTool(Func<ILayer, bool> layerFilter)
        {
            LayerFilter = layerFilter;
        }

        public override IEnumerable<MapToolContextMenuItem> GetContextMenuItems(Coordinate worldPosition)
        {
            var clipBoardFeature = HydroNetworkCopyAndPasteHelper.GetBranchFeatureFromClipBoard();
            if (!Enabled || clipBoardFeature == null || clipBoardFeature.GetType() != MapControl.SelectedFeatures.First().GetType()) yield break;
            
            yield return new MapToolContextMenuItem
                {
                    Priority = 2,
                    MenuItem = new ToolStripMenuItem("Paste into", null, PasteIntoBranchFeatureEventHandler)
                        {
                            ShortcutKeys = Keys.Control | Keys.V
                        }
                };
        }

        private void PasteIntoBranchFeatureEventHandler(object sender, EventArgs e)
        {
            Execute();
        }

        public override void Execute()
        {
            foreach (var selectedFeature in MapControl.SelectedFeatures.OfType<IBranchFeature>())
            {
                string errorMessage;
                if (!HydroNetworkCopyAndPasteHelper.PasteBranchFeatureIntoBranchFeature(selectedFeature, out errorMessage))
                {
                    MessageBox.Show(errorMessage, "Confirm", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public override bool Enabled
        {
            get { return AlwaysActive; }
        }

        public override bool AlwaysActive
        {
            get { return Layers.Any() && MapControl.SelectedFeatures.Any() && MapControl.SelectedFeatures.First() is IBranchFeature; }
        }
    }
}
