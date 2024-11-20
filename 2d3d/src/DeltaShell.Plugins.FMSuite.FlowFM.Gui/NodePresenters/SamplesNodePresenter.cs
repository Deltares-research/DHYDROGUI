using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    /// <summary>
    /// Node presenter for <see cref="Samples"/>.
    /// </summary>
    public class SamplesNodePresenter : TreeViewNodePresenterBaseForPluginGui<Samples>
    {
        private readonly SamplesImageProvider imageProvider;

        /// <summary>
        /// Creates a new instance of <see cref="SamplesNodePresenter"/>.
        /// </summary>
        /// <param name="guiPlugin">The GUI plugin.</param>
        /// <param name="imageProvider">A samples image provider.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        public SamplesNodePresenter(GuiPlugin guiPlugin, SamplesImageProvider imageProvider)
            : base(guiPlugin)
        {
            Ensure.NotNull(guiPlugin, nameof(guiPlugin));
            Ensure.NotNull(imageProvider, nameof(imageProvider));

            this.imageProvider = imageProvider;
        }

        /// <summary>
        /// Updates the specified <paramref name="node"/> node for the corresponding <paramref name="nodeData"/>.
        /// </summary>
        /// <param name="parentNode">The parent node of the node to update.</param>
        /// <param name="node">The node to update.</param>
        /// <param name="nodeData">The node date to update the node with.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, Samples nodeData)
        {
            Ensure.NotNull(parentNode, nameof(parentNode));
            Ensure.NotNull(node, nameof(node));
            Ensure.NotNull(nodeData, nameof(nodeData));

            node.Text = nodeData.Name;
            node.Image = imageProvider.GetImage(nodeData);
        }

        /// <summary>
        /// Gets the context menu for the given node.
        /// </summary>
        /// <param name="sender">The node for which to get the context menu.</param>
        /// <param name="nodeData">The node data.</param>
        /// <returns>The context menu for the given node and node data.</returns>
        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            IMenuItem menuBase = base.GetContextMenu(sender, nodeData);
            IMenuItem menu = NodePresenterHelper.GetContextMenuFromPluginGuis(Gui, sender, nodeData);

            if (menuBase != null)
            {
                menu.Add(menuBase);
            }

            ContextMenuStrip additionalMenu = ContextMenuFactory.CreateMenuFor(nodeData, Gui, this, sender);
            menu.Add(new MenuItemContextMenuStripAdapter(additionalMenu));

            return menu;
        }
    }
}