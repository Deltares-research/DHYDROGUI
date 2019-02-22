using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    /// <summary>
    /// WaterFlowModelGlobalValueSetter is responsible for setting the global values of
    /// the WaterFlowModel1D as specified in a .md1d data access model.
    /// </summary>
    /// <seealso cref="WaterFlowModelCategoryPropertySetter" />
    public class WaterFlowModelGlobalValuesSetter : WaterFlowModelCategoryPropertySetter
    {
        private readonly string[] knownTimePropertyNames =
        {
            ModelDefinitionsRegion.UseInitialWaterDepth.Key,
            ModelDefinitionsRegion.InitialWaterLevel.Key,
            ModelDefinitionsRegion.InitialWaterDepth.Key,
            ModelDefinitionsRegion.InitialDischarge.Key,
            ModelDefinitionsRegion.InitialSalinity.Key,
            ModelDefinitionsRegion.InitialTemperature.Key,
            ModelDefinitionsRegion.Dispersion.Key,
            ModelDefinitionsRegion.DispersionF3.Key,
            ModelDefinitionsRegion.DispersionF4.Key
        };

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
        public override void SetProperties(DelftIniCategory category, 
                                  WaterFlowModel1D model,
                                  IList<string> errorMessages)
        {
            if (!category.Name.Equals(ModelDefinitionsRegion.GlobalValuesHeader, StringComparison.OrdinalIgnoreCase))
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

            // TODO: the DispersionFormulaType is not explicitly set in the md1d file, thus we need to deduce it.
            // TODO: This can be changed once we explicitly set the DispersionFormulaType (see issue SOBEK3-1622).
            if (model.UseSalt &&
                category.Properties.Any(e =>
                    string.Equals(e.Name, ModelDefinitionsRegion.DispersionF3.Key, StringComparison.OrdinalIgnoreCase)) && 
                category.Properties.Any(e =>
                    string.Equals(e.Name, ModelDefinitionsRegion.DispersionF4.Key, StringComparison.OrdinalIgnoreCase)))
            {
                model.DispersionFormulationType = DispersionFormulationType.KuijperVanRijnPrismatic;

                model.DispersionF3Coverage.DefaultValue =
                    category.ReadProperty<double>(ModelDefinitionsRegion.DispersionF3.Key);
                model.DispersionF4Coverage.DefaultValue =
                    category.ReadProperty<double>(ModelDefinitionsRegion.DispersionF4.Key);
            }

            var unsupportedProperties = category.Properties.Where(p => !knownTimePropertyNames.Contains(p.Name));
            unsupportedProperties.ForEach(property => errorMessages.Add(GetUnsupportedPropertyWarningMessage(property)));
        }

        private static T GetWithDefault<T>(IDelftIniCategory category,
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
