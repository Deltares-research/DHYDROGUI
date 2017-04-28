using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    /// <summary>
    /// Provides xml serialization of WaterFlowModel1d
    /// </summary>
    public static class WaterFlowModel1DHelper
    {
        public static WaterFlowModel1DBoundaryNodeData CreateDefaultBoundaryCondition(INode node, bool useSalt, bool useTemperature)
        {
            var bc = new WaterFlowModel1DBoundaryNodeData
                         {
                             Feature = node,
                             DataType = WaterFlowModel1DBoundaryNodeDataType.None,
                             UseSalt = useSalt,
                             UseTemperature = useTemperature
                         };

            return bc;
        }

        public static BoundaryType GetBoundaryType(WaterFlowModel1DBoundaryNodeDataType boundaryNodeDataType)
        {
            switch (boundaryNodeDataType)
            {
                case WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries:
                    return BoundaryType.Level;
                case WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries:
                    return BoundaryType.Discharge;
                case WaterFlowModel1DBoundaryNodeDataType.FlowConstant:
                    return BoundaryType.Discharge;
                case WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant:
                    return BoundaryType.Level;
                case WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable:
                    return BoundaryType.Discharge;
                default:
                    throw new ArgumentOutOfRangeException(string.Format("BoundaryNodeDataType {0} is not supported by the ModelApi",boundaryNodeDataType));
            }
        }

        public static IEnumerable<WaterFlowModel1DBoundaryNodeDataType> GetTimeSeriesDataTypes(IFunction series)
        {
            var isDischargeSeries = false;

            foreach (var component in series.Components)
            {
                if (component.Attributes != null)
                {
                    string dischargeName;
                    if (component.Attributes.TryGetValue(FunctionAttributes.StandardName,
                                                                    out dischargeName))
                    {
                        isDischargeSeries = (dischargeName == FunctionAttributes.StandardNames.WaterDischarge);
                    }
                }
                yield return isDischargeSeries
                           ? WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries
                           : WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries;
            }
        }

        /// <summary>
        /// Determines whether a model can change the initial condition (are there enough cross-sections?)
        /// </summary>
        /// <param name="waterFlowModel">Water flow model to check</param>
        public static bool CanChangeInitialConditionsType(WaterFlowModel1D waterFlowModel,out string message)
        {
            message = "";
            
            //can change if no location can be found for which no crossection can be found (on the branch)
            var crossSections = waterFlowModel.Network.CrossSections;
            var locationsWithoutCrossSection = waterFlowModel.InitialConditions.Locations.Values
                .Where(l => crossSections.All(c => c.Branch != l.Branch)).ToList();
            
            //no locations without crossections...can change
            if (!locationsWithoutCrossSection.Any())
            {
                return true;
            }

            var location = locationsWithoutCrossSection.First();
            message = string.Format("Cannot change the type of the initial conditions. " +
                                    "No cross-sections found on channel '{0}' for location '{1}'.",
                                    location.Branch.Name, location.Name);

            return false;
        }
        public static string GetFeatureCategory(this IFeature feature)
        {
            if (feature is IPump)
            {
                return WaterFlowParametersCategories.Pumps;;
            }
            if (feature is IWeir)
            {
                return WaterFlowParametersCategories.Weirs;
            }
            if (feature is ICulvert)
            {
                return WaterFlowParametersCategories.Culverts;
            }
            if (feature is IObservationPoint)
            {
                return WaterFlowParametersCategories.ObservationPoints;
            }
            if (feature is IRetention)
            {
                return WaterFlowParametersCategories.Retentions;
            }
            if (feature is ILateralSource)
            {
                return WaterFlowParametersCategories.Laterals;
            }
            if (feature is IHydroNode)
            {
                return WaterFlowParametersCategories.BoundaryConditions;
            }
            return null;
        }

        public static string BMIPropertyString(this IFeature feature, string property)
        {
            const string seperator = "/"; 
            var category = feature.GetFeatureCategory();
            var featureName = feature.ToString();
            return string.Format("{1}{0}{2}{0}{3}", seperator, category, featureName, property);
        }
    }
}