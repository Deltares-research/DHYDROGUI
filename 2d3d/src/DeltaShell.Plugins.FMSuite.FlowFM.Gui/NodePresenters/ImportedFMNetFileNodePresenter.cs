using System.Drawing;
using System.IO;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    public class ImportedFMNetFileNodePresenter : TreeViewNodePresenterBaseForPluginGui<ImportedFMNetFile>
    {
        private static readonly Bitmap Unstruc = Resources.unstruc;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, ImportedFMNetFile nodeData)
        {
            node.Image = Unstruc;
            node.Text = Path.GetFileName(nodeData.Path);
        }

        public override bool CanRenameNode(ITreeNode node)
        {
            return false;
        }
    }
}