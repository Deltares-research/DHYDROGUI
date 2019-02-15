using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;


namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters
{
    /// <summary>
    /// The MeteoFunction describes the meteorological situation in a model as a function
    /// dependent on time. It provides the air temperature, relative humidity and
    /// cloudiness.
    /// </summary>
    /// <seealso cref="DelftTools.Functions.Function" />
    /// <inheritdoc/>
    public class MeteoFunction : Function
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeteoFunction"/> class with "Meteo Data" as name.
        /// </summary>
        public MeteoFunction()
            : this("Meteo Data")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeteoFunction"/> class with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of this new MeteoFunction.</param>
        public MeteoFunction(string name)
            : base(name)
        {
            Arguments.Add(new Variable<DateTime>("Time")
            {
                InterpolationType = InterpolationType.Linear,
                ExtrapolationType = ExtrapolationType.Constant
            });
            Components.Add(new Variable<double>("Air temperature", new Unit("Degree Celsius", "°C")));
            Components.Add(new Variable<double>("Relative humidity", new Unit("Percentage", "%")));
            Components.Add(new Variable<double>("Cloudiness", new Unit("Percentage", "%")));

            Attributes[FunctionAttributes.LocationType] = FunctionAttributes.Global;
        }

        /// <summary>
        /// Get the air temperature component of this MeteoFunction.
        /// </summary>
        /// <value>The air temperature in Degree Celsius (°C).</value>
        public IVariable AirTemperature => Components[0];

        /// <summary>
        /// Get the relative humidity component of this MeteoFunction.
        /// </summary>
        /// <value>The relative humidity in percentage.</value>
        public IVariable RelativeHumidity => Components[1];

        /// <summary>
        /// Get the cloudiness component of this MeteoFunction.
        /// </summary>
        /// <value>The cloudiness in percentage.</value>
        public IVariable Cloudiness => Components[2];
    }
}