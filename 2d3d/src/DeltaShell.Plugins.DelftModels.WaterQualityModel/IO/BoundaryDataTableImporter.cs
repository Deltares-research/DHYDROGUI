using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public class BoundaryDataTableImporter : DataTableImporter
    {
        public override string Name => "Data table boundary importer";

        public override bool CanImportOn(object targetObject)
        {
            var dataTableManager = targetObject as DataTableManager;

            if (dataTableManager != null)
            {
                return dataTableManager.Name == "Boundary Data";
            }

            return false;
        }
    }
}