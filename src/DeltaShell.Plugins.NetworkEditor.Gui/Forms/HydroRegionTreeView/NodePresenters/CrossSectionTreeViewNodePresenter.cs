using System.Drawing;
using System.Globalization;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Editing;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public class CrossSectionTreeViewNodePresenter : BranchFeatureTreeViewNodePresenterBase<ICrossSection>
    {
        public CrossSectionTreeViewNodePresenter(GuiPlugin guiPlugin)
            : base(guiPlugin)
        {
        }

        #region ITreeNodePresenter Members

        protected override bool CanRemove(ICrossSection nodeData)
        {
            return true;
        }

        protected override Image GetImage(ICrossSection crossSection)
        {
            return CrossSectionNodePresenterIconHelper.GetIcon(crossSection.CrossSectionType);
        }

        protected override string GetText(ICrossSection crossSection)
        {
            var chainageString = crossSection.Chainage.ToString("F", CultureInfo.InvariantCulture);

            return string.IsNullOrEmpty(crossSection.Name)
               ? string.Format("<no name>: {0}", chainageString)
               : (crossSection.Definition.IsProxy)
                     ? string.Format("{0}({1}): {2}", crossSection.Name,
                                     ((CrossSectionDefinitionProxy)crossSection.Definition).InnerDefinition
                                         .Name, chainageString)
                     : string.Format("{0}: {1}", crossSection.Name, chainageString);
        }

        protected override bool RemoveNodeData(object parentNodeData, ICrossSection crossSection)
        {
            //remove the cross section from both branch and networkschematization.
            var network = crossSection.Network;

            network.BeginEdit("Delete feature " + crossSection.Name);
            var channel = (IChannel) crossSection.Branch;
            channel.BranchFeatures.Remove(crossSection);
            network.EndEdit();

            return true;
        }

        /// <summary>
        /// Cross sections can be renamed by the user
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }


        #endregion
    }
}