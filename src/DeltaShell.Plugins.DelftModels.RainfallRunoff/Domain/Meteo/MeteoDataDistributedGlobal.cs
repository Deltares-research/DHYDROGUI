using DelftTools.Functions;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    [Entity]
    public class MeteoDataDistributedGlobal: Unique<long>, IMeteoDataDistributed  
    {
        private TimeSeries data;
        private readonly MeteoTimeSeriesInstanceCreator meteoTimeSeriesInstanceCreator;

        public MeteoDataDistributedGlobal(MeteoTimeSeriesInstanceCreator meteoTimeSeriesInstanceCreator)
        {
            this.meteoTimeSeriesInstanceCreator = meteoTimeSeriesInstanceCreator;
            data = meteoTimeSeriesInstanceCreator.CreateGlobalTimeSeries();
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
            return new MeteoDataDistributedGlobal(meteoTimeSeriesInstanceCreator) {Data = (IFunction) Data.Clone()};
        }
    }
}
