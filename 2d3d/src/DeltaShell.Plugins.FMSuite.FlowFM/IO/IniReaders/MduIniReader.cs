using System.Collections.Generic;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.NGHS.IO.Ini;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.IniReaders
{
    /// <summary>
    /// Reader for mdu files. This reader supports multiple-valued properties that
    /// are defined on multiple lines and are separated by backslashes. Comments that
    /// are not defined at the last line of the property definition are ignored.
    /// Examples:
    /// [output]
    /// ObsFile  = obs_1_obs.xyn obs_2_obs.xyn \
    /// obs_3_obs.xyn  # My comment
    /// [output]
    /// ObsFile  = obs_1_obs.xyn \
    /// obs_2_obs.xyn \
    /// obs_3_obs.xyn  # My comment
    /// </summary>
    public class MduIniReader : IniReader
    {
        private const string valueSlashPattern =
            @"^\s*" +               // pre-whitespaces
            @"(?<value>[^(\)]*)" +  // value, until first backslash
            @"\\+(?<comment>.*)?$"; // At least one backslash and every character until the end of the line

        private const string valueCommentPattern =
            @"^\s*" +                  // pre-whitespaces
            @"(?<value>[^#]*)" +       // value, until '#'-sign
            @"(#\s*(?<comment>.*))?$"; // comment, every character until the end of the line

        /// <summary>
        /// Parses one or multiple lines expecting a key-value-comment pattern or a multiline defined property.
        /// </summary>
        /// <param name="lineContent"> The line content to parse. </param>
        /// <returns>A size 3 array of strings, where first item is the key, second the value and third the comment.</returns>
        protected override string[] GetKeyValueComment(string lineContent)
        {
            string[] keyValueComment = base.GetKeyValueComment(lineContent);
            return keyValueComment[1].EndsWith(@"\")
                       ? ParseMultilineDefinedProperty(keyValueComment)
                       : keyValueComment;
        }

        private string[] ParseMultilineDefinedProperty(string[] keyValueComment)
        {
            string lineContent;
            while ((lineContent = GetNextLine()) != null)
            {
                if (!ParseValueSlashLine(keyValueComment, lineContent))
                {
                    break;
                }
            }

            ParseValueCommentLine(keyValueComment, lineContent);

            return keyValueComment;
        }

        private static bool ParseValueSlashLine(IList<string> keyValueComment, string lineContent)
        {
            MatchCollection matchesValueSlash = RegularExpression.GetMatches(valueSlashPattern, lineContent);
            if (matchesValueSlash.Count > 0)
            {
                string existingValue = keyValueComment[1].TrimEnd('\\', ' ');
                string additionalValue = matchesValueSlash[0].Groups["value"].Value.TrimEnd('\\', ' ');
                keyValueComment[1] = string.Join(" ", existingValue, additionalValue).Trim();
            }
            else
            {
                return false;
            }

            return true;
        }

        private static void ParseValueCommentLine(IList<string> keyValueComment, string lineContent)
        {
            MatchCollection matchesValueComment = RegularExpression.GetMatches(valueCommentPattern, lineContent);
            if (matchesValueComment.Count == 0)
            {
                return;
            }

            string existingValue = keyValueComment[1].TrimEnd('\\', ' ');
            string additionalValue = matchesValueComment[0].Groups["value"].Value.Trim();
            keyValueComment[1] = string.Join(" ", existingValue, additionalValue);

            if (!keyValueComment[1].EndsWith(@"\"))
            {
                keyValueComment[2] = matchesValueComment[0].Groups["comment"].Value;
            }
        }
    }
}