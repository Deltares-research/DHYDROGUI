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
            if (weir is IOrifice orifice)
            {
                orifice.CrestLevel = CrestLevel;
                orifice.MaxDischarge = MaxDischarge;
                if (WeirFormula is GatedWeirFormula sourceFormula && orifice.WeirFormula is GatedWeirFormula targetFormula)
                {
                    targetFormula.ContractionCoefficient = sourceFormula.ContractionCoefficient;
                    targetFormula.MaxFlowNeg = sourceFormula.MaxFlowNeg;
                    targetFormula.MaxFlowPos = sourceFormula.MaxFlowPos;
                }
            }
        }

        public override StructureType GetStructureType()
        {
            return StructureType.Orifice;
        }
    }
}