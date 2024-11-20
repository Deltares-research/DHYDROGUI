using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions
{
    /// <summary>
    /// <see cref="WaveEnergyFunction{TSpreading}"/> implements the WaveEnergyFunction as used within a
    /// <see cref="TimeDependentParameters{TSpreading}"/>.
    /// </summary>
    /// <typeparam name="TSpreading">The type of the spreading.</typeparam>
    public class WaveEnergyFunction<TSpreading> : IWaveEnergyFunction<TSpreading>
        where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        /// <summary>
        /// Creates a new empty <see cref="WaveEnergyFunction{TSpreading}"/>.
        /// </summary>
        public WaveEnergyFunction() : this(ConstructEmptyWaveEnergyFunction()) {}

        private WaveEnergyFunction(IFunction underlyingFunction)
        {
            UnderlyingFunction = underlyingFunction;
        }

        public IFunction UnderlyingFunction { get; }
        public IVariable<DateTime> TimeArgument => (IVariable<DateTime>) UnderlyingFunction.Arguments[0];
        public IVariable<double> HeightComponent => (IVariable<double>) UnderlyingFunction.Components[0];
        public IVariable<double> PeriodComponent => (IVariable<double>) UnderlyingFunction.Components[1];
        public IVariable<double> DirectionComponent => (IVariable<double>) UnderlyingFunction.Components[2];
        public IVariable<double> SpreadingComponent => (IVariable<double>) UnderlyingFunction.Components[3];

        /// <summary>
        /// Converts the type of the spreading from the provided <paramref name="oldWaveEnergyFunction"/>.
        /// </summary>
        /// <typeparam name="TOldSpreading">The type of the old spreading.</typeparam>
        /// <param name="oldWaveEnergyFunction">The old wave function.</param>
        /// <returns>
        /// if <typeparamref name="TOldSpreading"/> != <typeparamref name="TSpreading"/>
        /// A new <see cref="IWaveEnergyFunction{TNewSpreading}"/> using the converted <paramref name="oldWaveEnergyFunction"/>'s
        /// underlying function.
        /// else
        /// <paramref name="oldWaveEnergyFunction"/>
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="oldWaveEnergyFunction"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Thrown when <typeparamref name="TOldSpreading"/> is not supported.
        /// </exception>
        /// <remarks>
        /// Note that this logically moves the underlying function from the provided <paramref name="oldWaveEnergyFunction"/>
        /// to the newly returned <see cref="IWaveEnergyFunction{TSpreading}"/>. The <paramref name="oldWaveEnergyFunction"/>
        /// should no longer be used.
        /// </remarks>
        public static IWaveEnergyFunction<TSpreading> ConvertSpreadingType<TOldSpreading>(IWaveEnergyFunction<TOldSpreading> oldWaveEnergyFunction)
            where TOldSpreading : class, IBoundaryConditionSpreading, new()
        {
            Ensure.NotNull(oldWaveEnergyFunction, nameof(oldWaveEnergyFunction));

            if (typeof(TSpreading) == typeof(TOldSpreading))
            {
                return (IWaveEnergyFunction<TSpreading>) oldWaveEnergyFunction;
            }

            double defaultValue = SpreadingConversion.GetSpreadingDefaultValue<TSpreading>();
            oldWaveEnergyFunction.SpreadingComponent.Unit = SpreadingConversion.GetSpreadingUnit<TSpreading>();
            oldWaveEnergyFunction.SpreadingComponent.DefaultValue = defaultValue;

            for (var i = 0; i < oldWaveEnergyFunction.SpreadingComponent.AllValues.Count; i++)
            {
                oldWaveEnergyFunction.SpreadingComponent.AllValues[i] = defaultValue;
            }

            return new WaveEnergyFunction<TSpreading>(oldWaveEnergyFunction.UnderlyingFunction);
        }

        private static IFunction ConstructEmptyWaveEnergyFunction()
        {
            var function = new Function(WaveTimeDependentParametersConstants.WaveQuantityName);

            function.Arguments.Add(new Variable<DateTime>(WaveTimeDependentParametersConstants.TimeVariableName));

            function.Components.Add(GetHeightVariable());
            function.Components.Add(GetPeriodVariable());
            function.Components.Add(GetDirectionVariable());
            function.Components.Add(GetSpreadingVariable());

            function.Attributes[BcwFile.TimeFunctionAttributeName] = WaveTimeDependentParametersConstants.NonEquidistantTimeFunctionAttributeName;
            function.Attributes[BcwFile.RefDateAttributeName] = new DateTime().ToString(BcwFile.DateFormatString);
            function.Attributes[BcwFile.TimeUnitAttributeName] = WaveTimeDependentParametersConstants.MinuteUnitName;

            return function;
        }

        private static Variable<double> GetHeightVariable() =>
            new Variable<double>(WaveTimeDependentParametersConstants.HeightVariableName,
                                 WaveTimeDependentParametersConstants.ConstructMeterUnit());

        private static Variable<double> GetPeriodVariable() =>
            new Variable<double>(WaveTimeDependentParametersConstants.PeriodVariableName,
                                 WaveTimeDependentParametersConstants.ConstructSecondUnit()) {DefaultValue = 1.0};

        private static Variable<double> GetDirectionVariable() =>
            new Variable<double>(WaveTimeDependentParametersConstants.DirectionVariableName,
                                 WaveTimeDependentParametersConstants.ConstructDegreesUnit());

        private static Variable<double> GetSpreadingVariable() =>
            new Variable<double>(WaveTimeDependentParametersConstants.SpreadingVariableName,
                                 SpreadingConversion.GetSpreadingUnit<TSpreading>()) {DefaultValue = SpreadingConversion.GetSpreadingDefaultValue<TSpreading>()};
    }
}