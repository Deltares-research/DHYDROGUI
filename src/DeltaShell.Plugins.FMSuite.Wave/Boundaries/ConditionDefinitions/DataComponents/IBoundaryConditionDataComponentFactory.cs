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
    }
}