using System;
using System.ComponentModel;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using GeoAPI.Extensions.Coverages;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
    public enum MeteoDataDistributionType
    {
        [Description("Global")]
        Global = 0,
        [Description("Per catchment")]
        PerFeature = 1,
        [Description("Meteo stations")]
        PerStation = 2
    }

    public enum MeteoDataAggregationType
    {
        NonCumulative = 0,
        Cumulative = 1
    }

    [Entity]
    public class MeteoData : EditableObjectUnique<long>, INameable, ICloneable
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (MeteoData));

        private static IMeteoDataDistributed CreateDataDistributed(MeteoDataDistributionType meteoDataDistributionType)
        {
            switch (meteoDataDistributionType)
            {
                case MeteoDataDistributionType.Global:
                    return new MeteoDataDistributedGlobal();
                case MeteoDataDistributionType.PerFeature:
                    return new MeteoDataDistributedPerFeature();
                case MeteoDataDistributionType.PerStation:
                    return new MeteoDataDistributedPerStation();
                default:
                    throw new ArgumentException("Meteo data distribution DataDistributionType unknown");
            }
        }

        private static IMeteoTimeAggregator CreateDataAggregator(MeteoDataAggregationType type)
        {
            switch (type)
            {
                case MeteoDataAggregationType.Cumulative:
                    return new MeteoDataTimeIntegrator();
                case MeteoDataAggregationType.NonCumulative:
                    return new MeteoDataTimeInterpolator();
                default:
                    throw new ArgumentException("Meteo data aggregation DataDistributionType unknown");
            }
        }

        private MeteoDataDistributionType dataDistributionType;
        private IMeteoDataDistributed meteoDataDistributed;

        public MeteoData()
        {
            dataDistributionType = MeteoDataDistributionType.Global;
            SetMeteoDataDistribution(CreateDataDistributed(dataDistributionType));
            DataAggregationType = MeteoDataAggregationType.Cumulative;
            if (DataAggregationType == MeteoDataAggregationType.NonCumulative)
            {
                Data.Arguments[0].InterpolationType = InterpolationType.Constant;
            }
        }

        public MeteoData(MeteoDataAggregationType timeAggregationtype)
        {
            dataDistributionType = MeteoDataDistributionType.Global;
            SetMeteoDataDistribution(CreateDataDistributed(dataDistributionType));
            DataAggregationType = timeAggregationtype;
            if (DataAggregationType == MeteoDataAggregationType.NonCumulative)
            {
                Data.Arguments[0].InterpolationType = InterpolationType.Constant;
            }
        }

        public MeteoDataDistributionType DataDistributionType
        {
            get { return dataDistributionType; }
            set 
            {
                if (dataDistributionType != value)
                {
                    SetMeteoDataDistributionFromType(value);
                    dataDistributionType = value;
                }
            }
        }

        [EditAction]
        private void SetMeteoDataDistributionFromType(MeteoDataDistributionType type)
        {
            MeteoDataDistributed = CreateDataDistributed(type);
        }

        private void SetMeteoDataDistribution(IMeteoDataDistributed data)
        {
            Unsubscribe();
            meteoDataDistributed = data;
            Subscribe();
        }

        public MeteoDataAggregationType DataAggregationType { get; protected set; }

        #region ICloneable members

        public object Clone()
        {
            var clone = new MeteoData(DataAggregationType)
                {
                    dataDistributionType = dataDistributionType,
                    DataAggregationType = DataAggregationType,
                    Name = Name
                };
            clone.SetMeteoDataDistribution((IMeteoDataDistributed) meteoDataDistributed.Clone());
            return clone;
        }

        #endregion

        public IFunction Data
        {
            get { return meteoDataDistributed.Data; }
        }

        public IMeteoDataDistributed MeteoDataDistributed
        {
            get { return meteoDataDistributed; }
            set
            {
                SetMeteoDataDistribution(value);
            }
        }

        public double[] GetMeteoForPeriod(DateTime startDate, DateTime endDate, TimeSpan timeStep, object item)
        {
            var timeAggregator = CreateDataAggregator(DataAggregationType);

            var timeSeries = meteoDataDistributed.GetTimeSeries(item);

            var valueVariable = timeSeries.Components[0] as Variable<double>;
            var timeVariable = timeSeries.Arguments[0] as Variable<DateTime>;
  
            if (valueVariable != null && timeVariable != null)
            {
                try
                {
                    return timeAggregator.GetTimeSeriesForPeriod(valueVariable, timeVariable, startDate, endDate,
                                                                 timeStep);
                }
                catch (Exception e)
                {
                    log.Error("Fetching meteo data failed, " + e.Message);
                    return null;
                }
            }
            log.Error("Failed to evaluate meteo data for this DataDistributionType of function");
            return null;
        }

        #region INameable Members

        public
            string Name { get; set; }

        #endregion

        private void Subscribe()
        {
            if (meteoDataDistributed != null && Data != null)
            {
                Data.ValuesChanged += DataCollectionChanged;
            }
        }

        private void Unsubscribe()
        {
            if (meteoDataDistributed != null && Data != null)
            {
                Data.ValuesChanged -= DataCollectionChanged;
            }
        }

        private void DataCollectionChanged(object sender,
                                           FunctionValuesChangingEventArgs functionValuesChangingEventArgs)
        {
            var featureCoverage = Data as IFeatureCoverage;
            if (featureCoverage != null)
            {
                if (sender != featureCoverage.FeatureVariable)
                    return;

                if (CatchmentsChanged != null)
                {
                    CatchmentsChanged(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler<EventArgs> CatchmentsChanged;
    }
}