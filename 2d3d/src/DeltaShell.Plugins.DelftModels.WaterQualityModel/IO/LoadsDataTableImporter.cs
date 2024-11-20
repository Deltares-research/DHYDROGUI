using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public class LoadsDataTableImporter : DataTableImporter
    {
        public override string Name => "Data table loads importer";

        public override bool CanImportOn(object targetObject)
        {
            var dataTableManager = targetObject as DataTableManager;

            if (dataTableManager != null)
            {
                return dataTableManager.Name == "Loads Data";
            }

            return false;
        }
    }
}