using System;
using System.Collections.Generic;
using DelftTools.Utils.Guards;
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
        /// <param name="parametersFactory">The <see cref="IBoundaryParametersFactory"/> with which parameters are constructed.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="parametersFactory"/> is <c>null</c>.
        /// </exception>
        public BoundaryConditionDataComponentFactory(IBoundaryParametersFactory parametersFactory)
        {
            Ensure.NotNull(parametersFactory, nameof(parametersFactory));
            this.parametersFactory = parametersFactory;

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
            constructionMap.Add(typeof(UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>>), 
                                ConstructUniformTimeDependentComponent<PowerDefinedSpreading>);
            constructionMap.Add(typeof(UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>), 
                                ConstructUniformTimeDependentComponent<DegreesDefinedSpreading>);
            constructionMap.Add(typeof(SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>>), 
                                ConstructSpatiallyVaryingTimeDependentDataComponent<PowerDefinedSpreading>);
            constructionMap.Add(typeof(SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>), 
                                ConstructSpatiallyVaryingTimeDependentDataComponent<DegreesDefinedSpreading>);
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

        private UniformDataComponent<TimeDependentParameters<TSpreading>> ConstructUniformTimeDependentComponent<TSpreading>()
            where TSpreading : class, IBoundaryConditionSpreading, new() =>
            new UniformDataComponent<TimeDependentParameters<TSpreading>>(parametersFactory.ConstructDefaultTimeDependentParameters<TSpreading>());

        private SpatiallyVaryingDataComponent<TimeDependentParameters<TSpreading>> ConstructSpatiallyVaryingTimeDependentDataComponent<TSpreading>()
            where TSpreading : class, IBoundaryConditionSpreading, new() =>
            new SpatiallyVaryingDataComponent<TimeDependentParameters<TSpreading>>();


        public IBoundaryConditionDataComponent ConvertDataComponentSpreading<TOldSpreading, TNewSpreading>(IBoundaryConditionDataComponent oldDataComponent) 
            where TOldSpreading : class, IBoundaryConditionSpreading, new() 
            where TNewSpreading : class, IBoundaryConditionSpreading, new()
        {
            Ensure.NotNull(oldDataComponent, nameof(oldDataComponent));

            if (typeof(TOldSpreading) == typeof(TNewSpreading))
            {
                throw new InvalidOperationException("Cannot convert to the same type.");
            }

            switch (oldDataComponent)
            {
                case UniformDataComponent<ConstantParameters<TOldSpreading>> uniformDataComponent:
                    return ConvertUniformConstantDataComponent<TOldSpreading, TNewSpreading>(uniformDataComponent);
                case SpatiallyVaryingDataComponent<ConstantParameters<TOldSpreading>> spatiallyVaryingDataComponent:
                    return ConvertSpatiallyVaryingConstantDataComponent<TOldSpreading, TNewSpreading>(spatiallyVaryingDataComponent);
                default:
                    throw new NotSupportedException($"The specified oldDataComponent could not be cast.");
            }
        }

        private IBoundaryConditionDataComponent ConvertUniformConstantDataComponent<TOldSpreading, TNewSpreading>(
            UniformDataComponent<ConstantParameters<TOldSpreading>> dataComponent) 
            where TOldSpreading : class, IBoundaryConditionSpreading, new() 
            where TNewSpreading : class, IBoundaryConditionSpreading, new()
        {
            ConstantParameters<TNewSpreading> newParameters = parametersFactory.ConvertConstantParameters<TOldSpreading, TNewSpreading>(dataComponent.Data);
            return new UniformDataComponent<ConstantParameters<TNewSpreading>>(newParameters);
        }

        private IBoundaryConditionDataComponent ConvertSpatiallyVaryingConstantDataComponent<TOldSpreading, TNewSpreading>(
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