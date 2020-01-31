using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public class NwrwDwfComponentFileWriter : NwrwComponentFileWriterBase
    {
        private const string NWRW_DWA_FILENAME = "pluvius.dwa";
        private const double DEFAULT_DOUBLE = 0.0;

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
            AppendIdToDwaLine(line, dryWeatherFlowDefinition); // 'id'
            AppendNameToDwaLine(line, dryWeatherFlowDefinition); // 'nm'

            switch (dryWeatherFlowDefinition.DistributionType)
            {
                case DwfDistributionType.Constant:
                    AppendConstantPropertiesToDwaLine(line, dryWeatherFlowDefinition);
                    break;
                case DwfDistributionType.Daily:
                    AppendDailyPropertiesToDwaLine(line, dryWeatherFlowDefinition);
                    break;
                case DwfDistributionType.Variable:
                    throw new ArgumentException($"'{nameof(DwfDistributionType.Variable)}' is not yet supported.");
                default:
                    throw new ArgumentException($"Invalid distribution type was provided.");
            } // 'do' 'wc' 'wd' 'wh'
            AppendClosingTagToDwaLine(line); // 'dwa'

            return line.ToString();
        }
        private void AppendConstantPropertiesToDwaLine(StringBuilder line, NwrwDryWeatherFlowDefinition dryWeatherFlowDefinition)
        {
            AppendDwfComputationOptionToDwaLine(line, NUMBER_OF_PEOPLE_TIMES_VARIABLE_DWA_PER_CAPITA_PER_HOUR); // 'do'
            AppendWaterUsePerCapitaAsConstantToDwaLine(line, dryWeatherFlowDefinition.DailyVolume); // 'wc'
            AppendWaterUsePerCapitaPerDayToDwaLine(line, DEFAULT_DOUBLE); // 'wd'
            AppendWaterUsePerHour(line, dryWeatherFlowDefinition); // 'wh'
        }
        private void AppendDailyPropertiesToDwaLine(StringBuilder line, NwrwDryWeatherFlowDefinition dryWeatherFlowDefinition)
        {
            AppendDwfComputationOptionToDwaLine(line, NUMBER_OF_PEOPLE_TIMES_CONSTANT_DWA_PER_CAPITA_PER_HOUR); // 'do'
            AppendWaterUsePerCapitaAsConstantToDwaLine(line, DEFAULT_DOUBLE); // 'wc'
            AppendWaterUsePerCapitaPerDayToDwaLine(line, dryWeatherFlowDefinition.DailyVolume); // 'wd'
            AppendWaterUsePerHour(line, dryWeatherFlowDefinition);  // 'wh'
        }
        private void AppendOpeningTagToDwaLine(StringBuilder line)
        {
            line.Append(NwrwKeywords.DwaOpeningKey);
            line.Append(" ");
        }
        private void AppendIdToDwaLine(StringBuilder line, NwrwDryWeatherFlowDefinition dryWeatherFlowDefinition)
        {
            line.Append(NwrwKeywords.IdKey);
            line.Append(" ");
            line.Append("'");
            line.Append(dryWeatherFlowDefinition.Name);
            line.Append("'");
            line.Append(" ");
        }
        private void AppendNameToDwaLine(StringBuilder line, NwrwDryWeatherFlowDefinition dryWeatherFlowDefinition)
        {
            line.Append(NwrwKeywords.NameKey);
            line.Append(" ");
            line.Append("'");
            line.Append(dryWeatherFlowDefinition.Name); // same as id
            line.Append("'");
            line.Append(" ");
        }
        private void AppendDwfComputationOptionToDwaLine(StringBuilder line, string option)
        {
            line.Append(NwrwKeywords.DwaComputationOptionKey); // do
            line.Append(" ");
            line.Append(option);
            line.Append(" ");
        }
        private void AppendWaterUsePerCapitaAsConstantToDwaLine(StringBuilder line, double waterUseConstant)
        {
            line.Append(NwrwKeywords.DwaWaterUsePerCapitaConstantValuePerHourKey);
            line.Append(" ");
            line.Append(waterUseConstant.ToString());
            line.Append(" ");

        }
        private void AppendWaterUsePerCapitaPerDayToDwaLine(StringBuilder line, double waterUseDaily)
        {
            line.Append(NwrwKeywords.DwaWaterUsePerCapitaPerDayKey);
            line.Append(" ");
            line.Append(waterUseDaily.ToString());
            line.Append(" ");
        }
        private void AppendWaterUsePerHour(StringBuilder line, NwrwDryWeatherFlowDefinition dryWeatherFlowDefinition)
        {
            line.Append(NwrwKeywords.DwaWaterUsePerCapitaPerHourKey);
            line.Append(" ");
            for (int i = 0; i < 23; i++)
            {
                line.Append(dryWeatherFlowDefinition.HourlyPercentageDailyVolume[i]);
                line.Append(" ");
            }
        }
        private void AppendClosingTagToDwaLine(StringBuilder line)
        {
            line.Append(NwrwKeywords.DwaClosingKey);
        }
    }
}