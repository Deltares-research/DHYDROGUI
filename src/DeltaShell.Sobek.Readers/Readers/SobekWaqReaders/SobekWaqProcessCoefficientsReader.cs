using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekWaqReaders
{
    public static class SobekWaqProcessCoefficientsReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekWaqProcessCoefficientsReader));

        # region Sobek212

        /// <summary>
        /// Read method for the Sobek 212 WAQ process coefficients file "CONSTANT.DWQ", 
        /// which defines constant process coefficient values on a per parameter basis.
        /// </summary>
        /// <example>
        /// File "CONSTANT.DWQ" has the following structure for constant process coefficient definitions:
        /// 
        /// CONSTANTS
        /// 'SWAdsP'
        /// 'KdPO4AAP'
        /// 'RcDetC'
        /// 'TcDetC'
        /// DATA
        /// 0              ; SWAdsP
        /// .075           ; KdPO4AAP
        /// .1             ; RcDetC
        /// 1.05           ; TcDetC
        /// </example>
        /// <param name="filePath">The path to "CONSTANT.DWQ"</param>
        /// <returns>A dictionary of "parameter name - parameter value" pairs</returns>
        public static Dictionary<string, double> ReadConstantValuesFromSobek212(string filePath)
        {
            var text = File.ReadAllText(filePath, Encoding.Default);

            return ParseConstantValuesFromSobek212(text);
        }

        private static Dictionary<string, double> ParseConstantValuesFromSobek212(string text)
        {
            var constantParametersTextBlock = SobekWaqReaderHelper.GetTextBlock(text, "CONSTANTS");
            var constantParameterNamesTextBlock = SobekWaqReaderHelper.GetTextBlock(constantParametersTextBlock, "CONSTANTS", "DATA");
            var constantParameterValuesTextBlock = SobekWaqReaderHelper.GetTextBlock(constantParametersTextBlock, "DATA");

            if (constantParameterNamesTextBlock.Length == 0 || constantParameterValuesTextBlock.Length == 0)
            {
                Log.Info("No constant process coefficient data was found");
                
                return new Dictionary<string, double>();
            }

            var parameterNames = SobekWaqReaderHelper.GetTextBlocks(constantParameterNamesTextBlock, "'", "'").Select(sn => sn.Replace("'", ""));
            var parameterValues = SobekWaqReaderHelper.GetDoublesFromMultipleTextLines(constantParameterValuesTextBlock);

            return SobekWaqReaderHelper.CreateDoubleDictionary(parameterNames, parameterValues,
                                                               string.Format("The constant process coefficients data block is partially imported because the number of parameter names did not equal the number of parameter values"));
        }

        /// <summary>
        /// Read method for the Sobek 212 WAQ process coefficients file "CONSTANT.DWQ", 
        /// which defines time dependent process coefficient values with block interpolation.
        /// </summary>
        /// <example>
        /// File "CONSTANT.DWQ" has the following structure for time dependent process coefficient definitions:
        /// 
        /// FUNCTIONS
        /// 'OOXNIT'
        /// DATA
        /// '2010/01/01-00:00:00' 5
        /// '2010/01/03-00:00:00' 4
        /// </example>
        /// <param name="filePath">The path to "CONSTANT.DWQ"</param>
        /// <returns>A dictionary mapped by parameter name, with "time step - parameter value" dictionary values (parameter name => "time step - parameter value" pair)</returns>
        public static Dictionary<string, Dictionary<DateTime, double>> ReadTimeDependentValuesWithBlockInterpolationFromSobek212(string filePath)
        {
            var text = File.ReadAllText(filePath, Encoding.Default);

            return ParseTimeDependentValuesWithBlockInterpolationFromSobek212(text);
        }

        private static Dictionary<string, Dictionary<DateTime, double>> ParseTimeDependentValuesWithBlockInterpolationFromSobek212(string text)
        {
            var processCoefficientDictionary = new Dictionary<string, Dictionary<DateTime, double>>();

            var textWithoutComment = SobekWaqReaderHelper.GetUnCommentedText(text, ";");
            var processCoefficientBlocks = SobekWaqReaderHelper.GetTextBlocks(textWithoutComment, "FUNCTIONS").Where(tb => !tb.Contains("LINEAR DATA"));

            ParseTimeDependentProcessCoefficientBlocks(processCoefficientBlocks, processCoefficientDictionary, "block");

            return processCoefficientDictionary;
        }

        /// <summary>
        /// Read method for the Sobek 212 WAQ process coefficients file "CONSTANT.DWQ", 
        /// which defines time dependent process coefficient values with linear interpolation.
        /// </summary>
        /// <example>
        /// File "CONSTANT.DWQ" has the following structure for time dependent process coefficient definitions:
        /// 
        /// FUNCTIONS
        /// 'OOXNIT'
        /// LINEAR DATA
        /// '2010/01/01-00:00:00' 5
        /// '2010/01/03-00:00:00' 4
        /// </example>
        /// <param name="filePath">The path to "CONSTANT.DWQ"</param>
        /// <returns>A dictionary mapped by parameter name, with "time step - parameter value" dictionary values (parameter name => "time step - parameter value" pair)</returns>
        public static Dictionary<string, Dictionary<DateTime, double>> ReadTimeDependentValuesWithLinearInterpolationFromSobek212(string filePath)
        {
            var text = File.ReadAllText(filePath, Encoding.Default);

            return ParseTimeDependentValuesWithLinearInterpolationFromSobek212(text);
        }

        private static Dictionary<string, Dictionary<DateTime, double>> ParseTimeDependentValuesWithLinearInterpolationFromSobek212(string text)
        {
            var processCoefficientDictionary = new Dictionary<string, Dictionary<DateTime, double>>();

            var textWithoutComment = SobekWaqReaderHelper.GetUnCommentedText(text, ";");
            var processCoefficientBlocks = SobekWaqReaderHelper.GetTextBlocks(textWithoutComment, "FUNCTIONS").Where(tb => tb.Contains("LINEAR DATA"));

            ParseTimeDependentProcessCoefficientBlocks(processCoefficientBlocks, processCoefficientDictionary, "linear");

            return processCoefficientDictionary;
        }

        private static void ParseTimeDependentProcessCoefficientBlocks(IEnumerable<string> processCoefficientBlocks, Dictionary<string, Dictionary<DateTime, double>> processCoefficientDictionary, string interpolationDescription)
        {
            foreach (var processCoefficientBlock in processCoefficientBlocks)
            {
                var parameterName = GetTimeDependentParameterName(processCoefficientBlock, "DATA");
                if (string.IsNullOrEmpty(parameterName)) continue;

                var parameterValueDictionary = GetTimeDependentParameterValues(processCoefficientBlock, "DATA", parameterName);
                if (parameterValueDictionary == null) continue;

                processCoefficientDictionary[parameterName] = parameterValueDictionary;
            }

            if (!processCoefficientBlocks.Any() || processCoefficientDictionary.Values.Count == 0)
            {
                Log.InfoFormat("No time dependent process coefficient data with {0} interpolation was found", interpolationDescription);
            }
        }

        private static string GetTimeDependentParameterName(string processCoefficientBlock, string blockEnd)
        {
            var parameterNamesTextBlock = SobekWaqReaderHelper.GetTextBlock(processCoefficientBlock, "FUNCTIONS", blockEnd);
            var parameterName = SobekWaqReaderHelper.GetTextBlock(parameterNamesTextBlock, "'", "'").Replace("'", "");

            if (string.IsNullOrEmpty(parameterName))
            {
                Log.Warn("A time dependent process coefficient data block is skipped because no parameter name was found");
                return null;
            }

            return parameterName;
        }

        private static Dictionary<DateTime, double> GetTimeDependentParameterValues(string processCoefficientBlock, string blockStart, string parameterName)
        {
            var parameterValuesTextBlock = SobekWaqReaderHelper.GetTextBlock(processCoefficientBlock, blockStart);
            
            if (string.IsNullOrEmpty(parameterValuesTextBlock))
            {
                Log.WarnFormat("The dependent process coefficient data block for '{0}' is skipped because no valid data was found", parameterName);
                return null;
            }

            var parameterValueDictionary = new Dictionary<DateTime, double>();
            var parameterValuesTextBlockLines = SobekWaqReaderHelper.GetTextLines(parameterValuesTextBlock.Replace(blockStart, ""));
            
            foreach (var parameterValuesTextBlockLine in parameterValuesTextBlockLines)
            {
                var parameterValueTuple = GetTimeDependentParameterValue(parameterValuesTextBlockLine, parameterName);
                if (parameterValueTuple != null)
                {
                    parameterValueDictionary[parameterValueTuple.First] = parameterValueTuple.Second;
                }
            }
        
            if (parameterValueDictionary.Values.Count == 0)
            {
                Log.WarnFormat("The time dependent process coefficient data block for '{0}' is skipped because no valid data was found", parameterName);
                return null;                
            }

            return parameterValueDictionary;
        }

        private static DelftTools.Utils.Tuple<DateTime, double> GetTimeDependentParameterValue(string parameterValuesTextBlockLine, string parameterName)
        {
            if (parameterValuesTextBlockLine.Trim() == string.Empty) return null;

            var dateTimeRegex = new Regex(@"(\d{4})/(\d{2})/(\d{2})-(\d{2}):(\d{2}):(\d{2})", RegexOptions.Singleline);
            var dateTimeMatch = dateTimeRegex.Match(parameterValuesTextBlockLine);

            if (dateTimeMatch.Length == 0)
            {
                Log.WarnFormat("A line in the time dependent process coefficient data block for '{0}' is skipped because its format is invalid", parameterName);
                return null;
            }

            var parameterValue = SobekWaqReaderHelper.GetDouble(parameterValuesTextBlockLine.Replace(dateTimeMatch.Value, "").Replace("'", "").TrimStart());
            
            if (double.IsNaN(parameterValue))
            {
                Log.WarnFormat("A line in the time dependent process coefficient data block for '{0}' is skipped because its format is invalid", parameterName);
                return null;               
            }

            return new DelftTools.Utils.Tuple<DateTime, double>(SobekWaqReaderHelper.ParseDateTime(dateTimeMatch.Value), parameterValue);
        }

        # endregion
    }
}
