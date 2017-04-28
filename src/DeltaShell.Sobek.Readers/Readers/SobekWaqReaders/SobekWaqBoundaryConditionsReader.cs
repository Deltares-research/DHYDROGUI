using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DelftTools.Utils;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekWaqReaders
{
    public static class SobekWaqBoundaryConditionsReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekWaqBoundaryConditionsReader));

        # region Sobek212 constants

        /// <summary>
        /// Read method for the Sobek 212 WAQ boundary conditions file "BOUNDWQ.TYP", 
        /// which defines constant concentration values on a per fraction and per substance basis.
        /// </summary>
        /// <example>
        /// File "BOUNDWQ.TYP" has the following file structure:
        /// 
        /// ITEM
        /// USEFOR 'MyOwnLittleFraction' 'MyOwnLittleFraction'
        /// CONCENTRATION
        /// USEFOR 'AAP' 'AAP'
        /// USEFOR 'DetC' 'DetC'
        /// DATA           'AAP'         'DetC'
        ///                123.456       -999
        /// 
        /// ITEM
        /// USEFOR 'Lateral inflow' 'Lateral inflow'
        /// CONCENTRATION
        /// USEFOR 'AAP' 'AAP'
        /// USEFOR 'DetC' 'DetC'
        /// DATA        'AAP'         'DetC'        
        ///             111.222       -999
        /// </example>
        /// <param name="filePath">The path to "BOUNDWQ.TYP"</param>
        /// <returns>A dictionary mapped by fraction name, with "substance name - concentration value" dictionary values (fraction name => "substance name - concentration value" pair)</returns>
        public static Dictionary<string, Dictionary<string, double>> ReadConstantFractionValuesFromSobek212(string filePath)
        {
            return ParseConstantValuesFromSobek212(filePath, false, "fraction");
        }

        /// <summary>
        /// Read method for the Sobek 212 WAQ boundary conditions file "BOUNDWQ.DAT", 
        /// which defines constant concentration values on a per boundary and per substance basis.
        /// </summary>
        /// <example>
        /// File "BOUNDWQ.DAT" has the following structure:
        /// 
        /// ITEM
        /// USEFOR 'nN1' 'nN1'
        /// CONCENTRATION
        /// USEFOR 'AAP' 'AAP'
        /// USEFOR 'DetC' 'DetC'
        /// DATA           'AAP'         'DetC'
        ///                123.456       -999
        /// 
        /// ITEM
        /// USEFOR 'nLS1' 'nLS1'
        /// CONCENTRATION
        /// USEFOR 'AAP' 'AAP'
        /// USEFOR 'DetC' 'DetC'
        /// DATA        'AAP'         'DetC'        
        ///             111.222       -999
        /// </example>
        /// <param name="filePath">The path to "BOUNDWQ.DAT"</param>
        /// <returns>A dictionary mapped by boundary name, with "substance name - concentration value" dictionary values (boundary name => "substance name - concentration value" pair)</returns>
        public static Dictionary<string, Dictionary<string, double>> ReadConstantBoundaryValuesFromSobek212(string filePath)
        {
            return ParseConstantValuesFromSobek212(filePath, true, "boundary");
        }

        private static Dictionary<string, Dictionary<string, double>> ParseConstantValuesFromSobek212(string filePath, bool trimNamePrefixes, string dataTypeDescription)
        {
            var dataBlocks = GetBlocks(filePath, ";;", "ITEM").Where(tb => !tb.Contains("ABSOLUTE TIME"));

            var dataDictionary = new Dictionary<string, Dictionary<string, double>>();

            foreach (var dataBlock in dataBlocks)
            {
                var name = GetName(dataBlock, trimNamePrefixes, dataTypeDescription);
                if (name == null) continue;

                var subtanceConcentrationBlock = GetSubtanceConcentrationBlock(dataBlock, name, "DATA", dataTypeDescription);
                if (subtanceConcentrationBlock == null) continue;

                var subtanceConcentrationBlockTextLines = SobekWaqReaderHelper.GetTextLines(subtanceConcentrationBlock).ToList();
                if (subtanceConcentrationBlockTextLines.Count() < 2) continue;

                var subtanceNames = SobekWaqReaderHelper.GetTextBlocks(subtanceConcentrationBlockTextLines.ElementAt(0), "'", "'").Select(sn => sn.Replace("'", ""));
                var concentrationValues = SobekWaqReaderHelper.GetDoublesFromSingleTextLine(subtanceConcentrationBlockTextLines.ElementAt(1));

                dataDictionary[name] = SobekWaqReaderHelper.CreateDoubleDictionary(subtanceNames, concentrationValues,
                                                                                                   string.Format("The {0} data block for '{1}' is partially imported because the number of substances did not equal the number of concentrations", dataTypeDescription, name));
            }

            if (dataDictionary.Values.Count == 0)
            {
                Log.InfoFormat("No constant {0} data was found", dataTypeDescription);
            }
            else
            {
                Log.DebugFormat("Constant {0} data was found", dataTypeDescription);
            }

            return dataDictionary;
        }

        # endregion

        # region Sobek212 time series with block interpolation

        /// <summary>
        /// Read method for the Sobek 212 WAQ boundary conditions file "BOUNDWQ.TYP", 
        /// which defines time dependent concentration values with block interpolation.
        /// </summary>
        /// <example>
        /// File "BOUNDWQ.TYP" has the following structure:
        /// 
        /// ITEM
        /// USEFOR 'MyOwnLittleFraction' 'MyOwnLittleFraction'
        /// ABSOLUTE TIME
        /// CONCENTRATION
        /// USEFOR 'Continuity' 'Continuity'
        /// USEFOR 'NH4' 'NH4'
        /// DATA                      'Continuity'     'NH4'
        /// '2010/01/01-00:00:00'     1                1.5
        /// '2010/07/01-00:00:00'     1                1.6 
        /// </example>
        /// <param name="filePath">The path to "BOUNDWQ.TYP"</param>
        /// <returns>A dictionary mapped by fraction name, with dictionary values mapped by timestep, with "substance name - concentration value" dictionary values (fraction name => timestep => "substance name - concentration value" pair)</returns>
        public static Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>> ReadTimeDependentFractionValuesWithBlockInterpolationFromSobek212(string filePath)
        {
            return ParseTimeDependentValuesWithBlockInterpolationFromSobek212(filePath, false, "fraction");
        }

        /// <summary>
        /// Read method for the Sobek 212 WAQ boundary conditions file "BOUNDWQ.DAT", 
        /// which defines time dependent concentration values with block interpolation.
        /// </summary>
        /// <example>
        /// File "BOUNDWQ.DAT" has the following structure:
        /// 
        /// ITEM
        /// USEFOR 'nB1' 'nB1'
        /// ABSOLUTE TIME
        /// CONCENTRATION
        /// USEFOR 'Continuity' 'Continuity'
        /// USEFOR 'NH4' 'NH4'
        /// DATA                      'Continuity'     'NH4'
        /// '2010/01/01-00:00:00'     1                1.5
        /// '2010/07/01-00:00:00'     1                1.6 
        /// </example>
        /// <param name="filePath">The path to "BOUNDWQ.DAT"</param>
        /// <returns>A dictionary mapped by boundary name, with dictionary values mapped by timestep, with "substance name - concentration value" dictionary values (boundary name => timestep => "substance name - concentration value" pair)</returns>
        public static Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>> ReadTimeDependentBoundaryValuesWithBlockInterpolationFromSobek212(string filePath)
        {
            return ParseTimeDependentValuesWithBlockInterpolationFromSobek212(filePath, true, "boundary");
        }

        private static Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>> ParseTimeDependentValuesWithBlockInterpolationFromSobek212(string filePath, bool trimNamePrefixes, string dataTypeDescription)
        {
            var dataBlocks = GetBlocks(filePath, ";;", "ITEM").Where(tb => tb.Contains("ABSOLUTE TIME") && !tb.Contains("LINEAR DATA"));
            return ParseTimeDependentDataBlocks(dataBlocks, "block", trimNamePrefixes, dataTypeDescription);
        }

        # endregion

        # region Sobek212 time series with linear interpolation

        /// <summary>
        /// Read method for the Sobek 212 WAQ boundary conditions file "BOUNDWQ.TYP", 
        /// which defines time dependent concentration values with linear interpolation
        /// </summary>
        /// <example>
        /// File "BOUNDWQ.TYP" has the following structure:
        /// 
        /// ITEM
        /// USEFOR 'MyOwnLittleFraction' 'MyOwnLittleFraction'
        /// ABSOLUTE TIME
        /// CONCENTRATION
        /// USEFOR 'Continuity' 'Continuity'
        /// USEFOR 'NH4' 'NH4'
        /// LINEAR DATA               'Continuity'     'NH4'
        /// '2010/01/01-00:00:00'     1                1.5
        /// '2010/07/01-00:00:00'     1                1.6
        /// </example>
        /// <param name="filePath">The path to "BOUNDWQ.TYP"</param>
        /// <returns>A dictionary mapped by fraction name, with dictionary values mapped by timestep, with "substance name - concentration value" dictionary values (fraction name => timestep => "substance name - concentration value" pair)</returns>
        public static Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>> ReadTimeDependentFractionValuesWithLinearInterpolationFromSobek212(string filePath)
        {
            return ParseTimeDependentValuesWithLinearInterpolationFromSobek212(filePath, false, "fraction");
        }

        /// <summary>
        /// Read method for the Sobek 212 WAQ boundary conditions file "BOUNDWQ.DAT", 
        /// which defines time dependent concentration values with linear interpolation.
        /// </summary>
        /// <example>
        /// File "BOUNDWQ.DAT" has the following structure:
        /// 
        /// ITEM
        /// USEFOR 'nB1' 'nB1'
        /// ABSOLUTE TIME
        /// CONCENTRATION
        /// USEFOR 'Continuity' 'Continuity'
        /// USEFOR 'NH4' 'NH4'
        /// LINEAR DATA               'Continuity'     'NH4'
        /// '2010/01/01-00:00:00'     1                1.5
        /// '2010/07/01-00:00:00'     1                1.6
        /// </example>
        /// <param name="filePath">The path to "BOUNDWQ.DAT"</param>
        /// <returns>A dictionary mapped by boundary name, with dictionary values mapped by timestep, with "substance name - concentration value" dictionary values (boundary name => timestep => "substance name - concentration value" pair)</returns>
        public static Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>> ReadTimeDependentBoundaryValuesWithLinearInterpolationFromSobek212(string filePath)
        {

            return ParseTimeDependentValuesWithLinearInterpolationFromSobek212(filePath, true, "boundary");
        }

        private static Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>> ParseTimeDependentValuesWithLinearInterpolationFromSobek212(string filePath, bool trimNamePrefixes, string dataTypeDescription)
        {
            var dataBlocks = GetBlocks(filePath, ";;", "ITEM").Where(tb => tb.Contains("ABSOLUTE TIME") && tb.Contains("LINEAR DATA"));
            return ParseTimeDependentDataBlocks(dataBlocks, "linear", trimNamePrefixes, dataTypeDescription);
        }

        # endregion
         
        # region Sobek212 helper functions

        private static IEnumerable<string> GetBlocks(string filePath, string commentMarker, string blockHeader, string blockFooter = "")
        {
            var inBlock = false;

            using (var stream = new StreamReader(filePath, Encoding.Default))
            {
                var block = new StringBuilder();

                while (!stream.EndOfStream)
                {
                    var line = stream.ReadLine().Trim();

                    if (line.StartsWith(commentMarker))
                    {
                        continue;
                    }

                    if (line == blockHeader)
                    {
                        block = new StringBuilder();
                        inBlock = true;
                    }

                    block.AppendLine(line);

                    if (inBlock && line == blockFooter)
                    {
                        yield return block.ToString();
                        inBlock = false;
                    }
                }
            }
        }

        private static Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>> ParseTimeDependentDataBlocks(IEnumerable<string> dataBlocks, string interpolationDescription, bool trimNamePrefixes, string dataTypeDescription)
        {
            var dataDictionary = new Dictionary<string, Dictionary<DateTime, Dictionary<string, double>>>();

            foreach (var dataBlock in dataBlocks)
            {
                var name = GetName(dataBlock, trimNamePrefixes, dataTypeDescription);
                if (name == null) continue;

                var subtanceConcentrationBlock = GetSubtanceConcentrationBlock(dataBlock, name, "DATA", dataTypeDescription);
                if (subtanceConcentrationBlock == null) continue;

                var subtanceConcentrationBlockTextLines = SobekWaqReaderHelper.GetTextLines(subtanceConcentrationBlock).ToList();
                var timeDependentConcentrationValues = new Dictionary<DateTime, Dictionary<string, double>>();

                if (subtanceConcentrationBlockTextLines.Count >= 2)
                {
                    var subtanceNames = SobekWaqReaderHelper.GetTextBlocks(subtanceConcentrationBlockTextLines.ElementAt(0), "'", "'").Select(sn => sn.Replace("'", "")).ToList();

                    for (var i = 1; i < subtanceConcentrationBlockTextLines.Count(); i++)
                    {
                        var dateTimeValuesTuple = GetTimeDependentConcentrationValues(subtanceConcentrationBlockTextLines.ElementAt(i), name, dataTypeDescription);
                        if (dateTimeValuesTuple != null)
                        {
                            timeDependentConcentrationValues[dateTimeValuesTuple.First] = SobekWaqReaderHelper.CreateDoubleDictionary(subtanceNames, dateTimeValuesTuple.Second,
                                                                                                                                      string.Format("A line in the time dependent {0} data block for '{1}' is partially imported because the number of substances did not equal the number of concentrations", dataTypeDescription, name));
                        }
                    }
                }

                if (timeDependentConcentrationValues.Values.Count == 0)
                {
                    Log.WarnFormat("The {0} data block for '{1}' is skipped because no time dependent substances/concentrations values could be found", dataTypeDescription, name);
                }
                else
                {
                    dataDictionary[name] = timeDependentConcentrationValues;
                }
            }

            if (dataDictionary.Values.Count == 0)
            {
                Log.InfoFormat("No time dependent {0} data with {1} interpolation was found", dataTypeDescription, interpolationDescription);
            }
            else
            {
                Log.DebugFormat("Time dependent {0} data with {1} interpolation was found", dataTypeDescription, interpolationDescription);
            }
            return dataDictionary;
        }

        private static string GetName(string dataBlockText, bool trimPrefix, string dataTypeDescription)
        {
            var nameBlockRegex = new Regex("ITEM.*?\r\n.*?USEFOR.*?'(?<name>.*?)'", RegexOptions.Multiline);
            var nameBlockMatch = nameBlockRegex.Match(dataBlockText);

            if (!nameBlockMatch.Success)
            {
                Log.WarnFormat("A {0} data block is skipped because no valid {0} name could be retrieved", dataTypeDescription);
                return null;
            }

            var name = nameBlockMatch.Groups["name"].Value;
            if (name == string.Empty)
            {
                Log.WarnFormat("A {0} data block is skipped because no valid {0} name could be retrieved", dataTypeDescription);
                return null;
            }

            if (trimPrefix)
            {
                if (name.StartsWith("n")) return name.ReplaceFirst("n", "");
                if (name.StartsWith("bl_")) return name.ReplaceFirst("bl_", "");

                Log.WarnFormat("A {0} data block is skipped because the {0} name is invalid (needs to start with 'n' or 'bl_')", dataTypeDescription);
                return null;
            }

            return name;
        }

        private static string GetSubtanceConcentrationBlock(string dataBlockText, string name, string dataBlockStart, string dataTypeDescription)
        {
            var substanceConcentrationsBlock = SobekWaqReaderHelper.GetTextBlock(dataBlockText, dataBlockStart);

            if (substanceConcentrationsBlock.Length == 0)
            {
                Log.WarnFormat("The {0} data block for '{1}' is skipped because no valid substances/concentrations block could be found", dataTypeDescription, name);
                return null;
            }

            return substanceConcentrationsBlock;
        }

        private static DelftTools.Utils.Tuple<DateTime, IEnumerable<double>> GetTimeDependentConcentrationValues(string subtanceConcentrationBlockTextLine, string name, string dataTypeDescription)
        {
            var dateTimeRegex = new Regex(@"(\d{4})/(\d{2})/(\d{2})-(\d{2}):(\d{2}):(\d{2})", RegexOptions.Singleline);
            var dateTimeMatch = dateTimeRegex.Match(subtanceConcentrationBlockTextLine);

            if (dateTimeMatch.Length == 0)
            {
                Log.WarnFormat("A line in the time dependent {0} data block for '{1}' is skipped because its format is invalid", dataTypeDescription, name);
                return null;
            }

            var concentrationValues = SobekWaqReaderHelper.GetDoublesFromSingleTextLine(subtanceConcentrationBlockTextLine.Replace(dateTimeMatch.Value, ""));

            return new DelftTools.Utils.Tuple<DateTime, IEnumerable<double>>(SobekWaqReaderHelper.ParseDateTime(dateTimeMatch.Value), concentrationValues);
        }

        # endregion
    }
}
