using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekWaqReaders
{
    public static class SobekWaqDispersionReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekWaqDispersionReader));

        # region Sobek212

        /// <summary>
        /// Read method for the Sobek 212 WAQ dispersion file "DELWAQ3.INP", which defines constant dispersion.
        /// </summary>
        /// <example>
        /// File  has the following structure:
        /// 
        /// ;                         dispersions
        ///      1                 ;  dispersions in this file
        ///      1.0 1.0 1.0       ;  scale factors for 3 directions
        ///        0 0.0 0.0       ;  values (m2/s) for 3 directions
        /// </example>
        /// <param name="filePath">The path to "DELWAQ3.INP"</param>
        /// <returns>A constant dispersion value</returns>
        /// <remarks>An info message is logged when the text format of the file is invalid</remarks>
        public static double ReadConstantValuesFromSobek212(string filePath)
        {
            var text = File.ReadAllText(filePath, Encoding.Default);

            return ParseConstantValuesFromSobek212(text);
        }

        private static double ParseConstantValuesFromSobek212(string dispersionText)
        {
            var dispersionTextLines = GetDispersionTextLines(dispersionText);

            return dispersionTextLines == null ? double.NaN : GetDispersionValue(dispersionTextLines.ElementAt(2));
        }

        private static IEnumerable<string> GetDispersionTextLines(string dispersionText)
        {
            var dispersionTextLines = SobekWaqReaderHelper.GetTextLines(dispersionText, ";");

            if (dispersionTextLines.Count() != 3)
            {
                Log.InfoFormat("No constant dispersion data was found");
                return null;
            }

            return dispersionTextLines;
        }

        private static double GetDispersionValue(string dispersionTextLine)
        {
            var dispersionValue = SobekWaqReaderHelper.GetDouble(dispersionTextLine);

            if (double.IsNaN(dispersionValue))
            {
                Log.InfoFormat("No valid constant dispersion data was found");
            }

            return dispersionValue;
        }

        # endregion
    }
}
