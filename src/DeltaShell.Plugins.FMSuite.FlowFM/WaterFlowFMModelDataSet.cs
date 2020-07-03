using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DelftTools.Functions;
using DeltaShell.NGHS.IO.DataObjects.Model1D;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public static class WaterFlowFMModelDataSet
    {
        static WaterFlowFMModelDataSet()
        {
            StructureNames = CreateStructureNames();
            LateralNames = CreateLateralNames();
            RetentionNames = CreateRetentionNames();
            ObservationPointNames = CreateObservationPointNames();
        }

        public const string DiaFileDataItemTag = "DiaFile";
        public const string HydroAreaTag = "hydro_area_tag";

        public const string LateralSourcesDataTag = "1D Lateral Data";
        public const string BoundaryConditionsTag = "1D Boundary Data";
        public const string NetworkTag = "network";

        public enum UnitIds
        {
            None,
            Meter,
            CubicMeterPerSecond,
        }

        public const double DefaultSaltDispersion = 1;

        private static readonly Dictionary<string, string> StructureNames;
        private static readonly Dictionary<string, string> LateralNames;
        private static readonly Dictionary<string, string> RetentionNames;
        private static readonly Dictionary<string, string> ObservationPointNames;

        public static Dictionary<string, string> GetDictionaryForCategory(string category)
        {
            switch (category)
            {
                case Model1DParametersCategories.Laterals: return LateralNames;
                case Model1DParametersCategories.ObservationPoints: return ObservationPointNames;
                case Model1DParametersCategories.Culverts:
                case Model1DParametersCategories.Pumps:
                case Model1DParametersCategories.Weirs:
                    return StructureNames;
                case Model1DParametersCategories.Retentions:
                    return RetentionNames;
                default:
                    return null;
            }
        }

        private static Dictionary<string, string> CreateObservationPointNames()
        {
            return new Dictionary<string, string>
            {
                {Model1DParameterNames.ObservationPointWaterLevel, FunctionAttributes.StandardNames.WaterLevel},
                {Model1DParameterNames.ObservationPointWaterDepth, FunctionAttributes.StandardNames.WaterDepth},
                {Model1DParameterNames.ObservationPointDischarge, FunctionAttributes.StandardNames.WaterDischarge},
                { Model1DParameterNames.ObservationPointVelocity, FunctionAttributes.StandardNames.WaterVelocity},
                {Model1DParameterNames.ObservationPointSaltConcentration, FunctionAttributes.StandardNames.WaterSalinity}
            };
        }

        private static Dictionary<string, string> CreateRetentionNames()
        {
            return new Dictionary<string, string>
            {
                {Model1DParameterNames.RetentionWaterLevel, FunctionAttributes.StandardNames.WaterLevel},
                {Model1DParameterNames.RetentionVolume, FunctionAttributes.StandardNames.WaterVolume}
            };
        }

        private static Dictionary<string, string> CreateLateralNames()
        {
            return new Dictionary<string, string>
            {
                {Model1DParameterNames.LateralActualDischarge, FunctionAttributes.StandardNames.WaterDischarge},
                {Model1DParameterNames.LateralDefinedDischarge, FunctionAttributes.StandardNames.WaterDischarge},
                {Model1DParameterNames.LateralDifference, FunctionAttributes.StandardNames.WaterDischarge},
                {Model1DParameterNames.LateralWaterLevel, FunctionAttributes.StandardNames.WaterLevel}
            };
        }

        private static Dictionary<string, string> CreateStructureNames()
        {
            return new Dictionary<string, string>
            {
                {Model1DParameterNames.StructureDischarge, FunctionAttributes.StandardNames.WaterDischarge},
                {Model1DParameterNames.StructureVelocity, FunctionAttributes.StandardNames.WaterVelocity},
                {Model1DParameterNames.StructureFlowArea, FunctionAttributes.StandardNames.WaterFlowArea},
                {Model1DParameterNames.StructurePressureDifference, FunctionAttributes.StandardNames.StructurePressureDifference},
                {Model1DParameterNames.StructureCrestLevel, FunctionAttributes.StandardNames.StructureCrestLevel},
                {Model1DParameterNames.StructureCrestWidth, FunctionAttributes.StandardNames.StructureCrestWidth},
                {Model1DParameterNames.StructureGateLevel, FunctionAttributes.StandardNames.StructureGateLowerEdgeLevel},
                {Model1DParameterNames.StructureOpeningHeight, FunctionAttributes.StandardNames.StructureGateOpeningHeight},
                {Model1DParameterNames.StructureValveOpening, FunctionAttributes.StandardNames.StructureValveOpening},
                {Model1DParameterNames.StructureWaterlevelUp, FunctionAttributes.StandardNames.StructureWaterLevelUpstream},
                {Model1DParameterNames.StructureWaterlevelDown, FunctionAttributes.StandardNames.StructureWaterLevelDownstream},
                {Model1DParameterNames.StructureHeadDifference, FunctionAttributes.StandardNames.StructureWaterHead},
                {Model1DParameterNames.StructureWaterLevelAtCrest, FunctionAttributes.StandardNames.StructureWaterLevelAtCrest},
                {Model1DParameterNames.StructureSetPoint, FunctionAttributes.StandardNames.StructureSetPoint}
            };
        }

        [ExcludeFromCodeCoverage] // contains Data Only!
        public class Meteo
        {
            public const double valueLatitudeDefault = 52.00667;
            public const double valueLongitudDefault = 4.35556;
            public const double valueBackgroundTemperatureDefault = 0.0;
            public const double valueBackgroundTemperatureMin = 0.0;
            public const double valueBackgroundTemperatureMax = 60.0;
            public const double valueSurfaceAreaDefault = 1000000;
            public const double valueSurfaceAreaMin = 0.0;
            public const double valueAtmosphericPressureDefault = 100000;
            public const double valueDaltonNumberDefault = 0.0013;
            public const double valueDaltonNumberMin = 0.0;
            public const double valueDaltonNumberMax = 1.0;
            public const double valueStantonNumberDefault = 0.0013;
            public const double valueStantonNumberMin = 0.0;
            public const double valueStantonNumberMax = 1.0;
            public const double valueHeatCapacityWaterDefault = 3930;
        }
    }
}
