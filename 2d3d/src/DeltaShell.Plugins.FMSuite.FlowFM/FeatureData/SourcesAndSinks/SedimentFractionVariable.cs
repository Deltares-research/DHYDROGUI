using DelftTools.Functions.Generic;
using DelftTools.Units;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks
{
    /// <summary>
    /// This represents a sediment fraction variable for the <see cref="SourceAndSinkFunction"/>.
    /// </summary>
    public sealed class SedimentFractionVariable : Variable<double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SedimentFractionVariable"/> class.
        /// </summary>
        /// <param name="name"> The name of the sediment fraction. </param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="name"/> is <c>null</c> or empty.
        /// </exception>
        public SedimentFractionVariable(string name) : base(name)
        {
            Ensure.NotNullOrEmpty(name, nameof(name));

            Unit = new Unit(SourceSinkVariableInfo.SedimentFractionUnitDescription,
                            SourceSinkVariableInfo.SedimentFractionUnitSymbol);
        }
    }
}