using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters
{
    public abstract class FMSuiteNodePresenterBase<T> : TreeViewNodePresenterBaseForPluginGui<T> where T : class 
    {
        private Image lastImageForEmptyData;
        private Image emptyDataImage;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, T nodeData)
        {
            CheckNodeImageResourceLeak(nodeData);

            node.Text = GetNodeText(nodeData);
            var image = GetNodeImage(nodeData);
            if (IsDataEmpty())
            {
                if (!ReferenceEquals(image, lastImageForEmptyData)) // create new empty image
                {
                    lastImageForEmptyData = image;
                    if (emptyDataImage != null)
                        emptyDataImage.Dispose();
                    emptyDataImage = CreateEmptyDataImage(image);
                }
                image = emptyDataImage;
            }
            node.Image = image;
        }

        private static Image CreateEmptyDataImage(Image image)
        {
            var semiTransparentyImage = new Bitmap(image);
            using (var graphics = Graphics.FromImage(semiTransparentyImage))
            {
                graphics.Clear(Color.Transparent);
                GraphicsUtils.DrawImageTransparent(graphics, image, 0.5f);
            }
            return semiTransparentyImage;
        }

        protected abstract string GetNodeText(T data);
        protected abstract Image GetNodeImage(T data);
        
        protected virtual bool IsDataEmpty()
        {
            return false;
        }

        protected virtual object GetContextMenuData(T data)
        {
            return data;
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            var menuBase = base.GetContextMenu(sender, nodeData);
            var menu = NodePresenterHelper.GetContextMenuFromPluginGuis(Gui, sender, nodeData);
            if (menuBase != null)
                menu.Add(menuBase);

            var data = GetContextMenuData((T)nodeData);

            var menu1 = new ContextMenuStrip();

            var openWithItem = new ClonableToolStripMenuItem
            {
                Text = Resources.FMSuiteNodePresenterBase_GetContextMenu_Open__With___,
                Tag = data,
                Enabled = Gui.CommandHandler.CanOpenSelectViewDialog()
            };
            openWithItem.Click += OnOpenWithClicked;
            menu1.Items.Add(openWithItem);

            menu1.Items.Add(new ToolStripSeparator());

            bool addToolStripSeparator = false;
            if (CanRemove(nodeData as T))
            {
                var deleteItem = new ClonableToolStripMenuItem {Text = "Delete", Tag = data, Enabled = true, Image = Resources.DeleteHS};
                deleteItem.Click += (s, e) => RemoveNodeData(sender.Parent.Tag, data);
                menu1.Items.Add(deleteItem);
                addToolStripSeparator = true;
            }

            if (CanRenameNode(sender))
            {
                var renameItem = new ClonableToolStripMenuItem {Text = "Rename", Tag = data, Enabled = true};
                renameItem.Click += (s, e) => sender.TreeView.StartLabelEdit();
                menu1.Items.Add(renameItem);
                addToolStripSeparator = true;
            }

            if (addToolStripSeparator)
            {
                menu1.Items.Add(new ToolStripSeparator());                
            }

            var importItem = new ClonableToolStripMenuItem
            {
                Text = Resources.FMSuiteNodePresenterBase_GetContextMenu__Import___,
                Tag = data,
                Image = FMSuiteNodePresenterBaseNonGeneric.Import
            };
            importItem.Click += OnImportClicked;
            menu1.Items.Add(importItem);

            var exportItem = new ClonableToolStripMenuItem
            {
                Text = Resources.FMSuiteNodePresenterBase_GetContextMenu__Export___, 
                Tag = data
            };
            exportItem.Click += OnExportClicked;
            menu1.Items.Add(exportItem);

            menu1.Items.Add(new ToolStripSeparator());

            var propertiesItem = new ClonableToolStripMenuItem
            {
                Text = Resources.FMSuiteNodePresenterBase_GetContextMenu__Properties,
                Tag = data,
                Image = FMSuiteNodePresenterBaseNonGeneric.Properties
            };
            propertiesItem.Click += OnPropertiesClicked;
            menu1.Items.Add(propertiesItem);

            menu.Add(new MenuItemContextMenuStripAdapter(menu1));

            return menu;
        }

        private void OnOpenWithClicked(object sender, EventArgs e)
        {
            var data = ((ToolStripMenuItem) sender).Tag;
            Gui.Selection = data;
            Gui.CommandHandler.OpenSelectViewDialog();
        }
        
        private void OnImportClicked(object sender, EventArgs e)
        {
            FMSuiteNodePresenterBaseNonGeneric.SharedDataItem.Value = ((ToolStripMenuItem) sender).Tag;
            Gui.CommandHandler.ImportToDataItem(FMSuiteNodePresenterBaseNonGeneric.SharedDataItem);
        }
        
        private void OnExportClicked(object sender, EventArgs e)
        {
            FMSuiteNodePresenterBaseNonGeneric.SharedDataItem.Value = ((ToolStripMenuItem)sender).Tag;
            Gui.CommandHandler.ExportFromDataItem(FMSuiteNodePresenterBaseNonGeneric.SharedDataItem);
        }

        private void OnPropertiesClicked(object sender, EventArgs e)
        {
            var data = ((ToolStripMenuItem)sender).Tag;
            Gui.Selection = data;
            Gui.CommandHandler.ShowProperties();
        }

        private bool firstUpdate = true;

        [Conditional("DEBUG")]
        private void CheckNodeImageResourceLeak(T data)
        {
            if (!firstUpdate)
                return;

            firstUpdate = false;
            var image1 = GetNodeImage(data);
            var image2 = GetNodeImage(data);
            if (!ReferenceEquals(image1, image2))
                throw new InvalidOperationException(
                    string.Format("Apparent resource leak in {0}.GetNodeImage(): a new bitmap is returned each time, please reuse the bitmap",
                        GetType().Name));
        }
    }

    internal static class FMSuiteNodePresenterBaseNonGeneric
    {
        public static readonly DataItem SharedDataItem = new DataItem();
        public static readonly Bitmap Import = Resources.import;
        public static readonly Bitmap Properties = Resources.properties;
    }
}