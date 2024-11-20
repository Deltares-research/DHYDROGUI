using System;
using DelftTools.Functions;

namespace DelftTools.Hydro.Structures.SteerableProperties
{
    /// <summary>
    /// <see cref="SteerableProperty"/> defines a property that can either be constant, i.e. a <see cref="double"/>
    /// or a <see cref="TimeSeries"/>.
    /// </summary>
    public sealed class SteerableProperty
    { 
        private SteerablePropertyDriver currentDriver = SteerablePropertyDriver.Constant;

        /// <summary>
        /// Creates a new <see cref="SteerableProperty"/> with a default config.
        /// </summary>
        public SteerableProperty() : this(0.0)
        {
            // Necessary because TypeUtils.DeepClone :|
        }

        /// <summary>
        /// Creates a new constant <see cref="SteerableProperty"/>.
        /// </summary>
        /// <param name="defaultConstantValue">The default constant value used.</param>
        public SteerableProperty(double defaultConstantValue) : this(defaultConstantValue, null)
        {
            CanBeTimeDependent = false;
        }

        /// <summary>
        /// Creates a new time-dependent <see cref="SteerableProperty"/>.
        /// </summary>
        /// <param name="defaultConstantValue">The default constant value used.</param>
        /// <param name="seriesName">The name of the time series.</param>
        /// <param name="componentName">The name of the time series' component.</param>
        /// <param name="unit">The unit of the time series' component.</param>
        public SteerableProperty(double defaultConstantValue,
                                 string seriesName,
                                 string componentName,
                                 string unit) :
            this(defaultConstantValue, HydroTimeSeriesFactory.CreateTimeSeries(seriesName, componentName, unit))
        {
            CanBeTimeDependent = true;
        }
        
        /// <summary>
        /// Creates a deep copy of the provided <see cref="SteerableProperty"/> instance.
        /// </summary>
        /// <param name="source">The source property</param>
        public SteerableProperty(SteerableProperty source) : 
            this(source.Constant, (TimeSeries)source.TimeSeries?.Clone(true))
        {
            CanBeTimeDependent = source.CanBeTimeDependent;
            CurrentDriver = source.CurrentDriver;
        }

        private SteerableProperty(double defaultConstantValue,
                                  TimeSeries timeSeries)
        {
            Constant = defaultConstantValue;
            TimeSeries = timeSeries;
        }

        /// <summary>
        /// Gets or sets the current steerablePropertyDriver of this <see cref="SteerableProperty"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// Thrown if the steerablePropertyDriver is changed to a value that is not supported.
        /// </exception>
        public SteerablePropertyDriver CurrentDriver
        {
             get => currentDriver;
             set
             {
                 if (!IsSupported(value))
                 {
                     throw new NotSupportedException($"{value} is not supported within this {nameof(SteerableProperty)}");
                 }

                 currentDriver = value;
             }
        }

        /// <summary>
        /// Gets or sets the constant value of this <see cref="SteerableProperty"/>.
        /// </summary>
        public double Constant { get; set; }

        /// <summary>
        /// Gets the <see cref="TimeSeries"/> of this <see cref="SteerableProperty"/>.
        /// </summary>
        /// <remarks>
        /// If time series are not supported in this instance, or if the current steerablePropertyDriver has
        /// never been said to <see cref="SteerablePropertyDriver.TimeSeries"/>, this value will be null.
        /// </remarks>
        public TimeSeries TimeSeries { get; set; } = null;

        /// <summary>
        /// Gets whether this <see cref="SteerableProperty"/> can be time dependent.
        /// </summary>
        public bool CanBeTimeDependent { get; }

        private bool IsSupported(SteerablePropertyDriver steerablePropertyDriver) =>
            steerablePropertyDriver == SteerablePropertyDriver.Constant ||
            steerablePropertyDriver == SteerablePropertyDriver.TimeSeries && CanBeTimeDependent;
    }
}