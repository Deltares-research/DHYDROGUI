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

            AppendOpeningTagToDwaLine(line); // 'DWA'
            AppendIdToDwaLine(line, dryWeatherFlowDefinition.Name); // 'id'
            AppendNameToDwaLine(line, dryWeatherFlowDefinition.Name); // 'nm' (same as 'id')
            AppendDwfComputationOptionToDwaLine(line, dryWeatherFlowDefinition.DistributionType); // 'do'
            AppendWaterUsePerCapitaAsConstantToDwaLine(line, dryWeatherFlowDefinition.DailyVolumeConstant); // 'wc'
            AppendWaterUsePerCapitaPerDayToDwaLine(line, dryWeatherFlowDefinition.DailyVolumeVariable); // 'wd'
            AppendWaterUsePerHour(line, dryWeatherFlowDefinition.HourlyPercentageDailyVolume); // 'wh'
            AppendClosingTagToDwaLine(line); // 'dwa'

            return line.ToString();
        }
        
        private void AppendOpeningTagToDwaLine(StringBuilder line)
        {
            line.Append(NwrwKeywords.DwaOpeningKey);
            line.Append(" ");
        }
        private void AppendIdToDwaLine(StringBuilder line, string name)
        {
            line.Append(NwrwKeywords.IdKey);
            line.Append(" ");
            line.Append("'");
            line.Append(name);
            line.Append("'");
            line.Append(" ");
        }
        private void AppendNameToDwaLine(StringBuilder line, string name)
        {
            line.Append(NwrwKeywords.NameKey);
            line.Append(" ");
            line.Append("'");
            line.Append(name); // same as id
            line.Append("'");
            line.Append(" ");
        }
        private void AppendDwfComputationOptionToDwaLine(StringBuilder line, DwfDistributionType dwfDistributionType)
        {
            line.Append(NwrwKeywords.DwaComputationOptionKey); // do
            line.Append(" ");
            switch (dwfDistributionType)
            {
                case DwfDistributionType.Constant:
                    line.Append(NUMBER_OF_PEOPLE_TIMES_CONSTANT_DWA_PER_CAPITA_PER_HOUR);
                    line.Append(" ");
                    break;
                case DwfDistributionType.Daily:
                    line.Append(NUMBER_OF_PEOPLE_TIMES_VARIABLE_DWA_PER_CAPITA_PER_HOUR);
                    line.Append(" ");
                    break;
                case DwfDistributionType.Variable:
                    throw new ArgumentException($"'{nameof(DwfDistributionType.Variable)}' is not yet supported.");
                default:
                    throw new ArgumentException($"Invalid distribution type was provided.");
            }
        }
        private void AppendWaterUsePerCapitaAsConstantToDwaLine(StringBuilder line, double waterUseConstant)
        {
            line.Append(NwrwKeywords.DwaWaterUsePerCapitaConstantValuePerHourKey);
            line.Append(" ");
            line.Append(waterUseConstant);
            line.Append(" ");

        }
        private void AppendWaterUsePerCapitaPerDayToDwaLine(StringBuilder line, double waterUseDaily)
        {
            line.Append(NwrwKeywords.DwaWaterUsePerCapitaPerDayKey);
            line.Append(" ");
            line.Append(waterUseDaily.ToString());
            line.Append(" ");
        }
        private void AppendWaterUsePerHour(StringBuilder line, double[] hourlyPercentageDailyVolume)
        {
            line.Append(NwrwKeywords.DwaWaterUsePerCapitaPerHourKey);
            line.Append(" ");
            for (int i = 0; i < 23; i++)
            {
                line.Append(hourlyPercentageDailyVolume[i]);
                line.Append(" ");
            }
        }
        private void AppendClosingTagToDwaLine(StringBuilder line)
        {
            line.Append(NwrwKeywords.DwaClosingKey);
        }
    }
}