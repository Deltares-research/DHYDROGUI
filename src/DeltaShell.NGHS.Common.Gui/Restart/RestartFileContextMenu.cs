using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui.Properties;
using DeltaShell.NGHS.Common.IO.RestartFiles;

namespace DeltaShell.NGHS.Common.Gui.Restart
{
    /// <summary>
    /// Context menu for a <seealso cref="RestartFile"/>.
    /// </summary>
    /// <seealso cref="MenuItemContextMenuStripAdapter"/>
    public class RestartFileContextMenu : MenuItemContextMenuStripAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestartFileContextMenu"/> class.
        /// </summary>
        /// <param name="restartFile">The restart file.</param>
        /// <param name="node">The corresponding node.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="restartFile"/> or <paramref name="node"/> is <c>null</c>.
        /// </exception>
        public RestartFileContextMenu(RestartFile restartFile, ITreeNode node) : base(new ContextMenuStrip())
        {
            Ensure.NotNull(restartFile, nameof(restartFile));
            Ensure.NotNull(node, nameof(node));

            if (!TryGetModel(node, out ITimeDependentRestartModel model))
            {
                return;
            }

            if (model.RestartInput == restartFile)
            {
                AddItemsForInputRestartFile(model);
            }
            else if (model.RestartOutput.Contains(restartFile))
            {
                AddItemsForOutputRestartFile(model, restartFile);
            }
        }

        private void AddItemsForInputRestartFile(ITimeDependentRestartModel model)
        {
            ContextMenuStrip.Items.Add(GetRemoveRestartMenuItem(model));
            ContextMenuStrip.Items.Add(GetUseLastValidRestartMenuItem(model));

            ContextMenuStrip.Items.Add(new ToolStripSeparator());
        }

        private void AddItemsForOutputRestartFile(ITimeDependentRestartModel model, RestartFile restartFile)
        {
            ContextMenuStrip.Items.Add(GetUseAsRestartMenuItem(model, restartFile));
        }

        private static ClonableToolStripMenuItem GetUseAsRestartMenuItem(ITimeDependentRestartModel model, RestartFile restartFile)
        {
            var menuItem = new ClonableToolStripMenuItem {Text = Resources.UseAsRestart};
            menuItem.Click += (s, e) =>
            {
                model.RestartInput = restartFile.Clone();
                model.MarkOutputOutOfSync();
            };

            return menuItem;
        }

        private static ClonableToolStripMenuItem GetUseLastValidRestartMenuItem(ITimeDependentRestartModel model)
        {
            var menuItem = new ClonableToolStripMenuItem
            {
                Text = Resources.UseLastRestart,
                Enabled = false
            };

            RestartFile outputRestartFile = model.RestartOutput.LastOrDefault();
            if (outputRestartFile == null)
            {
                return menuItem;
            }

            menuItem.Enabled = true;
            menuItem.Click += (s, e) =>
            {
                model.RestartInput = outputRestartFile.Clone();
                model.MarkOutputOutOfSync();
            };

            return menuItem;
        }

        private static ClonableToolStripMenuItem GetRemoveRestartMenuItem(ITimeDependentRestartModel model)
        {
            var menuItem = new ClonableToolStripMenuItem
            {
                Text = Resources.RemoveRestart,
                Enabled = model.UseRestart
            };
            menuItem.Click += (s, e) =>
            {
                model.RestartInput = new RestartFile();
                model.MarkOutputOutOfSync();
            };

            return menuItem;
        }

        private static bool TryGetModel(ITreeNode node, out ITimeDependentRestartModel result)
        {
            ITreeNode parent = node.Parent;
            while (parent != null)
            {
                if (parent.Tag is ITimeDependentRestartModel model)
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