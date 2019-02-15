using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.DelftIniCategoryGenerators
{
    public static class GlobalValuesCategoryGenerator
    {
        /// <summary>
        /// Generate the Global category of the md1d file describing the specified <paramref name="waterFlowModel1D"/>.
        /// Adds the properties to the global values category.
        /// </summary>
        /// <param name="waterFlowModel1D">The WaterFlowModel1D.</param>
        /// <returns>A DelftIniCategory describing the <c>[Global]</c> header of the specified <paramref name="waterFlowModel1D"/></returns>
        public static DelftIniCategory GenerateGlobalValuesCategory(this WaterFlowModel1D waterFlowModel1D)
        {
            var globalValuesGroup = new DelftIniCategory(ModelDefinitionsRegion.GlobalValuesHeader);
            var useDepth = waterFlowModel1D.InitialConditionsType == InitialConditionsType.Depth ? 1 : 0;

            AddPropertiesToCategory(waterFlowModel1D, globalValuesGroup, useDepth);

            return globalValuesGroup;
        }

        private static void AddPropertiesToCategory(WaterFlowModel1D waterFlowModel1D, DelftIniCategory globalValuesGroup, int useDepth)
        {
            globalValuesGroup.AddProperty(ModelDefinitionsRegion.UseInitialWaterDepth.Key, useDepth,
                ModelDefinitionsRegion.UseInitialWaterDepth.Description);
            globalValuesGroup.AddProperty(ModelDefinitionsRegion.InitialWaterLevel.Key,
                waterFlowModel1D.DefaultInitialWaterLevel,
                ModelDefinitionsRegion.InitialWaterLevel.Description, ModelDefinitionsRegion.InitialWaterLevel.Format);
            globalValuesGroup.AddProperty(ModelDefinitionsRegion.InitialWaterDepth.Key, waterFlowModel1D.DefaultInitialDepth,
                ModelDefinitionsRegion.InitialWaterDepth.Description, ModelDefinitionsRegion.InitialWaterDepth.Format);
            globalValuesGroup.AddProperty(ModelDefinitionsRegion.InitialDischarge.Key,
                waterFlowModel1D.InitialFlow.DefaultValue,
                ModelDefinitionsRegion.InitialDischarge.Description, ModelDefinitionsRegion.InitialDischarge.Format);

            if (waterFlowModel1D.InitialSaltConcentration != null)
            {
                globalValuesGroup.AddProperty(ModelDefinitionsRegion.InitialSalinity.Key,
                    waterFlowModel1D.InitialSaltConcentration.DefaultValue,
                    ModelDefinitionsRegion.InitialSalinity.Description, ModelDefinitionsRegion.InitialSalinity.Format);
            }

            if (waterFlowModel1D.InitialTemperature != null)
            {
                globalValuesGroup.AddProperty(ModelDefinitionsRegion.InitialTemperature.Key,
                    waterFlowModel1D.InitialTemperature.DefaultValue,
                    ModelDefinitionsRegion.InitialTemperature.Description, ModelDefinitionsRegion.InitialTemperature.Format);
            }

            if (waterFlowModel1D.DispersionCoverage != null)
            {
                globalValuesGroup.AddProperty(ModelDefinitionsRegion.Dispersion.Key,
                    waterFlowModel1D.DispersionCoverage.DefaultValue,
                    ModelDefinitionsRegion.Dispersion.Description, ModelDefinitionsRegion.Dispersion.Format);
            }

            if (waterFlowModel1D.DispersionF3Coverage != null)
            {
                globalValuesGroup.AddProperty(ModelDefinitionsRegion.DispersionF3.Key,
                    waterFlowModel1D.DispersionF3Coverage.DefaultValue,
                    ModelDefinitionsRegion.DispersionF3.Description, ModelDefinitionsRegion.DispersionF3.Format);
            }

            if (waterFlowModel1D.DispersionF4Coverage != null)
            {
                globalValuesGroup.AddProperty(ModelDefinitionsRegion.DispersionF4.Key,
                    waterFlowModel1D.DispersionF4Coverage.DefaultValue,
                    ModelDefinitionsRegion.DispersionF4.Description, ModelDefinitionsRegion.DispersionF4.Format);
            }
        }
    }
}