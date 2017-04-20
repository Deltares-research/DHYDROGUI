using System.Collections;
using System.Drawing;
using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters
{
    public class WaveTreeShortcutNodePresenter : FMSuiteNodePresenterBase<WaveTreeShortcut>
    {
        public override IEnumerable GetChildNodeObjects(WaveTreeShortcut parentNodeData, ITreeNode node)
        {
            return parentNodeData.SubItems;
        }

        protected override string GetNodeText(WaveTreeShortcut data)
        {
            return data.Text;
        }

        protected override Image GetNodeImage(WaveTreeShortcut data)
        {
            return data.Image;
        }

        protected override object GetContextMenuData(WaveTreeShortcut data)
        {
            return data.ContextMenuData ?? data.TargetData;
        }
    }
}
