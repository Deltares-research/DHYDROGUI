using System;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    /// <summary>
    /// Special case of the hydraulic rule were input is transformed to output by applying a factor.
    /// A common use is -1 for lateral sources in WaterFlowModel1D
    /// </summary>
    [Entity(FireOnCollectionChange=false)]
    public class FactorRule : HydraulicRule
    {
        public FactorRule() : base (RtcXmlTag.FactorRule)
        {
            Factor = -1.0; // default an invertor
        }

        private double factor;
        public virtual double Factor
        {
            get { return factor; }
            set
            {
                factor = value;
                UpdateLookupTable();
            }
        }

        [EditAction]
        private void UpdateLookupTable()
        {
            Interpolation = InterpolationType.Linear;
            Extrapolation = ExtrapolationType.Linear;

            const double range = 1.0;
            Function[-range] = -range * factor;
            Function[range] = range * factor;
        }

        public override object Clone()
        {
            var factorRule = (FactorRule)Activator.CreateInstance(GetType());
            factorRule.CopyFrom(this);
            return factorRule;
        }

        public override void CopyFrom(object source)
        {
            var factorRule = source as FactorRule;
            if (factorRule == null)
            {
                return;
            }
            base.CopyFrom(source);
            Factor = factorRule.Factor;
        }
    }
}
