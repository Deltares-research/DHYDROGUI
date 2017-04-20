using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Units;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    public class EngineParameters
    {
        private static Unit GetCubicMeterUnit()
        {
            return new Unit("cubic meter", "m³");
        }

        private static Unit GetCubicMeterPerSecondUnit()
        {
            return new Unit("cubic meter per second", "m³/s");
        }

        private static Unit GetMillimeterUnit()
        {
            return new Unit("millimeter", "mm");
        }

        private static Unit GetMeterUnit()
        {
            return new Unit("meter", "mm");
        }

        private static Unit GetDegreeCelsiusUnit()
        {
            return new Unit("degree celsius", "°C");
        }

        /// <summary>
        /// Returns the parameters the 'SobekRR' engine supports. 
        /// If the unit is dimensionless, both name and symbol are set to ""
        /// </summary>
        /// <returns></returns>
        public static IEventedList<EngineParameter> EngineMapping()
        {
            return new EventedList<EngineParameter>
                {
                    // paved
                    new EngineParameter(QuantityType.StorageRWA_mm, ElementSet.PavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.PavedStorageRWA, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.StorageDWA_mm, ElementSet.PavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.PavedStorageDWA, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.StorageStreet_mm, ElementSet.PavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.PavedStorageStreet, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.SpillingTotal, ElementSet.PavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.PavedSpillingTotal,
                                        GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.SpillingRWA, ElementSet.PavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.PavedSpillingRWA, GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.SpillingDWA, ElementSet.PavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.PavedSpillingDWA, GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.PumpedTotal, ElementSet.PavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.PavedPumpedTotal, GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.PumpedRWA, ElementSet.PavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.PavedPumpedRWA, GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.PumpedDWA, ElementSet.PavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.PavedPumpedDWA, GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.DWA2RWA, ElementSet.PavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.PavedDWAToRWA, GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.DWA2DWA, ElementSet.PavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.PavedDWAToDWA, GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.RWA2DWA, ElementSet.PavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.PavedRWAToDWA, GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.SurfaceRWA, ElementSet.PavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.PavedSurface_RWA, GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.EvaporationSurface, ElementSet.PavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.PavedEvapSurface, GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.StorageVolDyn, ElementSet.PavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.PavedVolDynStorage, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.Rainfall, ElementSet.PavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.PavedRainfall, GetCubicMeterPerSecondUnit()),

                    // unpaved

                    new EngineParameter(QuantityType.SurfRunoff, ElementSet.UnpavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.UnpavedSurfaceRunoff,
                                        GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.GwOutflow, ElementSet.UnpavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.UnpavedGwOutflow, GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.Rainfall, ElementSet.UnpavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.UnpavedRainfall, GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.EvaporationSurface, ElementSet.UnpavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.UnpavedEvaporationSurface,
                                        GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.Infiltration, ElementSet.UnpavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.UnpavedInfiltration,
                                        GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.Seepage, ElementSet.UnpavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.UnpavedSeepage, GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.EvaporationActual, ElementSet.UnpavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.UnpavedEvaporationActual,
                                        GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.EvaporationPotential, ElementSet.UnpavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.UnpavedEvaporationPotential,
                                        GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.Percolation, ElementSet.UnpavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.UnpavedPercolation,
                                        GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.CapillaryRise, ElementSet.UnpavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.UnpavedCapillaryRise,
                                        GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.GroundwaterLevel, ElementSet.UnpavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.UnpavedGroundwaterLevel, GetMeterUnit()),
                    new EngineParameter(QuantityType.Storage_mm, ElementSet.UnpavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.UnpavedStorageLandmm, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.GroundwaterVolume, ElementSet.UnpavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.UnpavedGroundwaterVolume, GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.Storage_m3, ElementSet.UnpavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.UnpavedStorageLandm3, GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.GroundwaterLevelThreshold, ElementSet.UnpavedElmSet,
                                        DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.UnpavedGroundwaterLevelThreshold,
                                        new Unit("hour", "hour")),
                    new EngineParameter(QuantityType.GroundwaterLevelSurface, ElementSet.UnpavedElmSet,
                                        DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.UnpavedGroundwaterLevelSurface, GetMeterUnit()),
                    new EngineParameter(QuantityType.StorageCoeff, ElementSet.UnpavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.UnpavedStorageCoeff,
                                        new Unit("Storage coefficient", "--")),
                    new EngineParameter(QuantityType.UnsatZoneContent, ElementSet.UnpavedElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.UnpavedUnsaturatedZone, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.UnsaturatedZoneVolume, ElementSet.UnpavedElmSet,
                                        DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.UnpavedUnsaturatedZoneVolume,
                                        GetCubicMeterUnit()),


                    // greenhouse
                    new EngineParameter(QuantityType.Storage_m3, ElementSet.GreenhouseElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.GreenhouseStorageBasins, GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.Flow, ElementSet.GreenhouseElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.GreenhouseFlowBasins,
                                        GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.Rainfall, ElementSet.GreenhouseElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.GreenhouseRainfall,
                                        GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.EvaporationSurface, ElementSet.GreenhouseElmSet,
                                        DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.GreenhouseEvaporation,
                                        GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.WaterUse, ElementSet.GreenhouseElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.GreenhouseWaterUse,
                                        GetCubicMeterPerSecondUnit()),


                    //OpenWater
                    new EngineParameter(QuantityType.Rainfall, ElementSet.OpenWaterElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.OpenWaterRainfall,
                                        GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.EvaporationSurface, ElementSet.OpenWaterElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.OpenWaterEvaporation,
                                        GetCubicMeterPerSecondUnit()),

                    //Sacramento
                    new EngineParameter(QuantityType.SacrUZTWC, ElementSet.SacramentoElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.SacramentoUpperZoneTensionWaterCapacity,
                                        GetMillimeterUnit()),
                    new EngineParameter(QuantityType.SacrUZFWC, ElementSet.SacramentoElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.SacramentoUpperZoneFreeWaterCapacity,
                                        GetMillimeterUnit()),
                    new EngineParameter(QuantityType.SacrLZTWC, ElementSet.SacramentoElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.SacramentoLowerZoneTensionWaterCapacity,
                                        GetMillimeterUnit()),
                    new EngineParameter(QuantityType.SacrLZFSC, ElementSet.SacramentoElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames
                                            .SacramentoLowerZoneFreeSupplementalWaterCapacity,
                                        GetMillimeterUnit()),
                    new EngineParameter(QuantityType.SacrLZFPC, ElementSet.SacramentoElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.SacramentoLowerZoneFreePrimaryWaterCapacity,
                                        GetMillimeterUnit()),
                    new EngineParameter(QuantityType.SacrPrecip, ElementSet.SacramentoElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.SacramentoRainfall, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.SacrPotEvap, ElementSet.SacramentoElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.SacramentoPotEvap, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.SacrActEvap, ElementSet.SacramentoElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.SacramentoActEvap, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.SacrBaseFlow, ElementSet.SacramentoElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.SacramentoBaseFlow, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.SacrSurfFlow, ElementSet.SacramentoElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.SacramentoSurfaceFlow, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.SacrRunoffImpArea, ElementSet.SacramentoElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.SacramentoRunoffImpArea, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.SacrTotalRunoff, ElementSet.SacramentoElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.SacramentoTotalRunoff, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.SacrChannelInflow, ElementSet.SacramentoElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.SacramentoChannelInflow,
                                        GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.SacrSideSubSurfaceOutflow, ElementSet.SacramentoElmSet,
                                        DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.SacramentoSideSubsurfaceOutflow,
                                        GetMillimeterUnit()),
                    new EngineParameter(QuantityType.SacrAddImpAreaContent, ElementSet.SacramentoElmSet,
                                        DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.SacramentoAdditionalImpAreaContent,
                                        GetMillimeterUnit()),

                    //hbv
                    new EngineParameter(QuantityType.RunoffOutflow, ElementSet.HbvElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.HbvOutflow, GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.RunoffRainfall, ElementSet.HbvElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.HbvRainfall, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.RunoffSnowfall, ElementSet.HbvElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.HbvSnowfall, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.RunoffPotEvap, ElementSet.HbvElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.HbvPotEvap, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.RunoffActEvap, ElementSet.HbvElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.HbvActEvap, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.RunoffBaseflow, ElementSet.HbvElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.HbvBaseflow, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.RunoffInterflow, ElementSet.HbvElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.HbvInterflow, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.RunoffQuickflow, ElementSet.HbvElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.HbvQuickflow, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.RunoffDrySnowContent, ElementSet.HbvElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.HbvDrySnowContent, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.RunoffFreeWaterContent, ElementSet.HbvElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.HbvFreeWaterContent, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.RunoffSoilMoisture, ElementSet.HbvElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.HbvSoilMoisture, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.RunoffUpperZoneContent, ElementSet.HbvElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.HbvUpperZoneContent, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.RunoffLowerZoneContent, ElementSet.HbvElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.HbvLowerZoneContent, GetMillimeterUnit()),
                    new EngineParameter(QuantityType.RunoffTemperature, ElementSet.HbvElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.HbvTemperature, GetDegreeCelsiusUnit()),

                    //wwtp
                    new EngineParameter(QuantityType.FlowIn, ElementSet.WWTPElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.WWTPInFlow, GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.Flow, ElementSet.WWTPElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.WWTPOutFlow, GetCubicMeterPerSecondUnit()),



                    // balance per node
                    new EngineParameter(QuantityType.TotalInNonLinks_m3, ElementSet.BalanceNodeElmSet,
                                        DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.NodeBalanceFlowInNonLinks, GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.TotalInViaLinks_m3, ElementSet.BalanceNodeElmSet,
                                        DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.NodeBalanceFlowInViaLinks, GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.TotalOutNonLinks_m3, ElementSet.BalanceNodeElmSet,
                                        DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.NodeBalanceFlowOutNonLinks,
                                        GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.TotalOutViaLinks_m3, ElementSet.BalanceNodeElmSet,
                                        DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.NodeBalanceFlowOutViaLinks,
                                        GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.DeltaStorage_m3, ElementSet.BalanceNodeElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.NodeBalanceDeltaStorage, GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.BalanceError_m3, ElementSet.BalanceNodeElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.NodeBalanceBalanceError, GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.CumInNonLinks_m3, ElementSet.BalanceNodeElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.NodeBalanceCumFlowInNonLinks,
                                        GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.CumInViaLinks_m3, ElementSet.BalanceNodeElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.NodeBalanceCumFlowInViaLinks,
                                        GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.CumOutNonLinks_m3, ElementSet.BalanceNodeElmSet,
                                        DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.NodeBalanceCumFlowOutNonLinks,
                                        GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.CumOutViaLinks_m3, ElementSet.BalanceNodeElmSet,
                                        DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.NodeBalanceCumFlowOutViaLinks,
                                        GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.CumDeltaStorage_m3, ElementSet.BalanceNodeElmSet,
                                        DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.NodeBalanceCumDeltaStorage,
                                        GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.CumBalanceError_m3, ElementSet.BalanceNodeElmSet,
                                        DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.NodeBalanceCumBalanceError,
                                        GetCubicMeterUnit()),

                    // balance model
                    new EngineParameter(QuantityType.Rainfall, ElementSet.BalanceModelElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.ModelBalanceRainfall,
                                        GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.EvaporationPaved, ElementSet.BalanceModelElmSet,
                                        DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.ModelBalanceEvaporationPaved,
                                        GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.EvaporationUnpaved, ElementSet.BalanceModelElmSet,
                                        DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.ModelBalanceEvaporationUnpaved,
                                        GetCubicMeterPerSecondUnit()),
                    new EngineParameter(QuantityType.WaterUse, ElementSet.BalanceModelElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.ModelBalanceUseGreenhouses,
                                        GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.DWFPaved, ElementSet.BalanceModelElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.ModelBalanceDWFPaved, GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.NetSeepageUnpaved, ElementSet.BalanceModelElmSet,
                                        DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.ModelBalanceNetSeepageUnpaved,
                                        GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.StoragePaved, ElementSet.BalanceModelElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.ModelBalanceStoragePaved, GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.StorageUnpaved, ElementSet.BalanceModelElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.ModelBalanceStorageUnpaved,
                                        GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.StorageGreenhouses, ElementSet.BalanceModelElmSet,
                                        DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.ModelBalanceStorageGreenhouses,
                                        GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.StorageWWTP, ElementSet.BalanceModelElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.ModelBalanceStorageWWTP, GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.BoundariesOut, ElementSet.BalanceModelElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.ModelBalanceBoundariesOut, GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.BoundariesIn, ElementSet.BalanceModelElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.ModelBalanceBoundariesIn, GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.ExternalInflowRRRunoff, ElementSet.BalanceModelElmSet,
                                        DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.ModelBalanceExternalInFlowRRRunoff,
                                        GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.StorageChangeRRRunoff, ElementSet.BalanceModelElmSet,
                                        DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.ModelBalanceStorageChangeRRRunoff,
                                        GetCubicMeterUnit()),
                    new EngineParameter(QuantityType.BalanceError_m3, ElementSet.BalanceModelElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.ModelBalanceError, GetCubicMeterUnit()),


                    //link
                    new EngineParameter(QuantityType.Flow, ElementSet.LinkElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.LinkFlowOut, GetCubicMeterPerSecondUnit()),

                    //boundary
                    new EngineParameter(QuantityType.Flow, ElementSet.BoundaryElmSet, DataItemRole.Output,
                                        RainfallRunoffModelParameterNames.BoundaryDischarge,
                                        GetCubicMeterPerSecondUnit()),
                };
        }
    }
}