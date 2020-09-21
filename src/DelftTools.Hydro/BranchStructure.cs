using System.ComponentModel;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Networks;

namespace DelftTools.Hydro
{
    [Entity(FireOnCollectionChange = false)]
    public abstract class BranchStructure : BranchFeature, IStructure1D
    {
        [NoNotifyPropertyChange]
        public virtual double OffsetY { get; set; }

        public virtual string Description { get; set; }

        [DisplayName("Long name")]
        [FeatureAttribute(Order = 1)]
        public virtual string LongName { get; set; }

        public virtual int CompareTo(BranchStructure other)
        {
            return CompareTo((IBranchFeature) other);
        }

        public override void CopyFrom(object source)
        {
            base.CopyFrom(source);
            OffsetY = ((BranchStructure) source).OffsetY;
            LongName = ((BranchStructure) source).LongName;
        }

        public override int CompareTo(object obj)
        {
            var other = (BranchStructure) obj;
            return CompareTo(other);
        }

        public IHydroRegion Region { get; }
    }
}