using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition
{
    public sealed class WaterFlowFMPropertyDefinition : ModelPropertyDefinition
    {
        public string MduPropertyName
        {
            get { return base.FilePropertyKey; }
            set { base.FilePropertyKey = value; }
        }
    }

    public static class KnownProperties
    {
        public const string PathsRelativeToParent = "pathsrelativetoparent";
        public const string BathymetryFile = "bathymetryfile";
        public const string ObsCrsFile = "crsfile";
        public const string ExtForceFile = "extforcefile";
        public const string BndExtForceFile = "extforcefilenew";
        public const string FlowGeomFile = "glowgeomfile";
        public const string HisInterval = "hisinterval";
        public const string LandBoundaryFile = "landboundaryfile";
        public const string DryPointsFile = "drypointsfile";
        public const string EnclosureFile = "gridenclosurefile";
        public const string RoofAreaFile = "roofsfile";
        public const string GulliesFile = "Gulliesfile";
        public const string ManholeFile = "manholefile";
        public const string MapInterval = "mapinterval";
        public const string ClassMapInterval = "classmapinterval";
        public const string NetFile = "netfile";
        public const string StorageNodeFile = "StorageNodeFile";
        public const string BranchFile = "BranchFile";
        public const string NodeFile = "nodefile";
        public const string ObsFile = "obsfile";
        public const string OutDir = "outputdir";
        public const string PartitionFile = "partitionfile";
        public const string CrossDefFile = "crossdeffile";
        public const string CrossLocFile = "crosslocfile";
        public const string FrictFile = "FrictFile";
        public const string RoughnessFile = "roughnessfiles";
        public const string RestartFile = "restartfile";
        public const string RstInterval = "rstinterval";
        public const string StatsInterVal = "statsinterval";
        public const string ThinDamFile = "thindamfile";
        public const string MapFile = "mapfile";
        public const string HisFile = "hisfile";
        public const string FixedWeirFile = "fixedweirfile";
        public const string BridgePillarFile = "pillarfile";
        public const string FixedWeirScheme = "fixedweirscheme";
        public const string LeveeBreachFile = "leveebreachfile";
        public const string StructuresFile = "structurefile";
        public const string Kmx = "kmx";
        public const string DtInit = "dtinit";
        public const string DtUser = "dtuser";
        public const string DtMax = "dtmax";
        public const string RefDate = "refdate";
        public const string TStart = "tstart";
        public const string TStop = "tstop";
        public const string TZone = "tzone";
        public const string Tunit = "tunit";
        public const string Version = "version";
        public const string GuiVersion = "guiversion";
        public const string WaqInterval = "waqinterval";
        public const string WaterLevIniFile = "waterlevinifile";
        public const string XLSInterval = "xlsinterval";
        public const string Bedlevuni = "Bedlevuni";
        public const string UniFrictCoef = "UnifFrictCoef";
        public const string UniHorEdViscCoef = "Vicouv";
        public const string ICdtyp = "icdtyp";
        public const string Cdbreakpoints = "cdbreakpoints";
        public const string Windspeedbreakpoints = "windspeedbreakpoints";
        public const string UseSalinity = "salinity";
        public const string Limtypsa = "limtypsa";
        public const string InitialSalinity = "InitialSalinity";
        public const string Temperature = "temperature";
        public const string FrictionType = "UnifFrictType";
        public const string InfiltrationModel = "Infiltrationmodel";
        public const string WaveModelNr = "Wavemodelnr";
        public const string WaterLevIni = "WaterLevIni";
        public const string IniFieldFile = "IniFieldFile";
        public const string COMFile = "comfile";
        public const string SolverType = "Icgsolver";
        public const string SecondaryFlow = "SecondaryFlow";
        public const string Irov = "Irov";
        public const string ISlope = "ISlope";
        public const string IHidExp = "IHidExp";
        public const string morphology = "morphology";
        public const string sediment = "sediment";
        public const string MorFile = "MorFile";
        public const string BcmFile = "BcFil";
        public const string SedFile = "SedFile";
        public const string BedlevType = "bedlevtype";
        public const string MapFormat = "MapFormat";
        public const string RenumberFlowNodes = "RenumberFlowNodes";
        public const string Conveyance2d = "conveyance2d";
        public const string Wrishp_crs = "Wrishp_crs";
        public const string Wrishp_weir = "Wrishp_weir";
        public const string Wrishp_gate = "Wrishp_gate";
        public const string Wrishp_fxw = "Wrishp_fxw";
        public const string Wrishp_thd = "Wrishp_thd";
        public const string Wrishp_obs = "Wrishp_obs";
        public const string Wrishp_emb = "Wrishp_emb";
        public const string Wrishp_dryarea = "Wrishp_dryarea";
        public const string Wrishp_enc = "Wrishp_enc";
        public const string Wrishp_src = "Wrishp_src";
        public const string Wrishp_pump = "Wrishp_pump";
        public const string TrtRou = "TrtRou";
        public const string TrtDef = "TrtDef";
        public const string TrtL = "TrtL";
        public const string DtTrt = "DtTrt";
        public const string Dxmin1D = "Dxmin1D";
        public const string WaqOutputDir = "WAQOutputDir";
        public const string UseCaching = "UseCaching";
        public const string FouFile = "FouFile";
        public const string UseVolumeTables = "useVolumeTables";
        public const string UseVolumeTablesFile = "useVolumeTablesFile";
    }


    public static class GuiProperties
    {
        public const string GUIonly = "GUIOnly"; // recognize GUI group name when writing writing MDU

        public const string StartTime = "starttime";
        public const string StopTime = "stoptime";

        public const string WriteHisFile = "writehisfile";
        public const string HisOutputDeltaT = "hisoutputdeltat";
        public const string SpecifyHisStart = "specifyhisstart";
        public const string HisOutputStartTime = "hisoutputstarttime";
        public const string SpecifyHisStop = "specifyhisstop";
        public const string HisOutputStopTime = "hisoutputstoptime";

        public const string WriteMapFile = "writemapfile";
        public const string MapOutputDeltaT = "mapoutputdeltat";
        public const string SpecifyMapStart = "specifymapstart";
        public const string MapOutputStartTime = "mapoutputstarttime";
        public const string SpecifyMapStop = "specifymapstop";
        public const string MapOutputStopTime = "mapoutputstoptime";

        public const string WriteRstFile = "writerstfile";
        public const string RstOutputDeltaT = "rstoutputdeltat";
        public const string SpecifyRstStart = "specifyrststart";
        public const string RstOutputStartTime = "rstoutputstarttime";
        public const string SpecifyRstStop = "specifyrststop";
        public const string RstOutputStopTime = "rstoutputstoptime";
        public const string SpecifyWaqOutputInterval = "SpecifyWaqOutputInterval";
        public const string WaqOutputDeltaT = "WaqOutputDeltaT";
        public const string SpecifyWaqOutputStartTime = "SpecifyWaqOutputStartTime";
        public const string WaqOutputStartTime = "WaqOutputStartTime";
        public const string SpecifyWaqOutputStopTime = "SpecifyWaqOutputStopTime";
        public const string WaqOutputStopTime = "WaqOutputStopTime";
        public const string UseMorSed = "UseMorSed";
        public const string WriteSnappedFeatures = "WriteSnappedFeatures";

        public const string TargetMduPath = "TargetMduPath";

        public const string UnifFrictCoefChannels = "UnifFrictCoefChannels";
        public const string UnifFrictTypeChannels = "UnifFrictTypeChannels";

        public const string InitialConditionGlobalValue1D = "InitialConditionGlobalValue1D";
        public const string InitialConditionGlobalQuantity1D = "InitialConditionGlobalQuantity1D";
        public const string InitialConditionGlobalQuantity2D = "InitialConditionGlobalQuantity2D";

        public const string WriteClassMapFile = "writeclassmapfile";
        public const string ClassMapOutputDeltaT = "classmapoutputdeltat";

        public const string WriteFouFile = "WriteFouFile";
    }


    public enum MapFormatType
    {
        Unknown = 0,
        NetCdf = 1,
        Tecplot = 2,
        Both = 3,
        Ugrid = 4
    }

    public enum Conveyance2DType
    {
        RisHU = -1,
        RisH = 0,
        RisAperP = 1,
        Kisanalytic1Dconv = 2,
        Kisanalytic2Dconv = 3
    }
}