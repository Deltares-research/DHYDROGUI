using System;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Properties;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    /// <summary>
    /// Builder that builds an <see cref="NwrwDryWeatherFlowDefinition"/> from a <see cref="SobekRRDryWeatherFlow"/>.
    /// </summary>
    public class NwrwDryWeatherFlowDefinitionBuilder
    {
        /// <summary>
        /// Builds an <see cref="NwrwDryWeatherFlowDefinition"/> from a <see cref="SobekRRDryWeatherFlow"/>.
        /// </summary>
        /// <param name="readDefinition"> The Sobek RR dry weather flow definition that was read from file. </param>
        /// <param name="logHandler"> The log handler to report user messages with. </param>
        /// <returns>
        /// The built <see cref="NwrwDryWeatherFlowDefinition"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="readDefinition"/> is <c>null</c>.
        /// </exception>
        public NwrwDryWeatherFlowDefinition Build(SobekRRDryWeatherFlow readDefinition, ILogHandler logHandler = null)
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
                logHandler?.ReportWarningFormat(Resources.NwrwDryWeatherFlowDefinitionBuilder_Build_Expected_24_values_but_got__0__values__Skipping_import_of_water_use_per_capita_per_hour,
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