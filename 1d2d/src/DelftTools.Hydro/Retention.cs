using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.ComponentModel;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro
{
    public enum RetentionType
    {
        Reservoir = 2,
        Closed  = 3 ,
        Loss = 4
    }


    ///<summary>
    /// Implements a retention area.
    ///</summary>
    [Entity(FireOnCollectionChange=false)]
    public class Retention : BranchFeatureHydroObject, IRetention, IItemContainer
    {
        public Retention()
        {
            Type = RetentionType.Reservoir;
            Data = FunctionHelper.Get1DFunction<double, double>("Storage", "Height", "Storage");
            Data.Arguments[0].InterpolationType = InterpolationType.Linear;
        }

        [DisplayName("Long name")]
        [FeatureAttribute(Order = 2)]
        public virtual string LongName { get; set; }

        [DisplayName("Chainage")]
        [FeatureAttribute(Order = 3)]
        public override double Chainage { get => base.Chainage; set => base.Chainage = value; }

        [FeatureAttribute(Order = 4)]
        public virtual RetentionType Type { get; set; }

        [DisplayName("Storage area")]
        [FeatureAttribute(Order = 5)]
        [DynamicReadOnly]
        public virtual double StorageArea { get; set; }
        
        [DisplayName("Bed level")]
        [DynamicReadOnly]
        [FeatureAttribute(Order = 6)]
        public virtual double BedLevel { get; set; }

        [DisplayName("Street storage area")]
        [DynamicReadOnly]
        public virtual double StreetStorageArea { get; set; }
        
        [DisplayName("Street level")]
        [DynamicReadOnly]
        public virtual double StreetLevel { get; set; }
    
        public override void CopyFrom(object source)
        {
            base.CopyFrom(source);
            Type = ((Retention) source).Type;
            StorageArea = ((Retention) source).StorageArea;
            StreetStorageArea = ((Retention)source).StreetStorageArea;
            BedLevel = ((Retention) source).BedLevel;
            StreetLevel = ((Retention)source).StreetLevel;
            Data = (IFunction) ((Retention)source).Data.Clone(true);
        }

        /// <summary>
        /// Whether or not to use a storage table.
        /// </summary>
        [FeatureAttribute(Order = 9)]
        [DisplayName("Use a storage table")]
        public virtual bool UseTable { get; set; }

        /// <summary>
        /// The storage table in this retention.
        /// </summary>
        [FeatureAttribute(Order = 10)]
        [DynamicReadOnly]
        [DisplayName("Storage table")]
        public virtual IFunction Data { get; set; }
        
        [Description("Interpolate")]
        [FeatureAttribute(Order = 11)]
        [DynamicReadOnly]
        public virtual InterpolationType InterpolationType
        {
            get { return Data.Arguments[0].InterpolationType; }
            set { Data.Arguments[0].InterpolationType = value; }
        }
        public virtual IHydroNetwork HydroNetwork
        {
            get { return Network as IHydroNetwork; }
        }

        public virtual IEnumerable<object> GetDirectChildren()
        {
            if (Data != null)
                yield return Data;
        }
        
        [DynamicReadOnlyValidationMethod]
        public virtual bool IsFieldReadOnly(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(StorageArea):
                case nameof(StreetLevel):
                case nameof(StreetStorageArea):
                case nameof(BedLevel):
                    return UseTable;
                case nameof(Data):
                case nameof(InterpolationType):
                    return !UseTable;
                default:
                    return true;
            }
        }
    }
}