using DelftTools.Units;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters
{
    /// <summary>
    /// <see cref="WaveTimeDependentParametersConstants"/> provides the constants used within the
    /// boundary condition parameters.
    /// </summary>
    public static class WaveTimeDependentParametersConstants
    {
        /// <summary>
        /// The wave quantity name.
        /// </summary>
        public const string WaveQuantityName = "wave_energy_density";

        /// <summary>
        /// The time variable name.
        /// </summary>
        public const string TimeVariableName = "Time";

        /// <summary>
        /// The height variable name.
        /// </summary>
        public const string HeightVariableName = "Hs";

        /// <summary>
        /// The period variable name.
        /// </summary>
        public const string PeriodVariableName = "Tp";

        /// <summary>
        /// The direction variable name.
        /// </summary>
        public const string DirectionVariableName = "Dir";

        /// <summary>
        /// The spreading variable name.
        /// </summary>
        public const string SpreadingVariableName = "Spreading";

        /// <summary>
        /// The minute unit name.
        /// </summary>
        public const string MinuteUnitName = "minutes";

        /// <summary>
        /// The meter unit name.
        /// </summary>
        public const string MeterUnitName = "meter";

        /// <summary>
        /// The meter unit symbol.
        /// </summary>
        public const string MeterUnitSymbol = "m";

        /// <summary>
        /// The second unit name.
        /// </summary>
        public const string SecondUnitName = "second";

        /// <summary>
        /// The second unit symbol.
        /// </summary>
        public const string SecondUnitSymbol = "s";

        /// <summary>
        /// The degrees unit name.
        /// </summary>
        public const string DegreesUnitName = "degrees";

        /// <summary>
        /// The degrees unit symbol.
        /// </summary>
        public const string DegreesUnitSymbol = "deg";

        /// <summary>
        /// The power unit name.
        /// </summary>
        public const string PowerUnitName = "power";

        /// <summary>
        /// The power unit symbol.
        /// </summary>
        public const string PowerUnitSymbol = "-";

        /// <summary>
        /// The non equidistant time function attribute name.
        /// </summary>
        public const string NonEquidistantTimeFunctionAttributeName = "non-equidistant";

        /// <summary>
        /// Constructs a new meter <see cref="Unit"/>.
        /// </summary>
        /// <returns>
        /// A new meter <see cref="Unit"/>.
        /// </returns>
        public static Unit ConstructMeterUnit() => new Unit(MeterUnitName,
                                                            MeterUnitSymbol);

        /// <summary>
        /// Construct a new second <see cref="Unit"/>.
        /// </summary>
        /// <returns>
        /// A new second <see cref="Unit"/>.
        /// </returns>
        public static Unit ConstructSecondUnit() => new Unit(SecondUnitName,
                                                             SecondUnitSymbol);

        /// <summary>
        /// Constructs a new degrees <see cref="Unit"/>.
        /// </summary>
        /// <returns>
        /// A new degrees <see cref="Unit"/>.
        /// </returns>
        public static Unit ConstructDegreesUnit() => new Unit(DegreesUnitName,
                                                              DegreesUnitSymbol);

        /// <summary>
        /// Constructs a new Power <see cref="Unit"/>.
        /// </summary>
        /// <returns>
        /// A new power <see cref="Unit"/>.
        /// </returns>
        public static Unit ConstructPowerUnit() => new Unit(PowerUnitName,
                                                            PowerUnitSymbol);
    }
}