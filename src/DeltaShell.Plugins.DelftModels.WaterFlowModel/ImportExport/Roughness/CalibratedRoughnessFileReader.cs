using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness
{
    public class CalibratedRoughnessFileReader : RoughnessReader
    {
        protected override RoughnessConverter GetRoughnessConverter()
        {
            return new CalibratedRoughnessConverter();
        }

        protected override void CreateErrorReport(string objectName, string filePath, List<string> errorMessages)
        {
            // Do nothing
        }
    }
}
