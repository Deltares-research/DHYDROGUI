using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;

namespace DeltaShell.Plugins.FMSuite.Wave.ModelDefinition
{
    /// <summary>
    /// Helper class which contains methods to support the loading of a <see cref="WaveModelDefinition"/>.
    /// </summary>
    public static class WaveModelDefinitionLoadHelper
    {
        /// <summary>
        /// Transfers the definitions from the <paramref name="loadedDefinition"/> to the <paramref name="targetDefinition"/>.
        /// </summary>
        /// <param name="targetDefinition">The <see cref="WaveModelDefinition"/> to transfer the properties to.</param>
        /// <param name="loadedDefinition">The <see cref="WaveModelDefinition"/> that contains the properties to transfer.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is <c>null</c>.</exception>
        public static void TransferLoadedProperties(WaveModelDefinition targetDefinition, WaveModelDefinition loadedDefinition)
        {
            if (targetDefinition == null)
            {
                throw new ArgumentNullException(nameof(targetDefinition));
            }

            if (loadedDefinition == null)
            {
                throw new ArgumentNullException(nameof(loadedDefinition));
            }

            targetDefinition.OuterDomain = loadedDefinition.OuterDomain;

            TransferFeatures(targetDefinition.FeatureContainer, loadedDefinition.FeatureContainer);
            TransferProperties(targetDefinition, loadedDefinition.Properties);
            TransferBoundaryContainer(targetDefinition.BoundaryContainer, loadedDefinition.BoundaryContainer);
        }

        private static void TransferFeatures(IWaveFeatureContainer targetFeatureContainer, IWaveFeatureContainer loadedFeatureContainer)
        {
            targetFeatureContainer.ObservationPoints.AddRange(loadedFeatureContainer.ObservationPoints);
            targetFeatureContainer.ObservationCrossSections.AddRange(loadedFeatureContainer.ObservationCrossSections);
            targetFeatureContainer.Obstacles.AddRange(loadedFeatureContainer.Obstacles);
        }

        private static void TransferBoundaryContainer(IBoundaryContainer targetBoundaryContainer, IBoundaryContainer loadedBoundaryContainer)
        {
            targetBoundaryContainer.UpdateGridBoundary(loadedBoundaryContainer.GetGridBoundary());
            if (loadedBoundaryContainer.DefinitionPerFileUsed)
            {
                targetBoundaryContainer.DefinitionPerFileUsed = true;
                targetBoundaryContainer.FilePathForBoundariesPerFile = loadedBoundaryContainer.FilePathForBoundariesPerFile;
            }
            else
            {
                targetBoundaryContainer.Boundaries.AddRange(loadedBoundaryContainer.Boundaries);
            }
        }

        private static void TransferProperties(WaveModelDefinition targetDefinition,
                                               IEnumerable<WaveModelProperty> loadedProperties)
        {
            foreach (WaveModelProperty loadedProperty in loadedProperties)
            {
                targetDefinition.SetModelProperty(loadedProperty.PropertyDefinition.FileCategoryName,
                                                  loadedProperty.PropertyDefinition.FilePropertyName,
                                                  loadedProperty);
            }
        }
    }
}