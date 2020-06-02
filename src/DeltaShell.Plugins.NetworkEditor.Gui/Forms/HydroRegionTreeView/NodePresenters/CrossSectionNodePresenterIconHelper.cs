using System.Drawing;
using DelftTools.Hydro.CrossSections;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.HydroRegionTreeView.NodePresenters
{
    public static class CrossSectionNodePresenterIconHelper
    {
        private static readonly Image CrossSectionDefinitionXYZImage = Resources.CrossSectionSmallXYZ;
        private static readonly Image CrossSectionTabulatedSmallImage = Resources.CrossSectionTabulatedSmall;
        private static readonly Image CrossSectionStandardSmallImage = Resources.CrossSectionStandardSmall;
        private static readonly Image CrossSectionSmallImage = Resources.CrossSectionSmall;

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