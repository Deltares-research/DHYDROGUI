using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;

namespace DelftTools.Hydro.Structures.WeirFormula
{
    ///<summary>
    /// Class to manage properties specific for the Sobek Simple Weir
    ///</summary>
    [Entity(FireOnCollectionChange=false)]
    public class SimpleWeirFormula : Unique<long>, IWeirFormula
    {
        public SimpleWeirFormula()
        {
            Initialize();
        }

        private void Initialize()
        {
            DischargeCoefficient = 1.0;
            LateralContraction = 1.0;
        }

        #region IWeirFormula Members

        public virtual string Name { get { return "Simple weir (Weir)"; } }

        public virtual bool IsRectangle
        {
            get { return true; }
        }

        public virtual bool IsGated
        {
            get { return false; }
        }

        public virtual bool HasFlowDirection
        {
            get { return true; }
        }

        #endregion
        
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
    }
}