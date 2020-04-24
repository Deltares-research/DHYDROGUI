using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
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

        [FeatureAttribute]
        public virtual RetentionType Type { get; set; }

        [DisplayName("Storage area")]
        [FeatureAttribute]
        public virtual double StorageArea { get; set; }

        [DisplayName("Street storage area")]
        [FeatureAttribute(ExportName = "StreetStore")]
        public virtual double StreetStorageArea { get; set; }

        [DisplayName("Bed level")]
        [FeatureAttribute]
        public virtual double BedLevel { get; set; }

        public virtual double LevelBL { get; set; }

        [DisplayName("Street level")]
        [FeatureAttribute(ExportName = "StreetLvl")]
        public virtual double StreetLevel { get; set; }
    
        public override void CopyFrom(object source)
        {
            base.CopyFrom(source);
            Type = ((Retention) source).Type;
            StorageArea = ((Retention) source).StorageArea;
            StreetStorageArea = ((Retention)source).StreetStorageArea;
            BedLevel = ((Retention) source).BedLevel;
            LevelBL = ((Retention) source).LevelBL;
            StreetLevel = ((Retention)source).StreetLevel;
            Data = (IFunction) ((Retention)source).Data.Clone(true);
        }

        public virtual IFunction Data { get; set; }

        public virtual IHydroNetwork HydroNetwork
        {
            get { return Network as IHydroNetwork; }
        }

        [DisplayName("Long name")]
        [FeatureAttribute(Order = 2)]
        public virtual string LongName { get; set; }

        public virtual bool UseTable { get; set; }

        public virtual IEnumerable<object> GetDirectChildren()
        {
            if (Data != null)
                yield return Data;
        }

        [DisplayName("Chainage")]
        [FeatureAttribute(Order = 3)]
        public override double Chainage { get => base.Chainage; set => base.Chainage = value; }
    }
}