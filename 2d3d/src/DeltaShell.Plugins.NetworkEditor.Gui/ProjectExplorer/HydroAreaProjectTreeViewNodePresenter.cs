using System.Collections;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.ProjectExplorer
{
    public class HydroAreaProjectTreeViewNodePresenter : TreeViewNodePresenterBaseForPluginGui<HydroArea>
    {
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, HydroArea nodeData)
        {
            node.Image = Resources.hydroarea;
        }

        public override IEnumerable GetChildNodeObjects(HydroArea parentNodeData, ITreeNode node)
        {
            return new object[]
            {
                parentNodeData.LandBoundaries,
                parentNodeData.DryPoints,
                parentNodeData.DryAreas,
                parentNodeData.ThinDams,
                parentNodeData.FixedWeirs,
                parentNodeData.ObservationPoints,
                parentNodeData.ObservationCrossSections,
                parentNodeData.Pumps,
                parentNodeData.Structures,
                parentNodeData.Enclosures,
                parentNodeData.BridgePillars
            };
        }
    }
}