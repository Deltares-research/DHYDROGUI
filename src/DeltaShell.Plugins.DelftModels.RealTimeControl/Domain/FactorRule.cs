using DelftTools.Functions.Generic;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    /// <summary>
    /// Special case of the hydraulic rule were input is transformed to output by applying a factor.
    /// A common use is -1 for lateral sources in WaterFlowModel1D
    /// </summary>
    [Entity(FireOnCollectionChange = false)]
    public class FactorRule : HydraulicRule
    {
        private double factor;

        public FactorRule()
        {
            Factor = -1.0; // default an invertor
        }

        public virtual double Factor
        {
            get
            {
                return factor;
            }
            set
            {
                factor = value;
                UpdateLookupTable();
            }
        }

        public override object Clone()
        {
            var factorRule = new FactorRule();
            factorRule.CopyFrom(this);
            return factorRule;
        }

        public override void CopyFrom(object source)
        {
            var factorRule = source as FactorRule;
            if (factorRule != null)
            {
                base.CopyFrom(source);
                Factor = factorRule.Factor;
            }
        }

        private void UpdateLookupTable()
        {
            Interpolation = InterpolationType.Linear;
            Extrapolation = ExtrapolationType.Linear;

            const double range = 1.0;
            Function[-range] = -range * factor;
            Function[range] = range * factor;
        }
    }
}