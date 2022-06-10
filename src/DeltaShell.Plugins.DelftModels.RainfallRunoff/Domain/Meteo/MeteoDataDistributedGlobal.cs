using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    [Entity]
    public class MeteoDataDistributedGlobal: Unique<long>, IMeteoDataDistributed  
    {
        private TimeSeries data;

        public MeteoDataDistributedGlobal()
        {
            data = new TimeSeries
            {
                Components =
                        {
                            new Variable<double>(MeteoData.GlobalMeteoName)
                                {
                                    DefaultValue = 0.0
                                }
                        },
                Name = MeteoData.GlobalMeteoName
            };
            data.Time.DefaultValue = new DateTime(2000, 1, 1);
            data.Time.InterpolationType = InterpolationType.Constant;
            data.Time.AllowSetInterpolationType = false;
            data.Time.ExtrapolationType = ExtrapolationType.None;
        }

        public IFunction Data
        {
            get { return data; }
            set { data = value as TimeSeries; }
        }

        public IFunction GetTimeSeries(object item)
        {
            return Data;
        }

        public object Clone()
        {
            return new MeteoDataDistributedGlobal {Data = (IFunction) Data.Clone()};
        }
    }
}
