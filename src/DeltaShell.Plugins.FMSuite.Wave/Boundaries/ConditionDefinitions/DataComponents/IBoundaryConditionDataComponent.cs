namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents
{
    /// <summary>
    /// <see cref="IBoundaryConditionDataComponent"/> defines the different boundary
    /// condition data components used within the <see cref="IWaveBoundaryConditionDefinition"/>.
    ///
    /// Currently, the following are defined:
    ///
    /// * <see cref="UniformDataComponent{T}"/>
    /// * <see cref="SpatiallyVaryingDataComponent{T}"/>
    /// 
    /// </summary>
    /// <remarks>
    /// This interface acts as a discriminated union for its implementing types.
    /// As such, this interface is empty. Other classes will use it to type
    /// cast to the implementing types.
    /// </remarks>
    public interface IBoundaryConditionDataComponent : IVisitableBoundaryConditionDataComponent {}
}