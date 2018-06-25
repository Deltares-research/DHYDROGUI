using System;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using GeoAPI.Extensions.Feature;

namespace DelftTools.ModelExchange
{
    public static class ExchangeItemHelper
    {
        // Enum that supports bitwise comparison
        // (no Capitals, to support case insensitivy, and for unification with the Fortran layer
        // (small caps to support case insensitivity when parsing)
        // ReSharper disable InconsistentNaming
        [Flags]
        public enum ExchangeItemGroupType
        {
            nhi = 1,
            sobek = 2,
            composer = 4,
            groundwater = 8,
            // next groups: 8, 16, etc
            All = 15,
        } ;
        // ReSharper restore InconsistentNaming

        public static bool IncludeExchangeItemForSelectedEIGroup(ExchangeItemGroupType exchangeItemGroups, IFeature dataItemLocation, string quantityName, bool checkForInput = false)
        {
            bool checkForOutput = !checkForInput;

            if (exchangeItemGroups == ExchangeItemGroupType.All)
            {
                return true;
            }

            if (exchangeItemGroups.HasFlag(ExchangeItemGroupType.groundwater))
            {
                string elementSetType = DetermineElementSetType(dataItemLocation);

                if (checkForOutput && elementSetType.Equals(FunctionAttributes.StandardFeatureNames.GridPoint) &&
                    quantityName.Equals(FunctionAttributes.StandardNames.WaterLevel))
                {
                    // water level on grid points
                    return true;
                }
                if (checkForInput && elementSetType.Equals(FunctionAttributes.StandardFeatureNames.LateralSource) &&
                    quantityName.Equals(FunctionAttributes.StandardNames.WaterDischarge))
                {
                    // incoming lateral discharge
                    return true;
                }
                if (checkForOutput && elementSetType.Equals(FunctionAttributes.StandardFeatureNames.LateralSource) &&
                    (quantityName.Equals(FunctionAttributes.StandardNames.WaterLevel) || 
                     quantityName.Equals(FunctionAttributes.StandardNames.WaterDischarge)))
                {
                    // resulting lateral discharge, or water level on discharges
                    return true;
                }
                if (checkForInput && quantityName.Equals(FunctionAttributes.StandardNames.WaterDischarge))
                {
                    // Q boundary
                    return true;
                }
                if (checkForInput && quantityName.Equals(FunctionAttributes.StandardNames.WaterLevel))
                {
                    // H boundary
                    return true;
                }
            }

            else if (exchangeItemGroups.HasFlag(ExchangeItemHelper.ExchangeItemGroupType.nhi))
            {
                throw new NotImplementedException("ExchangeItem Group" + exchangeItemGroups + "not yet implemented");
            }
            else if (exchangeItemGroups.HasFlag(ExchangeItemHelper.ExchangeItemGroupType.sobek))
            {
                throw new NotImplementedException("ExchangeItem Group" + exchangeItemGroups + "not yet implemented");
            }
            else if (exchangeItemGroups.HasFlag(ExchangeItemHelper.ExchangeItemGroupType.composer))
            {
                throw new NotImplementedException("ExchangeItem Group" + exchangeItemGroups + "not yet implemented");
            }
            return false;
        }

        public static string DetermineQuantityName(IDataItem dataItem, string elementSetName)
        {
            string elementSetPrefix = elementSetName + " - ";
            string quantityName = dataItem.Name;
            if (dataItem.Name.StartsWith(elementSetPrefix))
            {
                // network coverage (waterlevel on grid points, etc).
                quantityName = dataItem.Name.Substring(elementSetPrefix.Length);
            }
            else
            {
                // individual item from feature coverage (lateral discharge, pump capacity, etc.)
                int dashIndex = dataItem.Name.LastIndexOf(" - ");
                if (dashIndex > 0)
                {
                    quantityName = dataItem.Name.Substring(0, dashIndex);
                }
            }
            if (quantityName.StartsWith("Discharge"))
            {
                quantityName = FunctionAttributes.StandardNames.WaterDischarge;
            }
            else if (quantityName.StartsWith("Water level") && !(quantityName.Contains(" up ") || quantityName.Contains(" down ")))
            {
                quantityName = FunctionAttributes.StandardNames.WaterLevel;
            }
            return quantityName.Replace(" (rt)", "").Replace(" (op)", "").Replace(" (s)", "");
        }


        private static string DetermineElementSetType(IFeature dataItemLocation)
        {
            if (dataItemLocation is ILateralSource)
            {
                return FunctionAttributes.StandardFeatureNames.LateralSource;
            }
            if (dataItemLocation is IObservationPoint)
            {
                return FunctionAttributes.StandardFeatureNames.ObservationPoint;
            }
            if (dataItemLocation is IStructure1D)
            {
                return FunctionAttributes.StandardFeatureNames.Structure;
            }
            return dataItemLocation is INameable ? ((INameable)dataItemLocation).Name : "";
        }

        public static string StandardNameToUserName(string quantityName)
        {
            if (quantityName.Equals(FunctionAttributes.StandardNames.WaterDischarge))
            {
                quantityName = "Discharge";
            }
            else if (quantityName.Equals(FunctionAttributes.StandardNames.WaterLevel))
            {
                quantityName = "Water level";
            }
            return quantityName;
        }
    }
}
