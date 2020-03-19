using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions
{
    /// <summary>
    /// <see cref="WaveEnergyFunction{TSpreading}"/> implements the WaveEnergyFunction as used within a
    /// <see cref="TimeDependentParameters"/>.
    /// </summary>
    /// <typeparam name="TSpreading">The type of the spreading.</typeparam>
    public class WaveEnergyFunction<TSpreading> : IWaveEnergyFunction<TSpreading>  
        where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        public IFunction UnderlyingFunction { get; } = ConstructEmptyWaveEnergyFunction();
        public IVariable<DateTime> TimeArgument => (IVariable<DateTime>) UnderlyingFunction.Arguments[0];
        public IVariable<double> HeightComponent => (IVariable<double>) UnderlyingFunction.Components[0];
        public IVariable<double> PeriodComponent => (IVariable<double>) UnderlyingFunction.Components[1];
        public IVariable<double> DirectionComponent => (IVariable<double>) UnderlyingFunction.Components[2];
        public IVariable<double> SpreadingComponent => (IVariable<double>) UnderlyingFunction.Components[3];

        private static IFunction ConstructEmptyWaveEnergyFunction()
        {
            var function = new Function(WaveParametersConstants.WaveQuantityName);

            function.Arguments.Add(new Variable<DateTime>(WaveParametersConstants.TimeVariableName));

            function.Components.Add(GetHeightVariable());
            function.Components.Add(GetPeriodVariable());
            function.Components.Add(GetDirectionVariable());
            function.Components.Add(GetSpreadingVariable());

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

        private static Variable<double> GetSpreadingVariable() =>
            new Variable<double>(WaveParametersConstants.SpreadingVariableName,
                                 SpreadingConversion.GetSpreadingUnit<TSpreading>()) {DefaultValue = SpreadingConversion.GetSpreadingDefaultValue<TSpreading>()};
    }
}