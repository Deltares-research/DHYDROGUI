using System;
using System.Collections.Generic;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;

namespace DelftTools.Hydro
{
    public static class RoughnessHelper
    {
        public static RoughnessType ConvertStringToRoughnessType(string roughnessTypeString)
        {
            switch (roughnessTypeString.ToLower())
            {
                case "chezy":
                    return RoughnessType.Chezy;
                case "manning":
                    return RoughnessType.Manning;
                case "stricklernikuradse":
                    return RoughnessType.StricklerNikuradse;
                case "strickler":
                    return RoughnessType.Strickler;
                case "whitecolebrook":
                    return RoughnessType.WhiteColebrook;
                case "debosbijkerk":
                    return RoughnessType.DeBosBijkerk;
                case "walllawnikuradse":
                    return RoughnessType.WallLawNikuradse;
                default:
                    throw new InvalidOperationException($"{roughnessTypeString} is not a valid Roughness Type.");

            }
        }

        public static RoughnessFunction ConvertStringToRoughnessFunction(string roughnessFunctionString)
        {
            switch (roughnessFunctionString.ToLower())
            {
                case "constant":
                    return RoughnessFunction.Constant;
                case "absdischarge":
                case "functionofq":
                    return RoughnessFunction.FunctionOfQ;
                case "waterlevel":
                case "functionofh":
                    return RoughnessFunction.FunctionOfH;
                default:
                    throw new InvalidOperationException($"{roughnessFunctionString} is not a valid Roughness Function.");
            }
        }

        public static string ConvertRoughnessFunctionToString(RoughnessFunction roughnessFunction)
        {
            switch (roughnessFunction)
            {
                case RoughnessFunction.Constant:
                    return "constant";
                case RoughnessFunction.FunctionOfQ:
                    return "absDischarge";
                case RoughnessFunction.FunctionOfH:
                    return "waterLevel";
                default:
                    throw new InvalidOperationException($"{roughnessFunction} is not a valid Roughness Function.");
            }
        }
        
        private static readonly Dictionary<RoughnessType, double> RoughnessDefaultValueMapping =
            new Dictionary <RoughnessType, double>
            {
                {RoughnessType.Chezy, 45},
                {RoughnessType.Manning, 0.03},
                {RoughnessType.StricklerNikuradse, 0.2},
                {RoughnessType.Strickler, 33},
                {RoughnessType.WhiteColebrook, 0.2},
                {RoughnessType.DeBosBijkerk, 33.8},
                {RoughnessType.WallLawNikuradse, 0.2}
            };

        private static readonly IDictionary<RoughnessType, string> RoughnessUnitMapping =
            new Dictionary<RoughnessType, string>
        {
            {RoughnessType.Chezy, "m^1/2*s^-1"},
            {RoughnessType.Manning, "s*m^-1/3"},
            {RoughnessType.StricklerNikuradse, "m"},
            {RoughnessType.Strickler, "m^1/3*s^-1"},
            {RoughnessType.DeBosBijkerk, "s^-1"},
            {RoughnessType.WhiteColebrook, "m"},
            {RoughnessType.WallLawNikuradse, "m"}
        };

        public static double GetDefault(RoughnessType type)
        {
            if (RoughnessDefaultValueMapping.ContainsKey(type))
            {
                return RoughnessDefaultValueMapping[type];
            }
            throw new NotSupportedException("Unexpected roughness type! Cannot retrieve default value.");
        }

        public static string GetUnit(RoughnessType type)
        {
            if (RoughnessUnitMapping.ContainsKey(type))
            {
                return RoughnessUnitMapping[type];
            }
            throw new NotSupportedException("Unexpected roughness type! Cannot retrieve unit.");
        }

        public static double GetDefault(CulvertFrictionType type)
        {
            return GetDefault(ConvertRoughnessType(type));
        }

        public static double GetDefault(BridgeFrictionType type)
        {
            return GetDefault(ConvertRoughnessType(type));
        }

        public static string GetDialogTitle(RoughnessType type, string channelName)
        {
            return $"Roughness ({type.GetDescription()}, {GetUnit(type)}) for branch '{channelName}'";
        }

        private static RoughnessType ConvertRoughnessType(CulvertFrictionType frictionType)
        {
            switch (frictionType)
            {
                case CulvertFrictionType.Chezy:
                    return RoughnessType.Chezy;
                case CulvertFrictionType.Manning:
                    return RoughnessType.Manning;
                case CulvertFrictionType.StricklerKn:
                    return RoughnessType.StricklerNikuradse;
                case CulvertFrictionType.StricklerKs:
                    return RoughnessType.Strickler;
                case CulvertFrictionType.WhiteColebrook:
                    return RoughnessType.WhiteColebrook;
                default:
                    throw new InvalidOperationException();
            }
        }

        private static RoughnessType ConvertRoughnessType(BridgeFrictionType frictionType)
        {
            switch (frictionType)
            {
                case BridgeFrictionType.Chezy:
                    return RoughnessType.Chezy;
                case BridgeFrictionType.Manning:
                    return RoughnessType.Manning;
                case BridgeFrictionType.StricklerKn:
                    return RoughnessType.StricklerNikuradse;
                case BridgeFrictionType.StricklerKs:
                    return RoughnessType.Strickler;
                case BridgeFrictionType.WhiteColebrook:
                    return RoughnessType.WhiteColebrook;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}