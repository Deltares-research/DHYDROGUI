using System;
using System.Collections.Generic;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections.Reader
{
    public class CrossSectionLocationFileReader
    {
        private readonly Action<string, IList<string>> createAndAddErrorReport;

        public CrossSectionLocationFileReader(Action<string, IList<string>> createAndAddErrorReport)
        {
            this.createAndAddErrorReport = createAndAddErrorReport;
        }

        public IEnumerable<ICrossSectionLocation> Read(string filePath)
        {
            var errorMessages = new List<string>();

            var categories = ReadCategoriesFromFileAndCollectErrorMessages(filePath, errorMessages);

            var crossSectionLocations = CrossSectionLocationConverter.Convert(categories, errorMessages);

            CreateErrorReport("cross section locations", filePath, errorMessages);

            return crossSectionLocations;
        }

        private static IList<DelftIniCategory> ReadCategoriesFromFileAndCollectErrorMessages(string filePath, List<string> errorMessages)
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

        private void CreateErrorReport(string objectName, string filePath, List<string> errorMessages)
        {
            if (errorMessages.Count > 0)
                createAndAddErrorReport?.Invoke($"While reading the {objectName} from file '{filePath}', the following errors occured", errorMessages);
        }
    }
}
