using System.Collections;
using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro.CrossSections;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    internal class  CrossSectionSectionTypesTreeViewNodePresenter :
        TreeViewNodePresenterBaseForPluginGui<IEventedList<CrossSectionSectionType>>
    {
        private static readonly Image CrossSectionSectionTypesImage = Properties.Resources.CrossSectionSectionTypes;

        public CrossSectionSectionTypesTreeViewNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin)
        {
        }
       
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IEventedList<CrossSectionSectionType> nodeData)
        {
            node.Text = "Sections (roughness)";
            node.Image = CrossSectionSectionTypesImage;
        }

        public override IEnumerable GetChildNodeObjects(IEventedList<CrossSectionSectionType> parentNodeData, ITreeNode node)
        {
            foreach (var crossSectionSectionType in parentNodeData)
            {
                yield return crossSectionSectionType;
            }
        }
    }
}