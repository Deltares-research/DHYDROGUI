using DelftTools.Controls;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.PresentationObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    public class ChannelFrictionDefinitionsWrapperNodePresenter : TreeViewNodePresenterBaseForPluginGui<ChannelFrictionDefinitionsWrapper>
    {
        public override bool CanRenameNode(ITreeNode node)
        {
            return false;
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, ChannelFrictionDefinitionsWrapper nodeData)
        {
            node.Image = Resources.FrictionDefinition;
            node.Text = Resources.ChannelFrictionDefinitions_Name;
        }
    }
}