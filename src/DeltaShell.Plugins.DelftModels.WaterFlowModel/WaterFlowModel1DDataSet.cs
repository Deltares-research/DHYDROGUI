using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DelftTools.Functions;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Model1D;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    public class WaterFlowModel1DDataSet
    {
        static WaterFlowModel1DDataSet()
        {
            DHydroNames = CreateDHydroNamesDictionary();
        }
        
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
        public static readonly Dictionary<string, string> DHydroNames;
        private static Dictionary<string, string> CreateDHydroNamesDictionary()
        {
            Dictionary<string, string> dHydroNamesDictionary = new Dictionary<string, string>();

            dHydroNamesDictionary.Add(Model1DParameterNames.StructureDischarge, FunctionAttributes.StandardNames.WaterDischarge);
            dHydroNamesDictionary.Add(Model1DParameterNames.StructureVelocity, FunctionAttributes.StandardNames.WaterVelocity);
            dHydroNamesDictionary.Add(Model1DParameterNames.StructureFlowArea, FunctionAttributes.StandardNames.WaterFlowArea);
            dHydroNamesDictionary.Add(Model1DParameterNames.StructurePressureDifference, FunctionAttributes.StandardNames.StructurePressureDifference);
            dHydroNamesDictionary.Add(Model1DParameterNames.StructureCrestLevel, FunctionAttributes.StandardNames.StructureCrestLevel);

            dHydroNamesDictionary.Add(Model1DParameterNames.StructureCrestWidth, FunctionAttributes.StandardNames.StructureCrestWidth);
            dHydroNamesDictionary.Add(Model1DParameterNames.StructureGateLevel, FunctionAttributes.StandardNames.StructureGateLowerEdgeLevel);
            dHydroNamesDictionary.Add(Model1DParameterNames.StructureOpeningHeight, FunctionAttributes.StandardNames.StructureGateOpeningHeight);
            dHydroNamesDictionary.Add(Model1DParameterNames.StructureValveOpening, FunctionAttributes.StandardNames.StructureValveOpening);
            dHydroNamesDictionary.Add(Model1DParameterNames.StructureWaterlevelUp, FunctionAttributes.StandardNames.StructureWaterLevelUpstream);

            dHydroNamesDictionary.Add(Model1DParameterNames.StructureWaterlevelDown, FunctionAttributes.StandardNames.StructureWaterLevelDownstream);
            dHydroNamesDictionary.Add(Model1DParameterNames.StructureHeadDifference, FunctionAttributes.StandardNames.StructureWaterHead);
            dHydroNamesDictionary.Add(Model1DParameterNames.StructureWaterLevelAtCrest, FunctionAttributes.StandardNames.StructureWaterLevelAtCrest);
            dHydroNamesDictionary.Add(Model1DParameterNames.StructureSetPoint, FunctionAttributes.StandardNames.StructureSetPoint);

            dHydroNamesDictionary.Add(Model1DParameterNames.LateralActualDischarge, FunctionAttributes.StandardNames.WaterDischarge);
            dHydroNamesDictionary.Add(Model1DParameterNames.LateralDefinedDischarge, FunctionAttributes.StandardNames.WaterDischarge);
            dHydroNamesDictionary.Add(Model1DParameterNames.LateralDifference, FunctionAttributes.StandardNames.WaterDischarge);
            dHydroNamesDictionary.Add(Model1DParameterNames.LateralWaterLevel, FunctionAttributes.StandardNames.WaterLevel);

            dHydroNamesDictionary.Add(Model1DParameterNames.RetentionWaterLevel, FunctionAttributes.StandardNames.WaterLevel);
            dHydroNamesDictionary.Add(Model1DParameterNames.RetentionVolume, FunctionAttributes.StandardNames.WaterVolume);

            dHydroNamesDictionary.Add(Model1DParameterNames.ObservationPointWaterLevel, FunctionAttributes.StandardNames.WaterLevel);
            dHydroNamesDictionary.Add(Model1DParameterNames.ObservationPointWaterDepth, FunctionAttributes.StandardNames.WaterDepth);
            //dHydroNamesDictionary.Add(WaterFlowModelParameterNames.ObservationPointSurfaceArea, ????);
            dHydroNamesDictionary.Add(Model1DParameterNames.ObservationPointDischarge, FunctionAttributes.StandardNames.WaterDischarge);
            dHydroNamesDictionary.Add(Model1DParameterNames.ObservationPointVelocity, FunctionAttributes.StandardNames.WaterVelocity);
            dHydroNamesDictionary.Add(Model1DParameterNames.ObservationPointSaltConcentration, FunctionAttributes.StandardNames.WaterSalinity);


            return dHydroNamesDictionary;
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