using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using SharpMap.Api.Layers;
using SharpMap.Layers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.MapTools
{
    /// <summary>
    /// Maptool to paste a branchfeature from the clipboard by creating a new maptool, which is cleaned up after use
    /// </summary>
    public class PasteBranchFeaturesMapTool : MapTool
    {
        private double Tolerance { get; set; }
        private IBranchFeature BranchFeature { get; set; }
        private IMapTool newObjectTool;

        public PasteBranchFeaturesMapTool(Func<ILayer, bool> layerFilter)
        {
            LayerFilter = layerFilter;
            Tolerance = 10;
        }
        
        public override bool Enabled
        {
            get { return AlwaysActive; }
        }

        public override bool AlwaysActive
        {
            get { return HydroNetworkMapLayer != null && MapControl.SelectedFeatures.Any() && MapControl.SelectedFeatures.First() is IBranch; }
        }

        private HydroRegionMapLayer HydroNetworkMapLayer
        {
            get { return (HydroRegionMapLayer)Layers.FirstOrDefault(); }
        }

        public override IEnumerable<MapToolContextMenuItem> GetContextMenuItems(Coordinate worldPosition)
        {
            if (!Enabled || !HydroNetworkCopyAndPasteHelper.IsBranchFeatureSetToClipBoard()) yield break;

            yield return new MapToolContextMenuItem
                {
                    Priority = 2,
                    MenuItem = new ToolStripMenuItem("Paste", null, PasteBranchFeatureEventHandler)
                        {
                            ShortcutKeys = Keys.Control | Keys.V
                        }
                };
        }

        private void PasteBranchFeatureEventHandler(object sender, EventArgs e)
        {
            Execute();
        }

        public override void Execute()
        {
            CreateNewNodeToolForBranchFeatureFromClipBoard();
        }

        public override bool IsBusy
        {
            get { return (newObjectTool != null); }
            protected set { base.IsBusy = value; }
        }

        public override void Cancel()
        {
            if (newObjectTool != null)
            {
                newObjectTool = null;
            }
            base.Cancel();
        }

        public override void OnMouseMove(Coordinate worldPosition, MouseEventArgs e)
        {
            if (newObjectTool != null)
                newObjectTool.OnMouseMove(worldPosition, e);
            else
                base.OnMouseMove(worldPosition, e);
        }

        public override void OnMouseDown(Coordinate worldPosition, MouseEventArgs e)
        {
            if (newObjectTool != null)
                newObjectTool.OnMouseDown(worldPosition, e);
            else
                base.OnMouseDown(worldPosition, e);
        }

        public override void OnMouseUp(Coordinate worldPosition, MouseEventArgs e)
        {
            if (newObjectTool != null && e.Button == MouseButtons.Left)
            {
                newObjectTool.OnMouseUp(worldPosition, e);
                if (MapControl.SelectedFeatures.Count() != 0)
                {
                    var newBranchFeature = MapControl.SelectedFeatures.First();
                    if (newBranchFeature is ICopyFrom && !(newBranchFeature is ICompositeBranchStructure))
                    {
                        ((ICopyFrom) newBranchFeature).CopyFrom(BranchFeature);
                    }
                    if (newBranchFeature is ICompositeBranchStructure)
                    {
                        HydroNetworkCopyAndPasteHelper.PasteCompositeStructureToBranch(BranchFeature, newBranchFeature);
                    }
                    MapControl.ActivateTool(MapControl.SelectTool);
                }
                Cancel();
            }
            else
                base.OnMouseUp(worldPosition, e);
        }

        private void CreateNewNodeToolForBranchFeatureFromClipBoard()
        {
            if (!HydroNetworkCopyAndPasteHelper.IsBranchFeatureSetToClipBoard()) return;

            BranchFeature = HydroNetworkCopyAndPasteHelper.GetBranchFeatureFromClipBoard();
            if (BranchFeature == null) return;

            if (BranchFeature is ICrossSection)
            {
                string errorMessage;
                var network = (IHydroNetwork) HydroNetworkMapLayer.Region;
                if (!HydroNetworkCopyAndPasteHelper.IsCrossSectionPastableInNetwork(network, ((ICrossSection)BranchFeature), out errorMessage))
                {
                    MessageBox.Show(errorMessage, "Error pasting cross section", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var crossSectionClone = (ICrossSection) BranchFeature.Clone();
                if (!HydroNetworkCopyAndPasteHelper.AdaptCrossSectionBeforePastingInNetwork(network, crossSectionClone))
                {
                    return;
                }
                
                BranchFeature = crossSectionClone;
            }

            var targetLayer = Map.GetAllVisibleLayers(true).OfType<VectorLayer>().FirstOrDefault(l => l.DataSource.FeatureType == BranchFeature.GetEntityType());
            if (targetLayer == null) return;
            
            IMapTool pasteBranchFeatureClipBoardTool = MapControl.Tools.FirstOrDefault(tool => tool.Layers.Count() == 1 && tool.Layers.Contains(targetLayer));

            if (pasteBranchFeatureClipBoardTool == null) return;

            newObjectTool = pasteBranchFeatureClipBoardTool;
            MapControl.ActivateTool(newObjectTool);
            MapControl.SnapTool.IsActive = true;
            MapControl.Refresh();
        }
    }
}