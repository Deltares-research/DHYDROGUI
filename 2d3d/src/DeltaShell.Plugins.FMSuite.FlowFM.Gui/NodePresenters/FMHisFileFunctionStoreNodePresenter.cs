using System.Collections;
using System.IO;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    public class FMHisFileFunctionStoreNodePresenter : TreeViewNodePresenterBase<IFMHisFileFunctionStore>
    {
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IFMHisFileFunctionStore nodeData)
        {
            node.Text = Path.GetFileName(nodeData.Path);
        }

        public override IEnumerable GetChildNodeObjects(IFMHisFileFunctionStore parentNodeData, ITreeNode node)
        {
            return parentNodeData.Functions;
        }
    }
}