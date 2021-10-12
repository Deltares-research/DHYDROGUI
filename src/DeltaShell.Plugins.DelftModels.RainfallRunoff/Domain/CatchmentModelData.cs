using System;
using DelftTools.Hydro;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DelftTools.Utils.Reflection;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain
{
    [Entity]
    public abstract class CatchmentModelData : Unique<long>, INameable, ICloneable, ICatchmentSettable
    {
        protected CatchmentModelData(Catchment catchment)
        {
            Catchment = catchment;
            AreaAdjustmentFactor = 1.0;
            MeteoStationName = "";
        }
        
        [Aggregation]
        public Catchment Catchment { get; protected set; }
        
        public virtual double CalculationArea { get; set; }
        
        public string LongName { get { return Catchment.LongName; } }
        
        public string Name { get { return Catchment.Name; } set { Catchment.Name = value; } }
        
        public string MeteoStationName { get; set; }

        public string TemperatureStationName { get; set; }
        
        public double AreaAdjustmentFactor { get; set; }

        public virtual object Clone()
        {
            var clone = TypeUtils.MemberwiseClone(this); //copies members of subclasses
            clone.Catchment = Catchment; //aggregation, so no cloning
            clone.CalculationArea = CalculationArea;
            return clone;
        }

        Catchment ICatchmentSettable.CatchmentSetter { set { Catchment = value; } }
    }

    internal interface ICatchmentSettable
    {
        Catchment CatchmentSetter { set; }
    }
}