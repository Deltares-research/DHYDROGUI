using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    /// <summary>
    /// WaterFlowModelTimePropertiesSetter is responsible for setting the properties defined in the
    /// [Time] header of the md1d file on the 0WaterFlowModel1D.
    /// </summary>
    /// <seealso cref="WaterFlowModelCategoryPropertySetter" />
    public class WaterFlowModelTimePropertiesSetter : WaterFlowModelCategoryPropertySetter
    {
        private readonly string[] knownTimePropertyNames =
        {
            ModelDefinitionsRegion.StartTime.Key,
            ModelDefinitionsRegion.StopTime.Key,
            ModelDefinitionsRegion.TimeStep.Key,
            ModelDefinitionsRegion.MapOutputTimeStep.Key,
            ModelDefinitionsRegion.OutTimeStepGridPoints.Key,
            ModelDefinitionsRegion.HisOutputTimeStep.Key,
            ModelDefinitionsRegion.OutTimeStepStructures.Key
        };

        /// <inheritdoc />
        /// <summary>
        /// Set the model time properties as specified in the ModelDefinitionsRegion.TimeHeader
        /// of the <paramref name="timeCategory" />
        /// </summary>
        /// <param name="timeCategory"> A DelftIniCategory holding time settings data read from a md1d file. </param>
        /// <param name="model"> The model whose time variables should be changed. </param>
        /// <param name="errorMessages"> A collection of error messages that can be added to in case errors occur in this method. </param>
        /// <remarks>
        /// Pre-condition: model != null
        /// </remarks>
        public override void SetProperties(DelftIniCategory timeCategory, WaterFlowModel1D model, IList<string> errorMessages)
        {
            if (timeCategory == null) return;
            if (!string.Equals(timeCategory.Name, ModelDefinitionsRegion.TimeHeader, StringComparison.OrdinalIgnoreCase)) return;

            var startTime = timeCategory.ReadProperty<DateTime>(ModelDefinitionsRegion.StartTime.Key, true);
            var stopTime = timeCategory.ReadProperty<DateTime>(ModelDefinitionsRegion.StopTime.Key, true);
            var timeStep = timeCategory.ReadProperty<double>(ModelDefinitionsRegion.TimeStep.Key, true);
            var gridPointsOutputTimeStep = 
                GetCategoryWithDeprecation<double>(timeCategory,
                                                   ModelDefinitionsRegion.MapOutputTimeStep,
                                                   ModelDefinitionsRegion.OutTimeStepGridPoints,
                                                   errorMessages,
                                                   true);
            var structuresOutputTimeStep = 
                GetCategoryWithDeprecation<double>(timeCategory,
                                                   ModelDefinitionsRegion.HisOutputTimeStep,
                                                   ModelDefinitionsRegion.OutTimeStepStructures,
                                                   errorMessages,
                                                   true);

            model.StartTime = startTime;
            model.StopTime = stopTime;
            model.TimeStep = TimeSpan.FromSeconds(timeStep);

            var modelOutputSettings = model.OutputSettings;
            modelOutputSettings.GridOutputTimeStep = TimeSpan.FromSeconds(gridPointsOutputTimeStep);
            modelOutputSettings.StructureOutputTimeStep = TimeSpan.FromSeconds(structuresOutputTimeStep);

            var unsupportedProperties = timeCategory.Properties.Where(p => !knownTimePropertyNames.Contains(p.Name));
            unsupportedProperties.ForEach(c => errorMessages.Add(GetUnsupportedPropertyWarningMessage(c)));
        }

        /// <summary>
        /// Get the category specified with <paramref name="currentProperty"/> or
        /// with the deprecated <paramref name="previousProperty"/>.
        /// </summary>
        /// <typeparam name="T">The type of the specified property. </typeparam>
        /// <param name="category">The category.</param>
        /// <param name="currentProperty">The current property.</param>
        /// <param name="previousProperty">The previous property.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <param name="isOptional">if set to <c>true</c> [is optional].</param>
        /// <returns> The specified property read from the <paramref name="category"/> </returns>
        /// <remarks>
        /// if the <paramref name="previousProperty"/> is detected, the appropriate error message
        /// is added to <paramref name="errorMessages"/>.
        /// </remarks>
        private static T GetCategoryWithDeprecation<T>(IDelftIniCategory category,
                                                       ConfigurationSetting currentProperty,
                                                       ConfigurationSetting previousProperty,
                                                       IList<string> errorMessages,
                                                       bool isOptional)
        {
            var hasPrevious = category.Properties.Any(prop =>
                string.Equals(prop.Name, previousProperty.Key, StringComparison.OrdinalIgnoreCase));
            var hasCurrent  = category.Properties.Any(prop =>
                string.Equals(prop.Name, currentProperty.Key, StringComparison.OrdinalIgnoreCase));

            if (hasPrevious && hasCurrent)
            {
                errorMessages?.Add($"Detected both {currentProperty.Key} and deprecated {previousProperty.Key}, using {currentProperty.Key}, {previousProperty.Key} will be removed upon saving.");
            }
            else if (hasPrevious)
            {
                errorMessages?.Add($"{previousProperty.Key} has been deprecated and will be replaced with {currentProperty.Key} upon saving.");
                return category.ReadProperty<T>(previousProperty.Key, isOptional);
            }

            return category.ReadProperty<T>(currentProperty.Key, isOptional);
        }
    }
}
