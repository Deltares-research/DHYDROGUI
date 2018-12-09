using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    /// <summary>
    /// WaterFlowModelGlobalValueSetter is responsible for setting the global values of
    /// the WaterFlowModel1D as specified in a .md1d data access model.
    /// </summary>
    /// <seealso cref="DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition.IWaterFlowModelCategoryPropertySetter" />
    public class WaterFlowModelGlobalValuesSetter : IWaterFlowModelCategoryPropertySetter
    {
        ///  <summary>
        ///  Set the GlobalValues properties of the <paramref name="model"/> as
        ///  described in the GlobalValues <paramref name="category"/>.
        ///  </summary>
        ///  <param name="category">The category.</param>
        ///  <param name="model">The model.</param>
        ///  <param name="errorMessages"> A collection of error messages that can be added to in case errors occur in this method. </param>
        /// <remarks>
        ///  In order to this correctly, the UseSalt, UseTemperature and
        ///  DispersionCoefficient need to be correctly set.
        /// 
        ///  pre-condition: category != null && model != null
        ///  This method does not return any errors.
        ///  </remarks>
        public void SetProperties(DelftIniCategory category, 
                                  WaterFlowModel1D model,
                                  IList<string> errorMessages)
        {
            if (!category.Name.Equals(ModelDefinitionsRegion.GlobalValuesHeader))
                return;

            model.InitialConditionsType =
                GetWithDefault(category, ModelDefinitionsRegion.UseInitialWaterDepth.Key, 0) == 0
                    ? InitialConditionsType.WaterLevel
                    : InitialConditionsType.Depth;

            model.DefaultInitialWaterLevel =  
                GetWithDefault(category, ModelDefinitionsRegion.InitialWaterLevel.Key, 0.0);

            model.DefaultInitialDepth =
                GetWithDefault(category, ModelDefinitionsRegion.InitialWaterDepth.Key, 0.0);

            model.InitialFlow.DefaultValue =
                GetWithDefault(category, ModelDefinitionsRegion.InitialDischarge.Key, 0.0);

            if (model.InitialSaltConcentration != null)
                model.InitialSaltConcentration.DefaultValue =
                    GetWithDefault(category, ModelDefinitionsRegion.InitialSalinity.Key, 0.0);

            if (model.InitialTemperature != null)
                model.InitialTemperature.DefaultValue =
                    GetWithDefault(category, ModelDefinitionsRegion.InitialTemperature.Key, 15.0);

            if (model.DispersionCoverage != null)
                model.DispersionCoverage.DefaultValue =
                    GetWithDefault(category, ModelDefinitionsRegion.Dispersion.Key, 0.0);

            if (model.DispersionF3Coverage != null)
                model.DispersionF3Coverage.DefaultValue =
                    GetWithDefault(category, ModelDefinitionsRegion.DispersionF3.Key, 0.0);

            if (model.DispersionF4Coverage != null)
                model.DispersionF4Coverage.DefaultValue =
                    GetWithDefault(category, ModelDefinitionsRegion.DispersionF4.Key, 0.0);
        }

        private static T GetWithDefault<T>(DelftIniCategory category,
                                           string propertyName,
                                           T defaultValue)
        {
            try
            {
                return category.ReadProperty<T>(propertyName);
            }
            catch (PropertyNotFoundInFileException)
            {
                return defaultValue;
            }
        }
    }
}
