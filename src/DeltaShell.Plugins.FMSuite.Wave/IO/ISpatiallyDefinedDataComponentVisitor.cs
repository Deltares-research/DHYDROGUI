using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    /// <summary>
    /// <see cref="ISpatiallyDefinedDataComponentVisitor"/> contains visit methods for different spatially
    /// defined data components.
    /// </summary>
    public interface ISpatiallyDefinedDataComponentVisitor
    {
        /// <summary>
        /// Visit method for defining actions of visitors when they visit a <see cref="UniformDataComponent{T}"/>
        /// </summary>
        /// <typeparam name="T"> The forcing type. </typeparam>
        /// <param name="uniformDataComponent"> The visited <see cref="UniformDataComponent{T}"/></param>
        void Visit<T>(UniformDataComponent<T> uniformDataComponent) where T : IForcingTypeDefinedParameters;

        /// <summary>
        /// Visit method for defining actions of visitors when they visit a <see cref="SpatiallyVaryingDataComponent{T}"/>
        /// </summary>
        /// <typeparam name="T"> The forcing type.</typeparam>
        /// <param name="spatiallyVaryingDataComponent"> The visited <see cref="SpatiallyVaryingDataComponent{T}"/></param>
        void Visit<T>(SpatiallyVaryingDataComponent<T> spatiallyVaryingDataComponent) where T : IForcingTypeDefinedParameters;
    }
}