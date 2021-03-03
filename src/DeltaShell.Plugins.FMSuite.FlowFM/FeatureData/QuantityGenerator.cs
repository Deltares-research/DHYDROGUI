using System.Collections.Generic;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.KnownProperties;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Utils.Reflection;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    /// <summary>
    /// This class generates the possible quantities for every <see cref="IFeature"/> object, depending on
    /// the concrete type of <see cref="IFeature"/>.
    /// </summary>
    public static class QuantityGenerator
    {
        /// <summary>
        /// Gets the quantities for a feature.
        /// </summary>
        /// <param name="feature"> The feature. </param>
        /// <param name="useSalinity">
        /// When the requesting model uses salinity, this argument is true. This affects the quantities
        /// that are generated.
        /// </param>
        /// <returns> A collection of strings that describe every possible quantity for <paramref name="feature"/> </returns>
        public static IEnumerable<string> GetQuantitiesForFeature(IFeature feature, bool useSalinity)
        {
            switch (feature)
            {
                case IPump _:
                    yield return KnownStructureProperties.Capacity;
                    yield break;
                case IStructure weir:
                {
                    foreach (string quantity in GetWeirQuantities(weir))
                    {
                        yield return quantity;
                    }

                    yield break;
                }
                case GroupableFeature2DPoint _:
                {
                    yield return "water_level";
                    if (useSalinity)
                    {
                        yield return "salinity";
                    }

                    yield return "water_depth";
                    yield break;
                }
                case ObservationCrossSection2D _:
                    yield return "discharge";
                    yield return "velocity";
                    yield return "water_level";
                    yield return "water_depth";
                    break;
            }
        }

        private static IEnumerable<string> GetWeirQuantities(IStructure weir)
        {
            switch (weir.Formula)
            {
                case SimpleWeirFormula _:
                    yield return KnownStructureProperties.CrestLevel;
                    break;
                case GeneralStructureFormula _:
                    yield return KnownGeneralStructureProperties.CrestLevel.GetDescription();
                    yield return KnownGeneralStructureProperties.GateLowerEdgeLevel.GetDescription();
                    yield return KnownGeneralStructureProperties.GateOpeningWidth.GetDescription();
                    break;
                case SimpleGateFormula _:
                    yield return KnownStructureProperties.CrestLevel;
                    yield return KnownStructureProperties.GateLowerEdgeLevel;
                    yield return KnownStructureProperties.GateOpeningWidth;
                    break;
            }
        }
    }
}