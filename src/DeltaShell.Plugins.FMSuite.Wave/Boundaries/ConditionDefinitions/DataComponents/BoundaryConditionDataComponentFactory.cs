using System;
using System.Collections.Generic;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents
{
    /// <summary>
    /// <see cref="IBoundaryConditionDataComponentFactory"/> provides the
    /// methods to create new <see cref="IBoundaryConditionDataComponent"/>
    /// instances.
    /// </summary>
    public sealed class BoundaryConditionDataComponentFactory : IBoundaryConditionDataComponentFactory
    {
        private readonly IBoundaryParametersFactory parametersFactory;

        private readonly IDictionary<Type, Func<IBoundaryConditionDataComponent>> constructionMap =
            new Dictionary<Type, Func<IBoundaryConditionDataComponent>>();

        /// <summary>
        /// Creates a new <see cref="BoundaryConditionDataComponentFactory"/>.
        /// </summary>
        /// <param name="parameterFactory">The <see cref="IBoundaryParametersFactory"/> with which parameters are constructed.</param>
        public BoundaryConditionDataComponentFactory(IBoundaryParametersFactory parameterFactory)
        {
            Ensure.NotNull(parameterFactory, nameof(parameterFactory));
            this.parametersFactory = parameterFactory;

            InitialiseConstructionMethods();
        }

        private void InitialiseConstructionMethods()
        {
            constructionMap.Add(typeof(UniformDataComponent<ConstantParameters>), ConstructUniformConstantComponent);
            constructionMap.Add(typeof(SpatiallyVaryingDataComponent<ConstantParameters>), ConstructSpatiallyVaryingConstantDataComponent);
        }

        public T ConstructDefaultDataComponent<T>() where T : class, IBoundaryConditionDataComponent
        {

            if (!constructionMap.ContainsKey(typeof(T)))
            {
                throw new NotSupportedException($"{typeof(T)} is currently not supported.");
            }

            return constructionMap[typeof(T)].Invoke() as T;
        }

        private UniformDataComponent<ConstantParameters> ConstructUniformConstantComponent() =>
            new UniformDataComponent<ConstantParameters>(parametersFactory.ConstructDefaultConstantParameters());

        private SpatiallyVaryingDataComponent<ConstantParameters> ConstructSpatiallyVaryingConstantDataComponent() => 
            new SpatiallyVaryingDataComponent<ConstantParameters>();
    }
}