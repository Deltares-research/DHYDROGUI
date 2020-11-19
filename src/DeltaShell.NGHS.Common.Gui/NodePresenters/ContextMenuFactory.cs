using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Gui;
using DeltaShell.NGHS.Common.Gui.Properties;

namespace DeltaShell.NGHS.Common.Gui.NodePresenters
{
    /// <summary>
    /// <see cref="ContextMenuFactory"/> is responsible for building context
    /// menus given data, and a corresponding node presenter.
    /// </summary>
    public static class ContextMenuFactory
    {
        private static readonly Bitmap import = Resources.import;
        private static readonly Bitmap properties = Resources.properties;

        /// <summary>
        /// Generates a context menu for the provided <paramref name="data"/>.
        /// </summary>
        /// <param name="data"> Data to generate a menu for. </param>
        /// <param name="gui"> The gui (needed for calling commands) </param>
        /// <param name="nodePresenter">
        /// The node presenter for the <paramref name="data"/> object.
        /// </param>
        /// <param name="node">
        /// Tree node for the <paramref name="data"/> object.
        /// </param>
        /// <returns> A <see cref="ContextMenuStrip"/> object with defined functionality. </returns>
        public static ContextMenuStrip CreateMenuFor(object data, IGui gui, ITreeNodePresenter nodePresenter, ITreeNode node)
        {
            var menu = new ContextMenuStrip();

            if (HasOpenWithItem(data, gui))
            {
                menu.Items.Add(GetOpenWithItem(data, gui));
                menu.Items.Add(new ToolStripSeparator());
            }

            var addToolStripSeparator = false;
            if (HasDeleteItem(nodePresenter, node))
            {
                menu.Items.Add(GetDeleteItem(data, nodePresenter, node));
                addToolStripSeparator = true;
            }

            if (HasRenameItem(nodePresenter, node))
            {
                menu.Items.Add(GetRenameItem(data, node));
                addToolStripSeparator = true;
            }

            if (addToolStripSeparator)
            {
                menu.Items.Add(new ToolStripSeparator());
                addToolStripSeparator = false;
            }

            if (HasImportItem(data, gui))
            {
                menu.Items.Add(GetImportItem(data, gui));
                addToolStripSeparator = true;
            }

            if (HasExportItem(data, gui))
            {
                menu.Items.Add(GetExportItem(data, gui));
                addToolStripSeparator = true;
            }

            if (addToolStripSeparator)
            {
                menu.Items.Add(new ToolStripSeparator());
            }

            menu.Items.Add(GetPropertiesItem(data, gui));

            return menu;
        }

        private static bool HasOpenWithItem(object data, IGui gui) =>
            gui.CommandHandler.CanOpenSelectViewDialog()
            && gui.DocumentViewsResolver.GetViewInfosFor(data).Count() > 1;

        private static ToolStripItem GetOpenWithItem(object data, IGui gui)
        {
            var openWithItem = new ClonableToolStripMenuItem
            {
                Text = Resources.ContextMenuFactory_Open_with,
                Tag = data,
                Enabled = true,
            };

            openWithItem.Click += (s, a) =>
            {
                gui.Selection = ((ToolStripMenuItem) s).Tag;
                gui.CommandHandler.OpenSelectViewDialog();
            };

            return openWithItem;
        }

        private static bool HasRenameItem(ITreeNodePresenter nodePresenter, ITreeNode node) =>
            nodePresenter.CanRenameNode(node);

        private static ToolStripItem GetRenameItem(object data, ITreeNode node)
        {
            var renameItem = new ClonableToolStripMenuItem
            {
                Text = Resources.ContextMenuFactory_Rename,
                Tag = data,
                Enabled = true
            };
            renameItem.Click += (s, e) => node.TreeView.StartLabelEdit();

            return renameItem;
        }

        private static bool HasDeleteItem(ITreeNodePresenter nodePresenter, ITreeNode node) =>
            node != null && nodePresenter.CanRemove(null, node.Tag);

        private static ToolStripItem GetDeleteItem(object data,
                                                   ITreeNodePresenter nodePresenter,
                                                   ITreeNode node)
        {
            var deleteItem = new ClonableToolStripMenuItem
            {
                Text = Resources.ContextMenuFactory_Delete,
                Tag = data,
                Enabled = true,
                Image = Resources.DeleteHS
            };
            deleteItem.Click += (s, e) => nodePresenter.RemoveNodeData(node.Parent.Tag, data);

            return deleteItem;
        }

        private static bool HasImportItem(object data, IGui gui)
        {
            return gui.CommandHandler.CanImportOn(data);
        }

        private static ToolStripItem GetImportItem(object data, IGui gui)
        {
            var importItem = new ClonableToolStripMenuItem
            {
                Text = Resources.ContextMenuFactory_Import,
                Tag = data,
                Image = import,
                Enabled = true
            };
            importItem.Click += (s, a) => gui.CommandHandler.ImportOn(data);

            return importItem;
        }

        private static bool HasExportItem(object data, IGui gui)
        {
            return gui.CommandHandler.CanExportFrom(data);
        }

        private static ToolStripItem GetExportItem(object data, IGui gui)
        {
            var exportItem = new ClonableToolStripMenuItem
            {
                Text = Resources.ContextMenuFactory_Export,
                Tag = data,
                Enabled = true
            };
            exportItem.Click += (s, a) =>
            {
                gui.CommandHandler.ExportFrom(data);
            };

            return exportItem;
        }

        private static ToolStripItem GetPropertiesItem(object data, IGui gui)
        {
            var propertiesItem = new ClonableToolStripMenuItem
            {
                Text = Resources.ContextMenuFactory_Properties,
                Tag = data,
                Image = properties
            };

            propertiesItem.Click += (s, a) =>
            {
                gui.Selection = data;
                gui.CommandHandler.ShowProperties();
            };

            return propertiesItem;
        }
    }
}