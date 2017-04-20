using System;
using System.Linq;
using DelftTools.Hydro;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    /// <summary>
    /// Changes the initial conditions of the model to the target type by :
    /// * adding or substracting the bed level to the initial conditions coverage.
    ///   Can only be done if for all locations in the coverage a bed level can be determined.
    /// * Renames the coverage.
    /// </summary>
    public static class InitialConditionsConverter
    {
        /// <summary>
        /// Changes the initial conditions type of the water flow model
        /// </summary>
        /// <param name="waterFlowModel1D">Model to change the type for</param>
        /// <param name="targetType">Type to change to</param>
        /// <exception cref="InvalidOperationException">Thrown when change can not be made</exception>
        public static void ChangeInitialConditionsType(WaterFlowModel1D waterFlowModel1D, InitialConditionsType targetType)
        {
            //check if the switch can be made..
            ThrowIfUnableToChangeType(waterFlowModel1D,targetType);

            //switch coverage name
            waterFlowModel1D.InitialConditions.Name =  GetCoverageName(targetType);
            waterFlowModel1D.InitialConditions.Components[0].Name = GetCoverageComponentName(targetType);

            //change the values of the coverage
            UpdateCoverageValues(waterFlowModel1D.InitialConditions,waterFlowModel1D.Network,targetType);

            //change the default values of the coverage
            if (targetType == InitialConditionsType.WaterLevel)
            {
                waterFlowModel1D.InitialConditions.DefaultValue = waterFlowModel1D.DefaultInitialWaterLevel;
            }
            else
            {
                waterFlowModel1D.InitialConditions.DefaultValue = waterFlowModel1D.DefaultInitialDepth;
            }
            
        }

        private static string GetCoverageComponentName(InitialConditionsType targetType)
        {
            return (targetType == InitialConditionsType.WaterLevel)
                       ? "Water level" : "Water depth";
        }


        private static void UpdateCoverageValues(INetworkCoverage initialConditions, IHydroNetwork network, InitialConditionsType targetType)
        {
            //get the bed level coverage..
            var bedLevelCoverage = BedLevelNetworkCoverageBuilder.BuildBedLevelCoverage(network);

            //if target is level add it to the initial Conditions
            if (targetType == InitialConditionsType.WaterLevel)
            {
                initialConditions.Add(bedLevelCoverage);
            }
            else
            {
                initialConditions.Substract(bedLevelCoverage);
            }
        }

        private static string GetCoverageName(InitialConditionsType targetType)
        {
            return (targetType == InitialConditionsType.WaterLevel)
                       ? "Initial Water Level"
                       : "Initial Water Depth";
        }

        private static void ThrowIfUnableToChangeType(WaterFlowModel1D waterFlowModel1D, InitialConditionsType targetType)
        {
            if (targetType == waterFlowModel1D.InitialConditionsType)
            {
                throw new InvalidOperationException(string.Format("Can not switch to target type. Model is already of initial conditions type {0}.",targetType));
            }
            //check the branches on which data is defined
            var branches = waterFlowModel1D.InitialConditions.Locations.Values.Select(loc => loc.Branch).Distinct();

            //check if all branches can find a crossSection
            if (branches.Count()!= 0 && !branches.All(b => waterFlowModel1D.Network.CrossSections.Any(c => c.Branch == b)))
            {
                throw new InvalidOperationException("Unable to change initial condition type. Insufficient crossections.");
            }
        }
    }
}