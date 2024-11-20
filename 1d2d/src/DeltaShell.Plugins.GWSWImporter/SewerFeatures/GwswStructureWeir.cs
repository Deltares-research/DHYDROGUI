using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;

namespace DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures
{
    public class GwswStructureWeir : Weir
    {
        public GwswStructureWeir(string name) : base(name)
        {
        }
        
        protected override void CopyPropertyValuesToExistingWeir(IWeir weir)
        {
            weir.CrestWidth = CrestWidth;
            weir.CrestLevel = CrestLevel;
            weir.WeirFormula = new SimpleWeirFormula
            {
                CorrectionCoefficient = ((SimpleWeirFormula)WeirFormula).CorrectionCoefficient
            };
        }
    }
}
