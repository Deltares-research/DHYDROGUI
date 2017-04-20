using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekWaqReaders
{
    public static class SobekWaqInitialConditionsReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekWaqInitialConditionsReader));

        # region Sobek212

        /// <summary>
        /// Read method for the Sobek 212 WAQ initial conditions file "BOUNDWQ.GLB", 
        /// which defines constant concentration values on a per substance basis.
        /// </summary>
        /// <example>
        /// File "BOUNDWQ.GLB" has the following structure:
        /// 
        /// ITEM
        /// USEFOR
        ///   'Global Initials'
        /// CONCENTRATION
        ///   'AAP'
        ///   'DetC'
        ///   'DetN'
        /// DATA
        ///  11.2233
        ///  2
        ///  22.33
        /// </example>
        /// <param name="filePath">The path to "BOUNDWQ.GLB"</param>
        /// <returns>A dictionary of "substance name - concentration value" pairs</returns>
        /// <remarks>An info message is logged when the text format of the file is invalid</remarks>
        public static Dictionary<string, double> ReadConstantValuesFromSobek212(string filePath)
        {
            var text = File.ReadAllText(filePath, Encoding.Default);

            return ParseConstantValuesFromSobek212(text);
        }

        private static Dictionary<string, double> ParseConstantValuesFromSobek212(string text)
        {
            var substancesTextBlock = SobekWaqReaderHelper.GetTextBlock(text, "CONCENTRATION", "DATA");
            var concentrationsTextBlock = SobekWaqReaderHelper.GetTextBlock(text, "DATA");

            if (substancesTextBlock.Length == 0 || concentrationsTextBlock.Length == 0)
            {
                Log.InfoFormat("No constant initial conditions data was found");
                return null;
            }

            var substanceNames = SobekWaqReaderHelper.GetTextBlocks(substancesTextBlock, "'", "'").Select(sn => sn.Replace("'", ""));
            var concentrationValues = SobekWaqReaderHelper.GetDoublesFromMultipleTextLines(concentrationsTextBlock);

            return SobekWaqReaderHelper.CreateDoubleDictionary(substanceNames, concentrationValues,
                                                               string.Format("The constant initial conditions data block is partially imported because the number of substances did not equal the number of concentrations"));
        }

        # endregion
    }
}
