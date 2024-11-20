using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters
{
    /// <summary>
    /// <see cref="IForcingTypeDefinedParametersFactory"/> defines the interface with which to construct
    /// <see cref="IForcingTypeDefinedParameters"/>.
    /// </summary>
    public interface IForcingTypeDefinedParametersFactory
    {
        /// <summary>
        /// Construct a new <see cref="ConstantParameters{TSpreading}"/> instance with default values.
        /// </summary>
        /// <typeparam name="TSpreading">The type of the spreading.</typeparam>
        /// <returns>
        /// A new default <see cref="ConstantParameters{TSpreading}"/> instance with default
        /// values.
        /// </returns>
        ConstantParameters<TSpreading> ConstructDefaultConstantParameters<TSpreading>()
            where TSpreading : class, IBoundaryConditionSpreading, new();

        /// <summary>
        /// Construct a new <see cref="ConstantParameters{TSpreading}"/> instance.
        /// </summary>
        /// <typeparam name="TSpreading">The type of the spreading.</typeparam>
        /// <param name="height">The height.</param>
        /// <param name="period">The period.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="spreading">The spreading.</param>
        /// <returns>
        /// A new <see cref="ConstantParameters{TSpreading}"/> instance with the provided
        /// values.
        /// </returns>
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
        /// A new <see cref="ConstantParameters{TNewSpreading}"/> equal to <paramref name="parameters"/>
        /// but with <typeparamref name="TNewSpreading"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="parameters"/> is <c>null</c>.
        /// </exception>
        ConstantParameters<TNewSpreading> ConvertConstantParameters<TOldSpreading, TNewSpreading>(ConstantParameters<TOldSpreading> parameters)
            where TOldSpreading : class, IBoundaryConditionSpreading, new()
            where TNewSpreading : class, IBoundaryConditionSpreading, new();

        /// <summary>
        /// Constructs a <see cref="TimeDependentParameters{TSpreading}"/> with a default wave energy function.
        /// </summary>
        /// <typeparam name="TSpreading">The type of the spreading.</typeparam>
        /// <returns>
        /// A new <see cref="TimeDependentParameters{TSpreading}"/> with a default wave energy function.
        /// </returns>
        TimeDependentParameters<TSpreading> ConstructDefaultTimeDependentParameters<TSpreading>()
            where TSpreading : class, IBoundaryConditionSpreading, new();

        /// <summary>
        /// Creates a new <see cref="TimeDependentParameters{TSpreading}"/>
        /// with the provided <paramref name="waveEnergyFunction"/>.
        /// </summary>
        /// <typeparam name="TSpreading">The type of the spreading.</typeparam>
        /// <param name="waveEnergyFunction">The wave energy function.</param>
        /// <returns>
        /// A new <see cref="TimeDependentParameters{TSpreading}"/> with the provided <paramref name="waveEnergyFunction"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="waveEnergyFunction"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// No additional verification happens on <paramref name="waveEnergyFunction"/>
        /// as such it is expected to be in the correct WaveEnergyFunction form.
        /// </remarks>
        TimeDependentParameters<TSpreading> ConstructTimeDependentParameters<TSpreading>(IWaveEnergyFunction<TSpreading> waveEnergyFunction)
            where TSpreading : class, IBoundaryConditionSpreading, new();

        /// <summary>
        /// Converts the time dependent parameters from the <typeparamref name="TOldSpreading"/>
        /// to <typeparamref name="TNewSpreading"/>.
        /// </summary>
        /// <typeparam name="TOldSpreading">The type of the old spreading.</typeparam>
        /// <typeparam name="TNewSpreading">The type of the new spreading.</typeparam>
        /// <param name="parameters">The time dependent parameters to change.</param>
        /// <returns>
        /// Time-dependent parameters with the spreading type adjusted to <typeparamref name="TNewSpreading"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="parameters`"/> is <c>null</c>.
        /// </exception>
        TimeDependentParameters<TNewSpreading> ConvertTimeDependentParameters<TOldSpreading, TNewSpreading>(TimeDependentParameters<TOldSpreading> parameters)
            where TOldSpreading : class, IBoundaryConditionSpreading, new()
            where TNewSpreading : class, IBoundaryConditionSpreading, new();

        /// <summary>
        /// Construct a new <see cref="FileBasedParameters"/> instance with default values.
        /// </summary>
        /// <returns>
        /// A new default <see cref="FileBasedParameters"/> instance with default values.
        /// </returns>
        FileBasedParameters ConstructDefaultFileBasedParameters();
    }
}