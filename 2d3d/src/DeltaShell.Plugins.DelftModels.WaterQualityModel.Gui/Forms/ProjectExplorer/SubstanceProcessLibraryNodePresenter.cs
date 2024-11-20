using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.ProjectExplorer
{
    public class SubstanceProcessLibraryNodePresenter : TreeViewNodePresenterBaseForPluginGui<SubstanceProcessLibrary>
    {
        private static readonly Bitmap LibraryImage = Resources.Library;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, SubstanceProcessLibrary nodeData)
        {
            node.Text = nodeData.Name;
            node.Image = LibraryImage;
        }
    }
}