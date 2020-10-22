using System.Drawing;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters
{
    public class ReadOnlyOutputTextDocumentNodePresenter : TreeViewNodePresenterBase<ReadOnlyOutputTextDocument>
    {
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, ReadOnlyOutputTextDocument nodeData)
        {
            node.Text = nodeData.Name;
            node.Image = GetImage(nodeData.Name);
        }

        private static Image GetImage(string fileNameWithExtension)
        {
            string[] parts = fileNameWithExtension.Split('.');
            string extension = parts.Last();

            if (string.IsNullOrEmpty(extension)) return Resources.textdocument;
            
            switch (extension)
            {
                case "xml":
                    return Resources.textdocument;
                case "csv":
                    return Resources.table;
                default:
                    return Resources.textdocument;
            }
        }

    }
}