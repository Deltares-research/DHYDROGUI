using DelftTools.Hydro.CrossSections;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.StandardCrossSections
{
    public class CrossSectionDefinitionStandardViewModel
    {
        public CrossSectionDefinitionStandard Definition { get; set; }
        public double LevelShift
        {
            get { return Definition.LevelShift; }
        }
        public bool IsOnChannel { get; set; }
    }
}