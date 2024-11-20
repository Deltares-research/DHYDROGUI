using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Data;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    public enum AggregationOptions
    {
        Current = 1,
        Average = 2,
        Maximum = 3,
    }

    public static class RainfallRunoffModelParameterNames
    {
        // paved
        public const string PavedStorageRWA = @"Storage RWA (p)";
        public const string PavedStorageDWA = @"Storage DWA (p)";
        public const string PavedStorageStreet = @"Storage street (p)";
        public const string PavedSpillingTotal = @"Spilling (p)";
        public const string PavedSpillingDWA = @"Spilling DWA (p)";
        public const string PavedSpillingRWA = @"Spilling RWA (p)";
        public const string PavedPumpedTotal = @"Pumped flow (p)";
        public const string PavedPumpedDWA = @"Pumped DWA (p)";
        public const string PavedPumpedRWA = @"Pumped RWA (p)";
        public const string PavedRainfall = @"Rainfall (p)";
        public const string PavedDWAToRWA = @"DWA infl-RWA (p)";
        public const string PavedDWAToDWA = @"DWA infl-DWA (p)";
        public const string PavedSurface_RWA = @"Surface RWA (p)";
        public const string PavedRWAToDWA = @"RWA to DWA (p)";
        public const string PavedEvapSurface = @"Evaporation surface (p)";
        public const string PavedVolDynStorage = @"Volume dynamic storage (p)";

        // unpaved
        public const string UnpavedGroundwaterLevel = @"Groundwater level (unp)";
        public const string UnpavedGwOutflow = @"Groundwater outflow (unp)";
        public const string UnpavedStorageCoeff = @"Storage coefficient (unp)";
        public const string UnpavedSurfaceRunoff = @"Surface runoff (unp)";
        public const string UnpavedRainfall = @"Rainfall (unp)";
        public const string UnpavedEvaporationActual = @"Actual evaporation (unp)";
        public const string UnpavedEvaporationPotential = @"Potential evaporation (unp)";
        public const string UnpavedEvaporationSurface = @"Evaporation surface (unp)";
        public const string UnpavedInfiltration = @"Infiltration (unp)";
        public const string UnpavedSeepage = @"Net seepage (unp)";
        public const string UnpavedPercolation = @"Percolation (unp)";
        public const string UnpavedCapillaryRise = @"Capillary Rise (unp)";
        public const string UnpavedStorageLandmm = @"Storage land (unp)";
        public const string UnpavedGroundwaterVolume = @"Groundwater volume (unp)";
        public const string UnpavedStorageLandm3 = @"Storage land m3 (unp)";
        public const string UnpavedGroundwaterLevelThreshold = @"Groundwater level threshold (unp)";
        public const string UnpavedGroundwaterLevelSurface = @"Groundwater level surface (unp)";
        public const string UnpavedUnsaturatedZone = @"Unsaturated zone (unp)";
        public const string UnpavedUnsaturatedZoneVolume = @"Volume unsaturated zone (unp)";

        // greenhouse
        public const string GreenhouseStorageBasins = @"Storage basins (g)";
        public const string GreenhouseFlowBasins = @"Flow basins (g)";
        public const string GreenhouseRainfall = @"Rainfall (g)";
        public const string GreenhouseEvaporation = @"Evaporation (g)";
        public const string GreenhouseWaterUse = @"Water use (g)";

        // open water
        public const string OpenWaterRainfall = @"Rainfall (ow)";
        public const string OpenWaterEvaporation = @"Evaporation (ow)";

        // Sacramento
        public const string SacramentoUpperZoneTensionWaterCapacity = @"UZTW capacity (sac)";
        public const string SacramentoUpperZoneFreeWaterCapacity = @"UZFW capacity (sac)";
        public const string SacramentoLowerZoneTensionWaterCapacity = @"LZTW capacity (sac)";
        public const string SacramentoLowerZoneFreeSupplementalWaterCapacity = @"LZFSW capacity (sac)";
        public const string SacramentoLowerZoneFreePrimaryWaterCapacity = @"LZFPW capacity (sac)";
        public const string SacramentoRainfall = @"Rainfall (sac)";
        public const string SacramentoPotEvap = @"Potential evaporation (sac)";
        public const string SacramentoActEvap = @"Actual evaporation (sac)";
        public const string SacramentoBaseFlow = @"Base flow (sac)";
        public const string SacramentoSurfaceFlow = @"Surface runoff (sac)";
        public const string SacramentoRunoffImpArea = @"Runoff impervious area (sac)";
        public const string SacramentoTotalRunoff = @"Total runoff (sac)";
        public const string SacramentoChannelInflow = @"Channel inflow (sac)";
        public const string SacramentoSideSubsurfaceOutflow = @"Side + subsurface outflow (sac)";
        public const string SacramentoAdditionalImpAreaContent = @"Additional impervious area content (sac)";

        // HBV
        public const string HbvOutflow = @"Outflow (hbv)";
        public const string HbvRainfall = @"Rainfall (hbv)";
        public const string HbvSnowfall = @"Snowfall (hbv)";
        public const string HbvPotEvap = @"Potential evaporation (hbv)";
        public const string HbvActEvap = @"Actual evaporation (hbv)";
        public const string HbvBaseflow = @"Baseflow (hbv)";
        public const string HbvInterflow = @"Interflow (hbv)";
        public const string HbvQuickflow = @"Quickflow (hbv)";
        public const string HbvDrySnowContent = @"Dry snow content (hbv)";
        public const string HbvFreeWaterContent = @"Free water content (hbv)";
        public const string HbvSoilMoisture = @"Soil Moisture (hbv)";
        public const string HbvUpperZoneContent = @"Upper zone content (hbv)";
        public const string HbvLowerZoneContent = @"Lower zone content (hbv)";
        public const string HbvTemperature = @"Temperature (hbv)";
        
        // waste water treatment plant
        public const string WWTPInFlow = @"Inflow (wwtp)";
        public const string WWTPOutFlow = @"Outflow (wwtp)";

        // waterbalance per node
        public const string NodeBalanceFlowInNonLinks = @"Total in non-links (bn)";
        public const string NodeBalanceFlowOutNonLinks = @"Total out non-links (bn)";
        public const string NodeBalanceFlowInViaLinks = @"Total in via links (bn)";
        public const string NodeBalanceFlowOutViaLinks = @"Total out via links (bn)";
        public const string NodeBalanceDeltaStorage = @"Delta storage (bn)";
        public const string NodeBalanceBalanceError = @"Balance error (bn)";
        public const string NodeBalanceCumFlowInNonLinks = @"Cumulative in non-links (bn)";
        public const string NodeBalanceCumFlowInViaLinks = @"Cumulative in via links (bn)";
        public const string NodeBalanceCumFlowOutNonLinks = @"Cumulative out non-links (bn)";
        public const string NodeBalanceCumFlowOutViaLinks = @"Cumulative out via links (bn)";
        public const string NodeBalanceCumDeltaStorage = @"Cumulative delta storage (bn)";
        public const string NodeBalanceCumBalanceError = @"Cumulative balance error (bn)";
        
        // water balance model
        public const string ModelBalanceRainfall = @"Rainfall (bm)";
        public const string ModelBalanceEvaporationPaved = @"Evaporation paved (bm)";
        public const string ModelBalanceEvaporationUnpaved = @"Evaporation unpaved (bm)";
        public const string ModelBalanceUseGreenhouses = @"Use greenhouses (bm)";
        public const string ModelBalanceDWFPaved = @"DWF paved (bm)";
        public const string ModelBalanceNetSeepageUnpaved = @"Net seepage unpaved (bm)";
        public const string ModelBalanceStoragePaved = @"Storage paved (bm)";
        public const string ModelBalanceStorageUnpaved = @"Storage unpaved (bm)";
        public const string ModelBalanceStorageGreenhouses = @"Storage greenhouses (bm)";
        public const string ModelBalanceStorageWWTP = @"Storage wwtp (bm)";
        public const string ModelBalanceBoundariesOut = @"Boundaries out (bm)";
        public const string ModelBalanceBoundariesIn = @"Boundaries in (bm)";
        public const string ModelBalanceExternalInFlowRRRunoff = @"External inflow RRRunoff (bm)";
        public const string ModelBalanceStorageChangeRRRunoff = @"Storage change RRRunoff (bm)";
        public const string ModelBalanceError = @"Balance error RR Rural (bm)";

        public const string NwrwModelBalanceRainfall = @"Rainfall NWRW (bm)";
        public const string NwrwModelBalanceEvaporation = @"Evaporation NWRW (bm)";
        public const string NwrwModelBalanceInfiltrStorage = @"Infiltr.storage NWRW (bm)";
        public const string NwrwModelBalanceInfiltrRunOff = @"Infiltr.runoff NWRW (bm)";
        public const string NwrwModelBalanceStorage = @"Storage NWRW (bm)";
        public const string NwrwModelBalanceRWF = @"RWF NWRW (bm)";
        public const string NwrwModelBalanceDWFPeople = @"DWF people NWRW (bm)";
        public const string NwrwModelBalanceDWFCompanies = @"DWF companies NWRW (bm)";
        public const string NwrwModelBalanceRWFAndDWF = @"RWF+DWF NWRW (bm)";
        public const string NwrwModelBalanceError = @"Balance error NWRW (bm)";

        //NWRW
        public const string NwrwInflowSewer = @"Inflow sewer (nwrw)";
        public const string NwrwInfilDepress = @"Infiltration Depression (nwrw)";
        public const string NwrwInfilRunoff = @"Infiltration Runoff (nwrw)";
        public const string NwrwRainfall = @"Rainfall (nwrw)";
        public const string NwrwEvaporation = @"Evaporation (nwrw)";
        public const string NwrwRWF = @"RWF (nwrw)";
        public const string NwrwDWFPeople = @"DWF people (nwrw)";
        public const string NwrwDWFCompanies = @"DWF companies (nwrw)";
        public const string NwrwStorageDepress = @"Storage Depression (nwrw)";
        public const string NwrwDynamicStorage = @"Dynamic storage (nwrw)";
        public const string NwrwInfCapSurf = @"Infiltration capacity surface (nwrw)";
        public const string NwrwInfCapDyn = @"Infiltration capacity dynamic (nwrw)";
        public const string NwrwInflSewer1 = @"Inflow Sewer 1 (nwrw)";
        public const string NwrwInflSewer2 = @"Inflow Sewer 2 (nwrw)";
        public const string NwrwInflSewer3 = @"Inflow Sewer 3 (nwrw)";
        public const string NwrwInflSewer4 = @"Inflow Sewer 4 (nwrw)";
        public const string NwrwInflSewer5 = @"Inflow Sewer 5 (nwrw)";
        public const string NwrwInflSewer6 = @"Inflow Sewer 6 (nwrw)";
        public const string NwrwInflSewer7 = @"Inflow Sewer 7 (nwrw)";
        public const string NwrwInflSewer8 = @"Inflow Sewer 8 (nwrw)";
        public const string NwrwInflSewer9 = @"Inflow Sewer 9 (nwrw)";
        public const string NwrwInflSewer10 = @"Inflow Sewer 10 (nwrw)";
        public const string NwrwInflSewer11 = @"Inflow Sewer 11 (nwrw)";
        public const string NwrwInflSewer12 = @"Inflow Sewer 12 (nwrw)";
        public const string NwrwInflSewerSp1 = @"Inflow Sewer Special 1 (nwrw)";
        public const string NwrwInflSewerSp2 = @"Inflow Sewer Special 2 (nwrw)";
        public const string NwrwInflSewerSp3 = @"Inflow Sewer Special 3 (nwrw)";
        public const string NwrwInflSewerSp4 = @"Inflow Sewer Special 4 (nwrw)";
        public const string NwrwInflSewerSp5 = @"Inflow Sewer Special 5 (nwrw)";
        public const string NwrwInflSewerSp6 = @"Inflow Sewer Special 6 (nwrw)";
        public const string NwrwInflSewerSp7 = @"Inflow Sewer Special 7 (nwrw)";
        public const string NwrwInflSewerSp8 = @"Inflow Sewer Special 8 (nwrw)";
        public const string NwrwInflSewerSp9 = @"Inflow Sewer Special 9 (nwrw)";
        public const string NwrwInflSewerSp10 = @"Inflow Sewer Special 10 (nwrw)";
        public const string NwrwInflSewerSp11 = @"Inflow Sewer Special 11 (nwrw)";
        public const string NwrwInflSewerSp12 = @"Inflow Sewer Special 12 (nwrw)";
        public const string NwrwStorageSp1 = @"Storage Special 1 (nwrw)";
        public const string NwrwStorageSp2 = @"Storage Special 2 (nwrw)";
        public const string NwrwStorageSp3 = @"Storage Special 3 (nwrw)";
        public const string NwrwStorageSp4 = @"Storage Special 4 (nwrw)";
        public const string NwrwStorageSp5 = @"Storage Special 5 (nwrw)";
        public const string NwrwStorageSp6 = @"Storage Special 6 (nwrw)";
        public const string NwrwStorageSp7 = @"Storage Special 7 (nwrw)";
        public const string NwrwStorageSp8 = @"Storage Special 8 (nwrw)";
        public const string NwrwStorageSp9 = @"Storage Special 9 (nwrw)";
        public const string NwrwStorageSp10 = @"Storage Special 10 (nwrw)";
        public const string NwrwStorageSp11 = @"Storage Special 11 (nwrw)";
        public const string NwrwStorageSp12 = @"Storage Special 12 (nwrw)";
        public const string NwrwWadiInflow = @"Wadi inflow (nwrw)";
        public const string NwrwWadiInfiltr = @"Wadi infiltr (nwrw)";
        public const string NwrwWadiSpill = @"Wadi spill (nwrw)";
        public const string NwrwWadiDrain = @"Wadi drain (nwrw)";
        public const string NwrwWadiStorage = @"Wadi storage (nwrw)";
        public const string NwrwWadiLevel = @"Wadi level (nwrw)";
        
        // link
        public const string LinkFlowOut = @"Link flow (lnk)";

        // boundary
        public const string BoundaryDischarge = @"Discharge (bnd)";
    }

    public struct HisFileParameter
    {
        public string HisFileName { get; set; }

        public string ParameterName { get; set; }
    }

    public static class RainfallRunoffModelParameterHisFileMapping
    {
        private const string PavedHisFileName = "pvstordt.his";
        private const string UnPavedHisFileName = "upflowdt.his";
        private const string GreenhouseHisFileName = "grnstrdt.his";
        private const string OpenWaterHisFileName = "OW_LVLDT.HIS";
        private const string SacramentoHisFileName = "sacrmnto.his";
        private const string HBVHisFileName = "rrrunoff.his";
        private const string WWTPHisFileName = "wwtpdt.his";
        private const string BalansPerNodeHisFileName = "balansdt.his";
        private const string WaterBalanceModelHisFileName = "rrbalans.his";
        private const string linkHisFileName = "3blinks.his";
        private const string NwrwNodesHisFileName = "plvstrdt.his";

        private const string boundaryHisFileName = "bndflodt.his";


        public static IDictionary<string, HisFileParameter> HisFileParameterLookup = new Dictionary<string, HisFileParameter>
            {
                #region Paved
                
                {RainfallRunoffModelParameterNames.PavedStorageRWA ,new HisFileParameter {HisFileName = PavedHisFileName, ParameterName = "Storage CSS/SWS[mm]"} },
                {RainfallRunoffModelParameterNames.PavedStorageDWA ,new HisFileParameter {HisFileName = PavedHisFileName, ParameterName = "Storage WWS    [mm]"} },
                {RainfallRunoffModelParameterNames.PavedStorageStreet,new HisFileParameter {HisFileName = PavedHisFileName, ParameterName = "Storage Street [mm]"} },
                {RainfallRunoffModelParameterNames.PavedSpillingTotal,new HisFileParameter {HisFileName = PavedHisFileName, ParameterName = "Spill        [m3/s]"} },
                {RainfallRunoffModelParameterNames.PavedSpillingDWA ,new HisFileParameter {HisFileName = PavedHisFileName, ParameterName = "Spill WWS    [m3/s]"} },
                {RainfallRunoffModelParameterNames.PavedSpillingRWA ,new HisFileParameter {HisFileName = PavedHisFileName, ParameterName = "Spill CSS/SWS [m3/s]"} },
                {RainfallRunoffModelParameterNames.PavedPumpedTotal ,new HisFileParameter {HisFileName = PavedHisFileName, ParameterName = "Pump         [m3/s]"} },
                {RainfallRunoffModelParameterNames.PavedPumpedDWA ,new HisFileParameter {HisFileName = PavedHisFileName, ParameterName = "Pump WWS     [m3/s]"} },
                {RainfallRunoffModelParameterNames.PavedPumpedRWA ,new HisFileParameter {HisFileName = PavedHisFileName, ParameterName = "Pump CSS/SWS [m3/s]"} },
                {RainfallRunoffModelParameterNames.PavedRainfall ,new HisFileParameter {HisFileName = PavedHisFileName, ParameterName = "Rainfall     [m3/s]"} },
                {RainfallRunoffModelParameterNames.PavedDWAToRWA ,new HisFileParameter {HisFileName = PavedHisFileName, ParameterName = "DWF to CSS/SWS [m3/s"} },
                {RainfallRunoffModelParameterNames.PavedDWAToDWA ,new HisFileParameter {HisFileName = PavedHisFileName, ParameterName = "DWF to WWS   [m3/s]"} },
                {RainfallRunoffModelParameterNames.PavedSurface_RWA ,new HisFileParameter {HisFileName = PavedHisFileName, ParameterName = "Street to CSS/SWS [m"} },
                {RainfallRunoffModelParameterNames.PavedRWAToDWA ,new HisFileParameter {HisFileName = PavedHisFileName, ParameterName = "SWS to WWS   [m3/s]" } },
                {RainfallRunoffModelParameterNames.PavedEvapSurface ,new HisFileParameter {HisFileName = PavedHisFileName, ParameterName = "Street evap. [m3/s]"} },
                {RainfallRunoffModelParameterNames.PavedVolDynStorage,new HisFileParameter {HisFileName = PavedHisFileName, ParameterName = "Dynamic Storage[mm]"} },

            #endregion

                #region Unpaved
                
                {RainfallRunoffModelParameterNames.UnpavedGroundwaterLevel,new HisFileParameter {HisFileName = UnPavedHisFileName, ParameterName = "Groundw.Level   [m]"} },
                {RainfallRunoffModelParameterNames.UnpavedGwOutflow ,new HisFileParameter {HisFileName = UnPavedHisFileName, ParameterName = "Groundw.outfl.[m3/s]"} },
                {RainfallRunoffModelParameterNames.UnpavedStorageCoeff ,new HisFileParameter {HisFileName = UnPavedHisFileName, ParameterName = "Storage coeff."} },
                {RainfallRunoffModelParameterNames.UnpavedSurfaceRunoff,new HisFileParameter {HisFileName = UnPavedHisFileName, ParameterName = "Surf. Runoff  [m3/s]"} },
                {RainfallRunoffModelParameterNames.UnpavedRainfall ,new HisFileParameter {HisFileName = UnPavedHisFileName, ParameterName = "Rainfall     [m3/s]"} },
                {RainfallRunoffModelParameterNames.UnpavedEvaporationActual ,new HisFileParameter {HisFileName = UnPavedHisFileName, ParameterName = "Actual Evap. [m3/s]"} },
                {RainfallRunoffModelParameterNames.UnpavedEvaporationPotential ,new HisFileParameter {HisFileName = UnPavedHisFileName, ParameterName = "Pot. Evapor. [m3/s]"} },
                {RainfallRunoffModelParameterNames.UnpavedEvaporationSurface ,new HisFileParameter {HisFileName = UnPavedHisFileName, ParameterName = "Evap. surface[m3/s]"} },
                {RainfallRunoffModelParameterNames.UnpavedInfiltration ,new HisFileParameter {HisFileName = UnPavedHisFileName, ParameterName = "Infiltration [m3/s]"} },
                {RainfallRunoffModelParameterNames.UnpavedSeepage ,new HisFileParameter {HisFileName = UnPavedHisFileName, ParameterName = "Net Seepage  [m3/s]"} },
                {RainfallRunoffModelParameterNames.UnpavedPercolation ,new HisFileParameter {HisFileName = UnPavedHisFileName, ParameterName = "Percolation  [m3/s]"} },
                {RainfallRunoffModelParameterNames.UnpavedCapillaryRise,new HisFileParameter {HisFileName = UnPavedHisFileName, ParameterName = "Capill.Rise  [m3/s]"} },
                {RainfallRunoffModelParameterNames.UnpavedStorageLandmm ,new HisFileParameter {HisFileName = UnPavedHisFileName, ParameterName = "Storage Land   [mm]"} },
                {RainfallRunoffModelParameterNames.UnpavedGroundwaterVolume ,new HisFileParameter {HisFileName = UnPavedHisFileName, ParameterName = "Groundw.Volume [m3]"} },
                {RainfallRunoffModelParameterNames.UnpavedStorageLandm3 ,new HisFileParameter {HisFileName = UnPavedHisFileName, ParameterName = "Storage Land   [m3]"} },
                {RainfallRunoffModelParameterNames.UnpavedGroundwaterLevelThreshold,new HisFileParameter {HisFileName = UnPavedHisFileName, ParameterName = "GW>Threshold [hour]"} },
                {RainfallRunoffModelParameterNames.UnpavedGroundwaterLevelSurface ,new HisFileParameter {HisFileName = UnPavedHisFileName, ParameterName = "GWLevel-Surface[m]"} },
                {RainfallRunoffModelParameterNames.UnpavedUnsaturatedZone ,new HisFileParameter {HisFileName = UnPavedHisFileName, ParameterName = "Unsat.Zone    [mm]"} },
                {RainfallRunoffModelParameterNames.UnpavedUnsaturatedZoneVolume ,new HisFileParameter {HisFileName = UnPavedHisFileName, ParameterName = "Vol.Unsat.Zone[m3]"} },

                #endregion

                #region Greenhouse
                
                {RainfallRunoffModelParameterNames.GreenhouseStorageBasins,new HisFileParameter {HisFileName = GreenhouseHisFileName, ParameterName = "Storage basins[m3]"} },
                {RainfallRunoffModelParameterNames.GreenhouseFlowBasins ,new HisFileParameter {HisFileName = GreenhouseHisFileName, ParameterName = "Flow basins [m3/s]"} },
                {RainfallRunoffModelParameterNames.GreenhouseRainfall ,new HisFileParameter {HisFileName = GreenhouseHisFileName, ParameterName = "Rainfall    [m3/s]"} },
                {RainfallRunoffModelParameterNames.GreenhouseEvaporation,new HisFileParameter {HisFileName = GreenhouseHisFileName, ParameterName = "Evaporation [m3/s]"} },
                {RainfallRunoffModelParameterNames.GreenhouseWaterUse ,new HisFileParameter {HisFileName = GreenhouseHisFileName, ParameterName = "Water use   [m3/s]"} },

                #endregion

                #region OpenWater

                {RainfallRunoffModelParameterNames.OpenWaterRainfall,new HisFileParameter {HisFileName = OpenWaterHisFileName, ParameterName = "Rainfall    [m3/s]"} },
                {RainfallRunoffModelParameterNames.OpenWaterEvaporation,new HisFileParameter {HisFileName = OpenWaterHisFileName, ParameterName = "Evaporation [m3/s]"} },

                #endregion

                #region Sacramento
                
                {RainfallRunoffModelParameterNames.SacramentoUpperZoneTensionWaterCapacity ,new HisFileParameter {HisFileName = SacramentoHisFileName, ParameterName = "UppZoneTensionC [mm]"} },
                {RainfallRunoffModelParameterNames.SacramentoUpperZoneFreeWaterCapacity ,new HisFileParameter {HisFileName = SacramentoHisFileName, ParameterName = "UppZoneFreeWatC [mm]"} },
                {RainfallRunoffModelParameterNames.SacramentoLowerZoneTensionWaterCapacity ,new HisFileParameter {HisFileName = SacramentoHisFileName, ParameterName = "LowZoneTensionC [mm]" } },
                {RainfallRunoffModelParameterNames.SacramentoLowerZoneFreeSupplementalWaterCapacity,new HisFileParameter {HisFileName = SacramentoHisFileName, ParameterName = "LowZoneFreeSecC [mm]"} },
                {RainfallRunoffModelParameterNames.SacramentoLowerZoneFreePrimaryWaterCapacity ,new HisFileParameter {HisFileName = SacramentoHisFileName, ParameterName = "LowZoneFreePriC [mm]"} },
                {RainfallRunoffModelParameterNames.SacramentoRainfall ,new HisFileParameter {HisFileName = SacramentoHisFileName, ParameterName = "Precipitation [mm]"} },
                {RainfallRunoffModelParameterNames.SacramentoPotEvap ,new HisFileParameter {HisFileName = SacramentoHisFileName, ParameterName = "Pot.Evaporation [mm]"} },
                {RainfallRunoffModelParameterNames.SacramentoActEvap ,new HisFileParameter {HisFileName = SacramentoHisFileName, ParameterName = "Act.Evaporation [mm]"} },
                {RainfallRunoffModelParameterNames.SacramentoBaseFlow ,new HisFileParameter {HisFileName = SacramentoHisFileName, ParameterName = "Comp. Baseflow [mm]"} },
                {RainfallRunoffModelParameterNames.SacramentoSurfaceFlow ,new HisFileParameter {HisFileName = SacramentoHisFileName, ParameterName = "Surface Runoff [mm]"} },
                {RainfallRunoffModelParameterNames.SacramentoRunoffImpArea ,new HisFileParameter {HisFileName = SacramentoHisFileName, ParameterName = "Runoff Imp.Area[mm]"} },
                {RainfallRunoffModelParameterNames.SacramentoTotalRunoff ,new HisFileParameter {HisFileName = SacramentoHisFileName, ParameterName = "Total Runoff [mm]"} },
                {RainfallRunoffModelParameterNames.SacramentoChannelInflow ,new HisFileParameter {HisFileName = SacramentoHisFileName, ParameterName = "ChannelInflow[m3/s]"} },
                {RainfallRunoffModelParameterNames.SacramentoSideSubsurfaceOutflow ,new HisFileParameter {HisFileName = SacramentoHisFileName, ParameterName = "Side+SSoutflow [mm]"} },
                {RainfallRunoffModelParameterNames.SacramentoAdditionalImpAreaContent ,new HisFileParameter {HisFileName = SacramentoHisFileName, ParameterName = "AdimC Contents [mm]"} },

            #endregion

                #region Hbv
                
                {RainfallRunoffModelParameterNames.HbvOutflow ,new HisFileParameter {HisFileName = HBVHisFileName, ParameterName = "Total Outflow [m3/s]"} },
                {RainfallRunoffModelParameterNames.HbvRainfall,new HisFileParameter {HisFileName = HBVHisFileName, ParameterName = "Total Rainfall [mm]"} },
                {RainfallRunoffModelParameterNames.HbvSnowfall,new HisFileParameter {HisFileName = HBVHisFileName, ParameterName = "HBV-Snowfall [mm]"} },
                {RainfallRunoffModelParameterNames.HbvPotEvap ,new HisFileParameter {HisFileName = HBVHisFileName, ParameterName = "Total PotEvap [mm]"} },
                {RainfallRunoffModelParameterNames.HbvActEvap ,new HisFileParameter {HisFileName = HBVHisFileName, ParameterName = "Total ActEvap [mm]"} },
                {RainfallRunoffModelParameterNames.HbvBaseflow,new HisFileParameter {HisFileName = HBVHisFileName, ParameterName = "HBV-BaseFlow [mm]"} },
                {RainfallRunoffModelParameterNames.HbvInterflow,new HisFileParameter {HisFileName = HBVHisFileName, ParameterName = "HBV-InterFlow [mm]"} },
                {RainfallRunoffModelParameterNames.HbvQuickflow ,new HisFileParameter {HisFileName = HBVHisFileName, ParameterName = "HBV-QuickFlow [mm]"} },
                {RainfallRunoffModelParameterNames.HbvDrySnowContent,new HisFileParameter {HisFileName = HBVHisFileName, ParameterName = "HBV-DrySnowC. [mm]"} },
                {RainfallRunoffModelParameterNames.HbvFreeWaterContent,new HisFileParameter {HisFileName = HBVHisFileName, ParameterName = "HBV-FreeWaterC.[mm]"} },
                {RainfallRunoffModelParameterNames.HbvSoilMoisture,new HisFileParameter {HisFileName = HBVHisFileName, ParameterName = "HBV-SoilMoisture [mm"} },
                {RainfallRunoffModelParameterNames.HbvUpperZoneContent,new HisFileParameter {HisFileName = HBVHisFileName, ParameterName = "HBV-UpperZoneC [mm]"} },
                {RainfallRunoffModelParameterNames.HbvLowerZoneContent,new HisFileParameter {HisFileName = HBVHisFileName, ParameterName = "HBV-LowerZoneC [mm]"} },
                {RainfallRunoffModelParameterNames.HbvTemperature,new HisFileParameter {HisFileName = HBVHisFileName, ParameterName = "HBV_Temperature [oC]"} },
                   
                #endregion                                

                #region waste water treatment plant

                {RainfallRunoffModelParameterNames.WWTPInFlow,new HisFileParameter {HisFileName = WWTPHisFileName, ParameterName = "InFlow [m3/s]"} },
                {RainfallRunoffModelParameterNames.WWTPOutFlow,new HisFileParameter {HisFileName = WWTPHisFileName, ParameterName = "OutFlow [m3/s]"} },

                #endregion

                #region waterbalance per node

                {RainfallRunoffModelParameterNames.NodeBalanceFlowInNonLinks,new HisFileParameter {HisFileName = BalansPerNodeHisFileName, ParameterName = "TotalInAtNode  [m3]"} },
                {RainfallRunoffModelParameterNames.NodeBalanceFlowOutNonLinks,new HisFileParameter {HisFileName = BalansPerNodeHisFileName, ParameterName = "TotalOutAtNode [m3]"} },
                {RainfallRunoffModelParameterNames.NodeBalanceFlowInViaLinks ,new HisFileParameter {HisFileName = BalansPerNodeHisFileName, ParameterName = "TotalInViaLinks[m3]"} },
                {RainfallRunoffModelParameterNames.NodeBalanceFlowOutViaLinks ,new HisFileParameter {HisFileName = BalansPerNodeHisFileName, ParameterName = "TotalOutViaLinks[m3]"} },
                {RainfallRunoffModelParameterNames.NodeBalanceDeltaStorage ,new HisFileParameter {HisFileName = BalansPerNodeHisFileName, ParameterName = "DeltaStorage [m3]"} },
                {RainfallRunoffModelParameterNames.NodeBalanceBalanceError ,new HisFileParameter {HisFileName = BalansPerNodeHisFileName, ParameterName = "BalanceError [m3]"} },
                {RainfallRunoffModelParameterNames.NodeBalanceCumFlowInNonLinks ,new HisFileParameter {HisFileName = BalansPerNodeHisFileName, ParameterName = "Cum.InAtNode [m3]"} },
                {RainfallRunoffModelParameterNames.NodeBalanceCumFlowInViaLinks ,new HisFileParameter {HisFileName = BalansPerNodeHisFileName, ParameterName = "Cum.InViaLinks [m3]"} },
                {RainfallRunoffModelParameterNames.NodeBalanceCumFlowOutNonLinks,new HisFileParameter {HisFileName = BalansPerNodeHisFileName, ParameterName = "Cum.OutAtNode  [m3]"} },
                {RainfallRunoffModelParameterNames.NodeBalanceCumFlowOutViaLinks,new HisFileParameter {HisFileName = BalansPerNodeHisFileName, ParameterName = "Cum.OutViaLinks[m3]"} },
                {RainfallRunoffModelParameterNames.NodeBalanceCumDeltaStorage ,new HisFileParameter {HisFileName = BalansPerNodeHisFileName, ParameterName = "CumDeltaStorage[m3]"} },
                {RainfallRunoffModelParameterNames.NodeBalanceCumBalanceError,new HisFileParameter {HisFileName = BalansPerNodeHisFileName, ParameterName = "CumBalanceError[m3]"} },
                
                #endregion

                #region water balance model

                {RainfallRunoffModelParameterNames.ModelBalanceRainfall ,new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "Rainfall (+ext.irrig"} },
                {RainfallRunoffModelParameterNames.ModelBalanceEvaporationPaved ,new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "Evap.Paved"} },
                {RainfallRunoffModelParameterNames.ModelBalanceEvaporationUnpaved ,new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "Evap.+Irr.Loss.Unpav"} },
                {RainfallRunoffModelParameterNames.ModelBalanceUseGreenhouses ,new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "Use Greenhouses"} },
                {RainfallRunoffModelParameterNames.ModelBalanceDWFPaved ,new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "DWF Paved"} },
                {RainfallRunoffModelParameterNames.ModelBalanceNetSeepageUnpaved ,new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "Net Seepage Unpaved"} },
                {RainfallRunoffModelParameterNames.ModelBalanceStoragePaved ,new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "Storage Paved"} },
                {RainfallRunoffModelParameterNames.ModelBalanceStorageUnpaved ,new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "Storage Unpaved"} },
                {RainfallRunoffModelParameterNames.ModelBalanceStorageGreenhouses ,new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "Storage Greenhouses"} },
                {RainfallRunoffModelParameterNames.ModelBalanceStorageWWTP ,new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "Storage WWTP"} },
                {RainfallRunoffModelParameterNames.ModelBalanceBoundariesOut ,new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "Boundaries out"} },
                {RainfallRunoffModelParameterNames.ModelBalanceBoundariesIn ,new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "Boundaries in"} },
                {RainfallRunoffModelParameterNames.ModelBalanceExternalInFlowRRRunoff,new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "External inflow RRRu"} },
                {RainfallRunoffModelParameterNames.ModelBalanceStorageChangeRRRunoff ,new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "Storage change RRRun"} },
                {RainfallRunoffModelParameterNames.ModelBalanceError ,new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "Balance error RR Rur"} },
                
                #endregion

                #region nwrw model balance

                {RainfallRunoffModelParameterNames.NwrwModelBalanceRainfall, new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "Rainfall NWRW"} },
                {RainfallRunoffModelParameterNames.NwrwModelBalanceEvaporation, new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "Evaporation NWRW"} },
                {RainfallRunoffModelParameterNames.NwrwModelBalanceInfiltrStorage, new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "Infiltr.storage NWRW"} },
                {RainfallRunoffModelParameterNames.NwrwModelBalanceInfiltrRunOff, new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "Infiltr.runoff NWRW"} },
                {RainfallRunoffModelParameterNames.NwrwModelBalanceStorage, new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "Storage NWRW"} },
                {RainfallRunoffModelParameterNames.NwrwModelBalanceRWF, new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "RWF NWRW"} },
                {RainfallRunoffModelParameterNames.NwrwModelBalanceDWFPeople, new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "DWF people NWRW"} },
                {RainfallRunoffModelParameterNames.NwrwModelBalanceDWFCompanies, new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "DWF companies NWRW"} },
                {RainfallRunoffModelParameterNames.NwrwModelBalanceRWFAndDWF, new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "RWF+DWF NWRW"} },
                {RainfallRunoffModelParameterNames.NwrwModelBalanceError, new HisFileParameter {HisFileName = WaterBalanceModelHisFileName, ParameterName = "Balance error NWRW"} },
                
                #endregion

                #region nwrw
                {RainfallRunoffModelParameterNames.NwrwInflowSewer, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Inflow sewer [m3/s]"} },
                {RainfallRunoffModelParameterNames.NwrwInfilDepress, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infil.Depress.[m3/s]"} },
                {RainfallRunoffModelParameterNames.NwrwInfilRunoff, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infil.Runoff [m3/s]" } },
                {RainfallRunoffModelParameterNames.NwrwRainfall, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Rainfall [m3/s]" } },
                {RainfallRunoffModelParameterNames.NwrwEvaporation, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Evaporation [m3/s]" } },
                {RainfallRunoffModelParameterNames.NwrwRWF, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "RWF [m3/s]" } },
                {RainfallRunoffModelParameterNames.NwrwDWFPeople, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "DWF people [m3/s]" } },
                {RainfallRunoffModelParameterNames.NwrwStorageDepress, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "StorageDepress.[m3]"} },
                {RainfallRunoffModelParameterNames.NwrwDynamicStorage, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Dynamic storage [m3]"} },
                {RainfallRunoffModelParameterNames.NwrwInfCapSurf, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Inf.cap.Surf.[mm/hr]"} },
                {RainfallRunoffModelParameterNames.NwrwInfCapDyn, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Inf.cap.Dyn.[mm/hr]"} },
                {RainfallRunoffModelParameterNames.NwrwInflSewer1, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer 1 [m3/s]"} },
                {RainfallRunoffModelParameterNames.NwrwInflSewer2, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer 2 [m3/s]"} },
                {RainfallRunoffModelParameterNames.NwrwInflSewer3, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer 3 [m3/s]"} },
                {RainfallRunoffModelParameterNames.NwrwInflSewer4, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer 4 [m3/s]"} },
                {RainfallRunoffModelParameterNames.NwrwInflSewer5, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer 5 [m3/s]"} },
                {RainfallRunoffModelParameterNames.NwrwInflSewer6, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer 6 [m3/s]"} },
                {RainfallRunoffModelParameterNames.NwrwInflSewer7, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer 7 [m3/s]"} },
                {RainfallRunoffModelParameterNames.NwrwInflSewer8, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer 8 [m3/s]"} },
                {RainfallRunoffModelParameterNames.NwrwInflSewer9, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer 9 [m3/s]" } },
                {RainfallRunoffModelParameterNames.NwrwInflSewer10, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer 10 [m3/s]" } },
                {RainfallRunoffModelParameterNames.NwrwInflSewer11, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer 11 [m3/s]"} },
                {RainfallRunoffModelParameterNames.NwrwInflSewer12, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer 12 [m3/s]" } },
                {RainfallRunoffModelParameterNames.NwrwInflSewerSp1, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer sp1 [m3/s]"} },
                {RainfallRunoffModelParameterNames.NwrwInflSewerSp2, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer sp2 [m3/s]"} },
                {RainfallRunoffModelParameterNames.NwrwInflSewerSp3, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer sp3 [m3/s]"} },
                {RainfallRunoffModelParameterNames.NwrwInflSewerSp4, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer sp4 [m3/s]"} },
                {RainfallRunoffModelParameterNames.NwrwInflSewerSp5, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer sp5 [m3/s]"} },
                {RainfallRunoffModelParameterNames.NwrwInflSewerSp6, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer sp6 [m3/s]"} },
                {RainfallRunoffModelParameterNames.NwrwInflSewerSp7, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer sp7 [m3/s]"} },
                {RainfallRunoffModelParameterNames.NwrwInflSewerSp8, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer sp8 [m3/s]"} },
                {RainfallRunoffModelParameterNames.NwrwInflSewerSp9, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer sp9 [m3/s]" } },
                {RainfallRunoffModelParameterNames.NwrwInflSewerSp10, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer sp10 [m3/s]"} },
                {RainfallRunoffModelParameterNames.NwrwInflSewerSp11, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer sp11 [m3/s]"} },
                {RainfallRunoffModelParameterNames.NwrwInflSewerSp12, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Infl.sewer sp12 [m3/s]" } },
                {RainfallRunoffModelParameterNames.NwrwStorageSp1, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Storage sp1 [m3]"} },
                {RainfallRunoffModelParameterNames.NwrwStorageSp2, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Storage sp2 [m3]"} },
                {RainfallRunoffModelParameterNames.NwrwStorageSp3, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Storage sp3 [m3]"} },
                {RainfallRunoffModelParameterNames.NwrwStorageSp4, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Storage sp4 [m3]"} },
                {RainfallRunoffModelParameterNames.NwrwStorageSp5, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Storage sp5 [m3]"} },
                {RainfallRunoffModelParameterNames.NwrwStorageSp6, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Storage sp6 [m3]"} },
                {RainfallRunoffModelParameterNames.NwrwStorageSp7, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Storage sp7 [m3]"} },
                {RainfallRunoffModelParameterNames.NwrwStorageSp8, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Storage sp8 [m3]"} },
                {RainfallRunoffModelParameterNames.NwrwStorageSp9, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Storage sp9 [m3]"} },
                {RainfallRunoffModelParameterNames.NwrwStorageSp10, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Storage sp10 [m3]"} },
                {RainfallRunoffModelParameterNames.NwrwStorageSp11, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Storage sp11 [m3]"} },
                {RainfallRunoffModelParameterNames.NwrwStorageSp12, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Storage sp12 [m3]"} },
                {RainfallRunoffModelParameterNames.NwrwWadiInflow, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Wadi inflow  [m3/s]" } },
                {RainfallRunoffModelParameterNames.NwrwWadiInfiltr, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Wadi infiltr [m3/s]" } },
                {RainfallRunoffModelParameterNames.NwrwWadiSpill, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Wadi spill   [m3/s]" } },
                {RainfallRunoffModelParameterNames.NwrwWadiDrain, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Wadi drain   [m3/s]" } },
                {RainfallRunoffModelParameterNames.NwrwWadiStorage, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Wadi storage [m3]"} },
                {RainfallRunoffModelParameterNames.NwrwWadiLevel, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "Wadi level   [m]"} },
                {RainfallRunoffModelParameterNames.NwrwDWFCompanies, new HisFileParameter {HisFileName = NwrwNodesHisFileName, ParameterName = "DWF companies [m3/s]" } },
                #endregion

                #region link

                {RainfallRunoffModelParameterNames.LinkFlowOut,new HisFileParameter {HisFileName = linkHisFileName, ParameterName = "Link flow [m3/s]"} },

                #endregion

                #region boundary

                {RainfallRunoffModelParameterNames.BoundaryDischarge,new HisFileParameter {HisFileName = boundaryHisFileName, ParameterName = "Flow [m3/s]"} },

                #endregion
            };
    }

    /// <summary>
    /// Output settings of the RainfallRunoff model. 
    /// RainfallRunoffOutputSettingData gets initialized with a hardcoded array of engineparameters. These hold 
    /// the available parameters supported by the current version of Model.
    /// </summary>
    [Entity(FireOnCollectionChange=false)]
    public class RainfallRunoffOutputSettingData : EditableObjectUnique<long>, ICopyFrom, ICloneable
    {
        private IEventedList<EngineParameter> engineParameters;

        public RainfallRunoffOutputSettingData()
        {
            engineParameters = RainfallRunoff.EngineParameters.EngineMapping();
            var defaultTimeStep = new TimeSpan(0, 1, 0, 0);

            OutputTimeStep = defaultTimeStep;
            AggregationOption = AggregationOptions.Current;
        }

        public AggregationOptions AggregationOption { get; set; }

        public virtual IEventedList<EngineParameter> EngineParameters
        {
            get { return engineParameters; }
            private set // mapped to NHibernate storage!. 
            { engineParameters = value; }
        }
        
        public virtual TimeSpan OutputTimeStep { get; set; }

        [NoNotifyPropertyChange]
        public virtual EngineParameter BoundaryDischarge
        {
            get { return GetEngineParameter(QuantityType.Flow, ElementSet.BoundaryElmSet); }
        }

        #region ICopyFrom Members

        public virtual void CopyFrom(object source)
        {
            var sourceSettings = (RainfallRunoffOutputSettingData) source;
            if (EngineParameters.Count != sourceSettings.EngineParameters.Count)
            {
                throw new ArgumentException("EngineParameters do not match");
            }
            for (int i = 0; i < EngineParameters.Count; i++)
            {
                EngineParameters[i] = (EngineParameter) sourceSettings.EngineParameters[i].Clone();
            }
            OutputTimeStep = sourceSettings.OutputTimeStep;
        }

        #endregion

        public virtual EngineParameter GetEngineParameter(QuantityType quantityType, ElementSet elementSet)
        {
            return EngineParameters.FirstOrDefault(m => m.QuantityType == quantityType && m.ElementSet == elementSet);
        }

        public bool IsOutputEnabledForElementSet(ElementSet elementSet)
        {
            return EngineParameters.Any(ep => ep.ElementSet == elementSet && ep.IsEnabled);
        }

        public void ToggleEngineParametersForElementSet(ElementSet elementSet, bool enable)
        {
            foreach (var parameter in EngineParameters.Where(ep => ep.ElementSet == elementSet))
            {
                parameter.IsEnabled = enable;
            }
        }
        
        public object Clone()
        {
            var clone = new RainfallRunoffOutputSettingData();
            clone.CopyFrom(this);
            return clone;
        }
    }
}