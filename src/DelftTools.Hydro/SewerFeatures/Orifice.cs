using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Aop;
using log4net;

namespace DelftTools.Hydro.SewerFeatures
{
    [Entity]
    public class Orifice : Weir, IOrifice
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Orifice));

        public Orifice() : this("Orifice")
        {
            
        }

        public Orifice(string name) : base(name)
        {
            WeirFormula = new GatedWeirFormula();
        }

        public double BottomLevel { get; set; }
        public double MaxDischarge { get; set; }

        protected override void CopyPropertyValuesToExistingWeir(IWeir weir)
        {
            var orifice = weir as IOrifice;
            if(orifice == null) return;

            orifice.BottomLevel = BottomLevel;
            ((GatedWeirFormula)orifice.WeirFormula).ContractionCoefficient = ((GatedWeirFormula)WeirFormula).ContractionCoefficient;
            orifice.MaxDischarge = MaxDischarge;
        }

        public override StructureType GetStructureType()
        {
            return StructureType.Orifice;
        }
    }
}