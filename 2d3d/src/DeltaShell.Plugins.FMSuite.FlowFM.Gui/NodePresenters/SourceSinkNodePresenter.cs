using System.Collections.Generic;
using System.Drawing;
using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    public class SourceSinkNodePresenter : FMSuiteNodePresenterBase<SourceAndSink>
    {
        private static readonly Bitmap SourceSinkIcon = Resources.SourceSink;
        private static readonly Bitmap SourceIcon = Resources.LateralSourceMap;

        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }

        public override void OnNodeRenamed(SourceAndSink data, string newName)
        {
            data.Feature.Name = newName;
        }

        protected override string GetNodeText(SourceAndSink data)
        {
            return data.Feature != null ? data.Feature.Name : "<error>";
        }

        protected override Image GetNodeImage(SourceAndSink data)
        {
            return data.IsPointSource ? SourceIcon : SourceSinkIcon;
        }

        protected override bool CanRemove(SourceAndSink nodeData)
        {
            return true;
        }

        protected override bool RemoveNodeData(object parentNodeData, SourceAndSink nodeData)
        {
            var sourceAndSinks = parentNodeData as IList<SourceAndSink>;
            if (sourceAndSinks != null && sourceAndSinks.Remove(nodeData))
            {
                ResetGuiSelection();
                return true;
            }

            var treeShortCut = parentNodeData as FmModelTreeShortcut;
            if (treeShortCut != null)
            {
                sourceAndSinks = treeShortCut.Value as IList<SourceAndSink>;
                if (sourceAndSinks != null && sourceAndSinks.Remove(nodeData))
                {
                    ResetGuiSelection();
                    return true;
                }
            }

            return false;
        }
    }
}