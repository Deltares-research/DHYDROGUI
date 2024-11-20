using System.Linq;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers.fnm;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Importers.fnm
{
    [TestFixture]
    public class FnmDataTest
    {
        private static string[] GetInputData() =>
            new[] {
                "delft_3b.ini",
                "3b_nod.tp",
                "3b_link.tp",
                "3brunoff.tp",
                "paved.3b",
                "paved.sto",
                "paved.dwa",
                "paved.tbl",
                "pluvius.dwa",
                "pluvius.3b",
                "pluvius.alg",
                "kasklass",
                "T1.bui",
                "T1.evp",
                "unpaved.3b",
                "unpaved.sto",
                "kasinit",
                "kasgebr",
                "cropfact",
                "bergcoef",
                "unpaved.alf",
                "sobek_3b.log",
                "3b_gener.out",
                "paved.out",
                "unpaved.out",
                "grnhous.out",
                "openwate.out",
                "struct3b.out",
                "bound3b.out",
                "pluvius.out",
                "unpaved.inf",
                "sobek_3b.dbg",
                "unpaved.sep",
                "unpaved.tbl",
                "greenhse.3b",
                "greenhse.rf",
                "runoff.out",
                "sbk_rtc.his",
                "salt.3b",
                "crop_ow.prn",
                "RSRR_IN",
                "RSRR_OUT",
                "3b_input.bin",
                "sacrmnto.3b",
                "aanvoer.abr",
                "saltbnd.out",
                "salt.out",
                "greenhse.sil",
                "openwate.3b",
                "openwate.sep",
                "openwate.tbl",
                "struct3b.dat",
                "struct3b.def",
                "contr3b.def",
                "struct3b.tbl",
                "bound3b.3b",
                "bound3b.tbl",
                "sbk_loc.rtc",
                "wwtp.3b",
                "wwtp.tbl",
                "industry.3b",
                "pvstordt.his",
                "pvflowdt.his",
                "upflowdt.his",
                "upgwlvdt.his",
                "grnstrdt.his",
                "grnflodt.his",
                "ow_lvldt.his",
                "ow_excdt.his",
                "strflodt.his",
                "bndflodt.his",
                "plvstrdt.his",
                "plvflodt.his",
                "balansdt.his",
                "cumbaldt.his",
                "saltdt.his",
                "industry.tbl",
                "rtc_3b.his",
                "default.tmp",
                "rnff.#",
                "bndfltot.his",
                "sobek_3b.lng",
                "ow_vol.his",
                "ow_level.his",
                "3b_bal.out",
                "3bareas.his",
                "3bstrdim.his",
                "rrrunoff.his",
                "sacrmnto.his",
                "wwtpdt.his",
                "industdt.his",
                "ctrl.ini",
                "root_sim.inp",
                "unsa_sim.inp",
                "capsim.msg",
                "capsim.dbg",
                "restart1.out",
                "restart12.out",
                "RR-ready",
                "NwrwArea.His",
                "3blinks.his",
                "modflow_rr.His",
                "rr_modflow.His",
                "rr_wlmbal.His",
                "sacrmnto.out",
                "pluvius.tbl",
                "rrbalans.his",
                "KasKlasData.dat",
                "KasInitData.dat",
                "KasGebrData.dat",
                "CropData.dat",
                "CropOWData.dat",
                "SoilData.dat",
                "dioconfig.ini",
                "NWRWCONT.#",
                "NwrwSys.His",
                "3b_rout.3b",
                "3b_cel.3b",
                "3b_cel.his",
                "sobek3b_progress.txt",
                "wqrtc.his",
                "BoundaryConditions.bc",
                "ASCIIRestartOpenDA.txt",
            };

        [Test]
        public void Constructor_ExpectedResults()
        {
            string[] input = GetInputData();

            // Call
            var data = new FnmData(input);

            Assert.That(data.ControlFile, Is.EqualTo("delft_3b.ini"));
            Assert.That(data.NodeData, Is.EqualTo("3b_nod.tp"));
            Assert.That(data.LinkData, Is.EqualTo("3b_link.tp"));
            Assert.That(data.OpenWaterData, Is.EqualTo("3brunoff.tp"));
            Assert.That(data.PavedAreaGeneral, Is.EqualTo("paved.3b"));
            Assert.That(data.PavedAreaStorage, Is.EqualTo("paved.sto"));
            Assert.That(data.PavedAreaDwa, Is.EqualTo("paved.dwa"));
            Assert.That(data.PavedAreaSewerPumpCapacity, Is.EqualTo("paved.tbl"));
            Assert.That(data.Boundaries, Is.EqualTo("pluvius.dwa"));
            Assert.That(data.Pluvius, Is.EqualTo("pluvius.3b"));
            Assert.That(data.PluviusGeneral, Is.EqualTo("pluvius.alg"));
            Assert.That(data.Kasklasse, Is.EqualTo("kasklass"));
            Assert.That(data.BuiFile, Is.EqualTo("T1.bui"));
            Assert.That(data.VerdampingsFile, Is.EqualTo("T1.evp"));
            Assert.That(data.UnpavedAreaGeneral, Is.EqualTo("unpaved.3b"));
            Assert.That(data.UnpavedAreaStorage, Is.EqualTo("unpaved.sto"));
            Assert.That(data.GreenHouseAreaInitialisation, Is.EqualTo("kasinit"));
            Assert.That(data.GreenHouseAreaUsageData, Is.EqualTo("kasgebr"));
            Assert.That(data.CropFactors, Is.EqualTo("cropfact"));
            Assert.That(data.TableBergingscoef, Is.EqualTo("bergcoef"));
            Assert.That(data.UnpavedAlphaFactorDefinitions, Is.EqualTo("unpaved.alf"));
            Assert.That(data.RunLogFile, Is.EqualTo("sobek_3b.log"));
            Assert.That(data.SchemaOverview, Is.EqualTo("3b_gener.out"));
            Assert.That(data.OutputResultsPavedArea, Is.EqualTo("paved.out"));
            Assert.That(data.OutputResultsUnpavedArea, Is.EqualTo("unpaved.out"));
            Assert.That(data.OutputResultsGreenHouses, Is.EqualTo("grnhous.out"));
            Assert.That(data.OutputResultsOpenWater, Is.EqualTo("openwate.out"));
            Assert.That(data.OutputResultsStructures, Is.EqualTo("struct3b.out"));
            Assert.That(data.OutputResultsBoundaries, Is.EqualTo("bound3b.out"));
            Assert.That(data.OutputResultsPluvius, Is.EqualTo("pluvius.out"));
            Assert.That(data.InfiltrationDefinitionsUnpaved, Is.EqualTo("unpaved.inf"));
            Assert.That(data.RunDebugFile, Is.EqualTo("sobek_3b.dbg"));
            Assert.That(data.UnpavedSeepage, Is.EqualTo("unpaved.sep"));
            Assert.That(data.UnpavedInitialValuesTables, Is.EqualTo("unpaved.tbl"));
            Assert.That(data.GreenHouseGeneral, Is.EqualTo("greenhse.3b"));
            Assert.That(data.GreenHouseRoofStorage, Is.EqualTo("greenhse.rf"));
            Assert.That(data.PluviusSewageEntry, Is.EqualTo("runoff.out"));
            Assert.That(data.InputVariableGaugesOnEdgeNodes, Is.EqualTo("sbk_rtc.his"));
            Assert.That(data.InputSaltData, Is.EqualTo("salt.3b"));
            Assert.That(data.InputCropFactorsOpenWater, Is.EqualTo("crop_ow.prn"));
            Assert.That(data.RestartFileInput, Is.EqualTo("RSRR_IN"));
            Assert.That(data.RestartFileOutput, Is.EqualTo("RSRR_OUT"));
            Assert.That(data.BinaryFileInput, Is.EqualTo("3b_input.bin"));
            Assert.That(data.SacramentoInput, Is.EqualTo("sacrmnto.3b"));
            Assert.That(data.OutputFlowRatesEdgeNodes, Is.EqualTo("aanvoer.abr"));
            Assert.That(data.OutputSaltConcentrationEdge, Is.EqualTo("saltbnd.out"));
            Assert.That(data.OutputSaltExportation, Is.EqualTo("salt.out"));
            Assert.That(data.GreenhouseSiloDefinitions, Is.EqualTo("greenhse.sil"));
            Assert.That(data.OpenWaterGeneral, Is.EqualTo("openwate.3b"));
            Assert.That(data.OpenWaterSeepageDefinitions, Is.EqualTo("openwate.sep"));
            Assert.That(data.OpenWaterTablesTargetLevels, Is.EqualTo("openwate.tbl"));
            Assert.That(data.StructureGeneral, Is.EqualTo("struct3b.dat"));
            Assert.That(data.StructureDefinitions, Is.EqualTo("struct3b.def"));
            Assert.That(data.ControllerDefinitions, Is.EqualTo("contr3b.def"));
            Assert.That(data.StructureTables, Is.EqualTo("struct3b.tbl"));
            Assert.That(data.BoundaryData, Is.EqualTo("bound3b.3b"));
            Assert.That(data.BoundaryTables, Is.EqualTo("bound3b.tbl"));
            Assert.That(data.SobekLocationRtc, Is.EqualTo("sbk_loc.rtc"));
            Assert.That(data.WwtpData, Is.EqualTo("wwtp.3b"));
            Assert.That(data.WwtpTables, Is.EqualTo("wwtp.tbl"));
            Assert.That(data.IndustryGeneral, Is.EqualTo("industry.3b"));
            Assert.That(data.MappixPavedAreaSewageStorageName, Is.EqualTo("pvstordt.his"));
            Assert.That(data.MappixPavedAreaFlowRatesName, Is.EqualTo("pvflowdt.his"));
            Assert.That(data.MappixUnpavedAreaFlowRatesName, Is.EqualTo("upflowdt.his"));
            Assert.That(data.MappixGroundWaterLevelsName, Is.EqualTo("upgwlvdt.his"));
            Assert.That(data.MappixGreenHouseBassinsStorageName, Is.EqualTo("grnstrdt.his"));
            Assert.That(data.MappixGreenHouseBassinsResultsName, Is.EqualTo("grnflodt.his"));
            Assert.That(data.MappixOpenWaterDetailsName, Is.EqualTo("ow_lvldt.his"));
            Assert.That(data.MappixExceedanceTimeReferenceLevelsName, Is.EqualTo("ow_excdt.his"));
            Assert.That(data.MappixFlowRatesOverStructuresName, Is.EqualTo("strflodt.his"));
            Assert.That(data.MappixFlowRatesToEdgeName, Is.EqualTo("bndflodt.his"));
            Assert.That(data.MappixPluviusMaxSewageStorageName, Is.EqualTo("plvstrdt.his"));
            Assert.That(data.MappixPluviusMaxFlowRatesName, Is.EqualTo("plvflodt.his"));
            Assert.That(data.MappixBalanceName, Is.EqualTo("balansdt.his"));
            Assert.That(data.MappixCumulativeBalanceName, Is.EqualTo("cumbaldt.his"));
            Assert.That(data.MappixSaltConcentrationsName, Is.EqualTo("saltdt.his"));
            Assert.That(data.IndustryTables, Is.EqualTo("industry.tbl"));
            Assert.That(data.Maalstop, Is.EqualTo("rtc_3b.his"));
            Assert.That(data.TimeSeriesTemperature, Is.EqualTo("default.tmp"));
            Assert.That(data.TimeSeriesRunoff, Is.EqualTo("rnff.#"));
            Assert.That(data.DischargesTotalsAtEdgeNodes, Is.EqualTo("bndfltot.his"));
            Assert.That(data.LanguageFile, Is.EqualTo("sobek_3b.lng"));
            Assert.That(data.OwVolume, Is.EqualTo("ow_vol.his"));
            Assert.That(data.OwLevels, Is.EqualTo("ow_level.his"));
            Assert.That(data.BalanceFile, Is.EqualTo("3b_bal.out"));
            Assert.That(data.His3BAreaLength, Is.EqualTo("3bareas.his"));
            Assert.That(data.His3BStructureData, Is.EqualTo("3bstrdim.his"));
            Assert.That(data.HisRRRunoff, Is.EqualTo("rrrunoff.his"));
            Assert.That(data.HisSacramento, Is.EqualTo("sacrmnto.his"));
            Assert.That(data.HisRwzi, Is.EqualTo("wwtpdt.his"));
            Assert.That(data.HisIndustry, Is.EqualTo("industdt.his"));
            Assert.That(data.CtrlIni, Is.EqualTo("ctrl.ini"));
            Assert.That(data.RootCapsimInputFile, Is.EqualTo("root_sim.inp"));
            Assert.That(data.UnsaCapsimInputFile, Is.EqualTo("unsa_sim.inp"));
            Assert.That(data.CapsimMessageFile, Is.EqualTo("capsim.msg"));
            Assert.That(data.CapsimDebugFile, Is.EqualTo("capsim.dbg"));
            Assert.That(data.Restart1Hour, Is.EqualTo("restart1.out"));
            Assert.That(data.Restart12Hours, Is.EqualTo("restart12.out"));
            Assert.That(data.RRReady, Is.EqualTo("RR-ready"));
            Assert.That(data.NwrwAreas, Is.EqualTo("NwrwArea.His"));
            Assert.That(data.LinkFlows, Is.EqualTo("3blinks.his"));
            Assert.That(data.ModflowRR, Is.EqualTo("modflow_rr.His"));
            Assert.That(data.RRModflow, Is.EqualTo("rr_modflow.His"));
            Assert.That(data.RRWlmBalance, Is.EqualTo("rr_wlmbal.His"));
            Assert.That(data.SacramentoAsciiOutput, Is.EqualTo("sacrmnto.out"));
            Assert.That(data.NwrwInputDwaTable, Is.EqualTo("pluvius.tbl"));
            Assert.That(data.RRBalance, Is.EqualTo("rrbalans.his"));
            Assert.That(data.GreenHouseClasses, Is.EqualTo("KasKlasData.dat"));
            Assert.That(data.GreenHouseInit, Is.EqualTo("KasInitData.dat"));
            Assert.That(data.GreenHouseUsage, Is.EqualTo("KasGebrData.dat"));
            Assert.That(data.CropFactor, Is.EqualTo("CropData.dat"));
            Assert.That(data.CropOw, Is.EqualTo("CropOWData.dat"));
            Assert.That(data.SoilData, Is.EqualTo("SoilData.dat"));
            Assert.That(data.DioConfigIniFile, Is.EqualTo("dioconfig.ini"));
            Assert.That(data.BuiFileForContinuousCalculationSeries, Is.EqualTo("NWRWCONT.#"));
            Assert.That(data.NwrwOutput, Is.EqualTo("NwrwSys.His"));
            Assert.That(data.RRRoutingLinkDefinitions, Is.EqualTo("3b_rout.3b"));
            Assert.That(data.CellInputFile, Is.EqualTo("3b_cel.3b"));
            Assert.That(data.CellOutputFile, Is.EqualTo("3b_cel.his"));
            Assert.That(data.RRSimulateLogFile, Is.EqualTo("sobek3b_progress.txt"));
            Assert.That(data.RtcCouplingWqSalt, Is.EqualTo("wqrtc.his"));
            Assert.That(data.RRBoundaryConditionsSobek3, Is.EqualTo("BoundaryConditions.bc"));
            Assert.That(data.RRAsciiRestartOpenda, Is.EqualTo("ASCIIRestartOpenDA.txt"));
        }

        [Test]
        public void Constructor_TooManyArguments_ExcessIsDropped()
        {
            string[] input = GetInputData().Concat(Enumerable.Repeat("🧙", 10)).ToArray();

            // Call
            var data = new FnmData(input);

            Assert.That(data.ControlFile, Is.EqualTo("delft_3b.ini"));
            Assert.That(data.NodeData, Is.EqualTo("3b_nod.tp"));
            Assert.That(data.LinkData, Is.EqualTo("3b_link.tp"));
            Assert.That(data.OpenWaterData, Is.EqualTo("3brunoff.tp"));
            Assert.That(data.PavedAreaGeneral, Is.EqualTo("paved.3b"));
            Assert.That(data.PavedAreaStorage, Is.EqualTo("paved.sto"));
            Assert.That(data.PavedAreaDwa, Is.EqualTo("paved.dwa"));
            Assert.That(data.PavedAreaSewerPumpCapacity, Is.EqualTo("paved.tbl"));
            Assert.That(data.Boundaries, Is.EqualTo("pluvius.dwa"));
            Assert.That(data.Pluvius, Is.EqualTo("pluvius.3b"));
            Assert.That(data.PluviusGeneral, Is.EqualTo("pluvius.alg"));
            Assert.That(data.Kasklasse, Is.EqualTo("kasklass"));
            Assert.That(data.BuiFile, Is.EqualTo("T1.bui"));
            Assert.That(data.VerdampingsFile, Is.EqualTo("T1.evp"));
            Assert.That(data.UnpavedAreaGeneral, Is.EqualTo("unpaved.3b"));
            Assert.That(data.UnpavedAreaStorage, Is.EqualTo("unpaved.sto"));
            Assert.That(data.GreenHouseAreaInitialisation, Is.EqualTo("kasinit"));
            Assert.That(data.GreenHouseAreaUsageData, Is.EqualTo("kasgebr"));
            Assert.That(data.CropFactors, Is.EqualTo("cropfact"));
            Assert.That(data.TableBergingscoef, Is.EqualTo("bergcoef"));
            Assert.That(data.UnpavedAlphaFactorDefinitions, Is.EqualTo("unpaved.alf"));
            Assert.That(data.RunLogFile, Is.EqualTo("sobek_3b.log"));
            Assert.That(data.SchemaOverview, Is.EqualTo("3b_gener.out"));
            Assert.That(data.OutputResultsPavedArea, Is.EqualTo("paved.out"));
            Assert.That(data.OutputResultsUnpavedArea, Is.EqualTo("unpaved.out"));
            Assert.That(data.OutputResultsGreenHouses, Is.EqualTo("grnhous.out"));
            Assert.That(data.OutputResultsOpenWater, Is.EqualTo("openwate.out"));
            Assert.That(data.OutputResultsStructures, Is.EqualTo("struct3b.out"));
            Assert.That(data.OutputResultsBoundaries, Is.EqualTo("bound3b.out"));
            Assert.That(data.OutputResultsPluvius, Is.EqualTo("pluvius.out"));
            Assert.That(data.InfiltrationDefinitionsUnpaved, Is.EqualTo("unpaved.inf"));
            Assert.That(data.RunDebugFile, Is.EqualTo("sobek_3b.dbg"));
            Assert.That(data.UnpavedSeepage, Is.EqualTo("unpaved.sep"));
            Assert.That(data.UnpavedInitialValuesTables, Is.EqualTo("unpaved.tbl"));
            Assert.That(data.GreenHouseGeneral, Is.EqualTo("greenhse.3b"));
            Assert.That(data.GreenHouseRoofStorage, Is.EqualTo("greenhse.rf"));
            Assert.That(data.PluviusSewageEntry, Is.EqualTo("runoff.out"));
            Assert.That(data.InputVariableGaugesOnEdgeNodes, Is.EqualTo("sbk_rtc.his"));
            Assert.That(data.InputSaltData, Is.EqualTo("salt.3b"));
            Assert.That(data.InputCropFactorsOpenWater, Is.EqualTo("crop_ow.prn"));
            Assert.That(data.RestartFileInput, Is.EqualTo("RSRR_IN"));
            Assert.That(data.RestartFileOutput, Is.EqualTo("RSRR_OUT"));
            Assert.That(data.BinaryFileInput, Is.EqualTo("3b_input.bin"));
            Assert.That(data.SacramentoInput, Is.EqualTo("sacrmnto.3b"));
            Assert.That(data.OutputFlowRatesEdgeNodes, Is.EqualTo("aanvoer.abr"));
            Assert.That(data.OutputSaltConcentrationEdge, Is.EqualTo("saltbnd.out"));
            Assert.That(data.OutputSaltExportation, Is.EqualTo("salt.out"));
            Assert.That(data.GreenhouseSiloDefinitions, Is.EqualTo("greenhse.sil"));
            Assert.That(data.OpenWaterGeneral, Is.EqualTo("openwate.3b"));
            Assert.That(data.OpenWaterSeepageDefinitions, Is.EqualTo("openwate.sep"));
            Assert.That(data.OpenWaterTablesTargetLevels, Is.EqualTo("openwate.tbl"));
            Assert.That(data.StructureGeneral, Is.EqualTo("struct3b.dat"));
            Assert.That(data.StructureDefinitions, Is.EqualTo("struct3b.def"));
            Assert.That(data.ControllerDefinitions, Is.EqualTo("contr3b.def"));
            Assert.That(data.StructureTables, Is.EqualTo("struct3b.tbl"));
            Assert.That(data.BoundaryData, Is.EqualTo("bound3b.3b"));
            Assert.That(data.BoundaryTables, Is.EqualTo("bound3b.tbl"));
            Assert.That(data.SobekLocationRtc, Is.EqualTo("sbk_loc.rtc"));
            Assert.That(data.WwtpData, Is.EqualTo("wwtp.3b"));
            Assert.That(data.WwtpTables, Is.EqualTo("wwtp.tbl"));
            Assert.That(data.IndustryGeneral, Is.EqualTo("industry.3b"));
            Assert.That(data.MappixPavedAreaSewageStorageName, Is.EqualTo("pvstordt.his"));
            Assert.That(data.MappixPavedAreaFlowRatesName, Is.EqualTo("pvflowdt.his"));
            Assert.That(data.MappixUnpavedAreaFlowRatesName, Is.EqualTo("upflowdt.his"));
            Assert.That(data.MappixGroundWaterLevelsName, Is.EqualTo("upgwlvdt.his"));
            Assert.That(data.MappixGreenHouseBassinsStorageName, Is.EqualTo("grnstrdt.his"));
            Assert.That(data.MappixGreenHouseBassinsResultsName, Is.EqualTo("grnflodt.his"));
            Assert.That(data.MappixOpenWaterDetailsName, Is.EqualTo("ow_lvldt.his"));
            Assert.That(data.MappixExceedanceTimeReferenceLevelsName, Is.EqualTo("ow_excdt.his"));
            Assert.That(data.MappixFlowRatesOverStructuresName, Is.EqualTo("strflodt.his"));
            Assert.That(data.MappixFlowRatesToEdgeName, Is.EqualTo("bndflodt.his"));
            Assert.That(data.MappixPluviusMaxSewageStorageName, Is.EqualTo("plvstrdt.his"));
            Assert.That(data.MappixPluviusMaxFlowRatesName, Is.EqualTo("plvflodt.his"));
            Assert.That(data.MappixBalanceName, Is.EqualTo("balansdt.his"));
            Assert.That(data.MappixCumulativeBalanceName, Is.EqualTo("cumbaldt.his"));
            Assert.That(data.MappixSaltConcentrationsName, Is.EqualTo("saltdt.his"));
            Assert.That(data.IndustryTables, Is.EqualTo("industry.tbl"));
            Assert.That(data.Maalstop, Is.EqualTo("rtc_3b.his"));
            Assert.That(data.TimeSeriesTemperature, Is.EqualTo("default.tmp"));
            Assert.That(data.TimeSeriesRunoff, Is.EqualTo("rnff.#"));
            Assert.That(data.DischargesTotalsAtEdgeNodes, Is.EqualTo("bndfltot.his"));
            Assert.That(data.LanguageFile, Is.EqualTo("sobek_3b.lng"));
            Assert.That(data.OwVolume, Is.EqualTo("ow_vol.his"));
            Assert.That(data.OwLevels, Is.EqualTo("ow_level.his"));
            Assert.That(data.BalanceFile, Is.EqualTo("3b_bal.out"));
            Assert.That(data.His3BAreaLength, Is.EqualTo("3bareas.his"));
            Assert.That(data.His3BStructureData, Is.EqualTo("3bstrdim.his"));
            Assert.That(data.HisRRRunoff, Is.EqualTo("rrrunoff.his"));
            Assert.That(data.HisSacramento, Is.EqualTo("sacrmnto.his"));
            Assert.That(data.HisRwzi, Is.EqualTo("wwtpdt.his"));
            Assert.That(data.HisIndustry, Is.EqualTo("industdt.his"));
            Assert.That(data.CtrlIni, Is.EqualTo("ctrl.ini"));
            Assert.That(data.RootCapsimInputFile, Is.EqualTo("root_sim.inp"));
            Assert.That(data.UnsaCapsimInputFile, Is.EqualTo("unsa_sim.inp"));
            Assert.That(data.CapsimMessageFile, Is.EqualTo("capsim.msg"));
            Assert.That(data.CapsimDebugFile, Is.EqualTo("capsim.dbg"));
            Assert.That(data.Restart1Hour, Is.EqualTo("restart1.out"));
            Assert.That(data.Restart12Hours, Is.EqualTo("restart12.out"));
            Assert.That(data.RRReady, Is.EqualTo("RR-ready"));
            Assert.That(data.NwrwAreas, Is.EqualTo("NwrwArea.His"));
            Assert.That(data.LinkFlows, Is.EqualTo("3blinks.his"));
            Assert.That(data.ModflowRR, Is.EqualTo("modflow_rr.His"));
            Assert.That(data.RRModflow, Is.EqualTo("rr_modflow.His"));
            Assert.That(data.RRWlmBalance, Is.EqualTo("rr_wlmbal.His"));
            Assert.That(data.SacramentoAsciiOutput, Is.EqualTo("sacrmnto.out"));
            Assert.That(data.NwrwInputDwaTable, Is.EqualTo("pluvius.tbl"));
            Assert.That(data.RRBalance, Is.EqualTo("rrbalans.his"));
            Assert.That(data.GreenHouseClasses, Is.EqualTo("KasKlasData.dat"));
            Assert.That(data.GreenHouseInit, Is.EqualTo("KasInitData.dat"));
            Assert.That(data.GreenHouseUsage, Is.EqualTo("KasGebrData.dat"));
            Assert.That(data.CropFactor, Is.EqualTo("CropData.dat"));
            Assert.That(data.CropOw, Is.EqualTo("CropOWData.dat"));
            Assert.That(data.SoilData, Is.EqualTo("SoilData.dat"));
            Assert.That(data.DioConfigIniFile, Is.EqualTo("dioconfig.ini"));
            Assert.That(data.BuiFileForContinuousCalculationSeries, Is.EqualTo("NWRWCONT.#"));
            Assert.That(data.NwrwOutput, Is.EqualTo("NwrwSys.His"));
            Assert.That(data.RRRoutingLinkDefinitions, Is.EqualTo("3b_rout.3b"));
            Assert.That(data.CellInputFile, Is.EqualTo("3b_cel.3b"));
            Assert.That(data.CellOutputFile, Is.EqualTo("3b_cel.his"));
            Assert.That(data.RRSimulateLogFile, Is.EqualTo("sobek3b_progress.txt"));
            Assert.That(data.RtcCouplingWqSalt, Is.EqualTo("wqrtc.his"));
            Assert.That(data.RRBoundaryConditionsSobek3, Is.EqualTo("BoundaryConditions.bc"));
            Assert.That(data.RRAsciiRestartOpenda, Is.EqualTo("ASCIIRestartOpenDA.txt"));
        }

        [Test]
        public void Constructor_TooFewArguments_DefaultIsAdded()
        {
            string[] input = GetInputData();
            input = input.Take(input.Length - 10).ToArray();

            // Call
            var data = new FnmData(input);

            Assert.That(data.ControlFile, Is.EqualTo("delft_3b.ini"));
            Assert.That(data.NodeData, Is.EqualTo("3b_nod.tp"));
            Assert.That(data.LinkData, Is.EqualTo("3b_link.tp"));
            Assert.That(data.OpenWaterData, Is.EqualTo("3brunoff.tp"));
            Assert.That(data.PavedAreaGeneral, Is.EqualTo("paved.3b"));
            Assert.That(data.PavedAreaStorage, Is.EqualTo("paved.sto"));
            Assert.That(data.PavedAreaDwa, Is.EqualTo("paved.dwa"));
            Assert.That(data.PavedAreaSewerPumpCapacity, Is.EqualTo("paved.tbl"));
            Assert.That(data.Boundaries, Is.EqualTo("pluvius.dwa"));
            Assert.That(data.Pluvius, Is.EqualTo("pluvius.3b"));
            Assert.That(data.PluviusGeneral, Is.EqualTo("pluvius.alg"));
            Assert.That(data.Kasklasse, Is.EqualTo("kasklass"));
            Assert.That(data.BuiFile, Is.EqualTo("T1.bui"));
            Assert.That(data.VerdampingsFile, Is.EqualTo("T1.evp"));
            Assert.That(data.UnpavedAreaGeneral, Is.EqualTo("unpaved.3b"));
            Assert.That(data.UnpavedAreaStorage, Is.EqualTo("unpaved.sto"));
            Assert.That(data.GreenHouseAreaInitialisation, Is.EqualTo("kasinit"));
            Assert.That(data.GreenHouseAreaUsageData, Is.EqualTo("kasgebr"));
            Assert.That(data.CropFactors, Is.EqualTo("cropfact"));
            Assert.That(data.TableBergingscoef, Is.EqualTo("bergcoef"));
            Assert.That(data.UnpavedAlphaFactorDefinitions, Is.EqualTo("unpaved.alf"));
            Assert.That(data.RunLogFile, Is.EqualTo("sobek_3b.log"));
            Assert.That(data.SchemaOverview, Is.EqualTo("3b_gener.out"));
            Assert.That(data.OutputResultsPavedArea, Is.EqualTo("paved.out"));
            Assert.That(data.OutputResultsUnpavedArea, Is.EqualTo("unpaved.out"));
            Assert.That(data.OutputResultsGreenHouses, Is.EqualTo("grnhous.out"));
            Assert.That(data.OutputResultsOpenWater, Is.EqualTo("openwate.out"));
            Assert.That(data.OutputResultsStructures, Is.EqualTo("struct3b.out"));
            Assert.That(data.OutputResultsBoundaries, Is.EqualTo("bound3b.out"));
            Assert.That(data.OutputResultsPluvius, Is.EqualTo("pluvius.out"));
            Assert.That(data.InfiltrationDefinitionsUnpaved, Is.EqualTo("unpaved.inf"));
            Assert.That(data.RunDebugFile, Is.EqualTo("sobek_3b.dbg"));
            Assert.That(data.UnpavedSeepage, Is.EqualTo("unpaved.sep"));
            Assert.That(data.UnpavedInitialValuesTables, Is.EqualTo("unpaved.tbl"));
            Assert.That(data.GreenHouseGeneral, Is.EqualTo("greenhse.3b"));
            Assert.That(data.GreenHouseRoofStorage, Is.EqualTo("greenhse.rf"));
            Assert.That(data.PluviusSewageEntry, Is.EqualTo("runoff.out"));
            Assert.That(data.InputVariableGaugesOnEdgeNodes, Is.EqualTo("sbk_rtc.his"));
            Assert.That(data.InputSaltData, Is.EqualTo("salt.3b"));
            Assert.That(data.InputCropFactorsOpenWater, Is.EqualTo("crop_ow.prn"));
            Assert.That(data.RestartFileInput, Is.EqualTo("RSRR_IN"));
            Assert.That(data.RestartFileOutput, Is.EqualTo("RSRR_OUT"));
            Assert.That(data.BinaryFileInput, Is.EqualTo("3b_input.bin"));
            Assert.That(data.SacramentoInput, Is.EqualTo("sacrmnto.3b"));
            Assert.That(data.OutputFlowRatesEdgeNodes, Is.EqualTo("aanvoer.abr"));
            Assert.That(data.OutputSaltConcentrationEdge, Is.EqualTo("saltbnd.out"));
            Assert.That(data.OutputSaltExportation, Is.EqualTo("salt.out"));
            Assert.That(data.GreenhouseSiloDefinitions, Is.EqualTo("greenhse.sil"));
            Assert.That(data.OpenWaterGeneral, Is.EqualTo("openwate.3b"));
            Assert.That(data.OpenWaterSeepageDefinitions, Is.EqualTo("openwate.sep"));
            Assert.That(data.OpenWaterTablesTargetLevels, Is.EqualTo("openwate.tbl"));
            Assert.That(data.StructureGeneral, Is.EqualTo("struct3b.dat"));
            Assert.That(data.StructureDefinitions, Is.EqualTo("struct3b.def"));
            Assert.That(data.ControllerDefinitions, Is.EqualTo("contr3b.def"));
            Assert.That(data.StructureTables, Is.EqualTo("struct3b.tbl"));
            Assert.That(data.BoundaryData, Is.EqualTo("bound3b.3b"));
            Assert.That(data.BoundaryTables, Is.EqualTo("bound3b.tbl"));
            Assert.That(data.SobekLocationRtc, Is.EqualTo("sbk_loc.rtc"));
            Assert.That(data.WwtpData, Is.EqualTo("wwtp.3b"));
            Assert.That(data.WwtpTables, Is.EqualTo("wwtp.tbl"));
            Assert.That(data.IndustryGeneral, Is.EqualTo("industry.3b"));
            Assert.That(data.MappixPavedAreaSewageStorageName, Is.EqualTo("pvstordt.his"));
            Assert.That(data.MappixPavedAreaFlowRatesName, Is.EqualTo("pvflowdt.his"));
            Assert.That(data.MappixUnpavedAreaFlowRatesName, Is.EqualTo("upflowdt.his"));
            Assert.That(data.MappixGroundWaterLevelsName, Is.EqualTo("upgwlvdt.his"));
            Assert.That(data.MappixGreenHouseBassinsStorageName, Is.EqualTo("grnstrdt.his"));
            Assert.That(data.MappixGreenHouseBassinsResultsName, Is.EqualTo("grnflodt.his"));
            Assert.That(data.MappixOpenWaterDetailsName, Is.EqualTo("ow_lvldt.his"));
            Assert.That(data.MappixExceedanceTimeReferenceLevelsName, Is.EqualTo("ow_excdt.his"));
            Assert.That(data.MappixFlowRatesOverStructuresName, Is.EqualTo("strflodt.his"));
            Assert.That(data.MappixFlowRatesToEdgeName, Is.EqualTo("bndflodt.his"));
            Assert.That(data.MappixPluviusMaxSewageStorageName, Is.EqualTo("plvstrdt.his"));
            Assert.That(data.MappixPluviusMaxFlowRatesName, Is.EqualTo("plvflodt.his"));
            Assert.That(data.MappixBalanceName, Is.EqualTo("balansdt.his"));
            Assert.That(data.MappixCumulativeBalanceName, Is.EqualTo("cumbaldt.his"));
            Assert.That(data.MappixSaltConcentrationsName, Is.EqualTo("saltdt.his"));
            Assert.That(data.IndustryTables, Is.EqualTo("industry.tbl"));
            Assert.That(data.Maalstop, Is.EqualTo("rtc_3b.his"));
            Assert.That(data.TimeSeriesTemperature, Is.EqualTo("default.tmp"));
            Assert.That(data.TimeSeriesRunoff, Is.EqualTo("rnff.#"));
            Assert.That(data.DischargesTotalsAtEdgeNodes, Is.EqualTo("bndfltot.his"));
            Assert.That(data.LanguageFile, Is.EqualTo("sobek_3b.lng"));
            Assert.That(data.OwVolume, Is.EqualTo("ow_vol.his"));
            Assert.That(data.OwLevels, Is.EqualTo("ow_level.his"));
            Assert.That(data.BalanceFile, Is.EqualTo("3b_bal.out"));
            Assert.That(data.His3BAreaLength, Is.EqualTo("3bareas.his"));
            Assert.That(data.His3BStructureData, Is.EqualTo("3bstrdim.his"));
            Assert.That(data.HisRRRunoff, Is.EqualTo("rrrunoff.his"));
            Assert.That(data.HisSacramento, Is.EqualTo("sacrmnto.his"));
            Assert.That(data.HisRwzi, Is.EqualTo("wwtpdt.his"));
            Assert.That(data.HisIndustry, Is.EqualTo("industdt.his"));
            Assert.That(data.CtrlIni, Is.EqualTo("ctrl.ini"));
            Assert.That(data.RootCapsimInputFile, Is.EqualTo("root_sim.inp"));
            Assert.That(data.UnsaCapsimInputFile, Is.EqualTo("unsa_sim.inp"));
            Assert.That(data.CapsimMessageFile, Is.EqualTo("capsim.msg"));
            Assert.That(data.CapsimDebugFile, Is.EqualTo("capsim.dbg"));
            Assert.That(data.Restart1Hour, Is.EqualTo("restart1.out"));
            Assert.That(data.Restart12Hours, Is.EqualTo("restart12.out"));
            Assert.That(data.RRReady, Is.EqualTo("RR-ready"));
            Assert.That(data.NwrwAreas, Is.EqualTo("NwrwArea.His"));
            Assert.That(data.LinkFlows, Is.EqualTo("3blinks.his"));
            Assert.That(data.ModflowRR, Is.EqualTo("modflow_rr.His"));
            Assert.That(data.RRModflow, Is.EqualTo("rr_modflow.His"));
            Assert.That(data.RRWlmBalance, Is.EqualTo("rr_wlmbal.His"));
            Assert.That(data.SacramentoAsciiOutput, Is.EqualTo("sacrmnto.out"));
            Assert.That(data.NwrwInputDwaTable, Is.EqualTo("pluvius.tbl"));
            Assert.That(data.RRBalance, Is.EqualTo("rrbalans.his"));
            Assert.That(data.GreenHouseClasses, Is.EqualTo("KasKlasData.dat"));
            Assert.That(data.GreenHouseInit, Is.EqualTo("KasInitData.dat"));
            Assert.That(data.GreenHouseUsage, Is.EqualTo("KasGebrData.dat"));
            Assert.That(data.CropFactor, Is.EqualTo("CropData.dat"));
            Assert.That(data.CropOw, Is.EqualTo("CropOWData.dat"));
            Assert.That(data.SoilData, Is.EqualTo("SoilData.dat"));
            Assert.That(data.DioConfigIniFile, Is.EqualTo(""));
            Assert.That(data.BuiFileForContinuousCalculationSeries, Is.EqualTo(""));
            Assert.That(data.NwrwOutput, Is.EqualTo(""));
            Assert.That(data.RRRoutingLinkDefinitions, Is.EqualTo(""));
            Assert.That(data.CellInputFile, Is.EqualTo(""));
            Assert.That(data.CellOutputFile, Is.EqualTo(""));
            Assert.That(data.RRSimulateLogFile, Is.EqualTo(""));
            Assert.That(data.RtcCouplingWqSalt, Is.EqualTo(""));
            Assert.That(data.RRBoundaryConditionsSobek3, Is.EqualTo(""));
            Assert.That(data.RRAsciiRestartOpenda, Is.EqualTo(""));
        }
    }
}