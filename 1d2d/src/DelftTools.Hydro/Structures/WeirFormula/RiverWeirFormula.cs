using DelftTools.Functions;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;

namespace DelftTools.Hydro.Structures.WeirFormula
{
    /// <summary>
    /// Weir with detailed description of crest (River weir)
    /// </summary>
    [Entity(FireOnCollectionChange=false)]
    public class RiverWeirFormula : Unique<long>, IWeirFormula
    {
        public RiverWeirFormula()
        {
            SubmergeReductionPos = CrestBroadSubmergeReduction;
            SubmergeReductionNeg = CrestBroadSubmergeReduction;
            CorrectionCoefficientNeg = 1.2;
            CorrectionCoefficientPos = 1.2;
            SubmergeLimitNeg = 0.01;
            SubmergeLimitPos = 0.01;
        }

        /// <summary>
        /// Creates a RiverWeirFormula with default values as found in Sobek.
        /// <returns></returns>
        public static RiverWeirFormula CreateDefault()
        {
            var riverWeirFormula = new RiverWeirFormula
                       {
                           CorrectionCoefficientNeg = 1,
                           CorrectionCoefficientPos = 1,
                           SubmergeLimitNeg = 0.82,
                           SubmergeLimitPos = 0.82
                       };
            riverWeirFormula.SubmergeReductionPos = CrestBroadSubmergeReduction;
            riverWeirFormula.SubmergeReductionNeg = CrestBroadSubmergeReduction;
            return riverWeirFormula;
        }


        public virtual string Name
        {
            get { return "Weir with detailed description of crest (River weir)"; }
        }

        /// <summary>
        /// Correction coefficient flow direction(pos_cwcoef)
        /// </summary>
        public virtual double CorrectionCoefficientPos { get; set; }
        
        /// <summary>
        /// Correction coefficient reverse direction(neg_cwcoef)
        /// </summary>
        public virtual double CorrectionCoefficientNeg { get; set; }

        /// <summary>
        /// Submerge-Reduction table positive flow.
        /// S R where column S is (h2-z)/(h1-z) and column R is reduction
        /// </summary>
        public virtual IFunction SubmergeReductionPos { get; set; }

        /// <summary>
        /// Submerge-Reduction table negative flow.
        /// S R where column S is (h2-z)/(h1-z) and column R is reduction
        /// </summary>
        public virtual IFunction SubmergeReductionNeg { get; set; }

        /// <summary>
        /// Submerge coefficient flow direction(pos_slimlimit)
        /// </summary>
        public virtual double SubmergeLimitPos { get; set; }

        /// <summary>
        /// Submerge coefficient reverse direction(neg_slimlimit)
        /// </summary>
        public virtual double SubmergeLimitNeg { get; set; }
        
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

        public static IFunction CrestBroadSubmergeReduction
        {
            get
            {
                var submergeReduction = FunctionHelper.Get1DFunction<double, double>("submergereduction", "S", "R");
                submergeReduction[0.82] = 1.0;
                submergeReduction[0.86] = 0.95;
                submergeReduction[0.9] = 0.9;
                submergeReduction[0.94] = 0.8;
                submergeReduction[0.96] = 0.7;
                submergeReduction[0.97] = 0.6;
                submergeReduction[1.0] = 0.0;
                return submergeReduction;
            }
        }

        public static IFunction CrestRoundSubmergeReduction
        {
            get
            {
                var submergeReduction = FunctionHelper.Get1DFunction<double, double>("submergereduction", "S", "R");
                submergeReduction[0.3] = 1.0;
                submergeReduction[0.61] = 0.95;
                submergeReduction[0.77] = 0.9;
                submergeReduction[0.8] = 0.85;
                submergeReduction[0.83] = 0.8;
                submergeReduction[0.87] = 0.7;
                submergeReduction[0.9] = 0.6;
                submergeReduction[1.0] = 0.0;
                return submergeReduction;
            }
        }

        public static IFunction CrestSharpSubmergeReduction
        {
            get
            {
                var submergeReduction = FunctionHelper.Get1DFunction<double, double>("submergereduction", "S", "R");
                submergeReduction[0.01] = 1.0;
                submergeReduction[0.27] = 0.9;
                submergeReduction[0.48] = 0.8;
                submergeReduction[0.65] = 0.7;
                submergeReduction[0.77] = 0.6;
                submergeReduction[0.86] = 0.5;
                submergeReduction[0.93] = 0.4;
                submergeReduction[1.00] = 0.0;
                return submergeReduction;
            }
        }

        public static IFunction CrestTriangularSubmergeReduction
        {
            get
            {
                var submergeReduction = FunctionHelper.Get1DFunction<double, double>("submergereduction", "S", "R");
                submergeReduction[0.67] = 1.0;
                submergeReduction[0.80] = 0.95;
                submergeReduction[0.85] = 0.9;
                submergeReduction[0.90] = 0.8;
                submergeReduction[0.92] = 0.7;
                submergeReduction[0.94] = 0.6;
                submergeReduction[0.95] = 0.5;
                submergeReduction[1.00] = 0.0;
                return submergeReduction;
            }
        }

        public virtual object Clone()
        {
            var clone = new RiverWeirFormula();
            clone.CorrectionCoefficientNeg = CorrectionCoefficientNeg;
            clone.CorrectionCoefficientPos = CorrectionCoefficientPos;
            clone.SubmergeLimitNeg = SubmergeLimitNeg;
            clone.SubmergeLimitPos = SubmergeLimitPos;
            clone.SubmergeReductionNeg = (IFunction)SubmergeReductionNeg.Clone(true);
            clone.SubmergeReductionPos = (IFunction)SubmergeReductionPos.Clone(true);
            return clone;
        }
    }
}