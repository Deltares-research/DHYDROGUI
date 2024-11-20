using System.Collections.Generic;
using System.Drawing;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    class SourceSinkNodePresenter : FMSuiteNodePresenterBase<SourceAndSink>
    {
        private static readonly Bitmap SourceSinkIcon = Properties.Resources.SourceSink;
        private static readonly Bitmap SourceIcon = Properties.Resources.LateralSourceMap;

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
            if (sourceAndSinks != null)
            {
                return sourceAndSinks.Remove(nodeData);
            }
            var treeShortCut = parentNodeData as FmModelTreeShortcut;
            if (treeShortCut != null)
            {
                sourceAndSinks = treeShortCut.Data as IList<SourceAndSink>;
                if (sourceAndSinks != null)
                {
                    return sourceAndSinks.Remove(nodeData);
                }
            }
            return false;
        }

        public override bool CanRenameNode(DelftTools.Controls.ITreeNode node)
        {
            return true;
        }

        public override void OnNodeRenamed(SourceAndSink data, string newName)
        {
            data.Feature.Name = newName;
        }
    }
}