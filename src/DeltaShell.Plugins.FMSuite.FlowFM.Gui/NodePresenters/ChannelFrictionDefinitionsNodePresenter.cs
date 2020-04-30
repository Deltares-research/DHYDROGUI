using DelftTools.Controls;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    public class ChannelFrictionDefinitionsNodePresenter : TreeViewNodePresenterBaseForPluginGui<IEventedList<ChannelFrictionDefinition>>
    {
        public override bool CanRenameNode(ITreeNode node)
        {
            return false;
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IEventedList<ChannelFrictionDefinition> nodeData)
        {
            node.Image = Resources.FrictionDefinition;
            node.Text = Resources.ChannelFrictionDefinitions_Name;
        }
    }
}