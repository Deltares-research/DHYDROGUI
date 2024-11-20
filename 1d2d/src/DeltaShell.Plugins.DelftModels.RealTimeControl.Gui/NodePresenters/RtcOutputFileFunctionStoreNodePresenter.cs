using System.Collections;
using System.IO;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DelftTools.Functions;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters
{
    public class RtcOutputFileFunctionStoreNodePresenter : TreeViewNodePresenterBase<RealTimeControlOutputFileFunctionStore>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RtcOutputFileFunctionStoreNodePresenter));

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, RealTimeControlOutputFileFunctionStore nodeData)
        {
            if (nodeData == null)
            {
                log.Warn("Unable to update node for Real-time control model Output, Function Store is null");
                return;
            }

            if (!File.Exists(nodeData.Path))
            {
                log.WarnFormat("Unable to update node for Real-time control model Output, file does not exist: {0}", nodeData.Path);
                return;
            }

            node.Text = Path.GetFileName(nodeData.Path);
        }

        public override IEnumerable GetChildNodeObjects(RealTimeControlOutputFileFunctionStore parentNodeData, ITreeNode node)
        {
            return parentNodeData != null && parentNodeData.Functions != null ? parentNodeData.Functions : Enumerable.Empty<IFunction>();
        }
    }
}