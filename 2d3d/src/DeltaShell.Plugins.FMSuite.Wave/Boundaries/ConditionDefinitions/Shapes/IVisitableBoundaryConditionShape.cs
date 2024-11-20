namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes
{
    /// <summary>
    /// <see cref="IVisitableBoundaryConditionShape"/> defines method to accept a <see cref="IShapeVisitor"/>
    /// </summary>
    public interface IVisitableBoundaryConditionShape
    {
        /// <summary>
        /// Method for accepting <see cref="IShapeVisitor"/> of the visitor design pattern,
        /// used for the export.
        /// </summary>
        /// <param name="visitor">Visitor who wants to visit this object</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="visitor"/>
        /// is <c>null</c>.
        /// </exception>
        void AcceptVisitor(IShapeVisitor visitor);
    }
}