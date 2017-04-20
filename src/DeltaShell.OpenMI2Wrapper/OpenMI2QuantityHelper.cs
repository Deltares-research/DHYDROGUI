using System;
using System.Collections.Generic;
using DelftTools.Functions;
using Deltares.OpenMI2.Oatc.Sdk.Backbone;
using OpenMI.Standard2;

namespace DeltaShell.OpenMI2Wrapper
{
    public static class OpenMI2QuantityHelper
    {
        private static readonly Dictionary<KnownUnitIds, Dimension> dimensions = new Dictionary<KnownUnitIds, Dimension>();
        private static readonly Dictionary<KnownUnitIds, Unit> units = new Dictionary<KnownUnitIds, Unit>();

        public enum KnownUnitIds
        {
            Meter,
            MilliMeter,

            SquareMeter,

            CubicMeter,
            Liter,

            MeterPerSecond,
            MillimeterPerDay,

            SquareMeterPerSecond,
            CubicMeterPerSecondPerMeter,

            CubicMeterPerSecond,
            LiterPerSecond,

            Kilogram,
            KilogramPerSecond,

            Pascal,

            PartsPerThousand
        }

        public static IDimension CreateOrFindDimension(KnownUnitIds knownUnitId)
        {
            Dimension dimension;

            if (dimensions.ContainsKey(knownUnitId))
            {
                dimension = dimensions[knownUnitId];
            }
            else
            {
                dimension = new Dimension();
                switch (knownUnitId)
                {
                    case KnownUnitIds.Meter:
                        dimension.SetPower(DimensionBase.Length, 1);
                        break;
                    case KnownUnitIds.CubicMeter:
                    case KnownUnitIds.Liter:
                        dimension.SetPower(DimensionBase.Length, 3);
                        break;
                    case KnownUnitIds.MeterPerSecond:
                    case KnownUnitIds.MillimeterPerDay:
                        dimension.SetPower(DimensionBase.Length, 1);
                        dimension.SetPower(DimensionBase.Time, -1);
                        break;
                    case KnownUnitIds.CubicMeterPerSecond:
                    case KnownUnitIds.LiterPerSecond:
                        dimension.SetPower(DimensionBase.Length, 3);
                        dimension.SetPower(DimensionBase.Time, -1);
                        break;
                    case KnownUnitIds.SquareMeterPerSecond:
                    case KnownUnitIds.CubicMeterPerSecondPerMeter:
                        dimension.SetPower(DimensionBase.Length, 2);
                        dimension.SetPower(DimensionBase.Time, -1);
                        break;
                    case KnownUnitIds.Kilogram:
                        dimension.SetPower(DimensionBase.Mass, 1);
                        break;
                    case KnownUnitIds.KilogramPerSecond:
                        dimension.SetPower(DimensionBase.Mass, 1);
                        dimension.SetPower(DimensionBase.Time, -1);
                        break;
                    case KnownUnitIds.Pascal:
                        dimension.SetPower(DimensionBase.Mass, 1);
                        dimension.SetPower(DimensionBase.Length, -1);
                        dimension.SetPower(DimensionBase.Time, -2);
                        break;
                }
                dimensions.Add(knownUnitId, dimension);
            }
            return dimension;
        }

