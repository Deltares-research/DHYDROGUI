using System.ComponentModel;
using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro.CrossSections;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    internal class CrossSectionSectionTypeTreeViewNodePresenter :
        TreeViewNodePresenterBaseForPluginGui<CrossSectionSectionType>
    {
        private static readonly Image CrossSectionSectionTypeImage = Properties.Resources.CrossSectionSectionType;

        public CrossSectionSectionTypeTreeViewNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin)
        {
        }

        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }

        protected override bool CanRemove(CrossSectionSectionType nodeData)
        {
            return true;
        }

        protected override bool RemoveNodeData(object parentNodeData, CrossSectionSectionType nodeData)
        {
            var crossSectionSectionTypes =  (IEventedList<CrossSectionSectionType>) TreeView.SelectedNode.Parent.Tag;
            if (crossSectionSectionTypes != null)
            {
                crossSectionSectionTypes.Remove(nodeData);
            }

            return true;
        }

        public override void OnNodeRenamed(CrossSectionSectionType sectionType, string newName)
        {
            if (sectionType.Name != newName)
            {
                sectionType.Name = newName;
            }
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, CrossSectionSectionType sectionType)
        {
            node.Text = sectionType.Name;
            node.Image = CrossSectionSectionTypeImage;
        }

        protected override void OnPropertyChanged(CrossSectionSectionType item, ITreeNode node ,PropertyChangedEventArgs e)
        {
            if (node == null) return;

            if (e.PropertyName.Equals("Name"))
            {
                node.Text = item.Name;
            }
        }
    }
}