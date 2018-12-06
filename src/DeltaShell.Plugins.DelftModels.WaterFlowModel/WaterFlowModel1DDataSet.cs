using DelftTools.Functions;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    public class WaterFlowModel1DDataSet
    {
        static WaterFlowModel1DDataSet()
        {
            StructureNames = CreateStructureNames();
            LateralNames = CreateLateralNames();
            RetentionNames = CreateRetentionNames();
            ObservationPointNames = CreateObservationPointNames();
        }

        public const string MainChannelName = "Main";
        public const string Floodplain1Name = "FloodPlain1";
        public const string Floodplain2Name = "FloodPlain2";

        // Tags of DataItems
        public const string LateralSourcesDataTag = "Lateral Data";
        public const string BoundaryConditionsTag = "Boundary Data";
        public const string RoughnessSectionsTag = "Roughness";
        public const string InputInitialConditionsTag = "initial water depth";
        public const string InputInitialFlowTag = "initial water flow";
        public const string InputWindTag = "wind";
        public const string WindShieldingTag = "wind shielding";
        public const string InputInitialSaltConcentrationTag = "initial salinity concentration";
        public const string InputDispersionCoverageTag = "dispersion coefficient";
        public const string InputDispersionF3CoverageTag = "dispersion F3 coefficient";
        public const string InputDispersionF4CoverageTag = "dispersion F4 coefficient";
        public const string NetworkDiscretizationTag = "network discretization";
        public const string InflowsTag = "inflows"; //todo:find good name
        public const string NetworkTag = "network";
        public const string InitialConditionsTypeTag = "InitialConditionsType";
        public const string DispersionFormulationTypeTag = "DispersionFormulationType";
        public const string UseSaltParameterTag = "usesalt";
        public const string UseSaltInCalculationParameterTag = "usesaltincalculation";
        public const string UseReverseRoughnessParameterTag = "usereverseroughness";
        public const string UseReverseRoughnessInCalculationParameterTag = "usereverseroughnessincalculation";
        public const string DefaultInitialDepthTag = "defaultinitialdepth";
        public const string DefaultInitialWaterLevelTag = "defaultinitialwaterlevel";
        public const string SedimentPathTag = "sedimentpath";
        public const string InputMeteoDataTag = "Meteo data";
        public const string InputInitialTemperatureTag = "Initial temperature";
        public const string UseTemperatureParameterTag = "usetemperature";
        public const string TemperatureModelTypeTag = "TemperatureModelType";
        public const string DensityTypeTag = "DensityType";
        public const string DiscretizationDataObjectName = "Computational Grid";
        public const string MorphologyFileDataObjectTag = "MorphologyFile";
        public const string BcmFileDataObjectTag = "BcmFile";
        public const string SedimentFileDataObjectTag = "SedimentFile";
        public const string TraFileDataObjectTag = "TraFile";

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
                case WaterFlowParametersCategories.Laterals: return LateralNames;
                case WaterFlowParametersCategories.ObservationPoints: return ObservationPointNames;
                case WaterFlowParametersCategories.Culverts:
                case WaterFlowParametersCategories.Pumps:
                case WaterFlowParametersCategories.Weirs:
                    return StructureNames;
                case WaterFlowParametersCategories.Retentions:
                    return RetentionNames;
                default:
                    return null;
            }
        }

        private static Dictionary<string, string> CreateObservationPointNames()
        {
            return new Dictionary<string, string>
            {
                {WaterFlowModelParameterNames.ObservationPointWaterLevel, FunctionAttributes.StandardNames.WaterLevel},
                {WaterFlowModelParameterNames.ObservationPointWaterDepth, FunctionAttributes.StandardNames.WaterDepth},
                {WaterFlowModelParameterNames.ObservationPointDischarge, FunctionAttributes.StandardNames.WaterDischarge},
                { WaterFlowModelParameterNames.ObservationPointVelocity, FunctionAttributes.StandardNames.WaterVelocity},
                {WaterFlowModelParameterNames.ObservationPointSaltConcentration, FunctionAttributes.StandardNames.WaterSalinity}
            };
        }

        private static Dictionary<string, string> CreateRetentionNames()
        {
            return new Dictionary<string, string>
            {
                {WaterFlowModelParameterNames.RetentionWaterLevel, FunctionAttributes.StandardNames.WaterLevel},
                {WaterFlowModelParameterNames.RetentionVolume, FunctionAttributes.StandardNames.WaterVolume}
            };
        }

        private static Dictionary<string, string> CreateLateralNames()
        {
            return new Dictionary<string, string>
            {
                {WaterFlowModelParameterNames.LateralActualDischarge, FunctionAttributes.StandardNames.WaterDischarge},
                {WaterFlowModelParameterNames.LateralDefinedDischarge, FunctionAttributes.StandardNames.WaterDischarge},
                {WaterFlowModelParameterNames.LateralDifference, FunctionAttributes.StandardNames.WaterDischarge},
                {WaterFlowModelParameterNames.LateralWaterLevel, FunctionAttributes.StandardNames.WaterLevel}
            };
        }

        private static Dictionary<string, string> CreateStructureNames()
        {
            return new Dictionary<string, string>
            {
                {WaterFlowModelParameterNames.StructureDischarge, FunctionAttributes.StandardNames.WaterDischarge},
                {WaterFlowModelParameterNames.StructureVelocity, FunctionAttributes.StandardNames.WaterVelocity},
                {WaterFlowModelParameterNames.StructureFlowArea, FunctionAttributes.StandardNames.WaterFlowArea},
                {WaterFlowModelParameterNames.StructurePressureDifference, FunctionAttributes.StandardNames.StructurePressureDifference},
                {WaterFlowModelParameterNames.StructureCrestLevel, FunctionAttributes.StandardNames.StructureCrestLevel},
                {WaterFlowModelParameterNames.StructureCrestWidth, FunctionAttributes.StandardNames.StructureCrestWidth},
                {WaterFlowModelParameterNames.StructureGateLevel, FunctionAttributes.StandardNames.StructureGateLowerEdgeLevel},
                {WaterFlowModelParameterNames.StructureOpeningHeight, FunctionAttributes.StandardNames.StructureGateOpeningHeight},
                {WaterFlowModelParameterNames.StructureValveOpening, FunctionAttributes.StandardNames.StructureValveOpening},
                {WaterFlowModelParameterNames.StructureWaterlevelUp, FunctionAttributes.StandardNames.StructureWaterLevelUpstream},
                {WaterFlowModelParameterNames.StructureWaterlevelDown, FunctionAttributes.StandardNames.StructureWaterLevelDownstream},
                {WaterFlowModelParameterNames.StructureHeadDifference, FunctionAttributes.StandardNames.StructureWaterHead},
                {WaterFlowModelParameterNames.StructureWaterLevelAtCrest, FunctionAttributes.StandardNames.StructureWaterLevelAtCrest},
                {WaterFlowModelParameterNames.StructureSetPoint, FunctionAttributes.StandardNames.StructureSetPoint}
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