using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders.SpatialData
{
    public class SalinityFileReader
    {
        private readonly Action<string, IList<string>> createAndAddErrorReport;

        public SalinityFileReader(Action<string, IList<string>> createAndAddErrorReport)
        {
            this.createAndAddErrorReport = createAndAddErrorReport;
        }

        public string ReadEstuaryMouthNodeId(string filePath)
        {
            var errorMessages = new List<string>();
            var categories = ReadCategoriesFromFileAndCollectErrorMessages(filePath, errorMessages);
            var estuaryMouthNodeId = SalinityConverter.Convert(categories, errorMessages);

            CreateErrorReport("estuary mouth node id", filePath, errorMessages);
            return estuaryMouthNodeId;
        }

        private static IEnumerable<DelftIniCategory> ReadCategoriesFromFileAndCollectErrorMessages(string filePath, ICollection<string> errorMessages)
        {
            IList<DelftIniCategory> categories = new List<DelftIniCategory>();
            try
            {
                categories.AddRange(DelftIniFileParser.ReadFile(filePath));
            }
            catch (Exception e)
            {
                errorMessages.Add(e.Message);
            }

            return categories;
        }

        private void CreateErrorReport(string objectName, string filePath, IList<string> errorMessages)
        {
            if (errorMessages.Any())
            {
                createAndAddErrorReport?.Invoke($"While reading the {objectName} from file '{filePath}', the following errors occured", errorMessages);
            }
        }
    }
}
