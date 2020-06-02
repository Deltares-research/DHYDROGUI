using System.Collections.Generic;
using System.Linq;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.MapTools
{
    public class SelectBranchesAlongShortestPathTool : SelectTool
    {
        private INode sourceNode;
        private INode targetNode;

        protected override void SelectFeature(Coordinate worldPosition, IFeature nearest, ILayer selectedLayer)
        {
            // Create or add a new FeatureInteractor
            if (SelectedFeatureInteractors.Count > 0)
            {
                IFeatureInteractor currentFeatureInteractor = GetActiveFeatureInteractor(nearest);
                if (KeyExtendSelection) // Shift key
                {
                    if (currentFeatureInteractor == null)
                    {
                        var selectedBranch = nearest as IBranch;
                        if (selectedBranch != null)
                        {
                            SelectBranchesAlongShortestPath(worldPosition, selectedLayer, nearest, selectedBranch);
                        }
                        else
                        {
                            // not in selection; add
                            AddSelection(selectedLayer, nearest);
                            targetNode = null;
                        }
                    } // else possibly set default focus tracker
                }
                else if (KeyToggleSelection) // CTRL key
                {
                    if (null == currentFeatureInteractor)
                    {
                        // not in selection; add
                        AddSelection(selectedLayer, nearest);
                        SetTargetNodeIfBranch(worldPosition, nearest);
                    }
                    else
                    {
                        // in selection; remove
                        RemoveSelection(nearest);
                        targetNode = null;
                    }
                }
                else
                {
                    // no special key processing; handle as a single select.
                    Clear();
                    if (!StartSelection(selectedLayer, nearest))
                    {
                        StartMultiSelect();
                    }

                    SetTargetNodeIfBranch(worldPosition, nearest);
                }
            }
            else
            {
                if (!StartSelection(selectedLayer, nearest))
                {
                    StartMultiSelect();
                }

                SetTargetNodeIfBranch(worldPosition, nearest);
            }
        }

        private void SelectBranchesAlongShortestPath(Coordinate worldPosition, ILayer selectedLayer, IFeature nearest, IBranch selectedBranch)
        {
            sourceNode = targetNode;
            targetNode = selectedBranch.Source.Geometry.Distance(new Point(worldPosition)) <
                         selectedBranch.Target.Geometry.Distance(new Point(worldPosition))
                             ? selectedBranch.Source
                             : selectedBranch.Target;
            IEnumerable<IBranch> result = selectedBranch.Network.GetShortestPath(sourceNode, targetNode, null);

            foreach (IBranch branch in result)
            {
                AddSelection(selectedLayer, branch);
            }

            // Ensure 'nearest' will be added to the selection
            if (!result.Contains(selectedBranch))
            {
                AddSelection(selectedLayer, nearest);
            }
        }

        private void SetTargetNodeIfBranch(Coordinate worldPosition, IFeature nearest)
        {
            var selectedBranch = nearest as IBranch;
            if (nearest is IBranch)
            {
                targetNode = selectedBranch.Source.Geometry.Distance(new Point(worldPosition)) <
                             selectedBranch.Target.Geometry.Distance(new Point(worldPosition))
                                 ? selectedBranch.Source
                                 : selectedBranch.Target;
            }
            else
            {
                // Not selecting a branch, thus clear 'targetNode'
                targetNode = null;
            }
        }
    }
}