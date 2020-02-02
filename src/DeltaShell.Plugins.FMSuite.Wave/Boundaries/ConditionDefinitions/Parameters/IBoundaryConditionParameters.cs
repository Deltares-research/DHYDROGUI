namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters
{
    /// <summary>
    /// <see cref="IBoundaryConditionParameters"/> defines the different boundary
    /// condition parameters used within the <see cref="IWaveBoundaryConditionDefinition"/>.
    ///
    /// Currently, the following are defined:
    ///
    /// * <see cref="ConstantParameters{TSpreading}"/>
    /// * <see cref="TimeDependentParameters"/>
    /// 
    /// </summary>
    /// <remarks>
    /// This interface acts as a discriminated union for its implementing types.
    /// As such, this interface is empty. Other classes will use it to type
    /// cast to the implementing types.
    /// </remarks>
    public interface IBoundaryConditionParameters { }
}