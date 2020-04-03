using System.Collections;
using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters
{
    public sealed class ControlGroupNodePresenter : TreeViewNodePresenterBaseForPluginGui<ControlGroup>
    {
        private static readonly Bitmap controlGroupIcon = RealTimeControl.Properties.Resources.controlgroup;

        public ControlGroupNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin)
        {
        }
        
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, ControlGroup nodeData)
        {
            node.Text = nodeData.Name;
            node.Image = controlGroupIcon;
        }

        protected override bool CanRemove(ControlGroup nodeData)
        {
            return true;
        }

        protected override bool RemoveNodeData(object parentNodeData, ControlGroup nodeData)
        {
            //base.RemoveNodeData(nodeData);
            var node = TreeView.GetNodeByTag(nodeData);
            var controlGroupsNode = node.Parent;
            var model = (RealTimeControlModel)controlGroupsNode.Parent.Parent.Tag;
            model.ControlGroups.Remove(nodeData);
            
            return true;
        }

        public override IEnumerable GetChildNodeObjects(ControlGroup parentNodeData, ITreeNode node)
        {
            foreach (var input in parentNodeData.Inputs)
            {
                yield return input;
            }
            foreach (var output in parentNodeData.Outputs)
            {
                yield return output;
            }
            foreach (var rtcObject in parentNodeData.Rules)
            {
                yield return rtcObject;
            }
            foreach (var conditionBase in parentNodeData.Conditions)
            {
                yield return conditionBase;
            }
            foreach (var signalBase in parentNodeData.Signals)
            {
                yield return signalBase;
            }
            foreach (var mathematicalExpression in parentNodeData.MathematicalExpressions)
            {
                yield return mathematicalExpression;
            }
        }
    }
}
