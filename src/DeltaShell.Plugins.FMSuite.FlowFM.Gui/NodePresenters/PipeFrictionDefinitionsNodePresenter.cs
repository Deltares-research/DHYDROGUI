using DelftTools.Controls;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    public class PipeFrictionDefinitionsNodePresenter : TreeViewNodePresenterBaseForPluginGui<IEventedList<PipeFrictionDefinition>>
    {
        public override bool CanRenameNode(ITreeNode node)
        {
            return false;
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IEventedList<PipeFrictionDefinition> nodeData)
        {
            node.Image = Resources.FrictionDefinition;
            node.Text = Resources.PipeFrictionDefinitions_Name;
        }
    }
}