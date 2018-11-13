using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness
{
    public abstract class RoughnessReader
    {
        protected abstract RoughnessConverter GetRoughnessConverter();
        protected abstract void CreateErrorReport(string objectName, string filePath, List<string> errorMessages);

        public RoughnessSection ReadFile(string filename, IHydroNetwork network, IList<RoughnessSection> roughnessSections)
        {
            var errorMessages = new List<string>();
            var categories = ReadCategoriesFromFileAndCollectErrorMessages(filename, errorMessages);

            var roughnessConverter = GetRoughnessConverter();
            var roughnessSection = roughnessConverter.Convert(categories, network, roughnessSections, filename);

            CreateErrorReport("roughness section", filename, errorMessages);

            return roughnessSection;
        }

        private static IList<DelftIniCategory> ReadCategoriesFromFileAndCollectErrorMessages(string filePath, IList<string> errorMessages)
        {
            IList<DelftIniCategory> categories = new List<DelftIniCategory>();
            try
            {
                categories = DelftIniFileParser.ReadFile(filePath);
            }
            catch (Exception e)
            {
                errorMessages.Add(e.Message);
            }

            return categories;
        }
    }
}
