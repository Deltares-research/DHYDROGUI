using DelftTools.Functions.Generic;
using DelftTools.Units;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks
{
    /// <summary>
    /// This represents a tracer variable for the <see cref="SourceAndSinkFunction"/>.
    /// </summary>
    public sealed class TracerVariable : Variable<double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TracerVariable"/> class.
        /// </summary>
        /// <param name="name"> The name of the tracer. </param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="name"/> is <c>null</c> or empty.
        /// </exception>
        public TracerVariable(string name) : base(name)
        {
            Ensure.NotNullOrEmpty(name, nameof(name));

            Unit = new Unit(SourceSinkVariableInfo.TracersUnitDescription,
                            SourceSinkVariableInfo.TracerUnitSymbol);
        }
    }
}