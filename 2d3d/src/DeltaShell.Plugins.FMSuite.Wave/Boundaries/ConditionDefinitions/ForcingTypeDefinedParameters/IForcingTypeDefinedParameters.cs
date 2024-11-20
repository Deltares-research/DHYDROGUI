namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters
{
    /// <summary>
    /// <see cref="IForcingTypeDefinedParameters"/> defines the different boundary
    /// condition parameters used within the <see cref="IWaveBoundaryConditionDefinition"/>.
    /// Currently, the following are defined:
    /// * <see cref="ConstantParameters{TSpreading}"/>
    /// * <see cref="TimeDependentParameters{TSpreading}"/>
    /// * <see cref="FileBasedParameters"/>
    /// </summary>
    /// <remarks>
    /// This interface acts as a discriminated union for its implementing types.
    /// As such, this interface is empty. Other classes will use it to type
    /// cast to the implementing types.
    /// </remarks>
    public interface IForcingTypeDefinedParameters : IVisitableForcingTypeDefinedParameters {}
}