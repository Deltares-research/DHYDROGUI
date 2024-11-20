using System.IO;
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
    /// Context menu for a <see cref="RestartFile"/>.
    /// </summary>
    /// <seealso cref="MenuItemContextMenuStripAdapter"/>
    /// <remarks>
    /// This class can be removed once the input restart file of the <see cref="RealTimeControlModel"/> is FileBased;
    /// instead, the <see cref="NGHS.Common.Gui.Restart.RestartFileContextMenu"/> should be used.
    /// </remarks>
    public class RealTimeControlOutputRestartFileContextMenu : MenuItemContextMenuStripAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NGHS.Common.Gui.Restart.RestartFileContextMenu"/> class.
        /// </summary>
        /// <param name="restartFile">The restart file.</param>
        /// <param name="node">The corresponding node.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="restartFile"/> or <paramref name="node"/> is <c>null</c>.
        /// </exception>
        public RealTimeControlOutputRestartFileContextMenu(RestartFile restartFile, ITreeNode node) : base(new ContextMenuStrip())
        {
            Ensure.NotNull(restartFile, nameof(restartFile));
            Ensure.NotNull(node, nameof(node));

            if (!TryGetModel(node, out IRealTimeControlModel model))
            {
                return;
            }

            if (model.RestartOutput.Contains(restartFile))
            {
                AddItemsForOutputRestartFile(model, restartFile);
            }
        }

        private void AddItemsForOutputRestartFile(IRealTimeControlModel model, RestartFile restartFile)
        {
            ContextMenuStrip.Items.Add(GetUseAsRestartMenuItem(model, restartFile));
        }

        private static ClonableToolStripMenuItem GetUseAsRestartMenuItem(IRealTimeControlModel model, RestartFile restartFile)
        {
            var menuItem = new ClonableToolStripMenuItem {Text = Resources.UseAsRestart};
            menuItem.Click += (s, e) =>
            {
                model.RestartInput = new RealTimeControlRestartFile(Path.GetFileName(restartFile.Path),
                                                                    File.ReadAllText(restartFile.Path));
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