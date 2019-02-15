using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;


namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    /// <summary>
    /// The WindFunction describes the wind in a model as a function.
    /// It provides a wind velocity and wind direction component for this.
    /// </summary>
    /// <seealso cref="DelftTools.Functions.Function" />
    /// <inheritdoc/>
    public class WindFunction : Function
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="WindFunction"/> class with "wind velocity" as name.
        /// </summary>
        /// <inheritdoc/>
        public WindFunction()
            : this("wind velocity")
        {
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="WindFunction"/> class with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of this new WindFunction.</param>
        public WindFunction(string name)
            : base (name)
        {
            Arguments.Add(new Variable<DateTime>("Time")
            {
                InterpolationType = InterpolationType.Linear, 
                ExtrapolationType = ExtrapolationType.Constant
            });
            Components.Add(new Variable<double>("Wind Velocity", new Unit("Velocity", "m/s")));
            Components.Add(new Variable<double>("Wind Direction", new Unit("Direction", "deg"))
            {
                MinValidValue = 0.0, 
                MaxValidValue = 360.0
            });

            Attributes[FunctionAttributes.LocationType] = FunctionAttributes.Global;
        }

        /// <summary>
        /// Get the wind velocity component of this WindFunction.
        /// </summary>
        /// <value> The wind velocity (m/s). </value>
        public IVariable Velocity => Components[0];

        /// <summary>
        /// Get the wind direction component of this WindFunction.
        /// </summary>
        /// <value>The wind direction (deg).</value>
        public IVariable Direction => Components[1];
    }
}