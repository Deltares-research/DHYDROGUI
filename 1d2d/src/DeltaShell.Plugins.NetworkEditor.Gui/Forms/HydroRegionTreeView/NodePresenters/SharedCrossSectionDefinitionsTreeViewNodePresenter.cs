using System.Collections;
using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro.CrossSections;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public class SharedCrossSectionDefinitionsTreeViewNodePresenter : TreeViewNodePresenterBaseForPluginGui<IEventedList<ICrossSectionDefinition>>
    {
        private static readonly Image SharedCrossSectionDefinitionsImage= Properties.Resources.SharedCrossSectionDefinitions;

        public SharedCrossSectionDefinitionsTreeViewNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin)
        {
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IEventedList<ICrossSectionDefinition> nodeData)
        {
            node.Text = "Shared Cross Section Definitions";
            node.Image = SharedCrossSectionDefinitionsImage;
        }

        public override IEnumerable GetChildNodeObjects(IEventedList<ICrossSectionDefinition> parentNodeData, ITreeNode node)
        {
            foreach (var crossSectionSectionType in parentNodeData)
            {
                yield return crossSectionSectionType;
            }
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            if (GuiPlugin == null)
            {
                return null;
            }
            return GuiPlugin.GetContextMenu(sender, nodeData);
        }
    }
}