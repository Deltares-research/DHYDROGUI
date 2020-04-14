using System;
using System.Collections.Generic;
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
    public interface IImportBoundaryConditionDataComponentFactory
    {
        /// <summary>
        /// Constructs a uniform constant data component from the
        /// specified <paramref name="parametersBlock" />.
        /// </summary>
        /// <typeparam name="TSpreading"> The type of the spreading. </typeparam>
        /// <param name="parametersBlock"> The parameters block. </param>
        /// <returns>
        /// The constructed uniform constant data component, if <paramref name="parametersBlock"/>
        /// is specified; otherwise the data component with default constant parameters.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="parametersBlock"/> is <c>null</c>.
        /// </exception>
        UniformDataComponent<ConstantParameters<TSpreading>> CreateUniformConstantComponent<TSpreading>(ParametersBlock parametersBlock)
            where TSpreading : class, IBoundaryConditionSpreading, new();

        /// <summary>
        /// Constructs a uniform time dependent data component from the
        /// specified <paramref name="waveEnergyFunction" />.
        /// </summary>
        /// <typeparam name="TSpreading"> The type of the spreading. </typeparam>
        /// <param name="waveEnergyFunction"> The wave energy function. </param>
        /// <returns>
        /// The constructed uniform time dependent data component.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="waveEnergyFunction"/> is <c>null</c>.
        /// </exception>
        UniformDataComponent<TimeDependentParameters<TSpreading>> CreateUniformTimeDependentComponent<TSpreading>(IWaveEnergyFunction<TSpreading> waveEnergyFunction)
            where TSpreading : class, IBoundaryConditionSpreading, new();

        /// <summary>
        /// Constructs a spatially varying constant data component from the
        /// specified <paramref name="dataPerSupportPoint" />.
        /// </summary>
        /// <typeparam name="TSpreading"> The type of the spreading. </typeparam>
        /// <param name="dataPerSupportPoint"> The support points with their respective parameters block. </param>
        /// <returns>
        /// The constructed spatially varying constant data component.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="dataPerSupportPoint"/> is <c>null</c>.
        /// </exception>
        SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>> CreateSpatiallyVaryingConstantComponent<TSpreading>(IEnumerable<Tuple<SupportPoint, ParametersBlock>> dataPerSupportPoint)
            where TSpreading : class, IBoundaryConditionSpreading, new();

        /// <summary>
        /// Constructs a spatially varying time dependent data component from the
        /// specified <paramref name="dataPerSupportPoint" />.
        /// </summary>
        /// <typeparam name="TSpreading"> The type of the spreading. </typeparam>
        /// <param name="dataPerSupportPoint"> The support points with their respective wave energy function. </param>
        /// <returns>
        /// The constructed spatially varying time dependent data component.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="dataPerSupportPoint"/> is <c>null</c>.
        /// </exception>
        SpatiallyVaryingDataComponent<TimeDependentParameters<TSpreading>> CreateSpatiallyVaryingTimeDependentComponent<TSpreading>(IEnumerable<Tuple<SupportPoint, IWaveEnergyFunction<TSpreading>>> dataPerSupportPoint)
            where TSpreading : class, IBoundaryConditionSpreading, new();
    }
}