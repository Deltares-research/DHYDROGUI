using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters
{
    /// <summary>
    /// <see cref="IBoundaryParametersFactory"/> defines the interface with which to construct
    /// <see cref="IBoundaryConditionParameters"/>.
    /// </summary>
    public interface IBoundaryParametersFactory
    {
        /// <summary>
        /// Construct a new <see cref="ConstructConstantParameters{TSpreading}"/> instance with default values.
        /// </summary>
        ConstantParameters<TSpreading> ConstructDefaultConstantParameters<TSpreading>()
            where TSpreading : class, IBoundaryConditionSpreading, new();

        /// <summary>
        /// Construct a new <see cref="ConstructConstantParameters{TSpreading}"/> instance.
        /// </summary>
        /// <param name="height">The height.</param>
        /// <param name="period">The period.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="spreading">The spreading.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="spreading"/> is <c>null</c>.
        /// </exception>
        ConstantParameters<TSpreading> ConstructConstantParameters<TSpreading>(double height,
                                                                               double period,
                                                                               double direction,
                                                                               TSpreading spreading) 
            where TSpreading : class, IBoundaryConditionSpreading, new();

        /// <summary>
        /// Converts the constant parameters with spreading <typeparamref name="TOldSpreading"/>
        /// to equal constant parameters with <typeparamref name="TNewSpreading"/>.
        /// </summary>
        /// <typeparam name="TOldSpreading">The type of the old spreading.</typeparam>
        /// <typeparam name="TNewSpreading">The type of the new spreading.</typeparam>
        /// <param name="parameters">The parameters to be converted.</param>
        /// <returns>
        /// <see cref="ConstantParameters{TNewSpreading}"/> equal to <paramref name="parameters"/>
        /// but with <typeparamref name="TNewSpreading"/>.
        /// </returns>
        ConstantParameters<TNewSpreading> ConvertConstantParameters<TOldSpreading, TNewSpreading>(ConstantParameters<TOldSpreading> parameters) 
            where TOldSpreading : class, IBoundaryConditionSpreading, new()
            where TNewSpreading : class, IBoundaryConditionSpreading, new();
    }
}