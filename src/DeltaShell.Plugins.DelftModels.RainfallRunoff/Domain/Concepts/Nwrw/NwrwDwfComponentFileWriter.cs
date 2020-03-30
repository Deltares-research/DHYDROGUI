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

            line.Append($"{NwrwKeywords.Pluv_dwa_wc} {dryWeatherFlowDefinition.DailyVolumeConstant} ");
            line.Append($"{NwrwKeywords.Pluv_dwa_wd} {dryWeatherFlowDefinition.DailyVolumeVariable.ToString()} ");

            AppendWaterUsePerHour(line, dryWeatherFlowDefinition.HourlyPercentageDailyVolume);

            line.Append(NwrwKeywords.Pluv_dwa_dwa);

            return line.ToString();
        }

        private void AppendDwfComputationOption(StringBuilder line, DwfDistributionType dwfDistributionType)
        {
            line.Append($"{NwrwKeywords.Pluv_dwa_do} ");
            switch (dwfDistributionType)
            {
                case DwfDistributionType.Constant:
                    line.Append($"{NUMBER_OF_PEOPLE_TIMES_CONSTANT_DWA_PER_CAPITA_PER_HOUR} ");
                    break;
                case DwfDistributionType.Daily:
                    line.Append($"{NUMBER_OF_PEOPLE_TIMES_VARIABLE_DWA_PER_CAPITA_PER_HOUR} ");
                    break;
                case DwfDistributionType.Variable:
                    throw new ArgumentException($"'{nameof(DwfDistributionType.Variable)}' is not yet supported.");
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
    }
}