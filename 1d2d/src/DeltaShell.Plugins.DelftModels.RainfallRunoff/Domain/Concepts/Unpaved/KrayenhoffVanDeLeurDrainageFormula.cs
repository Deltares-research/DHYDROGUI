using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DelftTools.Utils.Reflection;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved
{
    [Entity(FireOnCollectionChange = false)]
    public class KrayenhoffVanDeLeurDrainageFormula : Unique<long>, IDrainageFormula
    {
        public double ResevoirCoefficient { get; set; } //day

        #region IDrainageFormula Members

        public object Clone()
        {
            return TypeUtils.MemberwiseClone(this);
        }

        #endregion

        public override string ToString()
        {
            return "Krayenhoff Van De Leur";
        }
    }
}