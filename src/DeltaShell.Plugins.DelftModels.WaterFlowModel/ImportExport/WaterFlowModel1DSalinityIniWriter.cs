using System;
using System.Collections.Generic;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public static class WaterFlowModel1DSalinityIniWriter
    {

        #region Constants

        // Choosing a string representation of the values here, to get non-scientific (= more readable) notation in the files.

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

        #endregion

        /// <summary>
        /// This method will write the file to the target path (generally as part of a D-Flow1D model within DIMR). 
        /// </summary>
        /// <param name="targetPath">Path to write the salinity file to.</param>
        /// <param name="formulationType">The formulation to which the Cx values have to be changed.</param>
        /// <param name="nodeId">Estuary mouth node id</param>
        public static void WriteFile(string targetPath, DispersionFormulationType formulationType, string nodeId)
        {
            Dictionary<string, string> cxDictionary;
            switch (formulationType)
            {
               case DispersionFormulationType.KuijperVanRijnPrismatic:
                    cxDictionary = CxDictionaryKuijperVanRijnPrismatic;
                    break;
                default:
                    throw new InvalidOperationException(String.Format("Dispersion formulation {0} is not supported.", formulationType));
            }

            var numericalOptionsCategory = new DelftIniCategory("NumericalOptions");

            numericalOptionsCategory.SetProperty("teta", 1.0);
            numericalOptionsCategory.SetProperty("tidalPeriod", 12.417);
            numericalOptionsCategory.SetProperty("advectionScheme", "vanLeer-2");
            
            cxDictionary.ForEach(kvp => numericalOptionsCategory.SetProperty(kvp.Key, kvp.Value));

            var mouthCategory = new DelftIniCategory("Mouth");
            mouthCategory.SetProperty("nodeId", nodeId, "Estuary mouth node id");

            new DelftIniWriter().WriteDelftIniFile(new []{numericalOptionsCategory, mouthCategory}, targetPath); 
        }
    }
}
