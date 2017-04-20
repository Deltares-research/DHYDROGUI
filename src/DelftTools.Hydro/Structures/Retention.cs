using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Aop.NotifyPropertyChange;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Networks;

namespace DelftTools.Hydro.Structures
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
    [NotifyPropertyChange(EnableLogging = false)]
    public class Retention : BranchFeature, IRetention
    {
        public Retention()
        {
            Type = RetentionType.Reservoir;
            Data = FunctionHelper.Get1DFunction<double, double>("Storage", "Height", "Storage");
            Data.Arguments[0].InterpolationType = InterpolationType.Linear;
        }

        [FeatureAttribute]
        public RetentionType Type { get; set; }

        [FeatureAttribute]
        public double StorageArea { get; set; }

        [FeatureAttribute]
        public double StreetStorageArea { get; set; }

        [FeatureAttribute]
        public double BedLevel { get; set; }

        [FeatureAttribute]
        public double LevelBL { get; set; }
        
        [FeatureAttribute]
        public double StreetLevel { get; set; }
    
        public override void CopyFrom(object source)
        {
            base.CopyFrom(source);
            Attributes = (IFeatureAttributeCollection) ((Retention) source).Attributes.Clone();
            Type = ((Retention) source).Type;
            StorageArea = ((Retention) source).StorageArea;
            StreetStorageArea = ((Retention)source).StreetStorageArea;
            BedLevel = ((Retention) source).BedLevel;
            LevelBL = ((Retention) source).LevelBL;
            StreetLevel = ((Retention)source).StreetLevel;
            Data = (IFunction) ((Retention)source).Data.Clone(true);
        }

        [NoNotifyPropertyChange]
        public IFunction Data { get; set; }

        public IHydroNetwork HydroNetwork
        {
            get { return Network as IHydroNetwork; }
        }

        [FeatureAttribute(DisplayName = "Name")]
        public string LongName { get; set; }

        public bool UseTable { get; set; }
        
    }
}