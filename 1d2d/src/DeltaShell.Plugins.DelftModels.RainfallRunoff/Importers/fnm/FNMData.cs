using System;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers.fnm
{
    /// <summary>
    /// <see cref="FnmData"/> defines all the properties of a .fnm file.
    /// </summary>
    /// <remarks>
    /// See the Rainfall Runoff manual for more details about the properties.
    /// </remarks>
    public sealed class FnmData
    {
        private const int nProperties = 123;
        private readonly string[] properties;

        public FnmData(params string[] properties)
        {
            int nExtraElements = Math.Max(nProperties - properties.Length, 0);
            this.properties =
                properties.Concat(Enumerable.Repeat("", nExtraElements))
                          .Take(nProperties)
                          .ToArray();
        }

        // Order of the properties follows the fnm file.
        public string ControlFile => properties[0];
        public string NodeData => properties[1];
        public string LinkData => properties[2];
        public string OpenWaterData => properties[3];
        public string PavedAreaGeneral => properties[4];
        public string PavedAreaStorage => properties[5];
        public string PavedAreaDwa => properties[6];
        public string PavedAreaSewerPumpCapacity => properties[7];
        public string Boundaries => properties[8];
        public string Pluvius => properties[9];
        public string PluviusGeneral => properties[10];
        public string Kasklasse => properties[11];
        public string BuiFile => properties[12];
        public string VerdampingsFile => properties[13];
        public string UnpavedAreaGeneral => properties[14];
        public string UnpavedAreaStorage => properties[15];
        public string GreenHouseAreaInitialisation => properties[16];
        public string GreenHouseAreaUsageData => properties[17];
        public string CropFactors => properties[18];
        public string TableBergingscoef => properties[19];
        public string UnpavedAlphaFactorDefinitions => properties[20];
        public string RunLogFile => properties[21];
        public string SchemaOverview => properties[22];
        public string OutputResultsPavedArea => properties[23];
        public string OutputResultsUnpavedArea => properties[24];
        public string OutputResultsGreenHouses => properties[25];
        public string OutputResultsOpenWater => properties[26];
        public string OutputResultsStructures => properties[27];
        public string OutputResultsBoundaries => properties[28];
        public string OutputResultsPluvius => properties[29];
        public string InfiltrationDefinitionsUnpaved => properties[30];
        public string RunDebugFile => properties[31];
        public string UnpavedSeepage => properties[32];
        public string UnpavedInitialValuesTables => properties[33];
        public string GreenHouseGeneral => properties[34];
        public string GreenHouseRoofStorage => properties[35];
        public string PluviusSewageEntry => properties[36];
        public string InputVariableGaugesOnEdgeNodes => properties[37];
        public string InputSaltData => properties[38];
        public string InputCropFactorsOpenWater => properties[39];
        public string RestartFileInput => properties[40];
        public string RestartFileOutput => properties[41];
        public string BinaryFileInput => properties[42];
        public string SacramentoInput => properties[43];
        public string OutputFlowRatesEdgeNodes => properties[44];
        public string OutputSaltConcentrationEdge => properties[45];
        public string OutputSaltExportation => properties[46];
        public string GreenhouseSiloDefinitions => properties[47];
        public string OpenWaterGeneral => properties[48];
        public string OpenWaterSeepageDefinitions => properties[49];
        public string OpenWaterTablesTargetLevels => properties[50];
        public string StructureGeneral => properties[51];
        public string StructureDefinitions => properties[52];
        public string ControllerDefinitions => properties[53];
        public string StructureTables => properties[54];
        public string BoundaryData => properties[55];
        public string BoundaryTables => properties[56];
        public string SobekLocationRtc => properties[57];
        public string WwtpData => properties[58];
        public string WwtpTables => properties[59];
        public string IndustryGeneral => properties[60];
        public string MappixPavedAreaSewageStorageName => properties[61];
        public string MappixPavedAreaFlowRatesName => properties[62];
        public string MappixUnpavedAreaFlowRatesName => properties[63];
        public string MappixGroundWaterLevelsName => properties[64];
        public string MappixGreenHouseBassinsStorageName => properties[65];
        public string MappixGreenHouseBassinsResultsName => properties[66];
        public string MappixOpenWaterDetailsName => properties[67];
        public string MappixExceedanceTimeReferenceLevelsName => properties[68];
        public string MappixFlowRatesOverStructuresName => properties[69];
        public string MappixFlowRatesToEdgeName => properties[70];
        public string MappixPluviusMaxSewageStorageName => properties[71];
        public string MappixPluviusMaxFlowRatesName => properties[72];
        public string MappixBalanceName => properties[73];
        public string MappixCumulativeBalanceName => properties[74];
        public string MappixSaltConcentrationsName => properties[75];
        public string IndustryTables => properties[76];
        public string Maalstop => properties[77];
        public string TimeSeriesTemperature => properties[78];
        public string TimeSeriesRunoff => properties[79];
        public string DischargesTotalsAtEdgeNodes => properties[80];
        public string LanguageFile => properties[81];
        public string OwVolume => properties[82];
        public string OwLevels => properties[83];
        public string BalanceFile => properties[84];
        public string His3BAreaLength => properties[85];
        public string His3BStructureData => properties[86];
        public string HisRRRunoff => properties[87];
        public string HisSacramento => properties[88];
        public string HisRwzi => properties[89];
        public string HisIndustry => properties[90];
        public string CtrlIni => properties[91];
        public string RootCapsimInputFile => properties[92];
        public string UnsaCapsimInputFile => properties[93];
        public string CapsimMessageFile => properties[94];
        public string CapsimDebugFile => properties[95];
        public string Restart1Hour => properties[96];
        public string Restart12Hours => properties[97];
        public string RRReady => properties[98];
        public string NwrwAreas => properties[99];
        public string LinkFlows => properties[100];
        public string ModflowRR => properties[101];
        public string RRModflow => properties[102];
        public string RRWlmBalance => properties[103];
        public string SacramentoAsciiOutput => properties[104];
        public string NwrwInputDwaTable => properties[105];
        public string RRBalance => properties[106];
        public string GreenHouseClasses => properties[107];
        public string GreenHouseInit => properties[108];
        public string GreenHouseUsage => properties[109];
        public string CropFactor => properties[110];
        public string CropOw => properties[111];
        public string SoilData => properties[112];
        public string DioConfigIniFile => properties[113];
        public string BuiFileForContinuousCalculationSeries => properties[114];
        public string NwrwOutput => properties[115];
        public string RRRoutingLinkDefinitions => properties[116];
        public string CellInputFile => properties[117];
        public string CellOutputFile => properties[118];
        public string RRSimulateLogFile => properties[119];
        public string RtcCouplingWqSalt => properties[120];
        public string RRBoundaryConditionsSobek3 => properties[121];
        public string RRAsciiRestartOpenda => properties[122];
    }
}