using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    internal static class WeirViewModelDesignerDataContext
    {
        public static WeirViewModel SimpleWeirViewModel = new WeirViewModel
        {
            Weir = new Weir2D("Weir 1")
            {
                CrestWidth = 100,
                WeirFormula = new SimpleWeirFormula()
            },
            SelectedWeirType = SelectableWeirFormulaType.SimpleWeir
        };

        public static WeirViewModel GeneralStructurWeirViewModel = new WeirViewModel
        {
            Weir = new Weir2D("Weir 1")
            {
                CrestWidth = 100,
                CrestLevel = 20,
                WeirFormula = new GeneralStructureWeirFormula()
            },
            SelectedWeirType = SelectableWeirFormulaType.GeneralStructure
        };
    }
}