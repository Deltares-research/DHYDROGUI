namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions
{
    /// <summary>
    /// <see cref="IBoundaryConditionVisitor"/> contains visit method for a wave boundary condition definition.
    /// </summary>
    public interface IBoundaryConditionVisitor
    {
        /// <summary>
        /// Visit method for defining actions of visitors when they visit a <see cref="IWaveBoundaryConditionDefinition"/>
        /// </summary>
        /// <param name="waveBoundaryConditionDefinition">The visited <see cref="IWaveBoundaryConditionDefinition"/></param>
        void Visit(IWaveBoundaryConditionDefinition waveBoundaryConditionDefinition);
    }
}