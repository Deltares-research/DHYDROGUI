using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.DataComponents
{
    /// <summary>
    /// <see cref="DataComponentTestUtils"/> provides some test utilities common between the different
    /// DataComponent tests.
    /// </summary>
    public static class DataComponentTestUtils
    {
        /// <summary>
        /// Construct a new instance of <see cref="IBoundaryConditionParameters"/>
        /// of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"> The type of <see cref="IBoundaryConditionParameters"/>.</typeparam>
        /// <returns>
        /// A new instance of type <typeparamref name="T"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when an unsupported type <typeparamref name="T"/> is provided.
        /// </exception>
        public static T ConstructParameters<T>() where T : class, IBoundaryConditionParameters
        {
            if (typeof(T) == typeof(ConstantParameters))
            { 
                return GetConstantParameters() as T;
            }
            
            throw new InvalidOperationException("Type currently not supported.");
        }

        private static ConstantParameters GetConstantParameters()
        {
            return new ConstantParameters(1.0, 2.0, 3.0, 4.0);
        }
        
    }
}