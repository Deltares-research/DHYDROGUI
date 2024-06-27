using System;
using DelftTools.Functions;
using DelftTools.Units;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    [Entity]
    public class MeteoDataDistributedGlobal: Unique<long>, IMeteoDataDistributed  
    {
        private TimeSeries data;
        private readonly MeteoTimeSeriesInstanceCreator meteoTimeSeriesInstanceCreator;
        private readonly IUnit unit;

        /// <summary>
        /// Construct a new <see cref="MeteoDataDistributedGlobal"/>.
        /// </summary>
        /// <param name="meteoTimeSeriesInstanceCreator">Instance creator for meteo time series objects.</param>
        /// <param name="unit">Unit of the time series.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="unit"/> is <c>null</c>.
        /// Thrown when <paramref name="meteoTimeSeriesInstanceCreator"/> is <c>null</c>.
        /// </exception>
        public MeteoDataDistributedGlobal(MeteoTimeSeriesInstanceCreator meteoTimeSeriesInstanceCreator, IUnit unit)
        {
            Ensure.NotNull(unit, nameof(unit));
            Ensure.NotNull(meteoTimeSeriesInstanceCreator, nameof(meteoTimeSeriesInstanceCreator));
            
            this.unit = unit;
            this.meteoTimeSeriesInstanceCreator = meteoTimeSeriesInstanceCreator;
            data = meteoTimeSeriesInstanceCreator.CreateGlobalTimeSeries(unit);
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
            return new MeteoDataDistributedGlobal(meteoTimeSeriesInstanceCreator, unit) {Data = (IFunction) Data.Clone()};
        }
    }
}
