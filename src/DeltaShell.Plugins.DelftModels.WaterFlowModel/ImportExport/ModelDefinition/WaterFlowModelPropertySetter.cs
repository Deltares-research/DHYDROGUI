using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public static class WaterFlowModelPropertySetter
    {
        public static void SetProperties(IEnumerable<DelftIniCategory> modelSettingsCategories, WaterFlowModel1D model)
        {
            var timeCategory = modelSettingsCategories.FirstOrDefault(c => c.Name == ModelDefinitionsRegion.TimeHeader);
            var startTime = timeCategory.ReadProperty<DateTime>(ModelDefinitionsRegion.StartTime.Key);
            var stopTime = timeCategory.ReadProperty<DateTime>(ModelDefinitionsRegion.StopTime.Key);
            var timeStep = timeCategory.ReadProperty<double>(ModelDefinitionsRegion.TimeStep.Key);
            var gridPointsOutputTimeStep = timeCategory.ReadProperty<double>(ModelDefinitionsRegion.OutTimeStepGridPoints.Key);
            var structuresOutputTimeStep = timeCategory.ReadProperty<double>(ModelDefinitionsRegion.OutTimeStepStructures.Key);

            model.StartTime = startTime;
            model.StopTime = stopTime;
            model.TimeStep = TimeSpan.FromSeconds(timeStep);

            var modelOutputSettings = model.OutputSettings;
            modelOutputSettings.GridOutputTimeStep = TimeSpan.FromSeconds(gridPointsOutputTimeStep);
            modelOutputSettings.StructureOutputTimeStep = TimeSpan.FromSeconds(structuresOutputTimeStep);
        }
    }
}
