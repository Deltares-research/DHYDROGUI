using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Extensions;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    public class WaterFlowModel1D350LegacyLoader : LegacyLoader
    {
        public override void OnAfterProjectMigrated(Project project)
        {
            var flow1DModels = project.RootFolder.GetAllModelsRecursive().OfType<WaterFlowModel1D>();
            foreach (var waterFlowModel1D in flow1DModels)
            {
                WaterFlowModel1DImporterHelper.AdaptExistingUseThatcherHarlemanPropertyToNewDispersionFormulationTypeProperty(waterFlowModel1D);
                WaterFlowModel1DImporterHelper.AdaptExistingDispersionCoverageToNewDispersionCoverages(waterFlowModel1D);
            }
        }
    }
}