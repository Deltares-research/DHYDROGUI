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
                    return Friction.Manning;
                case RoughnessType.StricklerNikuradse:
                    return Friction.StricklerNikuradse;
                case RoughnessType.Strickler:
                    return Friction.Strickler;
                case RoughnessType.WhiteColebrook:
                    return Friction.WhiteColebrook;
                case RoughnessType.DeBosBijkerk:
                    return Friction.DeBosBijkerk;
                case RoughnessType.WallLawNikuradse:
                    return Friction.WallLawNikuradse;
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
                case Friction.Manning:
                    return RoughnessType.Manning;
                case Friction.StricklerNikuradse:
                    return RoughnessType.StricklerNikuradse;
                case Friction.Strickler:
                    return RoughnessType.Strickler;
                case Friction.WhiteColebrook:
                    return RoughnessType.WhiteColebrook;
                case Friction.DeBosBijkerk:
                    return RoughnessType.DeBosBijkerk;
                case Friction.WallLawNikuradse:
                    return RoughnessType.WallLawNikuradse;
                default:
                    return RoughnessType.Chezy;
            }
        }

        public static Friction ConvertFrictionType(BridgeFrictionType type)
        {
            switch (type)
            {
                case BridgeFrictionType.Chezy:
                    return Friction.Chezy;
                case BridgeFrictionType.Manning:
                    return Friction.Manning;
                case BridgeFrictionType.StricklerKn:
                    return Friction.StricklerNikuradse;
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
                case Friction.Manning:
                    return BridgeFrictionType.Manning;
                case Friction.StricklerNikuradse:
                    return BridgeFrictionType.StricklerKn;
                case Friction.Strickler:
                    return BridgeFrictionType.StricklerKs;
                case Friction.WhiteColebrook:
                    return BridgeFrictionType.WhiteColebrook;
                default:
                    return BridgeFrictionType.Chezy;
            }
        }

        public static Friction ConvertFrictionType(CulvertFrictionType type)
        {
            switch (type)
            {
                case CulvertFrictionType.Chezy:
                    return Friction.Chezy;
                case CulvertFrictionType.Manning:
                    return Friction.Manning;
                case CulvertFrictionType.StricklerKn:
                    return Friction.StricklerNikuradse;
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
                case Friction.Manning:
                    return CulvertFrictionType.Manning;
                case Friction.StricklerNikuradse:
                    return CulvertFrictionType.StricklerKn;
                case Friction.Strickler:
                    return CulvertFrictionType.StricklerKs;
                case Friction.WhiteColebrook:
                    return CulvertFrictionType.WhiteColebrook;
                default:
                    return CulvertFrictionType.Chezy;
            }
        }
    }
}