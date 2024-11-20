using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Fnm
{
    /// <summary>
    /// Represents an *.fnm file.
    /// </summary>
    public sealed class FnmFile
    {
        private readonly IDictionary<string, FnmSubFile> subFiles;

        /// <summary>
        /// Initializes a new instance of the <see cref="FnmFile"/> class.
        /// </summary>
        public FnmFile()
        {
            subFiles = CreateSubFileMapping();
            SubFiles = new ReadOnlyCollection<FnmSubFile>(subFiles.Values.ToList());
        }

        /// <summary>
        /// Returns a collection of the *.fnm sub files.
        /// </summary>
        public IEnumerable<FnmSubFile> SubFiles { get; }

        /// <summary>
        /// Gets the evaporation sub file (default.evp).
        /// </summary>
        public FnmSubFile Evaporation => subFiles[Keys.Evaporation];

        private static Dictionary<string, FnmSubFile> CreateSubFileMapping()
        {
            return new Dictionary<string, FnmSubFile>
            {
                { "control", new FnmSubFile("delft_3b.ini", 1, "Control file", FnmSubFileType.Input) },
                { "nodes", new FnmSubFile("3b_nod.tp", 2, "Knoop data", FnmSubFileType.Input) },
                { "links", new FnmSubFile("3b_link.tp", 3, "Tak data", FnmSubFileType.Input) },
                { "runoff_data", new FnmSubFile("3brunoff.tp", 4, "Open water data", FnmSubFileType.Input) },
                { "paved_data", new FnmSubFile("paved.3b", 5, "Verhard gebied algemeen", FnmSubFileType.Input) },
                { "paved_storage", new FnmSubFile("paved.sto", 6, "Verhard gebied storage", FnmSubFileType.Input) },
                { "paved_dry_weather_flow", new FnmSubFile("paved.dwa", 7, "Verhard gebied DWA", FnmSubFileType.Input) },
                { "paved_table", new FnmSubFile("paved.tbl", 8, "Verhard gebied sewer pump capacity", FnmSubFileType.Input) },
                { "nwrw_dry_weather_flow", new FnmSubFile("pluvius.dwa", 9, "Boundaries", FnmSubFileType.Input) },
                { "nwrw_data", new FnmSubFile("pluvius.3b", 10, "Pluvius", FnmSubFileType.Input) },
                { "nwrw_general_data", new FnmSubFile("pluvius.alg", 11, "Pluvius algemeen", FnmSubFileType.Input) },
                { "greenhouse_class", new FnmSubFile("kasklass", 12, "Kasklasse", FnmSubFileType.Input) },
                { "rainfall", new FnmSubFile("default.bui", 13, "buifile", FnmSubFileType.Input) },
                { Keys.Evaporation, new FnmSubFile("default.evp", 14, "verdampingsfile", FnmSubFileType.Input) },
                { "unpaved_data", new FnmSubFile("unpaved.3b", 15, "unpaved algemeen", FnmSubFileType.Input) },
                { "unpaved_storage", new FnmSubFile("unpaved.sto", 16, "unpaved storage", FnmSubFileType.Input) },
                { "greenhouse_initialization", new FnmSubFile("kasinit", 17, "kasgebied initialisatie (SC)", FnmSubFileType.Input) },
                { "greenhouse_water_use", new FnmSubFile("kasgebr", 18, "kasgebied verbruiksdata (SC)", FnmSubFileType.Input) },
                { "crop_factors", new FnmSubFile("cropfact", 19, "crop factors gewassen", FnmSubFileType.Input) },
                { "storage_coefficients", new FnmSubFile("bergcoef", 20, "tabel bergingscoef=f(ontw.diepte,grondsoort)", FnmSubFileType.Input) },
                { "unpaved_alfa", new FnmSubFile("unpaved.alf", 21, "Unpaved - alfa factor definities", FnmSubFileType.Input) },
                { "log", new FnmSubFile("sobek_3b.log", 22, "Run messages", FnmSubFileType.Output) },
                { "general_output", new FnmSubFile("3b_gener.out", 23, "Overzicht van schematisatie, algemene gegevens", FnmSubFileType.Output) },
                { "paved_output", new FnmSubFile("paved.out", 24, "Output results verhard", FnmSubFileType.Output) },
                { "unpaved_output", new FnmSubFile("unpaved.out", 25, "Output results onverhard", FnmSubFileType.Output) },
                { "greenhouse_output", new FnmSubFile("grnhous.out", 26, "Output results kas", FnmSubFileType.Output) },
                { "open_water_output", new FnmSubFile("openwate.out", 27, "Output results open water", FnmSubFileType.Output) },
                { "structure_output", new FnmSubFile("struct3b.out", 28, "Output results kunstwerk", FnmSubFileType.Output) },
                { "boundary_output", new FnmSubFile("bound3b.out", 29, "Output results boundaries", FnmSubFileType.Output) },
                { "pluvius_output", new FnmSubFile("pluvius.out", 30, "Output results Pluvius", FnmSubFileType.Output) },
                { "unpaved_infiltration", new FnmSubFile("unpaved.inf", 31, "Unpaved infiltratie definities", FnmSubFileType.Input) },
                { "debug", new FnmSubFile("sobek_3b.dbg", 32, "Debugfile", FnmSubFileType.Output) },
                { "unpaved_seepage", new FnmSubFile("unpaved.sep", 33, "Unpaved seepage", FnmSubFileType.Input) },
                { "unpaved_table", new FnmSubFile("unpaved.tbl", 34, "Unpaved tabels initial gwl and Scurve", FnmSubFileType.Input) },
                { "greenhouse_data", new FnmSubFile("greenhse.3b", 35, "Kassen general data", FnmSubFileType.Input) },
                { "greenhouse_roof_storage", new FnmSubFile("greenhse.rf", 36, "Kassen roof storage", FnmSubFileType.Input) },
                { "pluvius_sewer_inflouw_output", new FnmSubFile("runoff.out", 37, "Pluvius rioolinloop ASCII file", FnmSubFileType.Output) },
                { "communication_rtc_flow", new FnmSubFile("sbk_rtc.his", 38, "Invoerfile met variabele peilen op randknopen", FnmSubFileType.Input) },
                { "salt_data", new FnmSubFile("salt.3b", 39, "Invoerfile met zoutgegevens", FnmSubFileType.Input) },
                { "open_water_crop_factors", new FnmSubFile("crop_ow.prn", 40, "Invoerfile met cropfactors open water", FnmSubFileType.Input) },
                { "restart_input", new FnmSubFile("RSRR_IN", 41, "Restart file input", FnmSubFileType.Input) },
                { "restart_output", new FnmSubFile("RSRR_OUT", 42, "Restart file output", FnmSubFileType.Output) },
                { "binary_input", new FnmSubFile("3b_input.bin", 43, "Binary file input", FnmSubFileType.Input) },
                { "sacramento_data", new FnmSubFile("sacrmnto.3b", 44, "Sacramento input", FnmSubFileType.Input) },
                { "boundary_discharge_output", new FnmSubFile("aanvoer.abr", 45, "Uitvoer ASCII file met debieten van/naar randknopen", FnmSubFileType.Output) },
                { "boundary_salt_output", new FnmSubFile("saltbnd.out", 46, "Uitvoer ASCII file met zoutconcentratie op rand", FnmSubFileType.Output) },
                { "salt_output", new FnmSubFile("salt.out", 47, "Zout uitvoer in ASCII file", FnmSubFileType.Output) },
                { "greenhouse_silo", new FnmSubFile("greenhse.sil", 48, "Greenhouse silo definitions", FnmSubFileType.Input) },
                { "open_water_data", new FnmSubFile("openwate.3b", 49, "Open water general data", FnmSubFileType.Input) },
                { "open_water_seepage", new FnmSubFile("openwate.sep", 50, "Open water seepage definitions", FnmSubFileType.Input) },
                { "open_water_table", new FnmSubFile("openwate.tbl", 51, "Open water tables target levels", FnmSubFileType.Input) },
                { "structure_data", new FnmSubFile("struct3b.dat", 52, "General structure data", FnmSubFileType.Input) },
                { "structure_definitions", new FnmSubFile("struct3b.def", 53, "Structure definitions", FnmSubFileType.Input) },
                { "controller_definitions", new FnmSubFile("contr3b.def", 54, "Controller definitions", FnmSubFileType.Input) },
                { "structure_table", new FnmSubFile("struct3b.tbl", 55, "Tabellen structures", FnmSubFileType.Input) },
                { "boundary_data", new FnmSubFile("bound3b.3b", 56, "Boundary data", FnmSubFileType.Input) },
                { "boundary_table", new FnmSubFile("bound3b.tbl", 57, "Boundary tables", FnmSubFileType.Input) },
                { "location_data", new FnmSubFile("sbk_loc.rtc", 58, "", FnmSubFileType.Input) },
                { "waste_water_treatment_plant_data", new FnmSubFile("wwtp.3b", 59, "Wwtp data", FnmSubFileType.Input) },
                { "waste_water_treatment_plant_table", new FnmSubFile("wwtp.tbl", 60, "Wwtp tabellen", FnmSubFileType.Input) },
                { "industry_data", new FnmSubFile("industry.3b", 61, "Industry general data", FnmSubFileType.Input) },
                { "paved_storage_mappix", new FnmSubFile("pvstordt.his", 62, "Mappix output file detail berging riool verhard gebied  per tijdstap", FnmSubFileType.Output) },
                { "paved_discharge_mappix", new FnmSubFile("pvflowdt.his", 63, "Mappix output file detail debiet verhard gebied         per tijdstap", FnmSubFileType.Output) },
                { "unpaved_discharge_mappix", new FnmSubFile("upflowdt.his", 64, "Mappix output file detail debiet onverhard gebied       per tijdstap", FnmSubFileType.Output) },
                { "groundwater_level_mappix", new FnmSubFile("upgwlvdt.his", 65, "Mappix output file detail grondwaterstand               per tijdstap", FnmSubFileType.Output) },
                { "greenhouse_storage_mappix", new FnmSubFile("grnstrdt.his", 66, "Mappix output file detail bergingsgraad kasbassins      per tijdstap", FnmSubFileType.Output) },
                { "greenhouse_discharge_mappix", new FnmSubFile("grnflodt.his", 67, "Mappix output file detail uitslag kasbassins            per tijdstap", FnmSubFileType.Output) },
                { "open_water_level_mappix", new FnmSubFile("ow_lvldt.his", 68, "Mappix output file detail open water peil               per tijdstap", FnmSubFileType.Output) },
                { "open_water_level_exceedance_time_mappix", new FnmSubFile("ow_excdt.his", 69, "Mappix output file detail overschrijdingsduur ref. peil per tijdstap", FnmSubFileType.Output) },
                { "structure_discharge_mappix", new FnmSubFile("strflodt.his", 70, "Mappix output file detail debiet over kunstwerk         per tijdstap", FnmSubFileType.Output) },
                { "boundary_discharge_mappix", new FnmSubFile("bndflodt.his", 71, "Mappix output file detail debiet naar rand              per tijdstap", FnmSubFileType.Output) },
                { "pluvius_storage_mappix", new FnmSubFile("plvstrdt.his", 72, "Mappix output file max.berging riool Pluvius            per tijdstap", FnmSubFileType.Output) },
                { "pluvius_discharge_mappix", new FnmSubFile("plvflodt.his", 73, "Mappix output file max.debiet Pluvius                   per tijdstap", FnmSubFileType.Output) },
                { "balance_mappix", new FnmSubFile("balansdt.his", 74, "Mappix output file detail balans                        per tijdstap", FnmSubFileType.Output) },
                { "balance_cumulative_mappix", new FnmSubFile("cumbaldt.his", 75, "Mappix output file detail balans cumulatief             per tijdstap", FnmSubFileType.Output) },
                { "salt_concentration_mappix", new FnmSubFile("saltdt.his", 76, "Mappix output file detail zoutconcentraties             per tijdstap", FnmSubFileType.Output) },
                { "industry_table", new FnmSubFile("industry.tbl", 77, "Industry tabellen", FnmSubFileType.Input) },
                { "maalstop", new FnmSubFile("rtc_3b.his", 78, "Maalstop", FnmSubFileType.Input) },
                { "temperature", new FnmSubFile("default.tmp", 79, "Temperature time series", FnmSubFileType.Input) },
                { "runoff", new FnmSubFile("rnff.#", 80, "Runoff time series", FnmSubFileType.Undefined) },
                { "boundary_totals_discharge_his", new FnmSubFile("bndfltot.his", 81, "Totalen/lozingen op randknopen", FnmSubFileType.Output) },
                { "language", new FnmSubFile("sobek_3b.lng", 82, "Language file", FnmSubFileType.Input) },
                { "open_water_volume_his", new FnmSubFile("ow_vol.his", 83, "OW-volume", FnmSubFileType.Output) },
                { "open_water_levels_his", new FnmSubFile("ow_level.his", 84, "OW_peilen", FnmSubFileType.Output) },
                { "balance_output", new FnmSubFile("3b_bal.out", 85, "Balans file", FnmSubFileType.Output) },
                { "area_his", new FnmSubFile("3bareas.his", 86, "3B-arealen in HIS file", FnmSubFileType.Output) },
                { "structure_his", new FnmSubFile("3bstrdim.his", 87, "3B-structure data in HIS file", FnmSubFileType.Output) },
                { "runoff_his", new FnmSubFile("rrrunoff.his", 88, "RR Runoff his file", FnmSubFileType.Undefined) },
                { "sacramento_his", new FnmSubFile("sacrmnto.his", 89, "Sacramento HIS file", FnmSubFileType.Undefined) },
                { "waste_water_treatment_plant_his", new FnmSubFile("wwtpdt.his", 90, "rwzi HIS file", FnmSubFileType.Output) },
                { "industry_his", new FnmSubFile("industdt.his", 91, "Industry HIS file", FnmSubFileType.Output) },
                { "ctrl", new FnmSubFile("ctrl.ini", 92, "CTRL.INI", FnmSubFileType.Input) },
                { "capsim_root", new FnmSubFile("root_sim.inp", 93, "CAPSIM input file", FnmSubFileType.Input) },
                { "capsim_unsaturated", new FnmSubFile("unsa_sim.inp", 94, "CAPSIM input file", FnmSubFileType.Input) },
                { "capsim_message", new FnmSubFile("capsim.msg", 95, "CAPSIM message file", FnmSubFileType.Output) },
                { "capsim_debug", new FnmSubFile("capsim.dbg", 96, "CAPSIM debug file", FnmSubFileType.Output) },
                { "restart_01_hr", new FnmSubFile("restart1.out", 97, "Restart file na 1 uur", FnmSubFileType.Output) },
                { "restart_12_hr", new FnmSubFile("restart12.out", 98, "Restart file na 12 uur", FnmSubFileType.Output) },
                { "ready", new FnmSubFile("RR-ready", 99, "Ready", FnmSubFileType.Output) },
                { "nwrw_area_his", new FnmSubFile("NwrwArea.His", 100, "NWRW detailed areas", FnmSubFileType.Output) },
                { "links_his", new FnmSubFile("3blinks.his", 101, "Link flows", FnmSubFileType.Output) },
                { "modflow_rr_his", new FnmSubFile("modflow_rr.His", 102, "Modflow-RR", FnmSubFileType.Output) },
                { "rr_modflow_his", new FnmSubFile("rr_modflow.His", 103, "RR-Modflow", FnmSubFileType.Output) },
                { "balance_wlm_his", new FnmSubFile("rr_wlmbal.His", 104, "RR-balance for WLM", FnmSubFileType.Undefined) },
                { "sacramento_output", new FnmSubFile("sacrmnto.out", 105, "Sacramento ASCII output", FnmSubFileType.Undefined) },
                { "pluvius_table", new FnmSubFile("pluvius.tbl", 106, "Additional NWRW input file with DWA table", FnmSubFileType.Input) },
                { "balance_his", new FnmSubFile("rrbalans.his", 107, "RR balans", FnmSubFileType.Undefined) },
                { "greenhouse_class_new_format", new FnmSubFile("KasKlasData.dat", 108, "Kasklasse, new format", FnmSubFileType.Input) },
                { "greenhouse_initialization_new_format", new FnmSubFile("KasInitData.dat", 109, "KasInit, new format", FnmSubFileType.Input) },
                { "greenhouse_water_use_new_format", new FnmSubFile("KasGebrData.dat", 110, "KasGebr, new format", FnmSubFileType.Input) },
                { "crop_factors_new_format", new FnmSubFile("CropData.dat", 111, "CropFact, new format", FnmSubFileType.Input) },
                { "open_water_crop_factors_new_format", new FnmSubFile("CropOWData.dat", 112, "CropOW, new format", FnmSubFileType.Input) },
                { "soil_new_format", new FnmSubFile("SoilData.dat", 113, "Soildata, new format", FnmSubFileType.Input) },
                { "dio_config", new FnmSubFile("dioconfig.ini", 114, "DioConfig Ini file", FnmSubFileType.Undefined) },
                { "continuous_calculation", new FnmSubFile("NWRWCONT.#", 115, "Buifile voor continue berekening Reeksen", FnmSubFileType.Undefined) },
                { "nwrw_his", new FnmSubFile("NwrwSys.His", 116, "NWRW output", FnmSubFileType.Undefined) },
                { "routes", new FnmSubFile("3b_rout.3b", 117, "RR Routing link definitions", FnmSubFileType.Input) },
                { "cel_input", new FnmSubFile("3b_cel.3b", 118, "Cel input file", FnmSubFileType.Undefined) },
                { "cel_output", new FnmSubFile("3b_cel.his", 119, "Cel output file", FnmSubFileType.Undefined) },
                { "log_simulate", new FnmSubFile("sobek3b_progress.txt", 120, "RR Log file for Simulate", FnmSubFileType.Undefined) },
                { "coupling_waq_salt_rtc", new FnmSubFile("wqrtc.his", 121, "coupling WQ salt RTC", FnmSubFileType.Undefined) },
                { "boundary_conditions", new FnmSubFile("BoundaryConditions.bc", 122, "RR Boundary conditions file for SOBEK3", FnmSubFileType.Undefined) },
                { "restart_open_da", new FnmSubFile("ASCIIRestartOpenDA.txt", 123, "Optional RR ASCII restart (test) for OpenDA", FnmSubFileType.Undefined) }
            };
        }

        private static class Keys
        {
            public const string Evaporation = "evaporation";
        }
    }
}