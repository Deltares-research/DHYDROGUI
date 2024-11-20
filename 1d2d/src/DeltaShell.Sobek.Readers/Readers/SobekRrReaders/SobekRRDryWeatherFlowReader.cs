using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRDryWeatherFlowReader : SobekReader<SobekRRDryWeatherFlow>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRDryWeatherFlowReader));

        public override IEnumerable<SobekRRDryWeatherFlow> Parse(string fileContent)
        {
            const string pattern = @"DWA\s+" + IdAndOptionalNamePattern + "(?'text'.*?)dwa";

            return (from Match line in RegularExpression.GetMatches(pattern, fileContent)
                    select GetSobekRRDryWeatherFlow(line.Value)).ToList();
        }

        private static SobekRRDryWeatherFlow GetSobekRRDryWeatherFlow(string line)
        {
            var sobekRRDryWeatherFlow = new SobekRRDryWeatherFlow();
            //id   =          dwa identification
            //nm   =          dwa name
            //do   =          dwa computation option
            //                see NWRW dwa description.
            //wc   =          water use per capita as a constant value per hour (l/hour)           
            //wd   =          water use per capita per day (l/day)
            //wh   =          water use per capita per hour (24 percentages, total should be 100%)
            //sc   =          salt concentration (mg/l) of DWA. Default 400 mg/l. 
            //                sc keyword is NOT READ. Default value always used.

            //Id
            var label = "id";
            var pattern = RegularExpression.GetExtendedCharacters(label);
            var matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRDryWeatherFlow.Id = matches[0].Groups[label].Value;
            }

            label = "nm";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRDryWeatherFlow.Name = matches[0].Groups[label].Value;
            }

            label = "do";
            pattern = RegularExpression.GetInteger(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                var doAsInt = Convert.ToInt32(matches[0].Groups[label].Value);
                if (Enum.IsDefined(typeof(DWAComputationOption), doAsInt))
                {
                    sobekRRDryWeatherFlow.ComputationOption = (DWAComputationOption)doAsInt;
                }
                else
                {
                    log.ErrorFormat("Computation option of {0} is unkown.", sobekRRDryWeatherFlow.Id);
                }
            }

            label = "wc";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRDryWeatherFlow.WaterUsePerHourForConstant = Convert.ToDouble(matches[0].Groups[label].Value,
                                                                               CultureInfo.InvariantCulture);
            }

            label = "wd";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRDryWeatherFlow.WaterUsePerDayForVariable = Convert.ToDouble(matches[0].Groups[label].Value,
                                                                             CultureInfo.InvariantCulture);
            }

            label = "wh";
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(label + @"\s*(?<" + label + ">");
            for (int i = 0; i < 24; i++)
            {
                stringBuilder.Append(RegularExpression.Scientific + @"\s*");
            }
            stringBuilder.Append(")");
            pattern = stringBuilder.ToString();
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRDryWeatherFlow.WaterCapacityPerHour = ConvertToWaterCapacityPercentages(matches[0].Groups[label].Value);
            }

            label = "sc";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRDryWeatherFlow.SaltConcentration = Convert.ToDouble(matches[0].Groups[label].Value,
                                                                           CultureInfo.InvariantCulture);
            }

            return sobekRRDryWeatherFlow;
        }

        private static double[] ConvertToWaterCapacityPercentages(string values)
        {
            var lstValues = new List<double>();

            var valuesArray = values.SplitOnEmptySpace();
            foreach (var value in valuesArray)
            {
                lstValues.Add(Convert.ToDouble(value, CultureInfo.InvariantCulture));
            }

            return lstValues.ToArray();
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "dwa";
        }
    }
}