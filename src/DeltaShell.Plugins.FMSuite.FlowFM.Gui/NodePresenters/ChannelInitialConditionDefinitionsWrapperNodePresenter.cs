using DelftTools.Controls;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.PresentationObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    public class ChannelInitialConditionDefinitionsWrapperNodePresenter : TreeViewNodePresenterBaseForPluginGui<ChannelInitialConditionDefinitionsWrapper>
    {
        public override bool CanRenameNode(ITreeNode node)
        {
            return false;
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, ChannelInitialConditionDefinitionsWrapper nodeData)
        {
            node.Image = Resources.waterLayers;
            node.Text = "Channels";
        }

    }
}