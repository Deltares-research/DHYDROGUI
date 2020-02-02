using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents
{
    /// <summary>
    /// <see cref="IBoundaryConditionDataComponentFactory"/> defines the
    /// methods to create new <see cref="IBoundaryConditionDataComponent"/>
    /// instances.
    /// </summary>
    public interface IBoundaryConditionDataComponentFactory
    {
        /// <summary>
        /// Construct a new default <see cref="IBoundaryConditionDataComponent"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IBoundaryConditionDataComponent"/>.</typeparam>
        /// <returns>
        /// A new default <see cref="IBoundaryConditionDataComponent"/> of type <typeparamref name="T"/>.
        /// </returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown when <typeparamref name="T"/> is not supported.
        /// </exception>
        T ConstructDefaultDataComponent<T>() where T : class, IBoundaryConditionDataComponent;

        /// <summary>
        /// Converts the specified <paramref name="oldDataComponent"/> with spreading
        /// <typeparamref name="TOldSpreading"/> to a data component with a
        /// <typeparamref name="TNewSpreading"/>.
        /// </summary>
        /// <typeparam name="TOldSpreading">The type of the old spreading.</typeparam>
        /// <typeparam name="TNewSpreading">The type of the new spreading.</typeparam>
        /// <param name="oldDataComponent">The old data component.</param>
        /// <returns>
        /// A <see cref="IBoundaryConditionDataComponent"/> equal to <paramref name="oldDataComponent"/>
        /// but with <typeparamref name="TNewSpreading"/>.
        /// </returns>
        IBoundaryConditionDataComponent ConvertDataComponentSpreading<TOldSpreading, TNewSpreading>(
            IBoundaryConditionDataComponent oldDataComponent)
            where TOldSpreading : class, IBoundaryConditionSpreading, new()
            where TNewSpreading : class, IBoundaryConditionSpreading, new();
    }
}