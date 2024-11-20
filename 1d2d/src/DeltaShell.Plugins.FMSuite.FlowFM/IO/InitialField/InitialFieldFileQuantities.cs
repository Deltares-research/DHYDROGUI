using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DHYDRO.Common.IO.InitialField;
using SharpMap.Api.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.InitialField
{
    /// <summary>
    /// Defines the supported initial field file quantities.
    /// </summary>
    public static class InitialFieldFileQuantities
    {
        private static readonly IDictionary<InitialFieldQuantity, string> supportedQuantities = new Dictionary<InitialFieldQuantity, string>
        {
            { InitialFieldQuantity.BedLevel, WaterFlowFMModelDefinition.BathymetryDataItemName },
            { InitialFieldQuantity.WaterLevel, WaterFlowFMModelDefinition.InitialWaterLevelDataItemName },
            { InitialFieldQuantity.WaterDepth, WaterFlowFMModelDefinition.InitialWaterDepthDataItemName },
            { InitialFieldQuantity.FrictionCoefficient, WaterFlowFMModelDefinition.RoughnessDataItemName },
            { InitialFieldQuantity.InfiltrationCapacity, WaterFlowFMModelDefinition.InfiltrationDataItemName }
        };

        /// <summary>
        /// Dictionary containing the quantities written to the initial field file,
        /// with their corresponding name in our domain model.
        /// </summary>
        public static IReadOnlyDictionary<InitialFieldQuantity, string> SupportedQuantities { get; }
            = new ReadOnlyDictionary<InitialFieldQuantity, string>(supportedQuantities);

        /// <summary>
        /// Returns whether the model definition contains spatial operations for the supported quantities.
        /// </summary>
        /// <param name="modelDefinition">The model definition containing spatial operations.</param>
        /// <returns>Whether the model definition contains spatial operations for the supported quantities.</returns>
        public static bool ContainsSupportedSpatialOperations(WaterFlowFMModelDefinition modelDefinition)
            => SupportedQuantities.Values.Any(quantityName => ContainsSpatialOperation(modelDefinition, quantityName));

        private static bool ContainsSpatialOperation(WaterFlowFMModelDefinition modelDefinition, string quantityName)
        {
            IList<ISpatialOperation> spatialOperations = modelDefinition.GetSpatialOperations(quantityName);
            return spatialOperations != null && spatialOperations.Any();
        }
    }
}