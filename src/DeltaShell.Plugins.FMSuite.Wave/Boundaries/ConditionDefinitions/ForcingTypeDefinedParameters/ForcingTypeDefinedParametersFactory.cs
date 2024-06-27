using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters
{
    /// <summary>
    /// <see cref="ForcingTypeDefinedParametersFactory"/> provides the interface with which to construct
    /// <see cref="IForcingTypeDefinedParameters"/>.
    /// </summary>
    public sealed class ForcingTypeDefinedParametersFactory : IForcingTypeDefinedParametersFactory
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
            return ConstructConstantParameters(parameters.Height,
                                               parameters.Period,
                                               parameters.Direction,
                                               new TNewSpreading());
        }

        public TimeDependentParameters<TSpreading> ConstructDefaultTimeDependentParameters<TSpreading>()
            where TSpreading : class, IBoundaryConditionSpreading, new() =>
            ConstructTimeDependentParameters(new WaveEnergyFunction<TSpreading>());

        public TimeDependentParameters<TSpreading> ConstructTimeDependentParameters<TSpreading>(IWaveEnergyFunction<TSpreading> waveEnergyFunction)
            where TSpreading : class, IBoundaryConditionSpreading, new()
        {
            Ensure.NotNull(waveEnergyFunction, nameof(waveEnergyFunction));
            return new TimeDependentParameters<TSpreading>(waveEnergyFunction);
        }

        public TimeDependentParameters<TNewSpreading> ConvertTimeDependentParameters<TOldSpreading, TNewSpreading>(TimeDependentParameters<TOldSpreading> parameters)
            where TOldSpreading : class, IBoundaryConditionSpreading, new()
            where TNewSpreading : class, IBoundaryConditionSpreading, new()
        {
            Ensure.NotNull(parameters, nameof(parameters));
            return ConstructTimeDependentParameters(WaveEnergyFunction<TNewSpreading>.ConvertSpreadingType(parameters.WaveEnergyFunction));
        }

        public FileBasedParameters ConstructDefaultFileBasedParameters()
        {
            return new FileBasedParameters(string.Empty);
        }
    }
}