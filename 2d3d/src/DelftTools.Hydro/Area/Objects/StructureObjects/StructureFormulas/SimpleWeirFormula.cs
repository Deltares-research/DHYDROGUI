using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;

namespace DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas
{
    /// <summary>
    /// <see cref="SimpleWeirFormula"/> implements the Simple Weir
    /// <see cref="IStructureFormula"/> defining a <see cref="DischargeCoefficient"/>
    /// and <see cref="LateralContraction"/>.
    /// </summary>
    [Entity(FireOnCollectionChange = false)]
    public class SimpleWeirFormula : Unique<long>, IStructureFormula
    {
        public SimpleWeirFormula()
        {
            Initialize();
        }

        /// <summary>
        /// Discharge coefficient Ce
        /// </summary>
        public virtual double DischargeCoefficient { get; set; }

        /// <summary>
        /// Lateral contraction Cw
        /// </summary>
        public virtual double LateralContraction { get; set; }

        public virtual object Clone()
        {
            return new SimpleWeirFormula
            {
                DischargeCoefficient = DischargeCoefficient,
                LateralContraction = LateralContraction
            };
        }

        private void Initialize()
        {
            DischargeCoefficient = 1.0;
            LateralContraction = 1.0;
        }

        public virtual string Name => "Simple Weir";
    }
}