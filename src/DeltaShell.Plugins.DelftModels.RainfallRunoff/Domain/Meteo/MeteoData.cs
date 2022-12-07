using System;
using System.ComponentModel;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Properties;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    [Entity]
    public abstract class MeteoData : EditableObjectUnique<long>, IMeteoData
    {
        public const string GlobalMeteoName = "Global";

        private MeteoDataDistributionType dataDistributionType;
        private IMeteoDataDistributed meteoDataDistributed;
        private readonly IUnit unit;
        
        private IMeteoDataDistributed CreateDataDistributed()
        {
            switch (dataDistributionType)
            {
                case MeteoDataDistributionType.Global:
                    return new MeteoDataDistributedGlobal(new MeteoTimeSeriesInstanceCreator(), unit);
                case MeteoDataDistributionType.PerFeature:
                    return new MeteoDataDistributedPerFeature(new TimeDependentFunctionSplitter(), unit);
                case MeteoDataDistributionType.PerStation:
                    return new MeteoDataDistributedPerStation(new TimeDependentFunctionSplitter(), unit);
                default:
                    throw new ArgumentException(Resources.MeteoData_CreateDataDistributed_Meteo_data_distribution_DataDistributionType_unknown);
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MeteoData"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor should currently only be used by NHibernate.
        /// Using this constructor might otherwise lead to an invalid state of this <see cref="MeteoData"/>.
        /// </remarks>
        protected MeteoData()
        {
            unit = new Unit();
            dataDistributionType = MeteoDataDistributionType.Global;
            SetMeteoDataDistribution(CreateDataDistributed());
            DataAggregationType = MeteoDataAggregationType.Cumulative;
            if (DataAggregationType == MeteoDataAggregationType.NonCumulative)
            {
                Data.Arguments[0].InterpolationType = InterpolationType.Constant;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeteoData"/> class.
        /// </summary>
        /// <param name="timeAggregationtype">Aggregation for the <see cref="MeteoData"/>.</param>
        /// <param name="unit">Unit of the meteo data.</param>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when <paramref name="timeAggregationtype"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="unit"/> is <c>null</c>.
        /// </exception>
        protected MeteoData(MeteoDataAggregationType timeAggregationtype, IUnit unit)
        {
            Ensure.IsDefined(timeAggregationtype, nameof(MeteoDataAggregationType));
            Ensure.NotNull(unit, nameof(unit));
            
            this.unit = unit;
            dataDistributionType = MeteoDataDistributionType.Global;
            SetMeteoDataDistribution(CreateDataDistributed());
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
                    dataDistributionType = value;
                    MeteoDataDistributed = CreateDataDistributed();
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

        public abstract object Clone();

        protected virtual object Clone(MeteoData clonedMeteoData)
        {
            clonedMeteoData.dataDistributionType = dataDistributionType;
            clonedMeteoData.DataAggregationType = DataAggregationType;
            clonedMeteoData.Name = Name;
            clonedMeteoData.SetMeteoDataDistribution((IMeteoDataDistributed) meteoDataDistributed.Clone());
            return clonedMeteoData;
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