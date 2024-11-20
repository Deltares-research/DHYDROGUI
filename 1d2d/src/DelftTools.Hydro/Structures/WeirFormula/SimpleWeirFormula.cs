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
            CorrectionCoefficient = 1.0;
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
        /// Correction coefficient Cgf, Cgd, Cwf, Cwd
        /// </summary>
        public virtual double CorrectionCoefficient { get; set; }

        public virtual bool UseVelocityHeight { get; set; }

        public virtual object Clone()
        {
            return new SimpleWeirFormula
                {
                    CorrectionCoefficient = CorrectionCoefficient,
                    UseVelocityHeight = UseVelocityHeight
                };
        }
    }
}