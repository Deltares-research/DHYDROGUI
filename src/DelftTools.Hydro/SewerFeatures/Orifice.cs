using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.SewerFeatures
{
    [Entity]
    public class Orifice : Weir, IOrifice
    {
        public Orifice() : this("Orifice")
        {
            
        }
        public Orifice(bool allowTimeVaryingData = false) : this("Orifice", allowTimeVaryingData) { }
        public Orifice(string name, bool allowTimeVaryingData = false) : base(name, allowTimeVaryingData)
        {
            WeirFormula = new GatedWeirFormula();
        }

        public double MaxDischarge { get; set; }

        protected override void CopyPropertyValuesToExistingWeir(IWeir weir)
        {
            var orifice = weir as IOrifice;
            if(orifice == null) return;

            orifice.CrestLevel = CrestLevel;
            ((GatedWeirFormula)orifice.WeirFormula).ContractionCoefficient = ((GatedWeirFormula)WeirFormula).ContractionCoefficient;
            orifice.MaxDischarge = MaxDischarge;
        }

        public override StructureType GetStructureType()
        {
            return StructureType.Orifice;
        }
    }
}