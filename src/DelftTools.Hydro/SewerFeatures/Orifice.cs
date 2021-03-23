using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.SewerFeatures
{
    [Entity]
    public class Orifice : Weir, IOrifice
    {
        public Orifice() : this(false)
        {
            
        }

        public Orifice(bool allowTimeVaryingData) : this("Orifice", allowTimeVaryingData) { }

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

            var targetFormula = (GatedWeirFormula) orifice.WeirFormula;
            var sourceFormula = (GatedWeirFormula) WeirFormula;

            targetFormula.ContractionCoefficient = sourceFormula.ContractionCoefficient;
            targetFormula.MaxFlowNeg = sourceFormula.MaxFlowNeg;
            targetFormula.MaxFlowPos = sourceFormula.MaxFlowPos;
            orifice.MaxDischarge = MaxDischarge;
        }

        public override StructureType GetStructureType()
        {
            return StructureType.Orifice;
        }
    }
}