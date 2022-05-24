using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    [Entity]
    class MeteoDataDistributedPerStation : Unique<long>, IMeteoDataDistributed
    {
        private Function data;

        public MeteoDataDistributedPerStation()
        {
            data = new Function("Per station");

            data.Arguments.Add(new Variable<DateTime>("Time")
                {
                    InterpolationType = InterpolationType.Constant,
                    AllowSetInterpolationType = false,
                    ExtrapolationType = ExtrapolationType.None
                });

            data.Arguments.Add(new Variable<string>("Meteo Station Name")
                {
                    Unit = new Unit("", ""),
                    IsAutoSorted = false
                });

            data.Components.Add(new Variable<double> {DefaultValue = 0.0});
        }

        public IFunction Data
        {
            get { return data; }
            set { data = value as Function; }
        }

        public IFunction GetTimeSeries(object item)
        {
           return TimeDependentFunctionSplitter.ExtractSeriesForArgumentValue(Data, item as string);
        }

        public object Clone()
        {
            return new MeteoDataDistributedPerStation {Data = (IFunction) Data.Clone()};
        }
    }
}
