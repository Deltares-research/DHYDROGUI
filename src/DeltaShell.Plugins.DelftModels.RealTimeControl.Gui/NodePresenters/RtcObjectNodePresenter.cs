using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters
{
    class RtcObjectNodePresenter : TreeViewNodePresenterBaseForPluginGui<RtcBaseObject>
    {
        private static readonly Bitmap RuleIcon = RealTimeControl.Properties.Resources.rule;
        private static readonly Bitmap ConditionIcon = RealTimeControl.Properties.Resources.condition;
        private static readonly Bitmap SignalIcon = RealTimeControl.Properties.Resources.signal;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, RtcBaseObject nodeData)
        {
            node.Text = nodeData.Name;
            node.Tag = nodeData;
            if (nodeData is RuleBase)
            {
                node.Image = RuleIcon;
            }
            else if (nodeData is ConditionBase)
            {
                node.Image = ConditionIcon;
            }
            else if (nodeData is SignalBase)
            {
                node.Image = SignalIcon;
            }
            else
            {
                node.Image = RuleIcon;  // Prevent crash, should never happen.
            }
        }
    }
}
