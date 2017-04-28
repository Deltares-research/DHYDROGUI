using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition
{
    public sealed class WaterFlowFMPropertyDefinition : ModelPropertyDefinition
    {
        public string MduPropertyName
        {
            get { return base.FilePropertyName; }
            set { base.FilePropertyName = value; }
        }
    }

    public static class KnownProperties
    {
        public const string BathymetryFile = "bathymetryfile";
        public const string ObsCrsFile = "crsfile";
        public const string ExtForceFile = "extforcefile";
        public const string BndExtForceFile = "extforcefilenew";
        public const string FlowGeomFile = "glowgeomfile";
        public const string HisInterval = "hisinterval";
        public const string LandBoundaryFile = "landboundaryfile";
        public const string DryPointsFile = "drypointsfile";
        public const string ManholeFile = "manholefile";
        public const string MapInterval = "mapinterval";
        public const string NetFile = "netfile";
        public const string ObsFile = "obsfile";
        public const string OutDir = "outputdir";
        public const string PartitionFile = "partitionfile";
        public const string ProfdefFile = "profdeffile";
        public const string ProflocFile = "proflocfile";
        public const string RestartFile = "restartfile";
        public const string RstInterval = "rstinterval";
        public const string StatsInterVal = "statsinterval";
        public const string ThinDamFile = "thindamfile";
        public const string MapFile__Obsolete = "mapfile";
        public const string HisFile__Obsolete = "hisfile";
        public const string FixedWeirFile = "fixedweirfile";
        public const string StructuresFile = "structurefile";
        public const string Kmx = "kmx";
        public const string DtInit = "dtinit";
        public const string DtUser = "dtuser";
        public const string DtMax = "dtmax";
        public const string RefDate = "refdate";
        public const string TStart = "tstart";
        public const string TStop = "tstop";
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
        public const string WaveModelNr = "Wavemodelnr";
        public const string WaterLevIni = "WaterLevIni";
        public const string COMFile = "comfile";
        public const string SolverType = "Icgsolver";
        public const string SecondaryFlow = "SecondaryFlow";
        public const string Irov = "Irov";
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

        public const string UseTemperature = "UseTemperature";
    }
}