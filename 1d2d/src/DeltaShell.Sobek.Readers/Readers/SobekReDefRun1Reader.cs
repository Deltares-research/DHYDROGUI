using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers
{
    public sealed class SobekReDefRun1Reader : SobekReader<SobekCaseSettings>
    {
        public override IEnumerable<SobekCaseSettings> Parse(string text)
        {
            return RegularExpression.GetMatches(@"(FLTM (?'text'.*?)fltm)", text)
                .Cast<Match>()
                .Select(structureMatch => ParseSobekCaseSettings(structureMatch.Value))
                .Where(definition => definition != null);
        }

        // FLTM bt '2000/01/01;00:00:00' et '2000/02/01;00:00:00' ct '01:00:00' cd 0 tp '12:25:00' dp 0 ba '' tt 75 tf 1 nt 1 if 0 im 1 ri 0 fltm
        public static SobekCaseSettings ParseSobekCaseSettings(string record)
        {
            var sobekCaseSettings = new SobekCaseSettings();

            string pattern = RegularExpression.GetExtendedCharacters("bt") + "|" +
                             RegularExpression.GetExtendedCharacters("et") + "|" +
                             RegularExpression.GetExtendedCharacters("ct");

            foreach (Match match in RegularExpression.GetMatches(pattern, record))
            {
                if (match.Groups["bt"].Value.Trim().Length > 0)
                {
                    var dateTimeString = match.Groups["bt"].Value.Trim('\'').Replace(';', ' ');
                    sobekCaseSettings.StartTime = Convert.ToDateTime(dateTimeString, CultureInfo.InvariantCulture);
                }
                if (match.Groups["et"].Value.Trim().Length > 0)
                {
                    var dateTimeString = match.Groups["et"].Value.Trim('\'').Replace(';', ' ');
                    sobekCaseSettings.StopTime = Convert.ToDateTime(dateTimeString, CultureInfo.InvariantCulture);
                }
                if (match.Groups["ct"].Value.Trim().Length > 0)
                {
                    sobekCaseSettings.TimeStep = SobekReaderHelper.ParseTimeSpan(match.Groups["ct"].Value);
                    // for now assume same
                    sobekCaseSettings.OutPutTimeStep = sobekCaseSettings.TimeStep;
                }
            }
            return sobekCaseSettings;
        }
    }
}