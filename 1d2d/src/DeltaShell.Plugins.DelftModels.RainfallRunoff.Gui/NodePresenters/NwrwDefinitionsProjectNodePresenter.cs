using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.NodePresenters
{
    public class NwrwDefinitionsProjectNodePresenter : TreeViewNodePresenterBase<EventedList<NwrwDefinition>>
    {
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, EventedList<NwrwDefinition> nodeData)
        {
            node.Text = "Nwrw Surface Settings";
        }
    }
}