using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DelftTools.Functions.Generic;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekWaqReaders
{
    public static class SobekWaqMeteoReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekWaqMeteoReader));

        # region Sobek212

        /// <summary>
        /// Read method for the Sobek 212 WAQ file "casedesc.cmt", 
        /// which defines the presence of specific meteo data files.
        /// </summary>
        /// <example>
        /// File "casedesc.cmt" has the following relevant structure:
        /// 
        /// ...
        /// I \SOBEK212\FIXED\DEFAULT.QSC 47 '1187266144'
        /// ...
        /// I \SOBEK212\FIXED\DEFAULT.BUI 881 '1187266144'
        /// ...
        /// I \SOBEK212\FIXED\DEFAULT.QWC 51 '1187266144' 
        /// ...
        /// </example>
        /// <param name="filePath">The path to "casedesc.cmt"</param>
        /// <returns>A DelftTools.Utils.Tuple of strings in which the first element represents the Rad/Temp meteo file name and the second element the Vwind/Winddir meteo file name. If no valid meteo file name(s) is(/are) found, the DelftTools.Utils.Tuple element value(s) involved is(/are) set to null</returns>
        /// <remarks>Warnings are logged when the file does not contain information about the presence of specific meteo data files</remarks>
        public static DelftTools.Utils.Tuple<string, string> ReadMeteoDataTypesFromSobek212(string filePath)
        {
            var text = File.ReadAllText(filePath, Encoding.Default);

            return ParseMeteoDataTypesFromSobek212(text, filePath);
        }

        private static DelftTools.Utils.Tuple<string, string> ParseMeteoDataTypesFromSobek212(string text, string filePath)
        {
            string radTempIsTimeDependent = null;
            string windIsTimeDependent = null;

            var fileNameRegex = new Regex(@"\\FIXED\\.*\.BUI", RegexOptions.Multiline);
            var fileName = fileNameRegex.Match(text).Value.Replace(@"\FIXED\", "").Replace(".BUI", "");

            if (text.Contains(@"\FIXED\" + fileName + @".QSC"))
            {
                radTempIsTimeDependent = fileName + @".QSC";
            }
            else if (text.Contains(@"\FIXED\" + fileName + @".QST"))
            {
                radTempIsTimeDependent = fileName + @".QST";
            }
            else
            {
                Log.WarnFormat(@"no meteo file information found in '{0}' for Rad and Temp ('\FIXED\" + fileName + @".QSC' or '\FIXED\" + fileName + @".QST' not found)", filePath);                
            }

            if (text.Contains(@"\FIXED\" + fileName + @".QWC"))
            {
                windIsTimeDependent = fileName + @".QWC";
            }
            else if (text.Contains(@"\FIXED\" + fileName + @".QWT"))
            {
                windIsTimeDependent = fileName + @".QWT";
            }
            else
            {
                Log.WarnFormat(@"no meteo file information found in '{0}' for Vwind and Winddir ('\FIXED\" + fileName + @".QWC' or '\FIXED\" + fileName + @".QWT' not found)", filePath);
            }

            return new DelftTools.Utils.Tuple<string, string>(radTempIsTimeDependent, windIsTimeDependent);
        }

        /// <summary>
        /// Read method for the Sobek 212 WAQ file "FIXED\DEFAULT.QSC" or "FIXED\DEFAULT.QWC", 
        /// which define constant meteo data.
        /// </summary>
        /// <example>
        /// File "FIXED\DEFAULT.QSC" has the following structure:
        /// 
        /// CONSTANTS 'TEMP' 'RAD'
        /// DATA 18.2 50
        /// 
        /// 
        /// 
        /// File "FIXED\DEFAULT.QWC" has the following structure:
        /// 
        /// CONSTANTS 'VWIND' 'WINDDIR'
        /// DATA 18.2 50
        /// </example>
        /// <param name="filePath">The path to "FIXED\DEFAULT.QSC" or "FIXED\DEFAULT.QWC"</param>
        /// <returns>
        /// A DelftTools.Utils.Tuple of double values, which holds a double value for respectively:
        /// - "Temp" and "Rad" in case of the file "FIXED\DEFAULT.QSC"
        /// - "Vwind" and "Winddir" in case of the file "FIXED\DEFAULT.QWC"
        /// </returns>
        public static DelftTools.Utils.Tuple<double, double> ReadConstantValuesFromSobek212(string filePath)
        {
            var text = File.ReadAllText(filePath, Encoding.Default);

            return ParseConstantValuesFromSobek212(text, filePath);
        }

        private static DelftTools.Utils.Tuple<double, double> ParseConstantValuesFromSobek212(string text, string filePath)
        {
            var meteoConstantTextBlock = SobekWaqReaderHelper.GetTextBlock(text, "CONSTANTS");
            var meteoDataTextBlock = SobekWaqReaderHelper.GetTextBlock(meteoConstantTextBlock, "DATA");
            var meteoDataValues = SobekWaqReaderHelper.GetDoublesFromSingleTextLine(meteoDataTextBlock.Replace("DATA", ""));

            if (meteoDataValues.Count() != 2)
            {
                Log.InfoFormat("The format of the constant meteo data file '{0}' is invalid", filePath);
                return null;
            }

            return new DelftTools.Utils.Tuple<double, double>(meteoDataValues.ElementAt(0), meteoDataValues.ElementAt(1));
        }

        /// <summary>
        /// Read method for the Sobek 212 WAQ file "FIXED\DEFAULT.QST" or "FIXED\DEFAULT.QWT", 
        /// which define time dependent meteo data (able to handle data blocks starting with 
        /// "DATA" (=> block interpolation) and "LINEAR DATA" (=> linear interpolation) )
        /// </summary>
        /// <example>
        /// * File "FIXED\DEFAULT.QST" has the following structure:
        /// 
        /// FUNCTIONS 
        /// 'TEMP'
        /// LINEAR DATA
        /// '1951/01/01-00:00:00' 1.2 1.3
        /// '1951/01/01-01:00:00' 1.4 2
        /// FUNCTIONS 
        /// 'RAD'
        /// LINEAR DATA
        /// '1951/01/01-00:00:00' 1.5 1.6
        /// '1951/01/01-01:00:00' 1.7 3
        /// 
        /// * File ""FIXED\DEFAULT.QWT" has the following structure:
        /// 
        /// FUNCTIONS 
        /// 'VWIND'
        /// DATA
        /// '1951/01/01-00:00:00' 1.2 1.3
        /// '1951/01/01-01:00:00' 1.4 2
        /// FUNCTIONS 
        /// 'WINDDIR'
        /// DATA
        /// '1951/01/01-00:00:00' 1.5 1.6
        /// '1951/01/01-01:00:00' 1.7 3
        /// </example>
        /// <param name="filePath">The path to "FIXED\DEFAULT.QST" or "FIXED\DEFAULT.QWT"</param>
        /// <returns>
        /// An enumerable containing two DelftTools.Utils.Tuples with "time step - value" dictionary and interpolation type, in which:
        /// - the first DelftTools.Utils.Tuple and the second DelftTools.Utils.Tuple in the enumeration define "time step - value" data for respectively "Temp" and "Rad" in case of the file "FIXED\DEFAULT.QST" and for respectively "Vwind" and "Winddir" in case of the file "FIXED\DEFAULT.QWT"
        /// - the interpolation types specify whether the data was of the type block ("DATA") or the type linear ("LINEAR DATA")
        /// </returns>
        public static IEnumerable<DelftTools.Utils.Tuple<Dictionary<DateTime, double>, InterpolationType>> ReadTimeDependentValuesFromSobek212(string filePath)
        {
            var text = File.ReadAllText(filePath, Encoding.Default);

            return ParseTimeDependentValuesFromSobek212(text, filePath);
        }

        private static IEnumerable<DelftTools.Utils.Tuple<Dictionary<DateTime, double>, InterpolationType>> ParseTimeDependentValuesFromSobek212(string text, string filePath)
        {
            var meteoFunctionTextBlocks = SobekWaqReaderHelper.GetTextBlocks(text, "FUNCTIONS", "(\r\n *\r\n|$|(?=FUNCTIONS))");
            if (meteoFunctionTextBlocks.Count() < 2)
            {
                Log.ErrorFormat("At least two function blocks should be present in the time dependent meteo data file '{0}'", filePath);
                return null;
            }

            var meteoDataTupleFirst = ParseMeteoFunctionBlock(meteoFunctionTextBlocks.ElementAt(0), "first", filePath);
            if (meteoDataTupleFirst == null) return null;

            var meteoDataTupleSecond = ParseMeteoFunctionBlock(meteoFunctionTextBlocks.ElementAt(1), "second", filePath);
            if (meteoDataTupleSecond == null) return null;

            return new List<DelftTools.Utils.Tuple<Dictionary<DateTime, double>, InterpolationType>> { meteoDataTupleFirst, meteoDataTupleSecond };
        }

        private static DelftTools.Utils.Tuple<Dictionary<DateTime, double>, InterpolationType> ParseMeteoFunctionBlock(string meteoFunctionTextBlock, string numberDescription, string filePath)
        {
            var meteoDataDictionary = new Dictionary<DateTime, double>();
            var meteoFunctionTextBlockLines = SobekWaqReaderHelper.GetTextLines(meteoFunctionTextBlock.Replace(SobekWaqReaderHelper.GetTextBlock(meteoFunctionTextBlock, "FUNCTIONS", "DATA"), ""));
            var dateTimeRegex = new Regex(@"(\d{4})/(\d{2})/(\d{2})-(\d{2}):(\d{2}):(\d{2})", RegexOptions.Singleline);

            foreach (var meteoDataTextBlockLine in meteoFunctionTextBlockLines)
            {
                var dateTimeMatch = dateTimeRegex.Match(meteoDataTextBlockLine);
                if (dateTimeMatch.Length == 0)
                {
                    Log.WarnFormat("A line in the {0} time dependent meteo data block of the file '{1}' is skipped because no time step value was found", numberDescription, filePath);
                    continue;
                }

                var meteoValue = SobekWaqReaderHelper.GetDouble(meteoDataTextBlockLine.Replace(dateTimeMatch.Value, "").Replace("'", ""));
                if (double.IsNaN(meteoValue))
                {
                    Log.WarnFormat("A line in the {0} time dependent meteo data block of the file '{1}' is skipped because no valid meteo value was found", numberDescription, filePath);
                    continue;
                }

                meteoDataDictionary[SobekWaqReaderHelper.ParseDateTime(dateTimeMatch.Value)] = meteoValue;
            }

            if (meteoFunctionTextBlockLines.Count() == 0 || meteoDataDictionary.Values.Count == 0)
            {
                Log.InfoFormat("No valid time dependent meteo data was found in the {0} data block of the file '{1}'", numberDescription, filePath);
                return null;
            }

            return new DelftTools.Utils.Tuple<Dictionary<DateTime, double>, InterpolationType>(meteoDataDictionary, meteoFunctionTextBlock.Contains("LINEAR DATA") ? InterpolationType.Linear : InterpolationType.Constant);
        }

        # endregion
    }
}
