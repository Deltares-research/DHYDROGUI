using System.Collections;
using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Drawing;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters
{
    /// <summary>
    /// <see cref="OutputTreeFolderNodePresenter"/> implements the NodePresenter for
    /// <see cref="OutputTreeFolder"/>, such that they can be viewed within the Node Tree.
    /// </summary>
    public class OutputTreeFolderNodePresenter : TreeViewNodePresenterBaseForPluginGui<OutputTreeFolder>
    {
        /// <summary>
        /// Updates the specified <paramref name="node"/> node for the corresponding <paramref name="data"/>.
        /// </summary>
        /// <param name="parentNode"> This parameter is not used. </param>
        /// <param name="node"> The node.</param>
        /// <param name="data"> The OutputTreeFolder.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="node"/> or <paramref name="data"/> is <c>null</c>.
        /// </exception>
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, OutputTreeFolder data)
        {
            Ensure.NotNull(node, nameof(node));
            Ensure.NotNull(data, nameof(data));

            node.Text = data.Text;
            node.Tag = data;
            node.Image = GetImage(data);
        }

        /// <summary>
        /// Returns the children.
        /// </summary>
        /// <param name="parentNodeData"> The OutputTreeFolder.</param>
        /// <param name="node"> The node.</param>
        /// <returns> The children. </returns>
        public override IEnumerable GetChildNodeObjects(OutputTreeFolder parentNodeData, ITreeNode node)
        {
            return parentNodeData.ChildItems;
        }

        private static Image GetImage(OutputTreeFolder data)
        {
            Image image = data.Image;

            if (data.Parent.OutputOutOfSync)
            {
                image = image.AddOverlayImage(Resources.ExclamationOverlay, 5, 1, 10, 10);
            }

            return image;
        }
    }
}