using System;
using System.Collections.Generic;
using DelftTools.Hydro.Structures;

namespace DelftTools.Hydro
{
    public static class RoughnessTypeDefaults
    {
        private static Dictionary<RoughnessType, double> DefaultRoughnessValueMapping = new Dictionary
            <RoughnessType, double>
            {
                {RoughnessType.Chezy, 45},
                {RoughnessType.Manning, 0.03},
                {RoughnessType.StricklerKn, 0.2},
                {RoughnessType.StricklerKs, 33},
                {RoughnessType.WhiteColebrook, 0.2},
                {RoughnessType.DeBosAndBijkerk, 33.8}
            };

        public static RoughnessType ConvertRougnessType(CulvertFrictionType frictionType)
        {
            switch (frictionType)
            {
                case CulvertFrictionType.Chezy:
                    return RoughnessType.Chezy;
                case CulvertFrictionType.Manning:
                    return RoughnessType.Manning;
                case CulvertFrictionType.StricklerKn:
                    return RoughnessType.StricklerKn;
                case CulvertFrictionType.StricklerKs:
                    return RoughnessType.StricklerKs;
                case CulvertFrictionType.WhiteColebrook:
                    return RoughnessType.WhiteColebrook;
                default:
                    throw new InvalidOperationException();
            }
        }

        public static RoughnessType ConvertRougnessType(BridgeFrictionType frictionType)
        {
            switch (frictionType)
            {
                case BridgeFrictionType.Chezy:
                    return RoughnessType.Chezy;
                case BridgeFrictionType.Manning:
                    return RoughnessType.Manning;
                case BridgeFrictionType.StricklerKn:
                    return RoughnessType.StricklerKn;
                case BridgeFrictionType.StricklerKs:
                    return RoughnessType.StricklerKs;
                case BridgeFrictionType.WhiteColebrook:
                    return RoughnessType.WhiteColebrook;
                default:
                    throw new InvalidOperationException();
            }
        }

        public static double GetDefault(RoughnessType type)
        {
            if (DefaultRoughnessValueMapping.ContainsKey(type))
            {
                return DefaultRoughnessValueMapping[type];
            }

            throw new NotSupportedException("Unexpected roughness type! Cannot retrieve default value.");
        }

        public static double GetDefault(CulvertFrictionType type)
        {
            return GetDefault(ConvertRougnessType(type));
        }

        public static double GetDefault(BridgeFrictionType type)
        {
            return GetDefault(ConvertRougnessType(type));
        }
    }
}