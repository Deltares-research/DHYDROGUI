using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    /// <summary>
    /// Flow1DParameterGenerator is responsible for generating a set of Parameter categories from a
    /// WaterFlowModel1D. 
    /// </summary>
    public static class Flow1DParameterGenerator
    {
        /// <summary>
        /// Generates the set of parameter categories from the specified <paramref name="waterFlowModel1D"/>.
        /// </summary>
        /// <param name="waterFlowModel1D">The WaterFlowModel1D of which the parameters should be retrieved.</param>
        /// <returns>
        /// An enumeration of DelftIniCategories describing the Parameters of the specified
        /// <paramref name="waterFlowModel1D"/>.
        /// </returns>
        /// <remarks>  <paramref name="waterFlowModel1D"/> == null -> not return.any() </remarks>
        public static IEnumerable<DelftIniCategory> GenerateParameterCategories(WaterFlowModel1D waterFlowModel1D)
        {
            if (waterFlowModel1D == null) yield break;

            // Global values
            yield return Flow1DParameterCategoryGenerator.GenerateGlobalValues(waterFlowModel1D);

            // Initial Conditions
            yield return Flow1DParameterCategoryGenerator.GenerateInitialConditionsValues(waterFlowModel1D);

            // Time values
            yield return Flow1DParameterCategoryGenerator.GenerateTimeValues(waterFlowModel1D);

            //Sediment
            yield return Flow1DParameterCategoryGenerator.GenerateSedimentValues(waterFlowModel1D);

            //Specials
            yield return Flow1DParameterCategoryGenerator.GenerateSpecialsValues(waterFlowModel1D);

            //Numerical Parameters
            yield return Flow1DParameterCategoryGenerator.GenerateNumericalParametersValues(waterFlowModel1D);

            //Simulation Options
            yield return Flow1DParameterCategoryGenerator.GenerateSimulationOptionsValues(waterFlowModel1D);

            // TransportComputation Options
            yield return Flow1DParameterCategoryGenerator.GenerateTransportComputationOptionsValues(waterFlowModel1D);

            // Advanced Options
            yield return Flow1DParameterCategoryGenerator.GenerateAdvancedOptionsValues(waterFlowModel1D);

            //Salinity
            yield return Flow1DParameterCategoryGenerator.GenerateSalinityValues(waterFlowModel1D);

            // Temperature
            yield return Flow1DParameterCategoryGenerator.GenerateTemperatureValues(waterFlowModel1D);

            // Morphology
            yield return Flow1DParameterCategoryGenerator.GenerateMorphologyValues(waterFlowModel1D);

            //Observation points
            yield return Flow1DParameterCategoryGenerator.GenerateObservationValues(waterFlowModel1D);

            //Restart Options
            yield return Flow1DParameterCategoryGenerator.GenerateRestartOptionsValues(waterFlowModel1D);
        }
    }
}
