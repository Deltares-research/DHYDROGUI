using System;
using DelftTools.Units;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading
{
    /// <summary>
    /// <see cref="SpreadingConversion"/> provides methods to convert to and from <see cref="IBoundaryConditionSpreading"/>.
    /// </summary>
    public static class SpreadingConversion
    {
        /// <summary>
        /// Gets the spreading unit corresponding with the <typeparamref name="TSpreading"/>
        /// with the spreading value specified by <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="TSpreading"> The type of the spreading. </typeparam>
        /// <param name="value"> The spreading value. </param>
        /// <returns> The spreading unit. </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown when the <typeparamref name="TSpreading"/> is not a <see cref="PowerDefinedSpreading"/>
        /// or a <see cref="DegreesDefinedSpreading"/>.
        /// </exception>
        public static TSpreading FromDouble<TSpreading>(double value)
            where TSpreading : class, IBoundaryConditionSpreading, new()
        {
            var spreading = new TSpreading();

            // Awkward cast due to behaviour of C# 7, required to make type
            // matching on generics work (Roslyn will create a compilation error otherwise).
            // This could be removed if we ever decide to switch C# 7.1 or higher.
            switch ((object) spreading)
            {
                case PowerDefinedSpreading s:
                    s.SpreadingPower = value;
                    break;

                case DegreesDefinedSpreading s:
                    s.DegreesSpreading = value;
                    break;
                default:
                    throw new NotSupportedException();
            }

            return spreading;
        }

        /// <summary>
        /// Gets the spreading unit corresponding with the <typeparamref name="TSpreading"/>.
        /// </summary>
        /// <typeparam name="TSpreading">The type of the spreading.</typeparam>
        /// <returns>
        /// The unit corresponding with the <typeparamref name="TSpreading"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown when <typeparamref name="TSpreading"/> is not supported.
        /// </exception>
        public static Unit GetSpreadingUnit<TSpreading>()
            where TSpreading : class, IBoundaryConditionSpreading, new()
        {
            if (typeof(TSpreading) == typeof(DegreesDefinedSpreading))
            {
                return WaveTimeDependentParametersConstants.ConstructDegreesUnit();
            }

            if (typeof(TSpreading) == typeof(PowerDefinedSpreading))
            {
                return WaveTimeDependentParametersConstants.ConstructPowerUnit();
            }

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
            {
                return WaveSpreadingConstants.DegreesDefaultSpreading;
            }

            if (typeof(TSpreading) == typeof(PowerDefinedSpreading))
            {
                return WaveSpreadingConstants.PowerDefaultSpreading;
            }

            throw new NotSupportedException($"{typeof(TSpreading)} is not supported.");
        }
    }
}