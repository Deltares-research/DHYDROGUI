using System.IO;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters
{
    public class RtcOutputFileFunctionStoreNodePresenter : TreeViewNodePresenterBase<RealTimeControlOutputFileFunctionStore>
    {
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, RealTimeControlOutputFileFunctionStore nodeData)
        {
            node.Text = File.Exists(nodeData.Path) ? Path.GetFileName(nodeData.Path) : string.Empty;
        }

        public override System.Collections.IEnumerable GetChildNodeObjects(RealTimeControlOutputFileFunctionStore parentNodeData, ITreeNode node)
        {
            return parentNodeData.Functions;
        }
    }
}
