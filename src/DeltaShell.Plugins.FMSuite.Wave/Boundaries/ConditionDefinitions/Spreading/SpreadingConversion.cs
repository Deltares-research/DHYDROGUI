using System;
using DelftTools.Units;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading
{
    /// <summary>
    /// <see cref="SpreadingConversion"/> provides methods to convert to and from <see cref="IBoundaryConditionSpreading"/>.
    /// </summary>
    public static class SpreadingConversion
    {
        /// <summary>
        /// Gets the spreading unit corresponding with the <typeparamref name="TSpreading"/>.
        /// </summary>
        /// <typeparam name="TSpreading">The type of the spreading.</typeparam>
        /// <returns>
        /// The unit corresponding with the <typeparamref name="TSpreading"/>
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown when <typeparamref name="TSpreading"/> is not supported.
        /// </exception>
        public static Unit GetSpreadingUnit<TSpreading>()
            where TSpreading : class, IBoundaryConditionSpreading, new()
        {
            if (typeof(TSpreading) == typeof(DegreesDefinedSpreading))
                return WaveParametersConstants.ConstructDegreesUnit();

            if (typeof(TSpreading) == typeof(PowerDefinedSpreading))
                return WaveParametersConstants.ConstructPowerUnit();

            throw new NotSupportedException($"{typeof(TSpreading)} is not supported.");
        }

        /// <summary>
        /// Gets the spreading default value corresponding with the <typeparamref name="TSpreading"/>.
        /// </summary>
        /// <typeparam name="TSpreading">The type of the spreading.</typeparam>
        /// <returns>
        /// The default value corresponding with the <typeparamref name="TSpreading"/>
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown when <typeparamref name="TSpreading"/> is not supported.
        /// </exception>
        public static double GetSpreadingDefaultValue<TSpreading>()
            where TSpreading : class, IBoundaryConditionSpreading, new()
        {
            if (typeof(TSpreading) == typeof(DegreesDefinedSpreading))
                return WaveSpreadingConstants.DegreesDefaultSpreading;

            if (typeof(TSpreading) == typeof(PowerDefinedSpreading))
                return WaveSpreadingConstants.PowerDefaultSpreading;

            throw new NotSupportedException($"{typeof(TSpreading)} is not supported.");
        }
    }
}