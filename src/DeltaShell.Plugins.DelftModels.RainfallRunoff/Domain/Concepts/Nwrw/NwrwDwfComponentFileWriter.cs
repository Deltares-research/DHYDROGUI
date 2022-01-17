using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public class NwrwDwfComponentFileWriter : NwrwComponentFileWriterBase
    {
        private const string NWRW_DWA_FILENAME = "pluvius.dwa";

        private const string NUMBER_OF_PEOPLE_TIMES_CONSTANT_DWA_PER_CAPITA_PER_HOUR = "1";
        private const string NUMBER_OF_PEOPLE_TIMES_VARIABLE_DWA_PER_CAPITA_PER_HOUR = "2";
        public NwrwDwfComponentFileWriter(RainfallRunoffModel model) : base(model, NWRW_DWA_FILENAME)
        {
        }

        protected override IEnumerable<string> CreateContentLine(RainfallRunoffModel model)
        {
            IList<NwrwDryWeatherFlowDefinition> dryWeatherFlowDefinition = model.NwrwDryWeatherFlowDefinitions;

            foreach (var definition in dryWeatherFlowDefinition)
            {
                yield return CreateNwrwDwaLine(definition);
            }
        }

        private string CreateNwrwDwaLine(NwrwDryWeatherFlowDefinition dryWeatherFlowDefinition)
        {
            StringBuilder line = new StringBuilder();

            line.Append($"{NwrwKeywords.Pluv_dwa_DWA} ");

            var name = dryWeatherFlowDefinition.Name;
            line.Append($"{NwrwKeywords.Pluv_id} '{name}' ");
            line.Append($"{NwrwKeywords.Pluv_nm} '{name}' ");

            AppendDwfComputationOption(line, dryWeatherFlowDefinition.DistributionType);

            line.Append($"{NwrwKeywords.Pluv_dwa_wc} {Round(dryWeatherFlowDefinition.DailyVolumeConstant / 24)} ");
            line.Append($"{NwrwKeywords.Pluv_dwa_wd} {Round(dryWeatherFlowDefinition.DailyVolumeVariable).ToString()} ");

            AppendWaterUsePerHour(line, dryWeatherFlowDefinition.HourlyPercentageDailyVolume);

            line.Append(NwrwKeywords.Pluv_dwa_dwa);

            return line.ToString();
        }

        private void AppendDwfComputationOption(StringBuilder line, DryweatherFlowDistributionType dryweatherFlowDistributionType)
        {
            line.Append($"{NwrwKeywords.Pluv_dwa_do} ");
            switch (dryweatherFlowDistributionType)
            {
                case DryweatherFlowDistributionType.Constant:
                    line.Append($"{NUMBER_OF_PEOPLE_TIMES_CONSTANT_DWA_PER_CAPITA_PER_HOUR} ");
                    break;
                case DryweatherFlowDistributionType.Daily:
                    line.Append($"{NUMBER_OF_PEOPLE_TIMES_VARIABLE_DWA_PER_CAPITA_PER_HOUR} ");
                    break;
                case DryweatherFlowDistributionType.Variable:
                    throw new ArgumentException($"'{nameof(DryweatherFlowDistributionType.Variable)}' is not yet supported.");
                default:
                    throw new ArgumentException($"Invalid distribution type was provided.");
            }
        }

        private void AppendWaterUsePerHour(StringBuilder line, double[] hourlyPercentageDailyVolume)
        {
            line.Append($"{NwrwKeywords.Pluv_dwa_wh} ");
            for (int i = 0; i <= 23; i++)
            {
                line.Append($"{hourlyPercentageDailyVolume[i]} ");
            }
        }

        private static double Round(double value) => Math.Round(value, 6);
    }
}