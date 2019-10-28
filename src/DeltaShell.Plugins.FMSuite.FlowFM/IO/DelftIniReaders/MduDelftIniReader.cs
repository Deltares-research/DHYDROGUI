using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.NGHS.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Readers
{
    public class MduDelftIniReader : DelftIniReader
    {
        private const string ValueCommentPattern =
            @"^\s*" +                   // pre-whitespaces
            @"(?<value>[^#]*)" +        // value, until '#'-sign
            @"#\s*" +                   // '#'-sign with whitespaces
            @"((?<comment>.*))?\z";     // comment, every character until the end of the line

        private const string ValueSlashPattern =
            @"^\s*" +
            @"(?<value>[^(\)]*)" +
            @"\\+\z";

        protected override string[] GetKeyValueComment(string line)
        {
            string[] keyValueComment = base.GetKeyValueComment(line);
            if (keyValueComment[1].EndsWith(@"\"))
            {
                while ((line = GetNextLine()) != null)
                {
                    MatchCollection matchesValueSlash = RegularExpression.GetMatches(ValueSlashPattern, line);
                    if (matchesValueSlash.Count > 0)
                    {
                        keyValueComment[1] = string.Join(" ", keyValueComment[1].TrimEnd('\\', ' '), matchesValueSlash[0].Groups["value"].Value.TrimEnd('\\', ' '));
                    }
                    else
                    {
                        break;
                    }
                }

                MatchCollection matches = RegularExpression.GetMatches(ValueCommentPattern, line);
                if (matches.Count > 0)
                {
                    keyValueComment[1] = string.Join(" ", keyValueComment[1].TrimEnd('\\', ' '), matches[0].Groups["value"].Value.Trim());
                    keyValueComment[2] = matches[0].Groups["comment"].Value;
                }
            }


            return keyValueComment;
        }
    }
}