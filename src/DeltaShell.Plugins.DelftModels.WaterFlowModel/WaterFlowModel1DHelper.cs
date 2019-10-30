using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.DataObjects;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    /// <summary>
    /// Provides xml serialization of WaterFlowModel1d
    /// </summary>
    public static class WaterFlowModel1DHelper
    {
        public static Model1DBoundaryNodeData CreateDefaultBoundaryCondition(INode node, bool useSalt, bool useTemperature)
        {
            var bc = new Model1DBoundaryNodeData
                         {
                             Feature = node,
                             DataType = Model1DBoundaryNodeDataType.None,
                             UseSalt = useSalt,
                             UseTemperature = useTemperature
                         };

            return bc;
        }

        public static BoundaryType GetBoundaryType(Model1DBoundaryNodeData boundaryNodeData)
        {
            switch (boundaryNodeData.DataType)
            {
                case Model1DBoundaryNodeDataType.WaterLevelTimeSeries:
                    return BoundaryType.Level;
                case Model1DBoundaryNodeDataType.FlowTimeSeries:
                    return BoundaryType.Discharge;
                case Model1DBoundaryNodeDataType.FlowConstant:
                    return BoundaryType.Discharge;
                case Model1DBoundaryNodeDataType.WaterLevelConstant:
                    return BoundaryType.Level;
                case Model1DBoundaryNodeDataType.FlowWaterLevelTable:
                    // SOBEK3-1035
                    if (boundaryNodeData.Data != null && 
                        boundaryNodeData.Data.Components.Any() && 
                        boundaryNodeData.Data.Components[0].ValueType == typeof(double) &&
                        boundaryNodeData.Data.Components[0].Values.Count > 0 &&
                        (double)boundaryNodeData.Data.Components[0].MinValue <= 0 &&
                        (double)boundaryNodeData.Data.Components[0].MaxValue <= 0)
                    {
                        // if Q <= 0 return WaterLevel type
                        return BoundaryType.Level;
                    }
                    // if Q >= 0 return Discharge type (returned by default)
                    return BoundaryType.Discharge;         
                default:
                    throw new ArgumentOutOfRangeException(string.Format("BoundaryNodeDataType {0} is not supported by the ModelApi", boundaryNodeData.DataType));
            }
        }

        public static IEnumerable<Model1DBoundaryNodeDataType> GetTimeSeriesDataTypes(IFunction series)
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
                           ? Model1DBoundaryNodeDataType.FlowTimeSeries
                           : Model1DBoundaryNodeDataType.WaterLevelTimeSeries;
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