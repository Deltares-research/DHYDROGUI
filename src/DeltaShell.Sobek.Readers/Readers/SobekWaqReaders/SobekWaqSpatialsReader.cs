using System.Collections.Generic;
using System.IO;
using System.Text;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekWaqReaders
{
    public static class SobekWaqSpatialsReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekWaqSpatialsReader));

        # region Sobek212

        /// <summary>
        /// Read method for the Sobek 212 WAQ spatials file "coefx.dat", 
        /// which defines location dependent values.
        /// </summary>
        /// <example>
        /// File "coefx.dat" has the following structure:
        /// 
        /// PARAMETER
        /// "AAP"
        /// 
        /// ACTIVATED
        /// -1
        /// 
        /// SURFACE WATER TYPES
        /// "Normal",555
        /// "NotSoNormal",666
        /// 
        /// INDIVIDUAL OBJECTS
        /// "bl_1",121.121
        /// "nLN2",265
        /// "bl_8",333
        /// 
        /// PARAMETER
        /// "ExtVlBak"
        /// 
        /// ACTIVATED
        /// -1 
        /// 
        /// SURFACE WATER TYPES
        /// "Normal",555
        /// "NotSoNormal",666
        /// 
        /// INDIVIDUAL OBJECTS
        /// "bl_1",121.121
        /// "nLN2",265
        /// "bl_8",333
        /// 
        /// PARAMETER
        /// "Dispersion Coefficient"
        /// 
        /// ACTIVATED
        /// 0
        /// 
        /// SURFACE WATER TYPES
        /// "Normal",555
        /// "NotSoNormal",666
        /// 
        /// INDIVIDUAL OBJECTS
        /// "bl_1",121.121
        /// "nLN2",265
        /// "bl_8",333
        /// </example>
        /// <param name="filePath">The path to "coefx.dat"</param>
        /// <returns>
        /// A dictionary mapped by spatial name, with DelftTools.Utils.Tuple values in which:
        /// * the first element represents "surface water type - value" dictionary values (spatial name => First => "surface water type - value" pair)
        /// * the second element represents "individual object - value" dictionary values (spatial name => Second => "individual object - value" pair)
        /// </returns>
        /// <remarks>The "coefx.dat" file contains spatial data for initial conditions, process coefficients as well as dispersion</remarks>
        public static Dictionary<string, DelftTools.Utils.Tuple<Dictionary<string, double>, Dictionary<string, double>>> ReadLocationDependentValuesFromSobek212(string filePath)
        {
            var text = File.ReadAllText(filePath, Encoding.Default);

            return ParseLocationDependentValuesFromSobek212(text.Replace("\"", "'"), filePath); // Replace all " by ' first
        }

        private static Dictionary<string, DelftTools.Utils.Tuple<Dictionary<string, double>, Dictionary<string, double>>> ParseLocationDependentValuesFromSobek212(string text, string filePath)
        {
            var spatialDataDictionary = new Dictionary<string, DelftTools.Utils.Tuple<Dictionary<string, double>, Dictionary<string, double>>>();
            var spatialTextBlocks = SobekWaqReaderHelper.GetTextBlocks(text, "PARAMETER", "($|(?=PARAMETER))");

            foreach (var spatialTextBlock in spatialTextBlocks)
            {
                // Obtain the parameter name
                var parameterTextBlock = SobekWaqReaderHelper.GetTextBlock(spatialTextBlock, "PARAMETER", "(\r\n *\r\n|(?=ACTIVATED))");
                var parameterName = SobekWaqReaderHelper.GetTextBlock(parameterTextBlock, "'", "'").Replace("'", "");
                if (parameterName == "")
                {
                    Log.WarnFormat("A spatial data block in the file '{0}' is skipped because no parameter name was found", filePath);
                    continue;
                }

                // Obtain the activated text block
                var activatedTextBlock = SobekWaqReaderHelper.GetTextBlock(spatialTextBlock, "ACTIVATED", "(\r\n *\r\n|(?=SURFACE WATER TYPES))");
                if (activatedTextBlock == "")
                {
                    Log.WarnFormat("The spatial data block for '{0}' in the file '{1}' is skipped because no activated block was found", parameterName, filePath);
                    continue;
                }

                // Skip parameters that are not activated
                if (activatedTextBlock.Contains("0")) continue;

                // Obtain the surface water type text block
                var surfaceWaterTypesTextBlock = SobekWaqReaderHelper.GetTextBlock(spatialTextBlock, "SURFACE WATER TYPES", "(\r\n *\r\n|(?=INDIVIDUAL OBJECTS))");
                if (surfaceWaterTypesTextBlock == "")
                {
                    Log.WarnFormat("The spatial data block for '{0}' in the file '{1}' is skipped because no surface water types block was found", parameterName, filePath);
                    continue;    
                }

                var surfaceWaterTypesDataDictionary = new Dictionary<string, double>();
                var individualObjectDataDictionary = new Dictionary<string, double>();

                // Obtain the surface water type values
                var surfaceWaterTypesTextBlockLines = SobekWaqReaderHelper.GetTextLines(surfaceWaterTypesTextBlock.Replace("SURFACE WATER TYPES", ""));
                foreach (var surfaceWaterTypesTextBlockLine in surfaceWaterTypesTextBlockLines)
                {
                    ParseValueTextBlockLine(surfaceWaterTypesTextBlockLine, surfaceWaterTypesDataDictionary, parameterName, filePath, "surface water type");
                }

                // Obtain the individual objects text block and obtain any individual object value
                var individualObjectsTextBlock = SobekWaqReaderHelper.GetTextBlock(spatialTextBlock, "INDIVIDUAL OBJECTS");
                if (individualObjectsTextBlock != "")
                {
                    var individualObjectsTextBlockLines = SobekWaqReaderHelper.GetTextLines(individualObjectsTextBlock.Replace("INDIVIDUAL OBJECTS", ""));
                    foreach (var individualObjectsTextBlockLine in individualObjectsTextBlockLines)
                    {
                        ParseValueTextBlockLine(individualObjectsTextBlockLine, individualObjectDataDictionary, parameterName, filePath, "individual object");
                    }
                }

                spatialDataDictionary[parameterName] = new DelftTools.Utils.Tuple<Dictionary<string, double>, Dictionary<string, double>>(surfaceWaterTypesDataDictionary, individualObjectDataDictionary);
            }

            return spatialDataDictionary;
        }

        private static void ParseValueTextBlockLine(string valueTextBlockLine, Dictionary<string, double> valueDataDictionary, string parameterName, string filePath, string elementDescription)
        {
            var elementName = SobekWaqReaderHelper.GetTextBlock(valueTextBlockLine, "'", "'").Replace("'", "");
            if (elementName == "")
            {
                Log.WarnFormat("A line in the spatial data block for '{0}' in the file '{1}' is skipped because no {2} was found", parameterName, filePath, elementDescription);
                return;
            }

            var elementValue = SobekWaqReaderHelper.GetDouble(valueTextBlockLine.Replace("'" + elementName + "'", "").Replace(",", ""));
            if (double.IsNaN(elementValue))
            {
                Log.WarnFormat("A line in the spatial data block for '{0}' in the file '{1}' is skipped because no {2} value was found", parameterName, filePath, elementDescription);
                return;
            }

            valueDataDictionary[elementName] = elementValue;
        }

        # endregion
    }
}