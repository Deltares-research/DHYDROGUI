using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;

namespace DelftTools.Hydro.SewerFeatures
{
    public class GwswStructureWeir : Weir
    {
        public GwswStructureWeir(string name) : base(name)
        {
        }
        
        protected override ISewerConnection GetNewSewerConnectionWithWeir()
        {
            var sewerConnection = new SewerConnection(Name);
            sewerConnection.AddStructureToBranch(this);
            return sewerConnection;
        }

        protected override void CopyPropertyValuesToExistingWeir(IWeir weir)
        {
            weir.CrestWidth = CrestWidth;
            weir.CrestLevel = CrestLevel;
            weir.WeirFormula = new SimpleWeirFormula
            {
                DischargeCoefficient = ((SimpleWeirFormula)WeirFormula).DischargeCoefficient
            };
        }
    }
}
