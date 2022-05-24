using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    [Entity]
    class MeteoDataDistributedPerFeature : Unique<long>, IMeteoDataDistributed
    {
        private FeatureCoverage data;

        public MeteoDataDistributedPerFeature()
        {
            data = new FeatureCoverage("Per catchment")
                {
                    IsTimeDependent = true,
                    Time =
                        {
                            InterpolationType = InterpolationType.Constant,
                            AllowSetInterpolationType = false,
                            ExtrapolationType = ExtrapolationType.None
                        }
                };
            data.Arguments.Add(new Variable<IFeature>("Catchment"));
            data.Components.Add(new Variable<double> {DefaultValue = 0.0});
        }

        public IFunction Data
        {
            get { return data; }
            set { data = value as FeatureCoverage; }
        }

        public IFunction GetTimeSeries(object item)
        {
            return TimeDependentFunctionSplitter.ExtractSeriesForArgumentValue(Data, item);
        }

        public object Clone()
        {
            return new MeteoDataDistributedPerFeature {Data = (IFunction) Data.Clone()};
        }
    }
}
