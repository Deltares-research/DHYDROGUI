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
            //contextMenu.Items.Add(new ToolStripSeparator());
            //BuildConceptSwitchContextMenu(contextMenu);

            return new MenuItemContextMenuStripAdapter(contextMenu);
        }

        private void OnImportClicked(object sender, EventArgs args)
        {
            var folder = (TreeFolder)((ToolStripItem)sender).Tag;
            var model = (RainfallRunoffModel)folder.Parent;
            GuiPlugin.Gui.Selection = model;
            GuiPlugin.Gui.CommandHandler.ImportToGuiSelection();
        }

//        private void BuildConceptSwitchContextMenu(ContextMenuStrip contextMenu)
//        {
//            var subItem = new ToolStripMenuItem("Switch Unschematized to");
//            contextMenu.Items.Add(subItem);
//
//            IEnumerable<RainfallRunoffConceptsEnum> concepts =
//                Enum.GetValues(typeof (RainfallRunoffConceptsEnum)).OfType<RainfallRunoffConceptsEnum>().Where(
//                    v => v != RainfallRunoffConceptsEnum.NotSchematized);
//
//            TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof (RainfallRunoffConceptsEnum));
//                //use nice string if available
//
//            foreach (RainfallRunoffConceptsEnum concept in concepts)
//            {
//                string conceptName = typeConverter.ConvertToString(concept);
//                var toolStripButton = new ToolStripMenuItem(conceptName, null, OnConceptSwitchClicked) {Tag = concept};
//                subItem.DropDownItems.Add(toolStripButton);
//            }
//        }
//        private void OnConceptSwitchClicked(object sender, EventArgs args)
//        {
//            var targetConcept = (RainfallRunoffConceptsEnum) ((ToolStripItem) sender).Tag;
//            object selection = GuiPlugin.Gui.Selection;
//
//            var folder = selection as CatchmentModelDataTreeFolder;
//            if (folder != null)
//            {
//                throw new NotImplementedException("todo");
//            }
//        }
    }
}