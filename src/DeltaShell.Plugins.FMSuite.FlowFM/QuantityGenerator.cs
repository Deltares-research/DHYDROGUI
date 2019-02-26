using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.FlowFM
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
        /// <param name="feature">The feature.</param>
        /// <param name="useSalinity">When the requesting model uses salinity, this argument is true. This affects the quantities that are generated.</param>
        /// <returns>A collection of strings that describe every possible quantity for <paramref name="feature"/></returns>
        public static IEnumerable<string> GetQuantitiesForFeature(IFeature feature, bool useSalinity)
        {
            if (feature is IPump)
            {
                yield return KnownStructureProperties.Capacity;
                yield break;
            }

            if (feature is IWeir weir)
            {
                foreach (var quantity in GetWeirQuantities(weir)) yield return quantity;
                yield break;
            }

            if (feature is GroupableFeature2DPoint)
            {
                //TODO: add temperature and tracers
                yield return "water_level";
                if (useSalinity)
                {
                    yield return "salinity";
                }
                yield return "water_depth";
                yield break;
            }

            if (feature is ObservationCrossSection2D)
            {
                yield return "discharge";
                yield return "velocity";
                yield return "water_level";
                yield return "water_depth";
            }
        }

        private static IEnumerable<string> GetWeirQuantities(IWeir weir)
        {
            var weirFormula = weir.WeirFormula;
            if (weirFormula is SimpleWeirFormula)
            {
                yield return KnownStructureProperties.CrestLevel;
            }
            if (weirFormula is GeneralStructureWeirFormula)
            {
                yield return EnumDescriptionAttributeTypeConverter.GetEnumDescription(KnownGeneralStructureProperties.LevelCenter);
                yield return EnumDescriptionAttributeTypeConverter.GetEnumDescription(KnownGeneralStructureProperties.GateHeight);
                yield return EnumDescriptionAttributeTypeConverter.GetEnumDescription(KnownGeneralStructureProperties.HorizontalDoorOpeningWidth);
            }
            if (weirFormula is GatedWeirFormula)
            {
                yield return KnownStructureProperties.GateSillLevel;
                yield return KnownStructureProperties.GateLowerEdgeLevel;
                yield return KnownStructureProperties.GateOpeningWidth;
            }
        }
    }
}
