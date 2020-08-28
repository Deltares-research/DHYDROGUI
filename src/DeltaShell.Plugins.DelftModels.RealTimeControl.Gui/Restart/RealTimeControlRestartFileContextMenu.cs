using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Restart
{
    /// <summary>
    /// Context menu for a <see cref="RealTimeControlRestartFile"/>.
    /// </summary>
    /// <seealso cref="MenuItemContextMenuStripAdapter"/>
    public class RealTimeControlRestartFileContextMenu : MenuItemContextMenuStripAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeControlRestartFileContextMenu"/> class.
        /// </summary>
        /// <param name="restartFile">The restart file.</param>
        /// <param name="node">The corresponding node.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="restartFile"/> or <paramref name="node"/> is <c>null</c>.
        /// </exception>
        public RealTimeControlRestartFileContextMenu(RealTimeControlRestartFile restartFile, ITreeNode node) : base(new ContextMenuStrip())
        {
            Ensure.NotNull(restartFile, nameof(restartFile));
            Ensure.NotNull(node, nameof(node));

            if (!TryGetModel(node, out RealTimeControlModel model))
            {
                return;
            }

            if (model.RestartInput == restartFile)
            {
                AddItemsForInputRealTimeControlRestartFile(model);
            }
            else if (model.RestartOutput.Contains(restartFile))
            {
                AddItemsForOutputRealTimeControlRestartFile(model, restartFile);
            }
        }

        private void AddItemsForInputRealTimeControlRestartFile(RealTimeControlModel model)
        {
            ContextMenuStrip.Items.Add(GetRemoveRestartMenuItem(model));
            ContextMenuStrip.Items.Add(GetUseLastValidRestartMenuItem(model));

            ContextMenuStrip.Items.Add(new ToolStripSeparator());
        }

        private void AddItemsForOutputRealTimeControlRestartFile(RealTimeControlModel model, RealTimeControlRestartFile restartFile)
        {
            ContextMenuStrip.Items.Add(GetUseAsRestartMenuItem(model, restartFile));
        }

        private static ClonableToolStripMenuItem GetUseAsRestartMenuItem(RealTimeControlModel model, RealTimeControlRestartFile restartFile)
        {
            var menuItem = new ClonableToolStripMenuItem {Text = Resources.UseAsRestart};
            menuItem.Click += (s, e) => model.RestartInput = restartFile.Clone();

            return menuItem;
        }

        private static ClonableToolStripMenuItem GetUseLastValidRestartMenuItem(RealTimeControlModel model)
        {
            var menuItem = new ClonableToolStripMenuItem
            {
                Text = Resources.UseLastRestart,
                Enabled = false
            };

            RealTimeControlRestartFile outputRealTimeControlRestartFile = model.RestartOutput.LastOrDefault();
            if (outputRealTimeControlRestartFile == null)
            {
                return menuItem;
            }

            menuItem.Enabled = true;
            menuItem.Click += (s, e) => model.RestartInput = outputRealTimeControlRestartFile.Clone();

            return menuItem;
        }

        private static ClonableToolStripMenuItem GetRemoveRestartMenuItem(RealTimeControlModel model)
        {
            var menuItem = new ClonableToolStripMenuItem
            {
                Text = Resources.RemoveRestart,
                Enabled = model.UseRestart,
            };
            menuItem.Click += (s, e) => model.RestartInput = new RealTimeControlRestartFile();

            return menuItem;
        }

        private static bool TryGetModel(ITreeNode node, out RealTimeControlModel result)
        {
            ITreeNode parent = node.Parent;
            while (parent != null)
            {
                if (parent.Tag is RealTimeControlModel model)
                {
                    result = model;
                    return true;
                }

                parent = parent.Parent;
            }

            result = null;
            return false;
        }
    }
}