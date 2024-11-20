using System;
using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents
{
    /// <summary>
    /// <see cref="ISpatiallyDefinedDataComponentFactory"/> provides the
    /// methods to create new <see cref="ISpatiallyDefinedDataComponent"/>
    /// instances.
    /// </summary>
    public sealed class SpatiallyDefinedDataComponentFactory : ISpatiallyDefinedDataComponentFactory
    {
        private readonly IForcingTypeDefinedParametersFactory parametersFactory;

        private readonly IDictionary<Type, Func<ISpatiallyDefinedDataComponent>> constructionMap =
            new Dictionary<Type, Func<ISpatiallyDefinedDataComponent>>();

        /// <summary>
        /// Creates a new <see cref="SpatiallyDefinedDataComponentFactory"/>.
        /// </summary>
        /// <param name="parametersFactory">
        /// The <see cref="IForcingTypeDefinedParametersFactory"/> with which parameters are
        /// constructed.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="parametersFactory"/> is <c>null</c>.
        /// </exception>
        public SpatiallyDefinedDataComponentFactory(IForcingTypeDefinedParametersFactory parametersFactory)
        {
            Ensure.NotNull(parametersFactory, nameof(parametersFactory));
            this.parametersFactory = parametersFactory;

            InitialiseConstructionMethods();
        }

        public T ConstructDefaultDataComponent<T>() where T : class, ISpatiallyDefinedDataComponent
        {
            if (!constructionMap.ContainsKey(typeof(T)))
            {
                throw new NotSupportedException($"{typeof(T)} is currently not supported.");
            }

            return constructionMap[typeof(T)].Invoke() as T;
        }

        public ISpatiallyDefinedDataComponent ConvertDataComponentSpreading<TOldSpreading, TNewSpreading>(ISpatiallyDefinedDataComponent oldDataComponent)
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
                case UniformDataComponent<TimeDependentParameters<TOldSpreading>> uniformDataComponent:
                    return ConvertUniformTimeDependentDataComponent<TOldSpreading, TNewSpreading>(uniformDataComponent);
                case SpatiallyVaryingDataComponent<TimeDependentParameters<TOldSpreading>> spatiallyVaryingDataComponent:
                    return ConvertSpatiallyVaryingTimeDependentDataComponent<TOldSpreading, TNewSpreading>(spatiallyVaryingDataComponent);
                default:
                    throw new NotSupportedException($"The specified oldDataComponent could not be cast.");
            }
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
            constructionMap.Add(typeof(UniformDataComponent<FileBasedParameters>),
                                ConstructUniformFileBasedComponent);
            constructionMap.Add(typeof(SpatiallyVaryingDataComponent<FileBasedParameters>),
                                ConstructSpatiallyVaryingFileBasedDataComponent);
        }

        private UniformDataComponent<ConstantParameters<TSpreading>> ConstructUniformConstantComponent<TSpreading>()
            where TSpreading : class, IBoundaryConditionSpreading, new() =>
            new UniformDataComponent<ConstantParameters<TSpreading>>(parametersFactory.ConstructDefaultConstantParameters<TSpreading>());

        private static SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>> ConstructSpatiallyVaryingConstantDataComponent<TSpreading>()
            where TSpreading : class, IBoundaryConditionSpreading, new() =>
            new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();

        private UniformDataComponent<TimeDependentParameters<TSpreading>> ConstructUniformTimeDependentComponent<TSpreading>()
            where TSpreading : class, IBoundaryConditionSpreading, new() =>
            new UniformDataComponent<TimeDependentParameters<TSpreading>>(parametersFactory.ConstructDefaultTimeDependentParameters<TSpreading>());

        private static SpatiallyVaryingDataComponent<TimeDependentParameters<TSpreading>> ConstructSpatiallyVaryingTimeDependentDataComponent<TSpreading>()
            where TSpreading : class, IBoundaryConditionSpreading, new() =>
            new SpatiallyVaryingDataComponent<TimeDependentParameters<TSpreading>>();

        private UniformDataComponent<FileBasedParameters> ConstructUniformFileBasedComponent() =>
            new UniformDataComponent<FileBasedParameters>(parametersFactory.ConstructDefaultFileBasedParameters());

        private static SpatiallyVaryingDataComponent<FileBasedParameters> ConstructSpatiallyVaryingFileBasedDataComponent() =>
            new SpatiallyVaryingDataComponent<FileBasedParameters>();

        private ISpatiallyDefinedDataComponent ConvertUniformConstantDataComponent<TOldSpreading, TNewSpreading>(
            UniformDataComponent<ConstantParameters<TOldSpreading>> dataComponent)
            where TOldSpreading : class, IBoundaryConditionSpreading, new()
            where TNewSpreading : class, IBoundaryConditionSpreading, new()
        {
            ConstantParameters<TNewSpreading> newParameters =
                parametersFactory.ConvertConstantParameters<TOldSpreading, TNewSpreading>(dataComponent.Data);
            return new UniformDataComponent<ConstantParameters<TNewSpreading>>(newParameters);
        }

        private ISpatiallyDefinedDataComponent ConvertSpatiallyVaryingConstantDataComponent<TOldSpreading, TNewSpreading>(
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

        private ISpatiallyDefinedDataComponent ConvertUniformTimeDependentDataComponent<TOldSpreading, TNewSpreading>(
            UniformDataComponent<TimeDependentParameters<TOldSpreading>> dataComponent)
            where TOldSpreading : class, IBoundaryConditionSpreading, new()
            where TNewSpreading : class, IBoundaryConditionSpreading, new()
        {
            TimeDependentParameters<TNewSpreading> newParameters =
                parametersFactory.ConvertTimeDependentParameters<TOldSpreading, TNewSpreading>(dataComponent.Data);
            return new UniformDataComponent<TimeDependentParameters<TNewSpreading>>(newParameters);
        }

        private ISpatiallyDefinedDataComponent ConvertSpatiallyVaryingTimeDependentDataComponent<TOldSpreading, TNewSpreading>(
            SpatiallyVaryingDataComponent<TimeDependentParameters<TOldSpreading>> dataComponent)
            where TOldSpreading : class, IBoundaryConditionSpreading, new()
            where TNewSpreading : class, IBoundaryConditionSpreading, new()
        {
            var newDataComponent = new SpatiallyVaryingDataComponent<TimeDependentParameters<TNewSpreading>>();

            foreach (KeyValuePair<SupportPoint, TimeDependentParameters<TOldSpreading>> supportPointParameterPair in dataComponent.Data)
            {
                TimeDependentParameters<TNewSpreading> convertedParameters = parametersFactory.ConvertTimeDependentParameters<TOldSpreading, TNewSpreading>(supportPointParameterPair.Value);
                newDataComponent.AddParameters(supportPointParameterPair.Key, convertedParameters);
            }

            return newDataComponent;
        }
    }
}