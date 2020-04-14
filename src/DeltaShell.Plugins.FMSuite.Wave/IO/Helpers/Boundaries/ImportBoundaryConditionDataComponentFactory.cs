using System;
using System.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries
{
    /// <summary>
    /// Helper factory for creating an <see cref="IBoundaryConditionDataComponent" /> from import data.
    /// </summary>
    /// <seealso cref="IImportBoundaryConditionDataComponentFactory" />
    public class ImportBoundaryConditionDataComponentFactory : IImportBoundaryConditionDataComponentFactory
    {
        private readonly IBoundaryParametersFactory parametersFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportBoundaryConditionDataComponentFactory" /> class.
        /// </summary>
        /// <param name="parametersFactory"> The parameters factory. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="parametersFactory" /> is <c> null </c>.
        /// </exception>
        public ImportBoundaryConditionDataComponentFactory(IBoundaryParametersFactory parametersFactory)
        {
            Ensure.NotNull(parametersFactory, nameof(parametersFactory));

            this.parametersFactory = parametersFactory;
        }

        public UniformDataComponent<ConstantParameters<TSpreading>> CreateUniformConstantComponent<TSpreading>(
            ParametersBlock parametersBlock) where TSpreading : class, IBoundaryConditionSpreading, new()
        {
            ConstantParameters<TSpreading> parameters = parametersBlock != null
                                                            ? CreateConstantParameters<TSpreading>(parametersBlock)
                                                            : parametersFactory.ConstructDefaultConstantParameters<TSpreading>();

            return new UniformDataComponent<ConstantParameters<TSpreading>>(parameters);
        }

        public UniformDataComponent<TimeDependentParameters<TSpreading>> CreateUniformTimeDependentComponent<TSpreading>(
            IWaveEnergyFunction<TSpreading> waveEnergyFunction) where TSpreading : class, IBoundaryConditionSpreading, new()
        {
            Ensure.NotNull(waveEnergyFunction, nameof(waveEnergyFunction));

            TimeDependentParameters<TSpreading> timeDependentParameters =
                parametersFactory.ConstructTimeDependentParameters(waveEnergyFunction);

            return new UniformDataComponent<TimeDependentParameters<TSpreading>>(timeDependentParameters);
        }

        public SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>> CreateSpatiallyVaryingConstantComponent<TSpreading>(
            IEnumerable<Tuple<SupportPoint, ParametersBlock>> dataPerSupportPoint) where TSpreading : class, IBoundaryConditionSpreading, new()
        {
            Ensure.NotNull(dataPerSupportPoint, nameof(dataPerSupportPoint));

            var dataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();

            foreach (Tuple<SupportPoint, ParametersBlock> v in dataPerSupportPoint)
            {
                dataComponent.AddParameters(v.Item1, CreateConstantParameters<TSpreading>(v.Item2));
            }

            return dataComponent;
        }

        public SpatiallyVaryingDataComponent<TimeDependentParameters<TSpreading>> CreateSpatiallyVaryingTimeDependentComponent<TSpreading>(
            IEnumerable<Tuple<SupportPoint, IWaveEnergyFunction<TSpreading>>> dataPerSupportPoint) where TSpreading : class, IBoundaryConditionSpreading, new()

        {
            Ensure.NotNull(dataPerSupportPoint, nameof(dataPerSupportPoint));

            var dataComponent = new SpatiallyVaryingDataComponent<TimeDependentParameters<TSpreading>>();
            foreach (Tuple<SupportPoint, IWaveEnergyFunction<TSpreading>> v in dataPerSupportPoint)
            {
                dataComponent.AddParameters(v.Item1, parametersFactory.ConstructTimeDependentParameters(v.Item2));
            }

            return dataComponent;
        }

        private ConstantParameters<TSpreading> CreateConstantParameters<TSpreading>(ParametersBlock parametersBlock)
            where TSpreading : class, IBoundaryConditionSpreading, new()
        {
            var spreading = SpreadingConversion.FromDouble<TSpreading>(parametersBlock.DirectionalSpreading);

            ConstantParameters<TSpreading> parameters = parametersFactory.ConstructConstantParameters(
                parametersBlock.WaveHeight,
                parametersBlock.Period,
                parametersBlock.Direction,
                spreading);

            return parameters;
        }
    }
}