using System;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders
{
    public sealed class DelftIniMultiLineReader : DelftIniReader
    {
        /// <summary>
        /// Regular expression for a value/comment line, where value can be anything and
        /// an optional comment starting with the '#' character.
        /// </summary>
        private const string ValueCommentPattern = @"^\s*(?<value>[^#=]*)(#(?<comment>.*))?$";

        /// <summary>
        /// Parses a line into a DelftIniProperty.
        /// </summary>
        /// <param name="line">Line to be parsed.</param>
        /// <param name="currentCategory">The current category.</param>
        protected override void ReadFields(string line, DelftIniCategory currentCategory)
        {
            if (IsMultiLineValue(line, out var multiLineMatches))
            {
                (string value, string comment) valueAndComment = GetValueComment(multiLineMatches);
                var properties = currentCategory.Properties.LastOrDefault();
                if (properties == null)
                    throw new FormatException(String.Format("Invalid value-comment line on line {0} in file {1}",
                        LineNumber, InputFilePath));
                properties.Value += $"{Environment.NewLine} {valueAndComment.value}";
                properties.Comment += $"{Environment.NewLine} {valueAndComment.comment}";
            }
            else
            {
                var fields = GetKeyValueComment(line);
                currentCategory.Properties.Add(new DelftIniProperty
                    {Name = fields[0], Value = fields[1], Comment = fields[2], LineNumber = LineNumber});
            }
        }

        /// <summary>
        /// Determines if the line contains just a value with an optional comment.
        /// </summary>
        /// <param name="line">Line to be parsed.</param>
        /// <param name="matches">A MatchCollection of the value and optionally the comment.</param>
        /// <returns>True if the line matches the regex pattern.</returns>
        private bool IsMultiLineValue(string line, out MatchCollection matches)
        {
            matches = RegularExpression.GetMatches(ValueCommentPattern, line);
            return matches.Count != 0;
        }

        /// <summary>
        /// Parses a line expecting a value-comment pattern.
        /// </summary>
        /// <param name="matches">The matches found by the regular expression.</param>
        /// <returns>A size 2 array of strings, where first item is the value and second item the comment.</returns>
        /// <exception cref="FormatException">When <paramref name="matches"/> does not contain any matches.</exception>
        private (string,string) GetValueComment(MatchCollection matches)
        {
            (string value, string comment) valueAndComment = (String.Empty, String.Empty);

            if (matches.Count == 0) throw new FormatException(String.Format("Invalid value-comment line on line {0} in file {1}",
                LineNumber, InputFilePath));
            valueAndComment.value = matches[0].Groups["value"].Value.Trim();
            valueAndComment.comment = matches[0].Groups["comment"].Value.Trim(); // Returns "" if comment group not matched

            return valueAndComment;
        }
    }
}