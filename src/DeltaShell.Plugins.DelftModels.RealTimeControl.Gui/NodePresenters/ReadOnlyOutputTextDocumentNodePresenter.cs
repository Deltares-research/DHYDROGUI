using System.Drawing;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters
{
    /// <summary>
    /// <see cref="ReadOnlyOutputTextDocumentNodePresenter"/> implements the NodePresenter for
    /// <see cref="ReadOnlyOutputTextDocument"/>, such that they can be viewed within the Node Tree.
    /// </summary>
    public class ReadOnlyOutputTextDocumentNodePresenter : TreeViewNodePresenterBase<ReadOnlyOutputTextDocument>
    {
        /// <summary>
        /// Updates the specified <paramref name="node"/> node for the corresponding <paramref name="nodeData"/>.
        /// </summary>
        /// <param name="parentNode"> This parameter is not used. </param>
        /// <param name="node"> The node.</param>
        /// <param name="nodeData"> The ReadOnlyOutputTextDocument. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="node"/> or <paramref name="nodeData"/> is <c>null</c>.
        /// </exception>
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, ReadOnlyOutputTextDocument nodeData)
        {
            Ensure.NotNull(node, nameof(node));
            Ensure.NotNull(nodeData, nameof(nodeData));

            node.Text = nodeData.Name;
            node.Tag = nodeData;
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