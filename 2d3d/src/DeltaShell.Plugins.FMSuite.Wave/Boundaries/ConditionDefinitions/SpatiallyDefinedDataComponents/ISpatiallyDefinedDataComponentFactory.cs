using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents
{
    /// <summary>
    /// <see cref="ISpatiallyDefinedDataComponentFactory"/> defines the
    /// methods to create new <see cref="ISpatiallyDefinedDataComponent"/>
    /// instances.
    /// </summary>
    public interface ISpatiallyDefinedDataComponentFactory
    {
        /// <summary>
        /// Construct a new default <see cref="ISpatiallyDefinedDataComponent"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="ISpatiallyDefinedDataComponent"/>.</typeparam>
        /// <returns>
        /// A new default <see cref="ISpatiallyDefinedDataComponent"/> of type <typeparamref name="T"/>.
        /// </returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown when <typeparamref name="T"/> is not supported.
        /// </exception>
        T ConstructDefaultDataComponent<T>() where T : class, ISpatiallyDefinedDataComponent;

        /// <summary>
        /// Converts the specified <paramref name="oldDataComponent"/> with spreading
        /// <typeparamref name="TOldSpreading"/> to a data component with a
        /// <typeparamref name="TNewSpreading"/>.
        /// </summary>
        /// <typeparam name="TOldSpreading">The type of the old spreading.</typeparam>
        /// <typeparam name="TNewSpreading">The type of the new spreading.</typeparam>
        /// <param name="oldDataComponent">The old data component.</param>
        /// <returns>
        /// A <see cref="ISpatiallyDefinedDataComponent"/> equal to <paramref name="oldDataComponent"/>
        /// but with <typeparamref name="TNewSpreading"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="oldDataComponent"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when <typeparamref name="TOldSpreading"/> is equal to <typeparamref name="TNewSpreading"/>.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// Thrown when either of the type parameters is not supported.
        /// </exception>
        ISpatiallyDefinedDataComponent ConvertDataComponentSpreading<TOldSpreading, TNewSpreading>(
            ISpatiallyDefinedDataComponent oldDataComponent)
            where TOldSpreading : class, IBoundaryConditionSpreading, new()
            where TNewSpreading : class, IBoundaryConditionSpreading, new();
    }
}