using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters
{
    public sealed class ControlGroupNodePresenter : TreeViewNodePresenterBaseForPluginGui<ControlGroup>
    {
        private static readonly Bitmap controlGroupIcon = Resources.controlgroup;

        public ControlGroupNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin) {}

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, ControlGroup nodeData)
        {
            node.Text = nodeData.Name;
            node.Image = controlGroupIcon;
        }

        public override IEnumerable GetChildNodeObjects(ControlGroup parentNodeData, ITreeNode node)
        {
            foreach (Input input in parentNodeData.Inputs)
            {
                yield return input;
            }

            foreach (Output output in parentNodeData.Outputs)
            {
                yield return output;
            }

            foreach (RuleBase rtcObject in parentNodeData.Rules)
            {
                yield return rtcObject;
            }

            foreach (ConditionBase conditionBase in parentNodeData.Conditions)
            {
                yield return conditionBase;
            }

            foreach (SignalBase signalBase in parentNodeData.Signals)
            {
                yield return signalBase;
            }

            foreach (MathematicalExpression mathematicalExpression in parentNodeData.MathematicalExpressions)
            {
                yield return mathematicalExpression;
            }
        }

        protected override bool CanRemove(ControlGroup nodeData)
        {
            return true;
        }

        protected override bool RemoveNodeData(object parentNodeData, ControlGroup nodeData)
        {
            return parentNodeData is IList<ControlGroup> controlGroups 
                   && controlGroups.Remove(nodeData);
        }
    }
}