using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    public interface IDataComponentVisitor
    {
        /// <summary>
        /// Visit method for defining actions of visitors when they visit a <see cref="UniformDataComponent{T}"/>
        /// </summary>
        /// <typeparam name="T"> An <see cref="IBoundaryConditionParameters"/> object</typeparam>
        /// <param name="uniformDataComponent"> The visited <see cref="UniformDataComponent{T}"/></param>
        void Visit<T>(UniformDataComponent<T> uniformDataComponent) where T : IBoundaryConditionParameters;

        /// <summary>
        /// Visit method for defining actions of visitors when they visit a <see cref="SpatiallyVaryingDataComponent{T}"/>
        /// </summary>
        /// <typeparam name="T"> An <see cref="IBoundaryConditionParameters"/> object</typeparam>
        /// <param name="spatiallyVaryingDataComponent"> The visited <see cref="SpatiallyVaryingDataComponent{T}"/></param>
        void Visit<T>(SpatiallyVaryingDataComponent<T> spatiallyVaryingDataComponent) where T : IBoundaryConditionParameters;
    }
}