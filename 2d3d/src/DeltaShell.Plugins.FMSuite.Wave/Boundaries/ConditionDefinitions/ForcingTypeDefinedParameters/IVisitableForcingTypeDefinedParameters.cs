namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters
{
    /// <summary>
    /// <see cref="IVisitableForcingTypeDefinedParameters"/> defines method to accept a
    /// <see cref="IForcingTypeDefinedParametersVisitor"/>
    /// </summary>
    public interface IVisitableForcingTypeDefinedParameters
    {
        /// <summary>
        /// Method for accepting <see cref="IForcingTypeDefinedParametersVisitor"/> of the visitor design pattern,
        /// used for the export.
        /// </summary>
        /// <param name="visitor">Visitor who wants to visit this object</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="visitor"/>
        /// is <c>null</c>.
        /// </exception>
        void AcceptVisitor(IForcingTypeDefinedParametersVisitor visitor);
    }
}