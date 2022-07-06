using System;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.ProjectExplorer
{
    internal class FeatureProjectTreeViewNodePresenter<T> : TreeViewNodePresenterBaseForPluginGui<IEventedList<T>>
        where T : IFeature
    {
        private readonly string name;
        private readonly Image image;

        public FeatureProjectTreeViewNodePresenter(string name, Image image)
        {
            this.name = name;
            this.image = image;
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IEventedList<T> nodeData)
        {
            node.Text = name;
            node.Image = image;
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            var menuBase = base.GetContextMenu(sender, nodeData);
            var menu = NodePresenterHelper.GetContextMenuFromPluginGuis(Gui, sender, nodeData);
            if (menuBase != null)
                menu.Add(menuBase);

            var contextMenuStrip = new ContextMenuStrip();

            var openWithItem = new ClonableToolStripMenuItem
            {
                Text = Resources.FeatureProjectTreeViewNodePresenter_GetContextMenu_Open__With___,
                Tag = nodeData,
                Enabled = Gui.CommandHandler.CanOpenSelectViewDialog()
            };
            openWithItem.Click += OnOpenWithClicked;
            contextMenuStrip.Items.Add(openWithItem);

            contextMenuStrip.Items.Add(new ToolStripSeparator());

            var importItem = new ClonableToolStripMenuItem
            {
                Text = Resources.FeatureProjectTreeViewNodePresenter_GetContextMenu__Import___,
                Tag = nodeData,
                Image = FeatureProjectTreeViewNodePresenterNonGeneric.ImportImage
            };
            importItem.Click += OnImportClicked;
            contextMenuStrip.Items.Add(importItem);

            var exportItem = new ClonableToolStripMenuItem
            {
                Text = Resources.FeatureProjectTreeViewNodePresenter_GetContextMenu__Export___, 
                Tag = nodeData,
                Image = null
            };
            exportItem.Click += OnExportClicked;
            contextMenuStrip.Items.Add(exportItem);

            contextMenuStrip.Items.Add(new ToolStripSeparator());

            var propertiesItem = new ClonableToolStripMenuItem
            {
                Text = Resources.FeatureProjectTreeViewNodePresenter_GetContextMenu__Properties,
                Tag = nodeData,
                Image = null
            };
            propertiesItem.Click += OnPropertiesClicked;
            contextMenuStrip.Items.Add(propertiesItem);

            menu.Add(new MenuItemContextMenuStripAdapter(contextMenuStrip));

            return menu;
        }

        private void OnOpenWithClicked(object sender, EventArgs e)
        {
            var data = ((ToolStripMenuItem)sender).Tag;
            Gui.Selection = data;
            Gui.CommandHandler.OpenSelectViewDialog();
        }

        private void OnImportClicked(object sender, EventArgs e)
        {
            FeatureProjectTreeViewNodePresenterNonGeneric.SharedDataItem.Value = ((ToolStripMenuItem)sender).Tag;
            Gui.CommandHandler.ImportToDataItem(FeatureProjectTreeViewNodePresenterNonGeneric.SharedDataItem);
        }

        private void OnExportClicked(object sender, EventArgs e)
        {
            FeatureProjectTreeViewNodePresenterNonGeneric.SharedDataItem.Value = ((ToolStripMenuItem)sender).Tag;
            Gui.CommandHandler.ExportFromDataItem(FeatureProjectTreeViewNodePresenterNonGeneric.SharedDataItem);
        }

        private void OnPropertiesClicked(object sender, EventArgs e)
        {
            var data = ((ToolStripMenuItem)sender).Tag;
            Gui.Selection = data;
            Gui.CommandHandler.ShowProperties();
        }
    }


    internal static class FeatureProjectTreeViewNodePresenterNonGeneric
    {
        public static readonly DataItem SharedDataItem = new DataItem();
        public static readonly Bitmap ImportImage = Resources.import;
    }

}
