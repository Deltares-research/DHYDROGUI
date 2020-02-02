using System;
using System.Collections.Generic;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

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
            constructionMap.Add(typeof(UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>), 
                                ConstructUniformConstantComponent<PowerDefinedSpreading>);
            constructionMap.Add(typeof(UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>>), 
                                ConstructUniformConstantComponent<DegreesDefinedSpreading>);
            constructionMap.Add(typeof(SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>), 
                                ConstructSpatiallyVaryingConstantDataComponent<PowerDefinedSpreading>);
            constructionMap.Add(typeof(SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>>), 
                                ConstructSpatiallyVaryingConstantDataComponent<DegreesDefinedSpreading>);
        }

        public T ConstructDefaultDataComponent<T>() where T : class, IBoundaryConditionDataComponent
        {

            if (!constructionMap.ContainsKey(typeof(T)))
            {
                throw new NotSupportedException($"{typeof(T)} is currently not supported.");
            }

            return constructionMap[typeof(T)].Invoke() as T;
        }

        private UniformDataComponent<ConstantParameters<TSpreading>> ConstructUniformConstantComponent<TSpreading>()
            where TSpreading : class, IBoundaryConditionSpreading, new() =>
            new UniformDataComponent<ConstantParameters<TSpreading>>(parametersFactory.ConstructDefaultConstantParameters<TSpreading>());

        private SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>> ConstructSpatiallyVaryingConstantDataComponent<TSpreading>()
            where TSpreading : class, IBoundaryConditionSpreading, new() =>
            new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();

        public IBoundaryConditionDataComponent ConvertDataComponentSpreading<TOldSpreading, TNewSpreading>(IBoundaryConditionDataComponent oldDataComponent) 
            where TOldSpreading : class, IBoundaryConditionSpreading, new() 
            where TNewSpreading : class, IBoundaryConditionSpreading, new()
        {
            if (typeof(TOldSpreading) == typeof(TNewSpreading))
            {
                throw new InvalidOperationException("Cannot convert to the same type.");
            }

            switch (oldDataComponent)
            {
                case UniformDataComponent<ConstantParameters<TOldSpreading>> uniformDataComponent:
                    return ConvertUniformDataComponent<TOldSpreading, TNewSpreading>(uniformDataComponent);
                case SpatiallyVaryingDataComponent<ConstantParameters<TOldSpreading>> spatiallyVaryingDataComponent:
                    return ConvertSpatiallyVaryingDataComponent<TOldSpreading, TNewSpreading>(spatiallyVaryingDataComponent);
                default:
                    throw new NotSupportedException($"The specified oldDataComponent could not be cast.");
            }
        }

        private IBoundaryConditionDataComponent ConvertUniformDataComponent<TOldSpreading, TNewSpreading>(
            UniformDataComponent<ConstantParameters<TOldSpreading>> dataComponent) 
            where TOldSpreading : class, IBoundaryConditionSpreading, new() 
            where TNewSpreading : class, IBoundaryConditionSpreading, new()
        {
            ConstantParameters<TNewSpreading> newParameters = parametersFactory.ConvertConstantParameters<TOldSpreading, TNewSpreading>(dataComponent.Data);
            return new UniformDataComponent<ConstantParameters<TNewSpreading>>(newParameters);
        }

        private IBoundaryConditionDataComponent ConvertSpatiallyVaryingDataComponent<TOldSpreading, TNewSpreading>(
            SpatiallyVaryingDataComponent<ConstantParameters<TOldSpreading>> dataComponent) 
            where TOldSpreading : class, IBoundaryConditionSpreading, new() 
            where TNewSpreading : class, IBoundaryConditionSpreading, new()
        {
            var newDataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<TNewSpreading>>();

            foreach (KeyValuePair<SupportPoint, ConstantParameters<TOldSpreading>> supportPointParameterPair in dataComponent.Data)
            {
                ConstantParameters<TNewSpreading> convertedParameters = parametersFactory.ConvertConstantParameters<TOldSpreading, TNewSpreading>(supportPointParameterPair.Value);
                newDataComponent.AddParameters(supportPointParameterPair.Key, convertedParameters);
            }

            return newDataComponent;
        }
    }
}