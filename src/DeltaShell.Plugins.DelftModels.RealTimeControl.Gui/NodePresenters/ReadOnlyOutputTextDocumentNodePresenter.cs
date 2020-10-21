using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DelftTools.Utils.Drawing;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters
{
    public class ReadOnlyOutputTextDocumentNodePresenter : TreeViewNodePresenterBase<ReadOnlyOutputTextDocument>
    {
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, ReadOnlyOutputTextDocument nodeData)
        {
            node.Text = nodeData.Name;
            node.Image = CommonTools.Properties.Resources.textdocument;

            if (nodeData.Owner.OutputOutOfSync)
            {
                node.Image = node.Image.AddOverlayImage(Properties.Resources.ExclamationOverlay, 5, 1, 10, 10);
            }
        }
    }
}