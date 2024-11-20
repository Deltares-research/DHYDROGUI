using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro.Roughness;
using DelftTools.Shell.Gui.Swf;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ProjectExplorer
{
    public class RoughnessSectionNodePresenter : TreeViewNodePresenterBaseForPluginGui<RoughnessSection>
    {
        private static readonly Bitmap ReverseRoughnessSectionIcon = NetworkEditor.Properties.Resources.ReverseRoughnessSection;
        private static readonly Bitmap RoughnessSectionIcon = NetworkEditor.Properties.Resources.RoughnessSection;

        public override bool CanRenameNode(ITreeNode node)
        {
            return false;
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, RoughnessSection data)
        {
            node.Image = data.Reversed ? ReverseRoughnessSectionIcon : RoughnessSectionIcon;
            node.Text = data.Name;
        }
    }
}
