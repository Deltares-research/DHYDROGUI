using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.NodePresenters
{
    public class DryWeatherFlowDefinitionsProjectNodePresenter : TreeViewNodePresenterBase<EventedList<NwrwDryWeatherFlowDefinition>>
    {
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, EventedList<NwrwDryWeatherFlowDefinition> nodeData)
        {
            node.Text = "Dryweather Flow Definitions";
        }
    }
}