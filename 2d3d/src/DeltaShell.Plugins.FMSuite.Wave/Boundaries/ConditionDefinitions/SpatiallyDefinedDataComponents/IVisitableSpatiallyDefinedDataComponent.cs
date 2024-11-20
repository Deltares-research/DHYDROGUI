namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents
{
    /// <summary>
    /// <see cref="IVisitableSpatiallyDefinedDataComponent"/> defines method to accept a
    /// <see cref="ISpatiallyDefinedDataComponentVisitor"/>
    /// </summary>
    public interface IVisitableSpatiallyDefinedDataComponent
    {
        /// <summary>
        /// Method for accepting <see cref="ISpatiallyDefinedDataComponentVisitor"/> of the visitor design pattern,
        /// used for the export.
        /// </summary>
        /// <param name="visitor">Visitor who wants to visit this object</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="visitor"/>
        /// is <c>null</c>.
        /// </exception>
        void AcceptVisitor(ISpatiallyDefinedDataComponentVisitor visitor);
    }
}