using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui.Properties;
using DeltaShell.NGHS.Common.Restart;

namespace DeltaShell.NGHS.Common.Gui.Restart
{
    /// <summary>
    /// Context menu for classes implementing <seealso cref="IRestartFile"/>.
    /// </summary>
    /// <seealso cref="MenuItemContextMenuStripAdapter"/>
    public class RestartFileContextMenu<TRestartFile>: MenuItemContextMenuStripAdapter where TRestartFile: class, IRestartFile, new() 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestartFileContextMenu{TRestartFile}"/> class.
        /// </summary>
        /// <param name="restartFile">The restart file.</param>
        /// <param name="node">The corresponding node.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="restartFile"/> or <paramref name="node"/> is <c>null</c>.
        /// </exception>
        public RestartFileContextMenu(TRestartFile restartFile, ITreeNode node) : base(new ContextMenuStrip())
        {
            Ensure.NotNull(restartFile, nameof(restartFile));
            Ensure.NotNull(node, nameof(node));

            if (!TryGetModel(node, out IRestartModel<TRestartFile> model))
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

        private void AddItemsForInputRestartFile(IRestartModel<TRestartFile> model)
        {
            ContextMenuStrip.Items.Add(GetRemoveRestartMenuItem(model));
            ContextMenuStrip.Items.Add(GetUseLastValidRestartMenuItem(model));

            ContextMenuStrip.Items.Add(new ToolStripSeparator());
        }

        private void AddItemsForOutputRestartFile(IRestartModel<TRestartFile> model, TRestartFile restartFile)
        {
            ContextMenuStrip.Items.Add(GetUseAsRestartMenuItem(model, restartFile));
        }

        private static ClonableToolStripMenuItem GetUseAsRestartMenuItem(IRestartModel<TRestartFile> model, TRestartFile restartFile)
        {
            var menuItem = new ClonableToolStripMenuItem {Text = Resources.UseAsRestart};
            menuItem.Click += (s, e) =>
            {
                model.SetRestartInputToDuplicateOf(restartFile);
                if (model is ITimeDependentModel timeDependentModel)
                {
                    timeDependentModel.MarkOutputOutOfSync();
                }
            };

            return menuItem;
        }

        private static ClonableToolStripMenuItem GetUseLastValidRestartMenuItem(IRestartModel<TRestartFile> model)
        {
            var menuItem = new ClonableToolStripMenuItem
            {
                Text = Resources.UseLastRestart,
                Enabled = false
            };

            TRestartFile lastRestartFile = model.RestartOutput.LastOrDefault();
            if (lastRestartFile == null)
            {
                return menuItem;
            }

            menuItem.Enabled = true;
            menuItem.Click += (s, e) =>
            {
                model.SetRestartInputToDuplicateOf(lastRestartFile);
                if (model is ITimeDependentModel timeDependentModel)
                {
                    timeDependentModel.MarkOutputOutOfSync();
                }
            };

            return menuItem;
        }

        private static ClonableToolStripMenuItem GetRemoveRestartMenuItem(IRestartModel<TRestartFile> model)
        {
            var menuItem = new ClonableToolStripMenuItem
            {
                Text = Resources.RemoveRestart,
                Enabled = model.UseRestart
            };
            menuItem.Click += (s, e) =>
            {
                model.RestartInput = new TRestartFile();
                if (model is ITimeDependentModel timeDependentModel)
                {
                    timeDependentModel.MarkOutputOutOfSync();
                }
            };

            return menuItem;
        }

        private static bool TryGetModel(ITreeNode node, out IRestartModel<TRestartFile> result)
        {
            ITreeNode parent = node.Parent;
            while (parent != null)
            {
                if (parent.Tag is IRestartModel<TRestartFile> model)
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