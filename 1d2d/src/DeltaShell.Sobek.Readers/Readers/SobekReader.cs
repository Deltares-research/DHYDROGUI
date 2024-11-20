using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.Properties;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    /// <summary>
    /// Reads a file using <see cref="Read"/> method. This method will read the whole file line by line and will search for all matches 
    /// as indicated by <see cref="GetTags"/> method.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SobekReader<T> where T : class
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekReader<T>));

        public virtual IEnumerable<T> Read(string filePath)
        {
            if (!File.Exists(filePath))
            {
                yield break;
            }

            if (!IsFileReadable(filePath))
            {
                log.Error(string.Format(Resources.SobekReader_Could_not_read_file_because_it_is_locked, filePath));
                yield break;
            }

            using (var reader = new StreamReader(filePath, Encoding.Default))
            {
                var tagsUpper = GetTags().Select(s => s.ToUpper()).ToList();
                var tagsLower = GetTags().Select(s => s.ToLower()).ToList();

                var taggedItem = new StringBuilder();

                if (tagsUpper.Count == 0)
                {
                    taggedItem.Append(reader.ReadToEnd());
                    foreach (var match in Parse(taggedItem.ToString()))
                    {
                        yield return match;
                    }
                }
                else
                {
                    string readLine;
                    while ((readLine = reader.ReadLine()) != null)
                    {
                        if (readLine.Trim() == string.Empty) continue;

                        // continue until upper case tag is found:
                        if (!tagsUpper.Intersect(readLine.Split()).Any()) continue;

                        // our line contains start tag
                        taggedItem.Append(readLine + Environment.NewLine);

                        // find end tag (possibly in same line)
                        while (!tagsLower.Intersect(readLine.Split()).Any())
                        {
                            if ( (readLine =  reader.ReadLine()) == null) break;
                            taggedItem.Append(readLine + Environment.NewLine);
                        }
                        
                        var match = Parse(taggedItem.ToString()).ToList();
                        if (match.Any())
                        {
                            yield return match.First();
                        }
                        taggedItem.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// Parses a given string (<see cref="text"/>). Returns objects of type T found in <paramref name="text" />
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public abstract IEnumerable<T> Parse(string text);

        /// <summary>
        /// Can be used to speed-up reading of the file and to minimize memory use.
        /// When this method is implemented - it will be used by the <see cref="Read"/> method when reading the file.
        /// More specific, before <see cref="Parse"/> will be called - the content of the file will be first split into sub-strings which begin/end 
        /// using these tags.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<string> GetTags()
        {
            yield break;
        }

        // regex id and name pattern, with the latter optional
        protected const string IdAndOptionalNamePattern = @"id\s+?\'(?'id'.*?)\'\s+?(?:nm\s+?\'(?'nm'.*?)\'\s+?)?";

        protected bool TryGetDoubleParameter(string label, string record, out double value, bool reportError = true)
        {
            value = double.NaN;
            var pattern = RegularExpression.GetScientific(label);
            var matches = RegularExpression.GetMatches(pattern, record);
            if (matches.Count == 1 && double.TryParse(matches[0].Groups[label].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }

            if (reportError) ReportParseError(label, record);
            return false;
        } 
        
        protected bool TryGetStringParameter(string label, string record, out string value, bool reportError = true)
        {
            value = null;
            var pattern = RegularExpression.GetExtendedCharacters(label);
            var matches = RegularExpression.GetMatches(pattern, record);
            if (matches.Count == 1)
            {
                value = matches[0].Groups[label].Value;
                return true;
            }

            if (reportError) ReportParseError(label, record);
            return false;
        }
        
        protected bool TryGetArrayOfNumbers(string label, string record, int length, out double[] arrayValues, bool reportError = true)
        {
            arrayValues = new double[length];
            var pattern = RegularExpression.GetScientificArray(label, length);
            var matches = RegularExpression.GetMatches(pattern, record);
            if (matches.Count == 1)
            {
                string stringWithValues = matches[0].Groups[label].Value;
                string[] stringValues = stringWithValues.Contains(" ") 
                                        ? stringWithValues.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries) 
                                        : stringWithValues.Split();
                
                for (int i = 0; i < length; ++i)
                {
                    if (double.TryParse(stringValues[i], NumberStyles.Any, NumberFormatInfo.InvariantInfo, out double value))
                    {
                        arrayValues[i] = value;
                    }
                }
                return true;
            }

            if (reportError) ReportParseError(label, record);
            return false;
        }

        protected bool TryGetArrayOfNumbersStrings(string label, string text, int length, out string[] stringArray)
        {
            stringArray = new string[length];
            var pattern = string.Format("\\s+{0}\\s+(?<{0}>\\s*['A-Za-z0-9\\s\\(\\)-\\\\/\\.\\+\\<\\>,\\|_&;:\\[\\]]*\\s*\\s+)", label);
            var matches = RegularExpression.GetMatches(pattern, text);
            if (matches.Count == 1)
            {
                var stringValues = matches[0].Groups[label].Value.Split();
                for (int i = 0; i < length; ++i)
                {
                    stringArray[i] = stringValues[i].Replace("'","");
                }
                return true;
            }
            return false;
        }

        protected bool TryGetIntegerString(string label, string record, out string intString, bool reportError = true)
        {
            intString = null;
            var pattern = RegularExpression.GetInteger(label);
            var matches = RegularExpression.GetMatches(pattern, record);
            if (matches.Count == 1)
            {
                intString = matches[0].Groups[label].Value;
                return true;
            }
            if (reportError) ReportParseError(label, record);
            return false;
        }

        protected virtual void ReportParseError(string label, string record)
        {
            // override for error report
        }

        private static bool IsFileReadable(string filepath)
        {
            FileStream fileStream = (FileStream) null;
            try
            {
                fileStream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return fileStream.CanRead;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                fileStream?.Close();
            }
        }
       
    }
}