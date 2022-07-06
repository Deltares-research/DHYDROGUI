using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Properties;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    [Entity]
    public class MeteoData : EditableObjectUnique<long>, IMeteoData
    {
        public const string GlobalMeteoName = "Global";
        private static IMeteoDataDistributed CreateDataDistributed(MeteoDataDistributionType meteoDataDistributionType)
        {
            switch (meteoDataDistributionType)
            {
                case MeteoDataDistributionType.Global:
                    return new MeteoDataDistributedGlobal();
                case MeteoDataDistributionType.PerFeature:
                    return new MeteoDataDistributedPerFeature(new TimeDependentFunctionSplitter());
                case MeteoDataDistributionType.PerStation:
                    return new MeteoDataDistributedPerStation(new TimeDependentFunctionSplitter());
                default:
                    throw new ArgumentException(Resources.MeteoData_CreateDataDistributed_Meteo_data_distribution_DataDistributionType_unknown);
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
                    MeteoDataDistributed = CreateDataDistributed(value);
                    dataDistributionType = value;
                }
            }
        }

        private void SetMeteoDataDistribution(IMeteoDataDistributed data)
        {
            Unsubscribe();
            meteoDataDistributed = data;
            Subscribe();
        }

        public MeteoDataAggregationType DataAggregationType { get; private set; }

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

        #region INameable Members

        public string Name { get; set; }

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