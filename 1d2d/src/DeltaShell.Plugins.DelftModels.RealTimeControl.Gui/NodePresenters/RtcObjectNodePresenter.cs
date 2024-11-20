using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters
{
    /// <summary>
    /// This Node Presenter handles all Project Explorer presentation for all objects deriving from the
    /// <see cref="RtcBaseObject"/>.
    /// </summary>
    public sealed class RtcObjectNodePresenter : TreeViewNodePresenterBaseForPluginGui<RtcBaseObject>
    {
        /// <summary>
        /// A dictionary mapping all RTC Domain types to their particular icon used for representation in the Project Explorer
        /// </summary>
        private static readonly Dictionary<Type, Bitmap> rtcTypesToBitmaps = new Dictionary<Type, Bitmap>
        {
            {typeof(FactorRule), Resources.rule},
            {typeof(HydraulicRule), Resources.rule},
            {typeof(IntervalRule), Resources.rule},
            {typeof(PIDRule), Resources.rule},
            {typeof(RelativeTimeRule), Resources.rule},
            {typeof(TimeRule), Resources.rule},
            {typeof(DirectionalCondition), Resources.condition},
            {typeof(StandardCondition), Resources.condition},
            {typeof(TimeCondition), Resources.condition},
            {typeof(LookupSignal), Resources.signal},
            {typeof(Output), Resources.output},
            {typeof(Input), Resources.input},
            {typeof(MathematicalExpression), Resources.MathExpr}
        };

        /// <inheritdoc/>
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, RtcBaseObject nodeData)
        {
            node.Text = nodeData.Name;
            node.Tag = nodeData;

            if (!rtcTypesToBitmaps.ContainsKey(nodeData.GetType()))
            {
                node.Image = Resources.QuestionMark;
            }
            else
            {
                node.Image = rtcTypesToBitmaps[nodeData.GetType()];
            }
        }
    }
}