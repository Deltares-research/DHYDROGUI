using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.DataObjects
{
    public enum BoundaryType
    {
        None = 0,
        Level = 1,
        Discharge,
        Salinity,
        WindVelocity,
        WindDirection,
        AirTemperature,
        RelativeHumidity,
        Cloudiness
    }
    /// <summary>
    /// Provides xml serialization of WaterFlowModel1d
    /// </summary>
    public static class Helper1D
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
                    throw new ArgumentOutOfRangeException(String.Format("BoundaryNodeDataType {0} is not supported by the ModelApi", boundaryNodeData.DataType));
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

        public static string GetFeatureCategory(this IFeature feature)
        {
            if (feature is IPump)
            {
                return Model1DParametersCategories.Pumps;
            }
            if (feature is IWeir)
            {
                return Model1DParametersCategories.Weirs;
            }
            if (feature is ICulvert)
            {
                return Model1DParametersCategories.Culverts;
            }
            if (feature is IObservationPoint)
            {
                return Model1DParametersCategories.ObservationPoints;
            }
            if (feature is IRetention)
            {
                return Model1DParametersCategories.Retentions;
            }
            if (feature is ILateralSource)
            {
                return Model1DParametersCategories.Laterals;
            }
            if (feature is IHydroNode)
            {
                return Model1DParametersCategories.BoundaryConditions;
            }
            return null;
        }
    }
}