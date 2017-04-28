using System;
using DelftTools.Hydro.Structures;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Responsible to convert ds frictiontypes to modelapi types
    /// </summary>
    public static class FrictionTypeConverter
    {
        public static Friction ConvertFrictionType(RoughnessType roughnessType)
        {
            switch (roughnessType)
            {
                case RoughnessType.Chezy:
                    return Friction.Chezy;
                case RoughnessType.Manning:
                    return Friction.Mannings;
                case RoughnessType.StricklerKn:
                    return Friction.Nikuradse;
                case RoughnessType.StricklerKs:
                    return Friction.Strickler;
                case RoughnessType.WhiteColebrook:
                    return Friction.WhiteColebrook;
                case RoughnessType.DeBosAndBijkerk:
                    return Friction.BosBijkerk;
                default:
                    throw new InvalidOperationException();
            }
        }
        public static RoughnessType ConvertToRoughnessFrictionType(Friction type)
        {
            switch (type)
            {
                case Friction.Chezy:
                    return RoughnessType.Chezy;
                case Friction.Mannings:
                    return RoughnessType.Manning;
                case Friction.Nikuradse:
                    return RoughnessType.StricklerKn;
                case Friction.Strickler:
                    return RoughnessType.StricklerKs;
                case Friction.WhiteColebrook:
                    return RoughnessType.WhiteColebrook;
                case Friction.BosBijkerk:
                    return RoughnessType.DeBosAndBijkerk;
                default:
                    return RoughnessType.Chezy;//throw new InvalidOperationException();
            }
        }

        public static Friction ConvertFrictionType(BridgeFrictionType type)
        {
            switch (type)
            {
                case BridgeFrictionType.Chezy:
                    return Friction.Chezy;
                case BridgeFrictionType.Manning:
                    return Friction.Mannings;
                case BridgeFrictionType.StricklerKn:
                    return Friction.Nikuradse;
                case BridgeFrictionType.StricklerKs:
                    return Friction.Strickler;
                case BridgeFrictionType.WhiteColebrook:
                    return Friction.WhiteColebrook;
                default:
                    throw new InvalidOperationException();
            }
        }

        public static BridgeFrictionType ConvertToBridgeFrictionType(Friction type)
        {
            switch (type)
            {
                case Friction.Chezy:
                    return BridgeFrictionType.Chezy;
                case Friction.Mannings:
                    return BridgeFrictionType.Manning;
                case Friction.Nikuradse:
                    return BridgeFrictionType.StricklerKn;
                case Friction.Strickler:
                    return BridgeFrictionType.StricklerKs;
                case Friction.WhiteColebrook:
                    return BridgeFrictionType.WhiteColebrook;
                default:
                    return BridgeFrictionType.Chezy;//throw new InvalidOperationException();
            }
        }

        public static Friction ConvertFrictionType(CulvertFrictionType type)
        {
            switch (type)
            {
                case CulvertFrictionType.Chezy:
                    return Friction.Chezy;
                case CulvertFrictionType.Manning:
                    return Friction.Mannings;
                case CulvertFrictionType.StricklerKn:
                    return Friction.Nikuradse;
                case CulvertFrictionType.StricklerKs:
                    return Friction.Strickler;
                case CulvertFrictionType.WhiteColebrook:
                    return Friction.WhiteColebrook;
                default:
                    throw new InvalidOperationException();
            }
        }
        public static CulvertFrictionType ConvertToCulvertFrictionType(Friction type)
        {
            switch (type)
            {
                case Friction.Chezy:
                    return CulvertFrictionType.Chezy;
                case Friction.Mannings:
                    return CulvertFrictionType.Manning;
                case Friction.Nikuradse:
                    return CulvertFrictionType.StricklerKn;
                case Friction.Strickler:
                    return CulvertFrictionType.StricklerKs;
                case Friction.WhiteColebrook:
                    return CulvertFrictionType.WhiteColebrook;
                default:
                    return CulvertFrictionType.Chezy;//throw new InvalidOperationException();
            }
        }
    }
}