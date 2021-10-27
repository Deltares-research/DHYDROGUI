using System.Collections.Generic;
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
            WeirNames = CreateWeirNames();
            PumpNames = CreatePumpNames();
            CulvertNames = CreateCulvertsNames();
            GateNames = CreateGatesNames();
            OrificeNames = CreateOrificesNames();
            GeneralStructureNames = CreateGeneralStructuresNames();
            LeveeBreachNames = CreateLeveeBreachesNames();
            SourceSinkNames = CreateSourcesAndSinksNames();
            CrossSectionNames = CreateCrossSectionNames();
        }

        public const string DiaFileDataItemTag = "DiaFile";
        public const string HydroAreaTag = "hydro_area_tag";

        public const string LateralSourcesDataTag = "1D Lateral Data";
        public const string BoundaryConditionsTag = "1D Boundary Data";
        public const string NetworkTag = "network";

        public const double DefaultSaltDispersion = 1;

        private static readonly Dictionary<string, string> StructureNames;
        private static readonly Dictionary<string, string> LateralNames;
        private static readonly Dictionary<string, string> RetentionNames;
        private static readonly Dictionary<string, string> WeirNames;
        private static readonly Dictionary<string, string> PumpNames;
        private static readonly Dictionary<string, string> CulvertNames;
        private static readonly Dictionary<string, string> GateNames;
        private static readonly Dictionary<string, string> OrificeNames;
        private static readonly Dictionary<string, string> GeneralStructureNames;
        private static readonly Dictionary<string, string> LeveeBreachNames;
        private static readonly Dictionary<string, string> SourceSinkNames;
        private static readonly Dictionary<string, string> ObservationPointNames;
        private static readonly Dictionary<string, string> CrossSectionNames;

        public static Dictionary<string, string> GetDictionaryForCategory(string category)
        {
            switch (category)
            {
                case Model1DParametersCategories.Laterals: return LateralNames;
                case Model1DParametersCategories.ObservationPoints: return ObservationPointNames;
                case Model1DParametersCategories.Culverts: return CulvertNames; 
                case Model1DParametersCategories.Pumps: return PumpNames;
                case Model1DParametersCategories.Gates: return GateNames;
                case Model1DParametersCategories.Orifices: return OrificeNames;
                case Model1DParametersCategories.GeneralStructures: return GeneralStructureNames;
                case Model1DParametersCategories.SourceSinks: return SourceSinkNames;
                case Model1DParametersCategories.LeveeBreaches: return LeveeBreachNames;
                case Model1DParametersCategories.Weirs: return WeirNames;
                case Model1DParametersCategories.CrossSections: return CrossSectionNames;
                
                    
                case Model1DParametersCategories.Retentions:
                    
                default:
                    return null;
            }
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

        private static Dictionary<string, string> CreatePumpNames()
        {
            return new Dictionary<string, string>
            {
                {Model1DParameterNames.PumpCapacity, "capacity"},
            };
        }

        private static Dictionary<string, string> CreateWeirNames()
        {
            return new Dictionary<string, string>
            {
                {Model1DParameterNames.StructureCrestLevel, "crestLevel"},
                //{"lat_contr_coeff", "lat_contr_coeff"},
            };
        }

        private static Dictionary<string, string> CreateOrificesNames()
        {
            return new Dictionary<string, string>
            {
                {Model1DParameterNames.StructureGateLevel, "gateLowerEdgeLevel"},
            };
        }

        private static Dictionary<string, string> CreateGatesNames()
        {
            return new Dictionary<string, string>
            {
                {Model1DParameterNames.StructureCrestLevel, "CrestLevel"},
                {Model1DParameterNames.StructureOpeningHeight, "GateHeight"},
                {Model1DParameterNames.StructureGateLevel, "GateLowerEdgeLevel"},
                {Model1DParameterNames.StructureGateOpeningWidth, "GateOpeningWidth"},
                {Model1DParameterNames.StructureGateOpeningHorizontalDirection, "GateOpeningHorizontalDirection"},
            };
        }

        private static Dictionary<string, string> CreateGeneralStructuresNames()
        {
            return new Dictionary<string, string>
            {
                {Model1DParameterNames.StructureCrestLevel, "CrestLevel"},
                {Model1DParameterNames.StructureOpeningHeight, "GateHeight"},
                {Model1DParameterNames.StructureGateLevel, "GateLowerEdgeLevel"},
                {Model1DParameterNames.StructureGateOpeningWidth, "GateOpeningWidth"},
                {Model1DParameterNames.StructureGateOpeningHorizontalDirection, "GateOpeningHorizontalDirection"},
            };
        }
        private static Dictionary<string, string> CreateCulvertsNames()
        {
            return new Dictionary<string, string>
            {
                {Model1DParameterNames.StructureValveOpening, "valveOpeningHeight"},
            };
        }

        private static Dictionary<string, string> CreateSourcesAndSinksNames()
        {
            return new Dictionary<string, string>
            {
                {"discharge", "discharge"},
                {"change_in_salinity", "change_in_salinity"},
                {"change_in_temperature", "change_in_temperature"},
            };
        }
        private static Dictionary<string, string> CreateLeveeBreachesNames()
        {
            return new Dictionary<string, string>
            {
                {"dambreak_s1up", "dambreak_s1up"},
                {"dambreak_s1dn", "dambreak_s1dn"},
                {"dambreak_breach_depth", "dambreak_breach_depth"},
                {"dambreak_breach_width", "dambreak_breach_width"},
                {"dambreak_instantaneous_discharge", "dambreak_instantaneous_discharge"},
                {"dambreak_cumulative_discharge", "dambreak_cumulative_discharge"},
            };
        }
        private static Dictionary<string, string> CreateObservationPointNames()
        {
            return new Dictionary<string, string>
            {
                {Model1DParameterNames.ObservationPointWaterLevel, "water_level"},
                {Model1DParameterNames.ObservationPointWaterDepth, "water_depth"},
                {Model1DParameterNames.ObservationPointDischarge, "discharge"},
                {Model1DParameterNames.ObservationPointVelocity, "velocity"},
                {Model1DParameterNames.ObservationPointSaltConcentration, "salinity"},
                {Model1DParameterNames.ObservationPointSaltDispersion, "salinity"},
                {Model1DParameterNames.ObservationPointTemperature, "temperature"},
                
            };
        }
        private static Dictionary<string, string> CreateCrossSectionNames()
        {
            return new Dictionary<string, string>
            {
                {"discharge", "discharge"},
                {"velocity", "velocity"},
                {"water_level", "water_level"},
                {"water_depth", "water_depth"},
                
                
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
                {Model1DParameterNames.StructureWaterLevelAtCrest, FunctionAttributes.StandardNames.StructureWaterLevelAtCrest}
            };
        }
    }
}
