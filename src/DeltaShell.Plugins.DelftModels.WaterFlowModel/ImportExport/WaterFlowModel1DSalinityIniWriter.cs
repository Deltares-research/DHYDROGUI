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
            {SalinityRegion.C3.Key, "1.0"},
            {SalinityRegion.C4.Key, "1.0"},
            {SalinityRegion.C5.Key, "0.5"},
            {SalinityRegion.C6.Key, "1.0"},
            {SalinityRegion.C7.Key, "0.0"},
            {SalinityRegion.C8.Key, "1.0"},
            {SalinityRegion.C9.Key, "0.0"},
            {SalinityRegion.C10.Key, "0.5"},
            {SalinityRegion.C11.Key, "0.0"}
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
                    throw new InvalidOperationException($"Dispersion formulation {formulationType} is not supported.");
            }

            var numericalOptionsCategory = new DelftIniCategory(SalinityRegion.NumericalOptionsHeader);

            numericalOptionsCategory.SetProperty(SalinityRegion.Teta.Key, 1.0);
            numericalOptionsCategory.SetProperty(SalinityRegion.TidalPeriod.Key, 12.417);
            numericalOptionsCategory.SetProperty(SalinityRegion.AdvectionScheme.Key, "vanLeer-2");
            
            cxDictionary.ForEach(kvp => numericalOptionsCategory.SetProperty(kvp.Key, kvp.Value));

            var mouthCategory = new DelftIniCategory(SalinityRegion.MouthHeader);
            mouthCategory.SetProperty(SalinityRegion.NodeId.Key, nodeId, SalinityRegion.NodeId.Description);

            new DelftIniWriter().WriteDelftIniFile(new []{numericalOptionsCategory, mouthCategory}, targetPath); 
        }
    }
}
