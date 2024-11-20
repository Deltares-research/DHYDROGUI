using System.Collections.Generic;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.SteerableProperties;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.SewerFeatures
{
    /// <summary>
    /// <see cref="Orifice"/> defines the 1D orifice structure which can be defined on  branch.
    /// </summary>
    [Entity]
    public class Orifice : Weir, IOrifice
    {
        /// <summary>
        /// Creates a new <see cref="Orifice"/> with a default name and no time dependency.
        /// </summary>
        public Orifice() : this(false) { }

        /// <summary>
        /// Creates a new <see cref="Orifice"/> with a default name.
        /// </summary>
        /// <param name="allowTimeVaryingData">Whether to allow time varying data</param>
        public Orifice(bool allowTimeVaryingData) : this("Orifice", allowTimeVaryingData) { }

        /// <summary>
        /// Creates a new <see cref="Orifice"/>.
        /// </summary>
        /// <param name="name">The name of the new orifice</param>
        /// <param name="allowTimeVaryingData">Whether to allow time varying data</param>
        public Orifice(string name, bool allowTimeVaryingData = false) : base(name, allowTimeVaryingData)
        {
            WeirFormula = new GatedWeirFormula(allowTimeVaryingData);
        }

        public double MaxDischarge { get; set; }

        protected override void CopyPropertyValuesToExistingWeir(IWeir weir)
        {
            if (!(weir is IOrifice orifice))
            {
                return;
            }

            orifice.CrestLevel = CrestLevel;
            orifice.MaxDischarge = MaxDischarge;

            if (!(WeirFormula is GatedWeirFormula sourceFormula) || 
                !(orifice.WeirFormula is GatedWeirFormula targetFormula))
            {
                return;
            }

            targetFormula.ContractionCoefficient = sourceFormula.ContractionCoefficient;
            targetFormula.MaxFlowNeg = sourceFormula.MaxFlowNeg;
            targetFormula.MaxFlowPos = sourceFormula.MaxFlowPos;
            targetFormula.UseMaxFlowNeg = sourceFormula.UseMaxFlowNeg;
            targetFormula.UseMaxFlowPos = sourceFormula.UseMaxFlowPos;
        }

        public override StructureType GetStructureType() => StructureType.Orifice;

        public override IEnumerable<SteerableProperty> RetrieveSteerableProperties()
        {
            foreach (SteerableProperty property in base.RetrieveSteerableProperties())
            {
                yield return property;
            }

            if (WeirFormula is GatedWeirFormula gatedFormula && gatedFormula.LowerEdgeLevelProperty != null)
                yield return gatedFormula.LowerEdgeLevelProperty;
        }
    }
}