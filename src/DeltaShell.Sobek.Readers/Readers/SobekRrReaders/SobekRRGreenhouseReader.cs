using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.NGHS.Common.Extensions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRGreenhouseReader : SobekReader<SobekRRGreenhouse>
    {
        public override IEnumerable<SobekRRGreenhouse> Parse(string fileContent)
        {
            const string greenhousePattern = @"GRHS (?'text'.*?)grhs" + RegularExpression.EndOfLine;

                return (from Match greenhouseLine in RegularExpression.GetMatches(greenhousePattern, fileContent)
                        select GetSobekGreenhouse(greenhouseLine.Value)).ToList();
        }

        private static SobekRRGreenhouse GetSobekGreenhouse(string line)
        {
            //id   =          node identification
            //na   =          number or areas (default=10) 
            //ar   =          area (in m2) as a table with areas for all greenhouse classes (na  values)
            //as   =          greenhouse area connected to silo storage (m2)
            //sl    =          surface level in m NAP 
            //sd   =          storage definition on roofs
            //si    =          silo definition
            //ms  =          identification of the meteostation
            //is    =          initial salt concentration (mg/l)

            var sobekGreenhouse = new SobekRRGreenhouse();

            //Id
            var label = "id";
            var pattern = RegularExpression.GetExtendedCharacters(label);
            var matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekGreenhouse.Id = matches[0].Groups[label].Value;
            }

            //Area
            label = "ar";
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(label + @"\s*(?<" + label + ">");
            for (int i = 0; i < 10; i++)
            {
                stringBuilder.Append(RegularExpression.Scientific + @"\s*");
            }
            stringBuilder.Append(")");
            pattern = stringBuilder.ToString();
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekGreenhouse.Areas = ConvertToAreas(matches[0].Groups[label].Value);
            }


            //Silo Area
            label = "as";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekGreenhouse.SiloArea = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //Surface level
            label = "sl";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekGreenhouse.SurfaceLevel = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //Storage definition on roofs
            label = "sd";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekGreenhouse.StorageOnRoofsId = matches[0].Groups[label].Value;
            }

            //Silo Id
            label = "si";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekGreenhouse.SiloId = matches[0].Groups[label].Value;
            }


            //Meteo station Id
            label = "ms";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekGreenhouse.MeteoStationId = matches[0].Groups[label].Value;
            }

            //Area Ajustment Factor
            label = "aaf";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekGreenhouse.AreaAjustmentFactor = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //Initial salt concentration
            label = "is";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekGreenhouse.InitialSaltConcentration = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }


            return sobekGreenhouse;
        }

        private static double[] ConvertToAreas(string values)
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
            yield return "grhs";
        }
    }
}
