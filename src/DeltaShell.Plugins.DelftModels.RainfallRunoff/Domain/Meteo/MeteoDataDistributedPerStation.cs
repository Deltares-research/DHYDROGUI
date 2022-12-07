using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Properties;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    /// <summary>
    /// <see cref="MeteoDataDistributedPerStation"/> defines the data of a <see cref="IMeteoData"/>
    /// where the values are distributed per station.
    /// </summary>
    [Entity]
    internal class MeteoDataDistributedPerStation : Unique<long>, IMeteoDataDistributed
    {
        private readonly ITimeDependentFunctionSplitter functionSplitter;
        private Function data;
        private readonly IUnit unit;

        /// <summary>
        /// Construct a new <see cref="MeteoDataDistributedPerStation"/>.
        /// </summary>
        /// <param name="functionSplitter">
        /// The function splitter used to split a <see cref="IFunction"/> in its underlying functions
        /// per station.
        /// </param>
        /// <param name="unit">Unit of the time series.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="functionSplitter"/> is <c>null</c>.
        /// Thrown when <paramref name="unit"/> is <c>null</c>.
        /// </exception>
        public MeteoDataDistributedPerStation(ITimeDependentFunctionSplitter functionSplitter, IUnit unit)
        {
            Ensure.NotNull(functionSplitter, nameof(functionSplitter));
            Ensure.NotNull(unit, nameof(unit));
            this.functionSplitter = functionSplitter;
            this.unit = unit;

            data = new Function(Resources.MeteoDataDistributionType_Per_Station);

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

            data.Components.Add(new Variable<double> {DefaultValue = 0.0, Unit = unit});
        }

        public IFunction Data
        {
            get => data;
            set => data = value as Function;
        }

        public IFunction GetTimeSeries(object item)
        {
           return functionSplitter.ExtractSeriesForArgumentValue(Data, item as string);
        }

        public object Clone()
        {
            return new MeteoDataDistributedPerStation(functionSplitter, unit) {Data = (IFunction) Data.Clone()};
        }
    }
}
