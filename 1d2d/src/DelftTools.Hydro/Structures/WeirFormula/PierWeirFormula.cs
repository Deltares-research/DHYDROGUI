using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;

namespace DelftTools.Hydro.Structures.WeirFormula
{
    /// <summary>
    /// Weir with piers (Advanced weir)
    /// </summary>
    [Entity(FireOnCollectionChange=false)]
    public class PierWeirFormula : Unique<long>, IWeirFormula
    {
        public virtual string Name
        {
            get { return "Weir with piers (Advanced weir)"; }
        }

        /// <summary>
        /// Number of piers N (npiers)
        /// </summary>
        public virtual int NumberOfPiers{ get; set;}

        /// <summary>
        /// Upstream face flow direction P (pos_height)
        /// </summary>
        public virtual double UpstreamFacePos { get; set; }

        /// <summary>
        /// Upstream face reverse direction P (neg_height)
        /// </summary>
        public virtual double UpstreamFaceNeg { get; set; }

        /// <summary>
        /// Design head of weir flow H0 (pos_designhead)
        /// </summary>
        public virtual double DesignHeadPos { get; set; }

        /// <summary>
        /// Design head of weir reverse H0 (neg_designhead)
        /// </summary>
        public virtual double DesignHeadNeg { get; set; }

        /// <summary>
        /// Pier contraction coefficient Kp flow direction (pos_piercontractcoef)
        /// </summary>
        public virtual double PierContractionPos { get; set; }

        /// <summary>
        /// Pier contraction coefficient Kp reverse direction (neg_piercontractcoef)
        /// </summary>
        public virtual double PierContractionNeg { get; set; }

        /// <summary>
        /// Abutment contraction coefficient flow direction Ka (pos_abutcontractcoef)
        /// </summary>
        public virtual double AbutmentContractionPos { get; set; }

        /// <summary>
        /// Abutment contraction coefficient reverse direction Ka (neg_abutcontractcoef)
        /// </summary>
        public virtual double AbutmentContractionNeg { get; set; }
        
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
            get { return false; }
        }

        /// <summary>
        /// Creates a PierWeirFormula with default values as found in Sobek.
        /// <returns></returns>
        public static PierWeirFormula CreateDefault()
        {
            return new PierWeirFormula
                {
                    AbutmentContractionNeg = 0.1,
                    AbutmentContractionPos = 0.1,
                    DesignHeadNeg = 3,
                    DesignHeadPos = 3,
                    NumberOfPiers = 0,
                    PierContractionNeg = 0.01,
                    PierContractionPos = 0.01,
                    UpstreamFaceNeg = 10,
                    UpstreamFacePos = 10
                };
        }

        public virtual object Clone()
        {
            var clone = new PierWeirFormula();
            clone.AbutmentContractionNeg = AbutmentContractionNeg;
            clone.AbutmentContractionPos = AbutmentContractionPos;
            clone.DesignHeadNeg = DesignHeadNeg;
            clone.DesignHeadPos = DesignHeadPos;
            clone.NumberOfPiers = NumberOfPiers;
            clone.PierContractionNeg = PierContractionNeg;
            clone.PierContractionPos = PierContractionPos;
            clone.UpstreamFaceNeg = UpstreamFaceNeg;
            clone.UpstreamFacePos = UpstreamFacePos;
            return clone;
        }
    }
}