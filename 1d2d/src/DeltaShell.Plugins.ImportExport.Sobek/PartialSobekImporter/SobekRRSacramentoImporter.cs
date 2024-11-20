using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekRRSacramentoImporter: PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRSacramentoImporter));

        public override string DisplayName
        {
            get { return "Rainfall Runoff Sacramento HBV data"; }
        }

        public override SobekImporterCategories Category { get; } = SobekImporterCategories.RainfallRunoff;

        protected override void PartialImport()
        {
            log.DebugFormat("Importing Sacramento and HBV data ...");

            var rainfallRunoffModel = GetModel<RainfallRunoffModel>();
            var sacramentoCatchmentData = rainfallRunoffModel.GetAllModelData()
                                                        .OfType<SacramentoData>()
                                                        .ToDictionary(rra => rra.Name);
            ReadAndAddOrUpdateSacramentoAreas(GetFilePath(SobekFileNames.SobekRRSacramentoFileName), sacramentoCatchmentData);

            var hbvCatchmentData = rainfallRunoffModel.GetAllModelData()
                                                        .OfType<HbvData>()
                                                        .ToDictionary(rra => rra.Name);
            ReadAndAddOrUpdateHbvAreas(GetFilePath(SobekFileNames.SobekRRSacramentoFileName), hbvCatchmentData);
        }

        private void ReadAndAddOrUpdateHbvAreas(string filePath, Dictionary<string, HbvData> catchmentModelData)
        {
            var snowDict = new SobekRRHbvSnowReader().Read(filePath).ToDictionaryWithErrorDetails(filePath, s => s.Id);
            var soilDict = new SobekRRHbvSoilReader().Read(filePath).ToDictionaryWithErrorDetails(filePath, s => s.Id);
            var flowDict = new SobekRRHbvFlowReader().Read(filePath).ToDictionaryWithErrorDetails(filePath, f => f.Id);
            var hiniDict = new SobekRRHbvHiniReader().Read(filePath).ToDictionaryWithErrorDetails(filePath, h => h.Id);
            
            foreach (var hbvRecord in new SobekRRHbvReader().Read(filePath))
            {
                if (!catchmentModelData.ContainsKey(hbvRecord.Id))
                {
                    log.WarnFormat("Rainfall runoff Sacramento HBV area with id {0} has not been found. Item has been skipped...", hbvRecord.Id);
                    continue;
                }

                var hbvModelData = catchmentModelData[hbvRecord.Id];
                hbvModelData.Area = hbvRecord.Area;
                hbvModelData.SurfaceLevel = hbvRecord.SurfaceLevel;
                hbvModelData.AreaAdjustmentFactor = hbvRecord.AreaAdjustmentFactor;

                if (GetModel<RainfallRunoffModel>().MeteoStations.Contains(hbvRecord.MeteoStationId))
                {
                    hbvModelData.MeteoStationName = hbvRecord.MeteoStationId;
                }
                if (GetModel<RainfallRunoffModel>().TemperatureStations.Contains(hbvRecord.TemperatureStationId))
                {
                    hbvModelData.TemperatureStationName = hbvRecord.TemperatureStationId;
                }

                if (snowDict.ContainsKey(hbvRecord.SnowId))
                {
                    SetSnowData(hbvModelData, snowDict[hbvRecord.SnowId]);
                }  
                if (soilDict.ContainsKey(hbvRecord.SoilId))
                {
                    SetSoilData(hbvModelData, soilDict[hbvRecord.SoilId]);
                }
                if (flowDict.ContainsKey(hbvRecord.FlowId))
                {
                    SetFlowData(hbvModelData, flowDict[hbvRecord.FlowId]);
                }
                if (hiniDict.ContainsKey(hbvRecord.HiniId))
                {
                    SetHiniData(hbvModelData, hiniDict[hbvRecord.HiniId]);
                }
            }
        }

        private static void SetHiniData(HbvData hbvModelData, SobekRRHbvHini hini)
        {
            hbvModelData.InitialDrySnowContent = hini.InitialDrySnowContent;
            hbvModelData.InitialFreeWaterContent = hini.InitialFreeWaterContent;
            hbvModelData.InitialSoilMoistureContents = hini.InitialSoilMoistureContents;
            hbvModelData.InitialUpperZoneContent = hini.InitialUpperZoneContent;
            hbvModelData.InitialLowerZoneContent = hini.InitialLowerZoneContent;
        }

        private void SetFlowData(HbvData hbvModelData, SobekRRHbvFlow flow)
        {
            hbvModelData.BaseFlowReservoirConstant = flow.BaseFlowReservoirConstant;
            hbvModelData.InterflowReservoirConstant = flow.InterflowReservoirConstant;
            hbvModelData.QuickFlowReservoirConstant = flow.QuickFlowReservoirConstant;
            hbvModelData.UpperZoneThreshold = flow.UpperZoneThreshold;
            hbvModelData.MaximumPercolation = flow.MaximumPercolation;
        }

        private static void SetSnowData(HbvData hbvModelData, SobekRRHbvSnow snow)
        {
            hbvModelData.SnowFallTemperature = snow.SnowFallTemperature;
            hbvModelData.SnowMeltTemperature = snow.SnowMeltTemperature;
            hbvModelData.SnowMeltingConstant = snow.SnowMeltingConstant;
            hbvModelData.TemperatureAltitudeConstant = snow.TemperatureAltitudeConstant;
            hbvModelData.FreezingEfficiency = snow.FreezingEfficiency;
            hbvModelData.FreeWaterFraction = snow.FreeWaterFraction;
        }

        private static void SetSoilData(HbvData hbvModelData, SobekRRHbvSoil soil)
        {
            hbvModelData.Beta = soil.Beta;
            hbvModelData.FieldCapacity = soil.FieldCapacity;
            hbvModelData.FieldCapacityThreshold = soil.FieldCapacityThreshold;
        }

        private void ReadAndAddOrUpdateSacramentoAreas(string filePath, Dictionary<string, SacramentoData> catchmentModelData)
        {
            var capcFilePath = filePath.Replace(".3B", ".CAP");
            var oparFilePath = filePath.Replace(".3B", ".OTH");
            var unihFilePath = filePath.Replace(".3B", ".UH");

            var capcDict = new SobekRRCapacitiesReader().Read(capcFilePath).ToDictionaryWithErrorDetails(capcFilePath, c => c.Id);
            var capcDictMain = new SobekRRCapacitiesReader().Read(filePath).ToDictionaryWithErrorDetails(filePath, c => c.Id);
            var oparDict = new SobekRRSacramentoParametersReader().Read(oparFilePath).ToDictionaryWithErrorDetails(oparFilePath, c => c.Id);
            var oparDictMain = new SobekRRSacramentoParametersReader().Read(filePath).ToDictionaryWithErrorDetails(filePath, c => c.Id);
            var unihDict = new SobekRRUnitHydrographReader().Read(unihFilePath).ToDictionaryWithErrorDetails(unihFilePath, c => c.Id);
            var unihDictMain = new SobekRRUnitHydrographReader().Read(filePath).ToDictionaryWithErrorDetails(filePath, c => c.Id);

            foreach (var sacramentoRecord in new SobekRRSacramentoReader().Read(filePath))
            {
                if (!catchmentModelData.ContainsKey(sacramentoRecord.Id))
                {
                    log.WarnFormat("Rainfall runoff Sacramento area with id {0} has not been found. Item has been skipped...", sacramentoRecord.Id);
                    continue;
                }

                var sacramentoModelData = catchmentModelData[sacramentoRecord.Id];
                sacramentoModelData.Area = sacramentoRecord.Area;
                sacramentoModelData.AreaAdjustmentFactor = sacramentoRecord.AreaAdjustmentFactor;

                if (GetModel<RainfallRunoffModel>().MeteoStations.Contains(sacramentoRecord.MeteoStationId))
                {
                    sacramentoModelData.MeteoStationName = sacramentoRecord.MeteoStationId;
                }

                // capacities
                if (capcDict.ContainsKey(sacramentoRecord.CapacityId))
                {
                    SetCapacities(sacramentoModelData, capcDict[sacramentoRecord.CapacityId]);
                }
                else if (capcDictMain.ContainsKey(sacramentoRecord.CapacityId))
                {
                    SetCapacities(sacramentoModelData, capcDictMain[sacramentoRecord.CapacityId]);
                }

                // unit hydrograph
                if (unihDict.ContainsKey(sacramentoRecord.UnitHydrographId))
                {
                    SetUnitHydrograph(sacramentoModelData, unihDict[sacramentoRecord.UnitHydrographId]);
                }
                else if (unihDictMain.ContainsKey(sacramentoRecord.UnitHydrographId))
                {
                    SetUnitHydrograph(sacramentoModelData, unihDictMain[sacramentoRecord.UnitHydrographId]);
                }

                // other parameters
                if (oparDict.ContainsKey(sacramentoRecord.OtherParamsId))
                {
                    SetOtherParameters(sacramentoModelData, oparDict[sacramentoRecord.OtherParamsId]);
                }
                else if (oparDictMain.ContainsKey(sacramentoRecord.OtherParamsId))
                {
                    SetOtherParameters(sacramentoModelData, oparDictMain[sacramentoRecord.OtherParamsId]);
                }
            }
        }

        private static void SetCapacities(SacramentoData sacramentoModelData, SobekRRCapacities capacities)
        {
            sacramentoModelData.UpperZoneTensionWaterStorageCapacity =
                capacities.UpperZoneTensionWaterStorageCapacity;
            sacramentoModelData.UpperZoneTensionWaterInitialContent =
                capacities.UpperZoneTensionWaterInitialContent;
            sacramentoModelData.UpperZoneFreeWaterStorageCapacity =
                capacities.UpperZoneFreeWaterStorageCapacity;
            sacramentoModelData.UpperZoneFreeWaterInitialContent =
                capacities.UpperZoneFreeWaterInitialContent;
            sacramentoModelData.UpperZoneFreeWaterDrainageRate =
                capacities.UpperZoneFreeWaterDrainageRate;
            sacramentoModelData.LowerZoneTensionWaterStorageCapacity =
                capacities.LowerZoneTensionWaterStorageCapacity;
            sacramentoModelData.LowerZoneTensionWaterInitialContent =
                capacities.LowerZoneTensionWaterInitialContent;
            sacramentoModelData.LowerZoneSupplementalFreeWaterStorageCapacity =
                capacities.LowerZoneSupplementalFreeWaterStorageCapacity;
            sacramentoModelData.LowerZoneSupplementalFreeWaterInitialContent =
                capacities.LowerZoneSupplementalFreeWaterInitialContent;
            sacramentoModelData.LowerZoneSupplementalFreeWaterDrainageRate =
                capacities.LowerZoneSupplementalFreeWaterDrainageRate;
            sacramentoModelData.LowerZonePrimaryFreeWaterStorageCapacity =
                capacities.LowerZonePrimaryFreeWaterStorageCapacity;
            sacramentoModelData.LowerZonePrimaryFreeWaterInitialContent =
                capacities.LowerZonePrimaryFreeWaterInitialContent;
            sacramentoModelData.LowerZonePrimaryFreeWaterDrainageRate =
                capacities.LowerZonePrimaryFreeWaterDrainageRate;
        }

        private static void SetUnitHydrograph(SacramentoData sacramentoModelData, SobekRRUnitHydrograph unitHydrograph)
        {
            sacramentoModelData.HydrographStep = unitHydrograph.Stepsize;
            for (int i = 0; i < sacramentoModelData.HydrographValues.Count; i++)
            {
                sacramentoModelData.HydrographValues[i] = unitHydrograph.Values[i];
            }
        }

         private static void SetOtherParameters(SacramentoData sacramentoModelData, SobekRRSacramentoParameters parameters)
        {
            sacramentoModelData.PercolationIncrease = parameters.PercolationIncrease;
            sacramentoModelData.PercolationExponent = parameters.PercolationExponent;
            sacramentoModelData.PercolatedWaterFraction = parameters.PercolatedWaterFraction;
            sacramentoModelData.FreeWaterFraction = parameters.FreeWaterFraction;
            sacramentoModelData.RatioUnobservedToObservedBaseFlow = parameters.RatioUnobservedToObservedBaseFlow;
            sacramentoModelData.SubSurfaceOutflow = parameters.SubSurfaceOutflow;
            sacramentoModelData.PermanentlyImperviousFraction = parameters.PermanentlyImperviousFraction;
            sacramentoModelData.RainfallImperviousFraction = parameters.RainfallImperviousFraction;
            sacramentoModelData.WaterAndVegetationAreaFraction = parameters.WaterAndVegetationAreaFraction;
            sacramentoModelData.UpperRainfallThreshold = parameters.UpperRainfallThreshold;
            sacramentoModelData.LowerRainfallThreshold = parameters.LowerRainfallThreshold;
            sacramentoModelData.TimeIntervalIncrement = parameters.TimeIntervalIncrement;
        }

    }
}
