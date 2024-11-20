using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents
{
    /// <summary>
    /// <see cref="DataComponentTestUtils"/> provides some test utilities common between the different
    /// DataComponent tests.
    /// </summary>
    public static class DataComponentTestUtils
    {
        /// <summary>
        /// Construct a new instance of <see cref="IForcingTypeDefinedParameters"/>
        /// of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"> The type of <see cref="IForcingTypeDefinedParameters"/>.</typeparam>
        /// <returns>
        /// A new instance of type <typeparamref name="T"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when an unsupported type <typeparamref name="T"/> is provided.
        /// </exception>
        public static T ConstructParameters<T>() where T : class, IForcingTypeDefinedParameters
        {
            if (typeof(T) == typeof(ConstantParameters<PowerDefinedSpreading>))
            {
                return GetConstantParameters<PowerDefinedSpreading>() as T;
            }

            if (typeof(T) == typeof(ConstantParameters<DegreesDefinedSpreading>))
            {
                return GetConstantParameters<DegreesDefinedSpreading>() as T;
            }

            throw new InvalidOperationException("Type currently not supported.");
        }

        private static ConstantParameters<TSpreading> GetConstantParameters<TSpreading>()
            where TSpreading : class, IBoundaryConditionSpreading, new()
        {
            return new ConstantParameters<TSpreading>(1.0, 2.0, 3.0, new TSpreading());
        }
    }
}