using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public static class WaterFlowModel1DSalinityIniWriter
    {

        #region Constants

        // Choosing a string representation of the values here, to get non-scientific (= more readable) notation in the files.

        private static readonly Dictionary<string, string> CxDictionaryGisen = new Dictionary<string, string>
        {
            {"c3", "1.0"},
            {"c4", "1.0"},
            {"c5", "0.57"},
            {"c6", "0.0"},
            {"c7", "0.0"},
            {"c8", "0.0"},
            {"c9", "0.0"},
            // c10 is user-defined, so don't update this.
            {"c11", "0.0"}
        };

        private static readonly Dictionary<string, string> CxDictionaryKuijperVanRijnConvergent = new Dictionary<string, string>
        {
            {"c3", "1.0"},
            {"c4", "1.0"},
            {"c5", "0.5"},
            {"c6", "1.0"},
            {"c7", "1.0"},
            {"c8", "0.0"},
            {"c9", "0.0"},
            {"c10", "0.5"},
            {"c11", "0.0"}
        };

        private static readonly Dictionary<string, string> CxDictionaryKuijperVanRijnPrismatic = new Dictionary<string, string>
        {
            {"c3", "1.0"},
            {"c4", "1.0"},
            {"c5", "0.5"},
            {"c6", "1.0"},
            {"c7", "0.0"},
            {"c8", "1.0"},
            {"c9", "0.0"},
            {"c10", "0.5"},
            {"c11", "0.0"}
        };

        private static readonly Dictionary<string, string> CxDictionarySavenije = new Dictionary<string, string>
        {
            {"c3", "1.0"},
            {"c4", "1.0"},
            {"c5", "0.5"},
            {"c6", "0.0"},
            {"c7", "1.0"},
            {"c8", "0.0"},
            {"c9", "0.0"},
            // c10 is user-defined, so don't update this.
            {"c11", "0.0"}
        };

        private static readonly Dictionary<string, string> CxDictionaryThatcherHarleman = new Dictionary<string, string>
        {
            {"c3", "0.0"},
            {"c4", "0.0"},
            {"c5", "0.25"},
            {"c6", "0.0"},
            {"c7", "0.0"},
            {"c8", "0.0"},
            {"c9", "0.0"},
            {"c10", "1.0"},
            {"c11", "1.0"}
        };

        private static readonly Dictionary<string, string> CxDictionaryZhang = new Dictionary<string, string>
        {
            {"c3", "1.0"},
            {"c4", "1.0"},
            // c5 is user-defined, so don't update this.
            {"c6", "0.0"},
            {"c7", "0.0"},
            {"c8", "0.0"},
            {"c9", "10.0"},
            // c10 is user-defined, so don't update this.
            {"c11", "0.0"}
        };

        #endregion

        /// <summary>
        /// This method will read the source file (a DelftIni file), adapt the Cx values to the dispersion formulation, 
        /// and write the file to the target path (generally as part of a D-Flow1D model within DIMR). 
        /// </summary>
        /// <param name="sourcePath">Path of the original salinity.ini file.</param>
        /// <param name="targetPath">Path to write the adapted file to.</param>
        /// <param name="formulationType">The formulation to which the Cx values have to be changed.</param>
        public static void WriteFile(string sourcePath, string targetPath, DispersionFormulationType formulationType)
        {
            IList<DelftIniCategory> categories = new DelftIniReader().ReadDelftIniFile(sourcePath);

            DelftIniCategory numericalOptionsCategory = categories.FirstOrDefault(c => c.Name == "NumericalOptions");
            if (numericalOptionsCategory == null)
            {
                throw new InvalidOperationException(String.Format("Can not find the [NumericalOptions] category in file {0}.", sourcePath));
            }

            Dictionary<string, string> cxDictionary;
            switch (formulationType)
            {
               case DispersionFormulationType.Gisen:
                    cxDictionary = CxDictionaryGisen;
                    break;
               case DispersionFormulationType.KuijperVanRijnConvergent:
                    cxDictionary = CxDictionaryKuijperVanRijnConvergent;
                    break;
               case DispersionFormulationType.KuijperVanRijnPrismatic:
                    cxDictionary = CxDictionaryKuijperVanRijnPrismatic;
                    break;
               case DispersionFormulationType.Savenije:
                    cxDictionary = CxDictionarySavenije;
                    break;
               case DispersionFormulationType.ThatcherHarleman:
                    cxDictionary = CxDictionaryThatcherHarleman;
                    break;
               case DispersionFormulationType.Zhang:
                    cxDictionary = CxDictionaryZhang;
                    break;
                default:
                    // Writing the salinity.ini should only be done when a non-constant dispersion formulation type has been chosen by the user. 
                    throw new InvalidOperationException(String.Format("Dispersion formulation not supported when adapting the salinity.ini file: {0}", formulationType));
            }

            foreach (KeyValuePair<string, string> kvp in cxDictionary)
            {
                // In case the key is not there, it will be added.
                numericalOptionsCategory.SetProperty(kvp.Key, kvp.Value);
            }

            new DelftIniWriter().WriteDelftIniFile(categories, targetPath); 
        }
    }
}
