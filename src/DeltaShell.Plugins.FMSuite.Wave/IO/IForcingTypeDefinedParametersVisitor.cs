using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    /// <summary>
    /// <see cref="IForcingTypeDefinedParametersVisitor"/> contains visit methods for different forcing type parameters.
    /// </summary>
    public interface IForcingTypeDefinedParametersVisitor
    {
        /// <summary>
        /// Visit method for defining actions of visitors when they visit a <see cref="ConstantParameters{TSpreading}"/>.
        /// </summary>
        /// <typeparam name="T"> The type of the spreading.</typeparam>
        /// <param name="constantParameters"> The visited <see cref="ConstantParameters{TSpreading}"/></param>
        void Visit<T>(ConstantParameters<T> constantParameters) where T : IBoundaryConditionSpreading, new();

        /// <summary>
        /// Visit method for defining actions of visitors when they visit <see cref="TimeDependentParameters{TSpreading}"/>.
        /// </summary>
        /// <typeparam name="T"> The type of the spreading.</typeparam>
        /// <param name="timeDependentParameters"> The visited <see cref="TimeDependentParameters{TSpreading}"/></param>
        void Visit<T>(TimeDependentParameters<T> timeDependentParameters) where T : IBoundaryConditionSpreading, new();

        /// <summary>
        /// Visit method for defining actions of visitors when they visit <see cref="FileBasedParameters"/>.
        /// </summary>
        /// <param name="fileBasedParameters">The visited object.</param>
        void Visit(FileBasedParameters fileBasedParameters);
    }
}