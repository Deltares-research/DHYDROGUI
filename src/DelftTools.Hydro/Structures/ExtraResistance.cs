using System.ComponentModel;
using DelftTools.Functions;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro.Structures
{
    ///<summary>
    ///</summary>
    [Entity(FireOnCollectionChange = false)]
    public class ExtraResistance : BranchStructure, IExtraResistance
    {
        public ExtraResistance() : this("ER") {}

        public ExtraResistance(string name)
        {
            Name = name;
            FrictionTable = FunctionHelper.Get1DFunction<double, double>("Extra Resistance", "level", "resistance");
        }

        [DisplayName("Type")]
        [FeatureAttribute(Order = 5, ExportName = "Type")]
        public virtual ExtraResistanceType ExtraResistanceType { get; set; }

        public virtual IFunction FrictionTable { get; set; }

        public static ExtraResistance CreateDefault()
        {
            return new ExtraResistance();
        }

        public static ExtraResistance CreateDefault(IBranch branch)
        {
            ExtraResistance extraResistance = CreateDefault();
            AddStructureToNetwork(extraResistance, branch);
            return extraResistance;
        }

        public override void CopyFrom(object source)
        {
            base.CopyFrom(source);
            var sourceExtraResistance = (ExtraResistance) source;
            ExtraResistanceType = 0;
            FrictionTable =
                (IFunction) sourceExtraResistance.FrictionTable.Clone();
        }

        public override StructureType GetStructureType()
        {
            return StructureType.ExtraResistance;
        }
    }
}