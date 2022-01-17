using System;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.ImportExport.Sobek.Properties;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Plugins.ImportExport.Sobek.Builders
{
    /// <summary>
    /// Builder that builds an <see cref="NwrwDryWeatherFlowDefinition"/> from a <see cref="SobekRRDryWeatherFlow"/>.
    /// </summary>
    public class NwrwDryWeatherFlowDefinitionBuilder
    {
        private readonly ILogHandler logHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="NwrwDryWeatherFlowDefinitionBuilder"/> class.
        /// </summary>
        /// <param name="logHandler"> The log handler to report user messages with. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="logHandler"/> is <c>null</c>.
        /// </exception>
        public NwrwDryWeatherFlowDefinitionBuilder(ILogHandler logHandler)
        {
            Ensure.NotNull(logHandler, nameof(logHandler));

            this.logHandler = logHandler;
        }

        /// <summary>
        /// Builds an <see cref="NwrwDryWeatherFlowDefinition"/> from a <see cref="SobekRRDryWeatherFlow"/>.
        /// </summary>
        /// <param name="readDefinition"> The Sobek RR dry weather flow definition that was read from file. </param>
        /// <returns>
        /// The built <see cref="NwrwDryWeatherFlowDefinition"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="readDefinition"/> is <c>null</c>.
        /// </exception>
        public NwrwDryWeatherFlowDefinition Build(SobekRRDryWeatherFlow readDefinition)
        {
            Ensure.NotNull(readDefinition, nameof(readDefinition));

            var newDefinition = new NwrwDryWeatherFlowDefinition
            {
                Name = readDefinition.Id,
                DistributionType = GetDistributionType(readDefinition.ComputationOption),
                DailyVolumeConstant = readDefinition.WaterUsePerHourForConstant * 24,
                DailyVolumeVariable = readDefinition.WaterUsePerDayForVariable
            };

            int nWaterCapacityPerHourValues = readDefinition.WaterCapacityPerHour.Length;
            if (nWaterCapacityPerHourValues != 24)
            {
                logHandler.ReportWarningFormat(Resources.NwrwDryWeatherFlowDefinitionBuilder_WarningIncorrectNumberOfWaterCapacityPerHourValues,
                                               nWaterCapacityPerHourValues);
            }
            else
            {
                newDefinition.HourlyPercentageDailyVolume = readDefinition.WaterCapacityPerHour;
            }

            return newDefinition;
        }

        private static DryweatherFlowDistributionType GetDistributionType(DWAComputationOption computationOption)
        {
            switch (computationOption)
            {
                case DWAComputationOption.NrPeopleTimesConstantPerHour:
                case DWAComputationOption.ConstantDWAPerHour:
                    return DryweatherFlowDistributionType.Constant;
                case DWAComputationOption.NrPeopleTimesVariablePerHour:
                case DWAComputationOption.VariablePerHour:
                    return DryweatherFlowDistributionType.Daily;
                case DWAComputationOption.UseTable:
                default:
                    throw new NotSupportedException($"{computationOption} is not a valid computation option.");
            }
        }
    }
}