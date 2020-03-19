using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
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

        public TimeDependentParameters ConstructDefaultTimeDependentParameters<TSpreading>()
            where TSpreading : class, IBoundaryConditionSpreading, new()
        {
            IFunction waveEnergyFunction = ConstructEmptyWaveEnergyFunction<TSpreading>();
            return ConstructTimeDependentParameters(waveEnergyFunction);
        }

        public TimeDependentParameters ConstructTimeDependentParameters(IFunction waveEnergyFunction)
        {
            return new TimeDependentParameters(waveEnergyFunction);
        }

        private static IFunction ConstructEmptyWaveEnergyFunction<TSpreading>()
            where TSpreading : class, IBoundaryConditionSpreading, new()
        {
            var function = new Function(WaveParametersConstants.WaveQuantityName);

            function.Arguments.Add(new Variable<DateTime>(WaveParametersConstants.TimeVariableName));

            function.Components.Add(GetHeightVariable());
            function.Components.Add(GetPeriodVariable());
            function.Components.Add(GetDirectionVariable());
            function.Components.Add(GetSpreadingVariable<TSpreading>());

            function.Attributes[BcwFile.TimeFunctionAttributeName] = WaveParametersConstants.NonEquidistantTimeFunctionAttributeName;
            function.Attributes[BcwFile.RefDateAttributeName] = new DateTime().ToString(BcwFile.DateFormatString);
            function.Attributes[BcwFile.TimeUnitAttributeName] = WaveParametersConstants.MinuteUnitName;

            return function;
        }

        private static Variable<double> GetHeightVariable() => 
            new Variable<double>(WaveParametersConstants.HeightVariableName,
                                 WaveParametersConstants.ConstructMeterUnit());

        private static Variable<double> GetPeriodVariable() =>
            new Variable<double>(WaveParametersConstants.PeriodVariableName,
                                 WaveParametersConstants.ConstructSecondUnit()) {DefaultValue = 1.0};

        private static Variable<double> GetDirectionVariable() =>
            new Variable<double>(WaveParametersConstants.DirectionVariableName,
                                 WaveParametersConstants.ConstructDegreesUnit());

        private static Variable<double> GetSpreadingVariable<TSpreading>()
            where TSpreading : class, IBoundaryConditionSpreading, new() =>
            new Variable<double>(WaveParametersConstants.SpreadingVariableName,
                                 SpreadingConversion.GetSpreadingUnit<TSpreading>()) {DefaultValue = SpreadingConversion.GetSpreadingDefaultValue<TSpreading>()};
    }
}