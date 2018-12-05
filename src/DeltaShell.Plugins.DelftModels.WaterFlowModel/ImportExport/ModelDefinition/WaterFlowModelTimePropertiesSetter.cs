using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelTimePropertiesSetter : IWaterFlowModelCategoryPropertySetter
    {
        /// <summary>
        /// Set the model time properties as specified in the ModelDefinitionsRegion.TimeHeader
        /// of the <paramref name="timeCategory"/>
        /// </summary>
        /// <param name="timeCategory"> A DelftIniCategory holding time settings data read from a md1d file. </param>
        /// <param name="model"> The model whose time variables should be changed. </param>
        /// <remarks>
        /// Pre-condition: model != null
        /// </remarks>
        public void SetProperties(DelftIniCategory timeCategory, WaterFlowModel1D model, Action<string, IList<string>> createAndAddErrorReport)
        {
            if (timeCategory?.Name != ModelDefinitionsRegion.TimeHeader) return;

            var startTime = timeCategory.ReadProperty<DateTime>(ModelDefinitionsRegion.StartTime.Key, true);
            var stopTime = timeCategory.ReadProperty<DateTime>(ModelDefinitionsRegion.StopTime.Key, true);
            var timeStep = timeCategory.ReadProperty<double>(ModelDefinitionsRegion.TimeStep.Key, true);
            var gridPointsOutputTimeStep = timeCategory.ReadProperty<double>(ModelDefinitionsRegion.OutTimeStepGridPoints.Key, true);
            var structuresOutputTimeStep = timeCategory.ReadProperty<double>(ModelDefinitionsRegion.OutTimeStepStructures.Key, true);

            model.StartTime = startTime;
            model.StopTime = stopTime;
            model.TimeStep = TimeSpan.FromSeconds(timeStep);

            var modelOutputSettings = model.OutputSettings;
            modelOutputSettings.GridOutputTimeStep = TimeSpan.FromSeconds(gridPointsOutputTimeStep);
            modelOutputSettings.StructureOutputTimeStep = TimeSpan.FromSeconds(structuresOutputTimeStep);
        }
    }
}
