using System.Collections;
using System.IO;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    public class FMHisFileFunctionStoreNodePresenter : TreeViewNodePresenterBase<FMHisFileFunctionStore>
    {
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, FMHisFileFunctionStore nodeData)
        {
            node.Text = Path.GetFileName(nodeData.Path);
        }

        public override IEnumerable GetChildNodeObjects(FMHisFileFunctionStore parentNodeData, ITreeNode node)
        {
            return parentNodeData.Functions;
        }
    }
}