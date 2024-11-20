namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions
{
    /// <summary>
    /// <see cref="IVisitableWaveBoundaryConditionDefinition"/> defines method to accept a
    /// <see cref="IBoundaryConditionVisitor"/>
    /// </summary>
    public interface IVisitableWaveBoundaryConditionDefinition
    {
        /// <summary>
        /// Method for accepting IBoundaryConditionVisitor visitor of the visitor design pattern,
        /// used for the export.
        /// </summary>
        /// <param name="visitor">Visitor who wants to visit this object</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="visitor"/>
        /// is <c>null</c>.
        /// </exception>
        void AcceptVisitor(IBoundaryConditionVisitor visitor);
    }
}