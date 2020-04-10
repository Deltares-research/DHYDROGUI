using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    public interface IParametersVisitor
    {
        /// <summary>
        /// Visit method for defining actions of visitors when they visit a <see cref="ConstantParameters{TSpreading}"/>
        /// </summary>
        /// <typeparam name="T"> An <see cref="IBoundaryConditionSpreading"/> object</typeparam>
        /// <param name="constantParameters"> The visited <see cref="ConstantParameters{TSpreading}"/></param>
        void Visit<T>(ConstantParameters<T> constantParameters) where T : IBoundaryConditionSpreading, new();

        /// <summary>
        /// Visit method for defining actions of visitors when they visit <see cref="TimeDependentParameters{TSpreading}"/>
        /// </summary>
        /// <typeparam name="T"> An <see cref="IBoundaryConditionSpreading"/> object</typeparam>
        /// <param name="timeDependentParameters"> The visited object</param>
        void Visit<T>(TimeDependentParameters<T> timeDependentParameters) where T : IBoundaryConditionSpreading, new();
    }
}