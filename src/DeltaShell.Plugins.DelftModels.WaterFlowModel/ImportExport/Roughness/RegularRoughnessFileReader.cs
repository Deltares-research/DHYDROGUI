using System;
using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness
{
    public class RegularRoughnessFileReader : RoughnessReader
    {
        private readonly Action<string, IList<string>> createAndAddErrorReport;

        public RegularRoughnessFileReader(Action<string, IList<string>> createAndAddErrorReport)
        {
            this.createAndAddErrorReport = createAndAddErrorReport;
        }

        protected override RoughnessConverter GetRoughnessConverter()
        {
            return new RegularRoughnessConverter();
        }

        protected override void CreateErrorReport(string objectName, string filePath, List<string> errorMessages)
        {
            if (errorMessages.Count > 0)
                createAndAddErrorReport?.Invoke($"While reading the {objectName} from file '{filePath}', the following errors occured", errorMessages);
        }
    }
}