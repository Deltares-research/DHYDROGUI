using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Properties;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    /// <summary>
    /// <see cref="MeteoDataDistributedPerFeature"/> defines the data of a <see cref="IMeteoData"/>
    /// where the values are distributed per feature (catchment).
    /// </summary>
    [Entity]
    internal class MeteoDataDistributedPerFeature : Unique<long>, IMeteoDataDistributed
    {
        private readonly ITimeDependentFunctionSplitter functionSplitter;
        private FeatureCoverage data;
        private readonly IUnit unit;

        /// <summary>
        /// Construct a new <see cref="MeteoDataDistributedPerFeature"/>.
        /// </summary>
        /// <param name="functionSplitter">
        /// The function splitter used to split a <see cref="IFunction"/> in its underlying functions
        /// per catchment.
        /// </param>
        /// <param name="unit">Unit of the time series.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="functionSplitter"/> is <c>null</c>.
        /// Thrown when <paramref name="unit"/> is <c>null</c>.
        /// </exception>
        public MeteoDataDistributedPerFeature(ITimeDependentFunctionSplitter functionSplitter, IUnit unit)
        {
            Ensure.NotNull(functionSplitter, nameof(functionSplitter));
            Ensure.NotNull(unit, nameof(unit));
            this.functionSplitter = functionSplitter;
            this.unit = unit;

            data = new FeatureCoverage(Resources.MeteoDataDistributionType_Per_Catchment)
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
            data.Components.Add(new Variable<double> {DefaultValue = 0.0, Unit = unit});
        }

        public IFunction Data
        {
            get => data;
            set => data = value as FeatureCoverage;
        }

        public IFunction GetTimeSeries(object item)
        {
            return functionSplitter.ExtractSeriesForArgumentValue(Data, item);
        }

        public object Clone()
        {
            return new MeteoDataDistributedPerFeature(functionSplitter, unit) {Data = (IFunction) Data.Clone()};
        }
    }
}
