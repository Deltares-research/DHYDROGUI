using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;

namespace DeltaShell.NGHS.IO.Helpers
{
    /// <summary>
    /// Helper for changes related for the quantity in .bc files.
    /// </summary>
    public static class QuantityHelper
    {
        private const string orificeGateLowerEdgeLevelQuantity = "orifice_gateLowerEdgeLevel";
        private const string orificeCrestLevelQuantity = "orifice_crestLevel";
        private const string pumpQuantity = "pump_capacity";
        private const string weirQuantity = "weir_crestLevel";
        private const string culvertQuantity = "culvert_valveOpeningHeight";

        private const string timeSeriesNameCrestLevel = "Crest level";
        private const string timeSeriesNameSeriesName = "GateLowerEdgeLevel";
        private const string timeSeriesNameGateOpening = "Gate opening";

        /// <summary>
        /// Gets an expected string for the quantity based on the structure type.
        /// </summary>
        /// <param name="structureType">Type of the structure.</param>
        /// <param name="timeSeriesName">Name of the time series, used when multiple time series can be set for one structure.</param>
        /// <exception cref="NotSupportedException">Thrown when an invalid <paramref name="structureType"/> is provided.</exception>
        /// <exception cref="NotSupportedException">Thrown when <paramref name="structureType"/> is <see cref="Orifice"/> and an invalid <paramref name="timeSeriesName"/> is provided.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="structureType"/> is <see cref="Orifice"/> and <paramref name="timeSeriesName"/> is null.</exception>
        /// <returns>The expected Quantity string.</returns>
        public static string GetQuantity(IStructure1D structureType, string timeSeriesName)
        {
            Ensure.NotNull(structureType, nameof(structureType));
            Ensure.NotNull(timeSeriesName, nameof(timeSeriesName));
            
            switch (structureType)
            {
                case Orifice _:
                    return GetOrificeQuantity(timeSeriesName);
                case Pump _:
                    return pumpQuantity;
                case Weir _:
                    return weirQuantity;
                case Culvert _:
                    return culvertQuantity;
                default:
                    throw new NotSupportedException(nameof(structureType));
            }
        }

        private static string GetOrificeQuantity(string timeSeriesName)
        {
            switch (timeSeriesName)
            {
                case timeSeriesNameCrestLevel :
                    return orificeCrestLevelQuantity;
                case timeSeriesNameSeriesName :
                case timeSeriesNameGateOpening :
                    return orificeGateLowerEdgeLevelQuantity;
                default:
                    throw new NotSupportedException(nameof(timeSeriesName));
            }
        }

        /// <summary>
        /// Gets the quantity and unit in an expected form.
        /// </summary>
        /// <param name="boundaryNodeData">BoundaryNodeData</param>
        /// <param name="givenQuantity">Quantity used when it should be overwritten, if empty use data from <paramref name="boundaryNodeData"/></param>
        /// <returns>quantity and unit in an expected form</returns>
        public static IDictionary<string, string> GetQuantityAndUnit(IFunction boundaryNodeData, string givenQuantity)
        {
            string quantity = string.IsNullOrEmpty(givenQuantity) ? boundaryNodeData.Name : givenQuantity;
            string unit = boundaryNodeData.Components.First().Unit.Name;
            
            var data = new Dictionary<string, string> {{quantity, unit}};
            return data;
        }
    }
}