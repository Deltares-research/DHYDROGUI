using System;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.ProjectExplorer.NodePresenters;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.NodePresenters
{
    public class CatchmentModelDataTreeFolderProjectNodePresenter : TreeFolderNodePresenter
    {
        public CatchmentModelDataTreeFolderProjectNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin)
        {
        }

        public override Type NodeTagType
        {
            get { return typeof (CatchmentModelDataTreeFolder); }
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            var contextMenu = new ContextMenuStrip();

            var importButton = new ToolStripMenuItem("Import...", null, OnImportClicked) {Tag = nodeData};
            contextMenu.Items.Add(importButton);
            
            return new MenuItemContextMenuStripAdapter(contextMenu);
        }

        private void OnImportClicked(object sender, EventArgs args)
        {
            var folder = (TreeFolder)((ToolStripItem)sender).Tag;
            var model = (RainfallRunoffModel)folder.Parent;
            GuiPlugin.Gui.Selection = model;
            GuiPlugin.Gui.CommandHandler.ImportToGuiSelection();
        }
    }
}