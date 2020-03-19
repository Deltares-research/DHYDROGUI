using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters
{
    /// <summary>
    /// <see cref="BoundaryParametersFactory"/> provides the interface with which to construct
    /// <see cref="IBoundaryConditionParameters"/>.
    /// </summary>
    public sealed class BoundaryParametersFactory : IBoundaryParametersFactory
    {
        public ConstantParameters<TSpreading> ConstructDefaultConstantParameters<TSpreading>() 
            where TSpreading : class, IBoundaryConditionSpreading, new() =>
            ConstructConstantParameters(0.0, 1.0, 0.0, new TSpreading());

        public ConstantParameters<TSpreading> ConstructConstantParameters<TSpreading>(double height, 
                                                                                      double period, 
                                                                                      double direction,
                                                                                      TSpreading spreading)
            where TSpreading : class, IBoundaryConditionSpreading, new()
        {
            return new ConstantParameters<TSpreading>(height, period, direction, spreading);
        }

        public ConstantParameters<TNewSpreading> ConvertConstantParameters<TOldSpreading, TNewSpreading>(ConstantParameters<TOldSpreading> parameters) 
            where TOldSpreading : class, IBoundaryConditionSpreading, new() 
            where TNewSpreading : class, IBoundaryConditionSpreading, new()
        {
            Ensure.NotNull(parameters, nameof(parameters));
            return ConstructConstantParameters<TNewSpreading>(parameters.Height, 
                                                              parameters.Period,
                                                              parameters.Direction,
                                                              new TNewSpreading());
        }

        public TimeDependentParameters<TSpreading> ConstructDefaultTimeDependentParameters<TSpreading>()
            where TSpreading : class, IBoundaryConditionSpreading, new() =>
            ConstructTimeDependentParameters(new WaveEnergyFunction<TSpreading>());

        public TimeDependentParameters<TSpreading> ConstructTimeDependentParameters<TSpreading>(IWaveEnergyFunction<TSpreading> waveEnergyFunction)
            where TSpreading : class, IBoundaryConditionSpreading, new() =>
            new TimeDependentParameters<TSpreading>(waveEnergyFunction);
    }
}