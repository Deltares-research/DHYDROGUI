using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    [Entity]
    public class RainfallRunoffBoundaryData : Unique<long>, ICloneable
    {
        private static readonly double DefaultValue = 0.0;

        public RainfallRunoffBoundaryData()
        {
            Data = new TimeSeries();
            var waterLevelVariable = new Variable<double>("Water level", new Unit("m AD", "m AD"))
                {DefaultValue = DefaultValue};
            Data.Components.Add(waterLevelVariable);
            IsConstant = true;
        }

        public virtual TimeSeries Data { get; set; }

        [NoNotifyPropertyChange]
        public bool IsTimeSeries
        {
            get { return !IsConstant; }
            set { IsConstant = !value; }
        }

        public bool IsConstant { get; set; }

        public double Value { get; set; }

        public double Evaluate(DateTime currentTime)
        {
            if (IsConstant)
                return Value;
            return Data.Evaluate<double>(currentTime);
        }

        public object Clone()
        {
            return new RainfallRunoffBoundaryData()
                {
                    IsConstant = IsConstant,
                    Value = Value,
                    Data = Data != null ? (TimeSeries) Data.Clone() : null
                };
        }
    }
}