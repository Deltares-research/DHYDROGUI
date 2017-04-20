using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters
{
    class OutputNodePresenter : TreeViewNodePresenterBaseForPluginGui<Output>
    {
        private static readonly Bitmap OutputIcon = RealTimeControl.Properties.Resources.output;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, Output nodeData)
        {
            node.Text = nodeData.Name;
            node.Tag = nodeData;
            node.Image = OutputIcon;
        }
    }
}