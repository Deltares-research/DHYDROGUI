using System;
using System.Text.RegularExpressions;

namespace DeltaShell.Sobek.Readers
{
    public static class SobekReaderHelper
    {
        private static readonly Regex TimeSpanRegex = new Regex(@"(?<hour>[0-9]+)[:]+(?<minute>[0-9]+)[:]+(?<second>[0-9]+)", RegexOptions.Compiled | RegexOptions.Singleline);
        // sobek format : hh:mm:ss
        public static TimeSpan ParseTimeSpan(string value)
        {
            string date = StringUtils.RemoveQuotes(value);
            Match match = TimeSpanRegex.Match(date);
            int hour = int.Parse(match.Groups["hour"].Value);
            int minute = int.Parse(match.Groups["minute"].Value);
            int second = int.Parse(match.Groups["second"].Value);
            return new TimeSpan(0, hour, minute, second);
        }

        public static SobekType GetSobekType(string path)
        {
            var sobekType = path.EndsWith("DEFTOP.1", StringComparison.OrdinalIgnoreCase) ? SobekType.SobekRE : SobekType.Sobek212;
            var sobekFileNames = new SobekFileNames {SobekType = sobekType};
            if (!path.EndsWith(sobekFileNames.SobekNetworkFileName, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Point to a valid network file, ie " + sobekFileNames.SobekNetworkFileName);
            }
            return sobekType;
        }

    }
}