        public static IUnit CreateOrFindUnit(KnownUnitIds knownUnitId)
        {
            Unit unit;

            if (units.ContainsKey(knownUnitId))
            {
                unit = units[knownUnitId];
            }
            else
            {
                switch (knownUnitId)
                {
                    case KnownUnitIds.Meter:
                        unit = new Unit("m", 1, 0, "meter");
                        break;
                    case KnownUnitIds.MilliMeter:
                        unit = new Unit("mm", 0.001, 0, "milimeter");
                        break;
                    case KnownUnitIds.SquareMeter:
                        unit = new Unit("m²", 1, 0, "square meter");
                        break;
                    case KnownUnitIds.CubicMeter:
                        unit = new Unit("m³", 1, 0, "cubic meter");
                        break;
                    case KnownUnitIds.Liter:
                        unit = new Unit("L", 0.001, 0, "liter");
                        break;
                    case KnownUnitIds.MeterPerSecond:
                        unit = new Unit("m/s", 1, 0, "meter per second");
                        break;
                    case KnownUnitIds.SquareMeterPerSecond:
                        unit = new Unit("m²/s", 1, 0, "square meter per second");
                        break;
                    case KnownUnitIds.MillimeterPerDay:
                        unit = new Unit("mm/day", 1.15741E-08, 0, "millimeters per day");
                        break;
                    case KnownUnitIds.CubicMeterPerSecond:
                        unit = new Unit("m³/s", 1, 0, "cubic meter per second");
                        break;
                    case KnownUnitIds.LiterPerSecond:
                        unit = new Unit("L/s", 0.001, 0, "liter per second");
                        break;
                    case KnownUnitIds.Kilogram:
                        unit = new Unit("kg", 1, 0, "kilogram");
                        break;
                    case KnownUnitIds.KilogramPerSecond:
                        unit = new Unit("kg/s", 1, 0, "kilogram per second");
                        break;
                    case KnownUnitIds.Pascal:
                        unit = new Unit("Pa", 1, 0, "pascal");
                        break;
                    case KnownUnitIds.PartsPerThousand:
                        unit = new Unit("ppt", 1, 0, "parts per thousand");
                        break; 
                    default:
                        unit = new Unit("-", 1, 0);
                        break;
                }
                units.Add(knownUnitId, unit);
            }
            return unit;
        }

        public static IUnit CreateOrFindUnitByStandardName(string standardName)
        {
            switch (standardName)
            {
                case FunctionAttributes.StandardNames.WaterLevel:
                    return CreateOrFindUnit(KnownUnitIds.Meter);
                case FunctionAttributes.StandardNames.WaterDepth:
                    return CreateOrFindUnit(KnownUnitIds.Meter);
                case FunctionAttributes.StandardNames.WaterFlowArea:
                    return CreateOrFindUnit(KnownUnitIds.SquareMeter);
                case FunctionAttributes.StandardNames.WaterVolume:
                    return CreateOrFindUnit(KnownUnitIds.Liter);
                case FunctionAttributes.StandardNames.WaterDischarge:
                    return CreateOrFindUnit(KnownUnitIds.CubicMeterPerSecond);
                case FunctionAttributes.StandardNames.WaterVelocity:
                    return CreateOrFindUnit(KnownUnitIds.MeterPerSecond);
                default:
                    throw new Exception("Unknown standard name: " + standardName);
            }
        }

        public static IUnit CreateOrFindUnitByUnitString(string unitString)
        {
            switch (unitString)
            {
                case "m":
                    return CreateOrFindUnit(KnownUnitIds.Meter);
                case "m AD":
                    return CreateOrFindUnit(KnownUnitIds.Meter);
                case "m/s":
                    return CreateOrFindUnit(KnownUnitIds.MeterPerSecond);
                case "mm/d":
                    return CreateOrFindUnit(KnownUnitIds.MillimeterPerDay);
                case "m²":
                    return CreateOrFindUnit(KnownUnitIds.SquareMeter);
                case "m^2":
                    return CreateOrFindUnit(KnownUnitIds.SquareMeterPerSecond);
                case "m²/s":
                    return CreateOrFindUnit(KnownUnitIds.SquareMeterPerSecond);
                case "m^2/s":
                    return CreateOrFindUnit(KnownUnitIds.SquareMeterPerSecond);
                case "l":
                    return CreateOrFindUnit(KnownUnitIds.Liter);
                case "m³":
                    return CreateOrFindUnit(KnownUnitIds.CubicMeter);
                case "m³/s":
                    return CreateOrFindUnit(KnownUnitIds.CubicMeterPerSecond);
                case "m^3/s":
                    return CreateOrFindUnit(KnownUnitIds.CubicMeterPerSecond);
                case "l/s":
                    return CreateOrFindUnit(KnownUnitIds.LiterPerSecond);
                case "Pa":
                    return CreateOrFindUnit(KnownUnitIds.Pascal);
                case "ppt":
                    return CreateOrFindUnit(KnownUnitIds.PartsPerThousand);
                default:
                    throw new Exception("Unknown unit string: " + unitString);
            }
        }
    }
}
