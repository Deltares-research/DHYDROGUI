using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DelftTools.Utils;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers.SobekWaqReaders
{
    public static class SobekWaqNumericalSettingsReader
    {
        # region Sobek212

        /// <summary>
        /// Read method for the Sobek 212 WAQ numerical settings file "DELWAQ1.INP", 
        /// which defines numerical settings by the integration option.
        /// </summary>
        /// <example>
        /// File "DELWAQ1.INP" has the following structure:
        /// 
        ///    86400 'DDHHMMSS' 'DDHHMMSS'  ; system clock
        ///                           15.70 ; integration option
        ///             1997/01/01-01:00:00 ; simulation starting time
        ///             1998/01/01-01:00:00 ; simulation end time
        ///                               0 ; timestep constant
        ///                       000010018 ; simulation timestep
        /// </example>
        /// <param name="filePath">The path to "DELWAQ1.INP"</param>
        /// <returns>A NumericalSettings object</returns>
        /// <exception cref="FormatException">Thrown when the text format of the file is invalid</exception>
        public static SobekWaqNumericalSettings ReadNumericalSettingsFromSobek212(string filePath)
        {
            var text = File.ReadAllText(filePath, Encoding.Default);

            return ParseNumericalSettingsFromSobek212(text);
        }

        private static SobekWaqNumericalSettings ParseNumericalSettingsFromSobek212(string numericalSettingsText)
        {
            var numericalSettingsTextLines = GetNumericalSettingsTextLines(numericalSettingsText);
            var numericalSettingsValues = GetNumericalSettingsValues(numericalSettingsTextLines.ElementAt(1));

            return new SobekWaqNumericalSettings
                       {
                           NumericalScheme1D = numericalSettingsValues.ElementAt(0),
                           NoDispersionIfFlowIsZero = (numericalSettingsValues.ElementAt(1) & 1) != 0,
                           NoDispersionOverOpenBoundaries = (numericalSettingsValues.ElementAt(1) & 2) != 0,
                           UseFirstOrder = (numericalSettingsValues.ElementAt(1) & 4) == 0, // Negation
                           BalanceOutputLevel = numericalSettingsValues.ElementAt(2)
                       };
        }

        private static IEnumerable<string> GetNumericalSettingsTextLines(string numericalSettingsText)
        {
            var numericalSettingsTextLines = SobekWaqReaderHelper.GetTextLines(numericalSettingsText);

            if (numericalSettingsTextLines.Count() != 6)
            {
                throw new FormatException("no valid data was found");
            }

            return numericalSettingsTextLines;
        }

        private static IEnumerable<int> GetNumericalSettingsValues(string numericalSettingsLine)
        {
            // Strip off any comment first (comment might contain integer values too)
            var commentIndex = numericalSettingsLine.IndexOf(";");
            if (commentIndex != -1)
            {
                numericalSettingsLine = numericalSettingsLine.Substring(0, commentIndex);
            }

            var numericalSchemeRegex = new Regex(@"\d+\.", RegexOptions.Singleline);
            var numericalSchemeMatch = numericalSchemeRegex.Match(numericalSettingsLine);

            if (numericalSchemeMatch.Length == 0)
            {
                throw new FormatException("no valid numerical scheme was found");
            }

            var numericalSettingsValuesRegex = new Regex(@"\d", RegexOptions.Singleline);
            var numericalSettingsValuesMatches = numericalSettingsValuesRegex.Matches(StringExtensions.ReplaceFirst(numericalSettingsLine, numericalSchemeMatch.Value, ""));

            if (numericalSettingsValuesMatches.Count != 2)
            {
                throw new FormatException("no valid numerical settings data was found");                
            }

            return new List<int>
                       {
                           Convert.ToInt32(numericalSchemeMatch.Value.Replace(".", "")), 
                           Convert.ToInt32(numericalSettingsValuesMatches[0].Value), 
                           Convert.ToInt32(numericalSettingsValuesMatches[1].Value)
                       };
        }

        # endregion
    }
}
