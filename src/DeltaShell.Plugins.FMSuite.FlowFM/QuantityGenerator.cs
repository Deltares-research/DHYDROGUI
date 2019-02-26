using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public static class QuantityGenerator
    {
        public static IEnumerable<string> GetQuantitiesForLocation(IFeature feature, bool useSalinity)
        {
            var pump = feature as IPump;
            if (pump != null)
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
