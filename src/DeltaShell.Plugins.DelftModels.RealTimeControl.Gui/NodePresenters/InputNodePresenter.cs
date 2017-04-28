using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters
{
    class InputNodePresenter : TreeViewNodePresenterBaseForPluginGui<Input>
    {
        private static readonly Bitmap InputIcon = RealTimeControl.Properties.Resources.input;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, Input nodeData)
        {
            node.Text = nodeData.Name;
            node.Tag = nodeData;
            node.Image = InputIcon;
        }
    }
}