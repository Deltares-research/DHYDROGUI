using System;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.IO.RestartFiles;

namespace DeltaShell.NGHS.Common.Gui.Restart
{
    /// <summary>
    /// Context menu for a <seealso cref="RestartFile"/>.
    /// </summary>
    /// <typeparam name="T">Type of the <seealso cref="IRestartModel"/></typeparam>
    /// <seealso cref="MenuItemContextMenuStripAdapter"/>
    public class RestartFileContextMenu<T> : MenuItemContextMenuStripAdapter where T : IRestartModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestartFileContextMenu{T}"/> class.
        /// </summary>
        /// <param name="restartFile">The restart file.</param>
        /// <param name="node">The corresponding node.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="restartFile"/> or <paramref name="node"/> is <c>null</c>.
        /// </exception>
        public RestartFileContextMenu(RestartFile restartFile, ITreeNode node) : base(new ContextMenuStrip())
        {
            Ensure.NotNull(restartFile, nameof(restartFile));
            Ensure.NotNull(node, nameof(node));

            if (!TryGetModel(node, out T model))
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

        private void AddItemsForInputRestartFile(T model)
        {
            ContextMenuStrip.Items.Add(GetRemoveRestartMenuItem(model));
            ContextMenuStrip.Items.Add(GetUseLastValidRestartMenuItem(model));
        }

        private void AddItemsForOutputRestartFile(T model, RestartFile restartFile)
        {
            ContextMenuStrip.Items.Add(GetUseAsRestartMenuItem(model, restartFile));
        }

        private static ClonableToolStripMenuItem GetUseAsRestartMenuItem(T model, RestartFile restartFile)
        {
            var menuItem = new ClonableToolStripMenuItem {Text = "Use as restart"};
            menuItem.Click += (s, e) => model.RestartInput = restartFile;

            return menuItem;
        }

        private static ClonableToolStripMenuItem GetUseLastValidRestartMenuItem(T model)
        {
            var menuItem = new ClonableToolStripMenuItem
            {
                Text = "Use last restart",
                Enabled = false
            };

            RestartFile outputRestartFile = model.RestartOutput.LastOrDefault();
            if (outputRestartFile == null)
            {
                return menuItem;
            }

            menuItem.Enabled = true;
            menuItem.Click += (s, e) => model.RestartInput = outputRestartFile;

            return menuItem;
        }

        private static ClonableToolStripMenuItem GetRemoveRestartMenuItem(T model)
        {
            var menuItem = new ClonableToolStripMenuItem
            {
                Text = "Remove restart",
                Enabled = model.UseRestart,
            };
            menuItem.Click += (s, e) => model.RestartInput = new RestartFile();

            return menuItem;
        }

        private static bool TryGetModel(ITreeNode node, out T result)
        {
            ITreeNode parent = node.Parent;
            while (parent != null)
            {
                if (parent.Tag is T model)
                {
                    result = model;
                    return true;
                }

                parent = parent.Parent;
            }

            result = default(T);
            return false;
        }
    }
}