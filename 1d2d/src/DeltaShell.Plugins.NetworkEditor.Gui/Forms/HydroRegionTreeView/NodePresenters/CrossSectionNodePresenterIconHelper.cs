using System.Drawing;
using DelftTools.Hydro.CrossSections;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public static class CrossSectionNodePresenterIconHelper
    {
        private static readonly Image CrossSectionDefinitionXYZImage = Properties.Resources.CrossSectionSmallXYZ;
        private static readonly Image CrossSectionTabulatedSmallImage = Properties.Resources.CrossSectionTabulatedSmall;
        private static readonly Image CrossSectionStandardSmallImage = Properties.Resources.CrossSectionStandardSmall;
        private static readonly Image CrossSectionSmallImage = Properties.Resources.CrossSectionSmall;

        public static Image GetIcon(CrossSectionType type)
        {
            switch (type)
            {
                case CrossSectionType.GeometryBased:
                    return CrossSectionDefinitionXYZImage;
                case CrossSectionType.ZW:
                    return CrossSectionTabulatedSmallImage;
                case CrossSectionType.Standard:
                    return CrossSectionStandardSmallImage;
                default:
                    return CrossSectionSmallImage;
            }
        }
    }
}
