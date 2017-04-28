using System.Collections;
using System.Drawing;
using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    public class FlowFMTreeShortcutNodePresenter : FMSuiteNodePresenterBase<FlowFMTreeShortcut>
    {
        protected override string GetNodeText(FlowFMTreeShortcut data)
        {
            return data.Text;
        }

        protected override Image GetNodeImage(FlowFMTreeShortcut data)
        {
            return data.Image;
        }

        protected override object GetContextMenuData(FlowFMTreeShortcut data)
        {
            return data.ContextMenuData ?? data.TargetData;
        }

        public override IEnumerable GetChildNodeObjects(FlowFMTreeShortcut parentNodeData, ITreeNode node)
        {
            return parentNodeData.SubItems;
        }
    }
}