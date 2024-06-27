using System.IO;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui.Properties;
using DeltaShell.NGHS.Common.IO.RestartFiles;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Restart
{
    /// <summary>
    /// Context menu for a <see cref="RealTimeControlRestartFile"/>.
    /// </summary>
    /// <seealso cref="MenuItemContextMenuStripAdapter"/>
    /// <remarks>
    /// This class can be removed once the input restart file of the <see cref="RealTimeControlModel"/> is FileBased;
    /// instead, the <see cref="NGHS.Common.Gui.Restart.RestartFileContextMenu"/> should be used.
    /// </remarks>
    public class RealTimeControlInputRestartFileContextMenu : MenuItemContextMenuStripAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeControlInputRestartFileContextMenu"/> class.
        /// </summary>
        /// <param name="restartFile">The restart file.</param>
        /// <param name="node">The corresponding node.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="restartFile"/> or <paramref name="node"/> is <c>null</c>.
        /// </exception>
        public RealTimeControlInputRestartFileContextMenu(RealTimeControlRestartFile restartFile, ITreeNode node) : base(new ContextMenuStrip())
        {
            Ensure.NotNull(restartFile, nameof(restartFile));
            Ensure.NotNull(node, nameof(node));

            if (!TryGetModel(node, out IRealTimeControlModel model))
            {
                return;
            }

            if (model.RestartInput == restartFile)
            {
                AddItemsForInputRealTimeControlRestartFile(model);
            }
        }

        private void AddItemsForInputRealTimeControlRestartFile(IRealTimeControlModel model)
        {
            ContextMenuStrip.Items.Add(GetRemoveRestartMenuItem(model));
            ContextMenuStrip.Items.Add(GetUseLastValidRestartMenuItem(model));

            ContextMenuStrip.Items.Add(new ToolStripSeparator());
        }

        private static ClonableToolStripMenuItem GetUseLastValidRestartMenuItem(IRealTimeControlModel model)
        {
            var menuItem = new ClonableToolStripMenuItem
            {
                Text = Resources.UseLastRestart,
                Enabled = false
            };

            RestartFile outputRealTimeControlRestartFile = model.RestartOutput.LastOrDefault();
            if (outputRealTimeControlRestartFile == null)
            {
                return menuItem;
            }

            menuItem.Enabled = true;
            menuItem.Click += (s, e) =>
            {
                model.RestartInput = new RealTimeControlRestartFile(Path.GetFileName(outputRealTimeControlRestartFile.Path),
                                                                    File.ReadAllText(outputRealTimeControlRestartFile.Path));
                model.MarkOutputOutOfSync();
            };

            return menuItem;
        }

        private static ClonableToolStripMenuItem GetRemoveRestartMenuItem(IRealTimeControlModel model)
        {
            var menuItem = new ClonableToolStripMenuItem
            {
                Text = Resources.RemoveRestart,
                Enabled = !model.RestartInput.IsEmpty
            };
            menuItem.Click += (s, e) =>
            {
                model.RestartInput = new RealTimeControlRestartFile();
                model.MarkOutputOutOfSync();
            };

            return menuItem;
        }

        private static bool TryGetModel(ITreeNode node, out IRealTimeControlModel result)
        {
            ITreeNode parent = node.Parent;
            while (parent != null)
            {
                if (parent.Tag is IRealTimeControlModel model)
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