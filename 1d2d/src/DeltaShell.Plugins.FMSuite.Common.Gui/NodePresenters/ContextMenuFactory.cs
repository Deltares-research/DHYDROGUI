using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters
{
    internal static class ContextMenuFactory
    {
        private static readonly Bitmap Import = Resources.import;
        private static readonly Bitmap Properties = Resources.properties;

        /// <summary>
        /// Generates a context menu for the provided <param name="data"/>
        /// </summary>
        /// <param name="data">Data to generate a menu for</param>
        /// <param name="gui">The gui (needed for calling commands)</param>
        /// <param name="nodePresenter">The nodepresenter for the <param name="data"/> object</param>
        /// <param name="node">Tree node for the <param name="data"/> object</param>
        /// <returns>Contextmenu</returns>
        public static ContextMenuStrip CreateMenuFor(object data, IGui gui, ITreeNodePresenter nodePresenter, ITreeNode node)
        {
            var menu = new ContextMenuStrip();
            
            var openWithItem = new ClonableToolStripMenuItem
            {
                Text = Resources.FMSuiteNodePresenterBase_GetContextMenu_Open__With___,
                Tag = data,
                Enabled = gui.CommandHandler.CanOpenSelectViewDialog()
            };
            
            openWithItem.Click += (s, a) =>
            {
                gui.Selection = ((ToolStripMenuItem)s).Tag;
                gui.CommandHandler.OpenSelectViewDialog();
            };

            menu.Items.Add(openWithItem);
            menu.Items.Add(new ToolStripSeparator());

            bool addToolStripSeparator = false;
            if (node != null && nodePresenter.CanRemove(null, node.Tag))
            {
                var deleteItem = new ClonableToolStripMenuItem { Text = "Delete", Tag = data, Enabled = true, Image = Resources.DeleteHS };
                deleteItem.Click += (s, e) => nodePresenter.RemoveNodeData(node.Parent.Tag, data);
                menu.Items.Add(deleteItem);
                addToolStripSeparator = true;
            }

            if (nodePresenter.CanRenameNode(node))
            {
                var renameItem = new ClonableToolStripMenuItem { Text = "Rename", Tag = data, Enabled = true };
                renameItem.Click += (s, e) => node.TreeView.StartLabelEdit();
                menu.Items.Add(renameItem);
                addToolStripSeparator = true;
            }

            if (addToolStripSeparator)
            {
                menu.Items.Add(new ToolStripSeparator());
            }

            var importItem = new ClonableToolStripMenuItem
            {
                Text = Resources.FMSuiteNodePresenterBase_GetContextMenu__Import___,
                Tag = data,
                Image = Import
            };
            importItem.Click += (s, a) => gui.CommandHandler.ImportOn(data);

            menu.Items.Add(importItem);

            var exportItem = new ClonableToolStripMenuItem
            {
                Text = Resources.FMSuiteNodePresenterBase_GetContextMenu__Export___,
                Tag = data
            };
            exportItem.Click += (s, a) =>
            {
                gui.CommandHandler.ExportFrom(data);
            };
            menu.Items.Add(exportItem);

            menu.Items.Add(new ToolStripSeparator());

            var propertiesItem = new ClonableToolStripMenuItem
            {
                Text = Resources.FMSuiteNodePresenterBase_GetContextMenu__Properties,
                Tag = data,
                Image = Properties
            };

            propertiesItem.Click += (s,a)=>
            {
                gui.Selection = data;
                gui.CommandHandler.ShowProperties();
            };

            menu.Items.Add(propertiesItem);

            return menu;
        }
    }
}