namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading
{
    /// <summary>
    /// <see cref="IVisitableBoundaryConditionSpreading"/> defines method to accept a <see cref="ISpreadingVisitor"/>
    /// </summary>
    public interface IVisitableBoundaryConditionSpreading
    {
        /// <summary>
        /// Method for accepting <see cref="ISpreadingVisitor"/> of the visitor design pattern,
        /// used for the export.
        /// </summary>
        /// <param name="visitor">Visitor who wants to visit this object</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="visitor"/>
        /// is <c>null</c>.
        /// </exception>
        void AcceptVisitor(ISpreadingVisitor visitor);
    }
}