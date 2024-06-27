using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui.NodePresenters;
using DeltaShell.NGHS.Common.Gui.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Restart
{
    /// <summary>
    /// Node presenter for the project tree for a <seealso cref="TreeViewNodePresenterBaseForPluginGui{T}"/>
    /// </summary>
    /// <seealso cref="RealTimeControlRestartFile"/>
    /// <remarks>
    /// This class can be removed once the input restart file of the <see cref="RealTimeControlModel"/> is FileBased;
    /// instead, the <see cref="NGHS.Common.Gui.Restart.RestartFileNodePresenter"/> should be used.
    /// </remarks>
    public sealed class RealTimeControlInputRestartFileNodePresenter : TreeViewNodePresenterBaseForPluginGui<RealTimeControlRestartFile>
    {
        private static readonly Image restartIcon = Resources.restart;
        private static readonly Image emptyRestartIcon = Resources.restart_empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeControlInputRestartFileNodePresenter"/> class.
        /// </summary>
        /// <param name="guiPlugin">The GUI plugin.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="guiPlugin"/> is <c>null</c>.
        /// </exception>
        public RealTimeControlInputRestartFileNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin)
        {
            Ensure.NotNull(guiPlugin, nameof(guiPlugin));
        }

        /// <summary>
        /// Updates the specified <paramref name="node"/> node for the corresponding <paramref name="nodeData"/>.
        /// </summary>
        /// <param name="parentNode">This parameter is not used. </param>
        /// <param name="node">The node.</param>
        /// <param name="nodeData">The restart file as node data. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="node"/> or <paramref name="nodeData"/> is <c>null</c>.
        /// </exception>
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, RealTimeControlRestartFile nodeData)
        {
            Ensure.NotNull(node, nameof(node));
            Ensure.NotNull(nodeData, nameof(nodeData));

            if (nodeData.IsEmpty)
            {
                UpdateEmptyRestartNode(node);
            }
            else
            {
                UpdateRestartNode(node, nodeData);
            }
        }

        /// <summary>
        /// Gets the context menu.
        /// </summary>
        /// <param name="sender">The node for which to get the context menu.</param>
        /// <param name="nodeData">The node data.</param>
        /// <returns>The context menu for a <see cref="RealTimeControlRestartFile"/></returns>
        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            IMenuItem menuBase = base.GetContextMenu(sender, nodeData);
            IMenuItem menu = NodePresenterHelper.GetContextMenuFromPluginGuis(Gui, sender, nodeData);

            if (menuBase != null)
            {
                menu.Add(menuBase);
            }

            menu.Add(new RealTimeControlInputRestartFileContextMenu((RealTimeControlRestartFile) nodeData, sender));

            ContextMenuStrip contextMenu = ContextMenuFactory.CreateMenuFor(nodeData, Gui, this, sender);
            menu.Add(new MenuItemContextMenuStripAdapter(contextMenu));
            return menu;
        }

        private static void UpdateRestartNode(ITreeNode node, RealTimeControlRestartFile nodeData)
        {
            node.Text = nodeData.Name;
            node.Image = restartIcon;
        }

        private static void UpdateEmptyRestartNode(ITreeNode node)
        {
            node.Text = Resources.RestartFileNodePresenter_Restart_empty;
            node.Image = emptyRestartIcon;
        }
    }
}