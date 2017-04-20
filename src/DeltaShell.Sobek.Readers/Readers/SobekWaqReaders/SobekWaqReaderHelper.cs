using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DelftTools.Utils;
using DelftTools.Utils.RegularExpressions;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekWaqReaders
{
    public static class SobekWaqReaderHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekWaqReaderHelper));

        /// <summary>
        /// Parses a date time string with format "yyyy/MM/dd-H:mm:ss"
        /// </summary>
        /// <param name="dateTimeText">The date time string to parse</param>
        public static DateTime ParseDateTime(string dateTimeText)
        {
            return DateTime.ParseExact(dateTimeText.Replace("/", "-"), "yyyy-MM-dd-H:mm:ss", null);
        }

        /// <summary>
        /// Parses a time step from <paramref name="timeStepText"/> based on <paramref name="timeStepFormat"/>
        /// </summary>
        /// <param name="timeStepText">The time step text to parse the time step from (something like "000010018")</param>
        /// <param name="timeStepFormat">The time step format to use for parsing (something like "YYDDMMSS")</param>
        /// <returns>The parsed time step</returns>
        /// <exception cref="FormatException">Thrown when no valid time step was found</exception>
        /// <remarks><paramref name="timeStepText"/> and <paramref name="timeStepFormat"/> are allowed to differ in length</remarks>
        public static TimeSpan ParseTimeStep(string timeStepText, string timeStepFormat)
        {
            try
            {
                // Ensure the time step text length equals the time step format length
                var lengthMismatch = timeStepFormat.Length - timeStepText.Length;
                
                if (lengthMismatch != 0)
                {
                    timeStepText = (lengthMismatch > 0)
                        ? timeStepText.PadLeft(timeStepFormat.Length, '0')
                        : timeStepText.Substring(lengthMismatch * -1);
                }
                
                var valuePerChar = timeStepText
                    .Select((c, i) => new KeyValuePair<char, char>(timeStepFormat[i], c))
                    .GroupBy(kvp => kvp.Key, kvp => kvp.Value)
                    .ToDictionary(g => g.Key, g => Convert.ToInt32(new string(g.ToArray()), CultureInfo.InvariantCulture));

                return new TimeSpan((valuePerChar.ContainsKey('Y') ? valuePerChar['Y'] : 0)*365 +
                                    (valuePerChar.ContainsKey('D') ? valuePerChar['D'] : 0),
                                    valuePerChar.ContainsKey('H') ? valuePerChar['H'] : 0,
                                    valuePerChar.ContainsKey('M') ? valuePerChar['M'] : 0,
                                    valuePerChar.ContainsKey('S') ? valuePerChar['S'] : 0);
            }
            catch (Exception)
            {
                throw new FormatException("no valid time step was found");
            }
        }

        /// <summary>
        /// Creates a dictionary from <paramref name="keys"/> and <paramref name="values"/>
        /// </summary>
        /// <param name="keys">The string keys for the dictionary</param>
        /// <param name="values">The double values for the dictionary</param>
        /// <param name="warningMessage">A warning message that is logged when the number of <paramref name="keys"/> mismatches the number of <paramref name="values"/></param>
        /// <returns>A dictionary with 'string - double' pairs</returns>
        /// <remarks>When the number of <paramref name="keys"/> mismatches the number of <paramref name="values"/>, a dictionary is returned with a maximum number of values</remarks>
        public static Dictionary<string, double> CreateDoubleDictionary(IEnumerable<string> keys, IEnumerable<double> values, string warningMessage)
        {
            var keysArray = keys.ToArray();
            var valuesArray = values.ToArray();

            if (keysArray.Length != valuesArray.Length)
            {
                Log.WarnFormat(warningMessage);
            }

            var numberOfElements = Math.Min(keysArray.Length, valuesArray.Length);

            return keysArray.Take(numberOfElements)
                    .Select((k, i) => new KeyValuePair<string, double>(k, valuesArray[i])).
                    ToDictionary(k => k.Key, k => k.Value);
        }

        /// <summary>
        /// Returns text without lines that start with <paramref name="commentIndicator"/>
        /// </summary>
        /// <param name="text">The text to strip the commented lines from</param>
        /// <param name="commentIndicator">The string that indicates a commented line</param>
        public static string GetUnCommentedText(string text, string commentIndicator)
        {
            var builder = new StringBuilder("", text.Length);
            var reader = new StringReader(text);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.TrimStart().StartsWith(commentIndicator)) continue;  
                builder.Append(line + "\r\n");
            }
            return builder.ToString();
        }

        public static IEnumerable<string> GetTextLines(string text, string skipLineIndicator = null)
        {
            var reader = new StringReader(text);
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.TrimStart();
                if (string.IsNullOrEmpty(line)) continue;
                if (skipLineIndicator != null && line.StartsWith(skipLineIndicator)) continue;

                yield return line;
            }
        }

        /// <summary>
        /// Returns the first text block in <paramref name="text"/> that starts with <paramref name="blockStart"/> and ends with <paramref name="blockEnd"/>
        /// </summary>
        /// <param name="text">The text to obtain the text block from</param>
        /// <param name="blockStart">The start of the text block to search for</param>
        /// <param name="blockEnd">The end of the text block to search for</param>If no <paramref name="blockEnd"/> is specified, the text block is expected to end with '2 x NewLine'
        /// <remarks>-
        /// with possible spaces in between or 'EndOfString'</remarks>
        public static string GetTextBlock(string text, string blockStart, string blockEnd = "(\r\n *\r\n|$)")
        {
            var dataBlockRegex = new Regex(blockStart + RegularExpression.CharactersAndQuote + "?" + blockEnd, RegexOptions.Singleline);
            var dataBlockMatch = dataBlockRegex.Match(text);

            return dataBlockMatch.Value;
        }

        /// <summary>
        /// Returns all text blocks in <paramref name="text"/> that start with <paramref name="blockStart"/> and end with <paramref name="blockEnd"/>
        /// </summary>
        /// <param name="text">The text to obtain the text blocks from</param>
        /// <param name="blockStart">The start of the text blocks to search for</param>
        /// <param name="blockEnd">The end of the text blocks to search for</param>
        /// <remarks>If no <paramref name="blockEnd"/> is specified, the text blocks are expected to end with '2 x NewLine'
        ///  with possible spaces in between or 'EndOfString'</remarks>
        public static IEnumerable<string> GetTextBlocks(string text, string blockStart, string blockEnd = "(\r\n *\r\n|$)")
        {
            var dataBlockRegex = new Regex(blockStart + RegularExpression.CharactersAndQuote + "?" + blockEnd, RegexOptions.Singleline);
            var dataBlockMatches = dataBlockRegex.Matches(text);

            return from object dataBlockMatch in dataBlockMatches select ((Match) dataBlockMatch).Value;
        }

        /// <summary>
        /// Returns the first double in <paramref name="text"/>
        /// </summary>
        /// <param name="text">The text to search the double in</param>
        /// <remarks>Double.NaN is returned when no double was found</remarks>
        public static double GetDouble(string text)
        {
            var doubleRegex = new Regex("(" + RegularExpression.Scientific + "|" + RegularExpression.Integer + ")?", RegexOptions.Singleline);
            var doubleMatch = doubleRegex.Match(text.TrimStart());

            return doubleMatch.Length != 0 ? ConversionHelper.ToDouble(doubleMatch.Value) : double.NaN;
        }

        /// <summary>
        /// Returns all doubles in <paramref name="textLine"/>
        /// </summary>
        /// <param name="textLine">The text line to search doubles in</param>
        public static IEnumerable<double> GetDoublesFromSingleTextLine(string textLine)
        {
            var doubleRegex = new Regex("(" + RegularExpression.Scientific + "|" + RegularExpression.Integer + ")?", RegexOptions.Singleline);
            var doubleMatches = doubleRegex.Matches(textLine);

            return from object doubleMatch in doubleMatches
                   select ((Match) doubleMatch).Value
                   into doubleValue
                   where !string.IsNullOrEmpty(doubleValue)
                   select ConversionHelper.ToDouble(doubleValue);
        }

        /// <summary>
        /// Returns all doubles in <paramref name="text"/>, which are expected to be on separate text lines
        /// </summary>
        /// <param name="text">The text to search doubles in</param>
        public static IEnumerable<double> GetDoublesFromMultipleTextLines(string text)
        {
            var doubleRegex = new Regex(RegularExpression.Scientific);
            var lines = text.Split('\n'); 
            var matches = lines.Select(l => doubleRegex.Match(l)).Where(m => m.Success);

            return matches.Select(m => ConversionHelper.ToDouble(m.Value)); 
        }
    }
}
