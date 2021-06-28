using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DeltaShell.NGHS.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekRRPavedImporter : PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRPavedImporter)); 

        public override string DisplayName
        {
            get { return "Rainfall Runoff paved data"; }
        }

        public override SobekImporterCategories Category { get; } = SobekImporterCategories.RainfallRunoff;

        protected override void PartialImport()
        {
            log.DebugFormat("Importing paved data ...");

            var rainfallRunoffModel = GetModel<RainfallRunoffModel>();
            var catchmentModelData = rainfallRunoffModel.GetAllModelData()
                                                        .OfType<PavedData>()
                                                        .ToDictionary(rra => rra.Name);
            ReadAndAddOrUpdatePavedData(GetFilePath(SobekFileNames.SobekRRPavedFileName), catchmentModelData);
        }

        private void ReadAndAddOrUpdatePavedData(string filePath, Dictionary<string, PavedData> catchmentModelData)
        {
            var formatPumpCapacityTable = new DataTable();
            formatPumpCapacityTable.Columns.Add(new DataColumn("Time", typeof(DateTime)));
            formatPumpCapacityTable.Columns.Add(new DataColumn("ValueMixed", typeof(double)));
            formatPumpCapacityTable.Columns.Add(new DataColumn("ValueDwf", typeof(double)));

            var pathDwa = filePath.Replace(".3B", ".DWA");
            var pathSto = filePath.Replace(".3B", ".STO");
            var pathTbl = filePath.Replace(".3B", ".TBL"); // for now: no data for Tholen so we don't create it yet

            var dicDwa = new SobekRRDryWeatherFlowReader().Read(pathDwa).ToDictionaryWithErrorDetails(pathDwa, item => item.Id, item => item);
            var dicSto = new SobekRRStorageReader().Read(pathSto).ToDictionaryWithErrorDetails(pathSto, item => item.Id, item => item);
            var dicTbl = new SobekRRTableReader("QC_T", formatPumpCapacityTable).Read(pathTbl).ToDictionaryWithErrorDetails(pathTbl,
                item => item.TableName, item => item);

            foreach (var sobekPaved in new SobekRRPavedReader().Read(filePath))
            {
                if (catchmentModelData.ContainsKey(sobekPaved.Id))
                {
                    var modelData = catchmentModelData[sobekPaved.Id];

                    var pavedData = modelData as PavedData;

                    if (pavedData == null)
                    {
                        log.ErrorFormat("Expected paved data to be present in model for catchment with id {0}", sobekPaved.Id);
                        continue;
                    }

                    pavedData.CalculationArea = sobekPaved.Area;
                    pavedData.SurfaceLevel = sobekPaved.StreetLevel;

                    //Storage
                    if (dicSto.ContainsKey(sobekPaved.StorageId))
                    {
                        var storage = dicSto[sobekPaved.StorageId];
                        pavedData.StorageUnit = RainfallRunoffEnums.StorageUnit.mm;
                        pavedData.MaximumStreetStorage = storage.MaxStreetStorage;
                        pavedData.InitialStreetStorage = storage.InitialStreetStorage;
                        pavedData.MaximumSewerMixedAndOrRainfallStorage = storage.MaxSewerStorageMixedRainfall;
                        pavedData.InitialSewerMixedAndOrRainfallStorage = storage.InitialSewerStorageMixedRainfall;
                        pavedData.MaximumSewerDryWeatherFlowStorage = storage.MaxSewerStorageDWA;
                        pavedData.InitialSewerDryWeatherFlowStorage = storage.InitialSewerStorageDWA;
                    }
                    else
                    {
                        log.ErrorFormat("Storage table {0} has not been found.", sobekPaved.StorageId);
                    }

                    pavedData.SewerType = (PavedEnums.SewerType)sobekPaved.SewerSystem;
                    pavedData.IsSewerPumpCapacityFixed = sobekPaved.CapacitySewerTableId == null;

                    pavedData.CapacityMixedAndOrRainfall = Math.Round(RainfallRunoffUnitConverter.ConvertPumpCapacity(
                        PavedEnums.SewerPumpCapacityUnit.m3_s,
                        PavedEnums.SewerPumpCapacityUnit.m3_min,
                        sobekPaved.CapacitySewerConstantRainfallInM3S, double.NaN), 4);
                    pavedData.CapacityDryWeatherFlow = Math.Round(RainfallRunoffUnitConverter.ConvertPumpCapacity(
                        PavedEnums.SewerPumpCapacityUnit.m3_s,
                        PavedEnums.SewerPumpCapacityUnit.m3_min,
                        sobekPaved.CapacitySewerConstantDWAInM3S, double.NaN), 4);

                    if (!String.IsNullOrEmpty(sobekPaved.CapacitySewerTableId))
                    {
                        if (dicTbl.ContainsKey(sobekPaved.CapacitySewerTableId))
                        {
                            var table = dicTbl[sobekPaved.CapacitySewerTableId];
                            SetPumpCapacitiesFromTableFile(pavedData, table, sobekPaved.Id);
                        }
                        else
                        {
                            log.ErrorFormat("Cannot find paved sewer capacity table with id {0} for paved {1}", sobekPaved.CapacitySewerTableId, sobekPaved.Id);
                        }
                    }

                    switch (sobekPaved.SewerDischarge)
                    {
                        case SewerDischargeType.BothSewerPumpsToOpenWater:
                            pavedData.MixedAndOrRainfallSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.BoundaryNode;
                            pavedData.DryWeatherFlowSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.BoundaryNode;
                            break;
                        case SewerDischargeType.RainfallOrMixedToOpenWaterDWAToBoundary:
                            pavedData.MixedAndOrRainfallSewerPumpDischarge =
                                PavedEnums.SewerPumpDischargeTarget.BoundaryNode;
                            pavedData.DryWeatherFlowSewerPumpDischarge =
                                PavedEnums.SewerPumpDischargeTarget.BoundaryNode;
                            break;
                        case SewerDischargeType.RainfallOrMixedToOpenWaterDWAToWWTP:
                            pavedData.MixedAndOrRainfallSewerPumpDischarge =
                                PavedEnums.SewerPumpDischargeTarget.BoundaryNode;
                            pavedData.DryWeatherFlowSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.WWTP;
                            break;
                        case SewerDischargeType.BothSewerPumpsToBoundary:
                            pavedData.MixedAndOrRainfallSewerPumpDischarge =
                                PavedEnums.SewerPumpDischargeTarget.BoundaryNode;
                            pavedData.DryWeatherFlowSewerPumpDischarge =
                                PavedEnums.SewerPumpDischargeTarget.BoundaryNode;
                            break;
                        case SewerDischargeType.RainfallOrMixedToBoundaryDWAToOpenWater:
                            pavedData.MixedAndOrRainfallSewerPumpDischarge =
                                PavedEnums.SewerPumpDischargeTarget.BoundaryNode;
                            pavedData.DryWeatherFlowSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.BoundaryNode;
                            break;
                        case SewerDischargeType.RainfallOrMixedToBoundaryDWAToWWTP:
                            pavedData.MixedAndOrRainfallSewerPumpDischarge =
                                PavedEnums.SewerPumpDischargeTarget.BoundaryNode;
                            pavedData.DryWeatherFlowSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.WWTP;
                            break;
                        case SewerDischargeType.BothSewerPumpsToWWTP:
                            pavedData.MixedAndOrRainfallSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.WWTP;
                            pavedData.DryWeatherFlowSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.WWTP;
                            break;
                        case SewerDischargeType.RainfallOrMixedToWWTPDWAToOpenWater:
                            pavedData.MixedAndOrRainfallSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.WWTP;
                            pavedData.DryWeatherFlowSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.BoundaryNode;
                            break;
                        case SewerDischargeType.RainfallOrMixedToWWTPDWAToBoundary:
                            pavedData.MixedAndOrRainfallSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.WWTP;
                            pavedData.DryWeatherFlowSewerPumpDischarge =
                                PavedEnums.SewerPumpDischargeTarget.BoundaryNode;
                            break;
                        default:
                            log.WarnFormat("Paved sewer discharge value {0} ({1}) unknown...",
                                           sobekPaved.SewerDischarge, sobekPaved.Id);
                            break;
                    }

                    //meteo
                    pavedData.MeteoStationName = sobekPaved.MeteoStationId;
                    pavedData.AreaAdjustmentFactor = sobekPaved.AreaAjustmentFactor;

                    pavedData.NumberOfInhabitants = sobekPaved.NumberOfPeople;

                    // Dry weather flow
                    if (dicDwa.ContainsKey(sobekPaved.DryWeatherFlowId))
                    {
                        var dwa = dicDwa[sobekPaved.DryWeatherFlowId];
                        pavedData.DryWeatherFlowOptions = ((PavedEnums.DryWeatherFlowOptions)dwa.ComputationOption - 1);
                        if (dwa.ComputationOption == DWAComputationOption.UseTable)
                        {
                            log.WarnFormat(
                                "DWF option 'use table' not supported ({0}). Has been set to ConstantDWAPerHour.",
                                sobekPaved.Id);
                            pavedData.DryWeatherFlowOptions = PavedEnums.DryWeatherFlowOptions.ConstantDWF;
                        }

                        if (pavedData.DryWeatherFlowOptions == PavedEnums.DryWeatherFlowOptions.ConstantDWF
                            || pavedData.DryWeatherFlowOptions == PavedEnums.DryWeatherFlowOptions.NumberOfInhabitantsTimesConstantDWF)
                        {
                            pavedData.WaterUse = dwa.WaterUsePerHourForConstant * 24.0;
                        }
                        else
                        {
                            pavedData.WaterUse = dwa.WaterUsePerDayForVariable;
                        }

                        int i = 0;
                        foreach (var p in dwa.WaterCapacityPerHour)
                        {
                            pavedData.VariableWaterUseFunction[i++] = p;
                        }
                    }
                    else
                    {
                        log.ErrorFormat("Storage table {0} has not been found.", sobekPaved.StorageId);
                    }

                    pavedData.RunoffCoefficient = sobekPaved.SpillingRunoffCoefficient;
                    pavedData.SpillingDefinition = ConvertSobekSpilling(sobekPaved.SpillingOption, sobekPaved.Id);

                    // not used so far:
                    // sobekPaved.SewerOverflowLevelRWAMixed, sobekPaved.SewerOverFlowLevelDWA, sobekPaved.SewerInflowRWAMixed
                    // sobekPaved.SewerInflowDWA,sobekPaved.InitialSaltConcentration, sobekPaved.RunoffOption, sobekPaved.RunoffDelayFactor
                }
                else
                {
                    log.WarnFormat("Rainfall runoff area with id {0} has not been found. Item has been skipped...", sobekPaved.Id);
                }
            }
        }

        private static void SetPumpCapacitiesFromTableFile(PavedData pavedData, DataTable table, string sobekId)
        {
            try
            {
                var times = new List<DateTime>();
                var mixedValues = new List<double>();
                var dwfValues = new List<double>();
                foreach (DataRow row in table.Rows)
                {
                    times.Add((DateTime)row[0]);
                    mixedValues.Add((double)row[1]);
                    dwfValues.Add((double)row[2]);
                }

                pavedData.MixedSewerPumpVariableCapacitySeries.Time.SetValues(times);
                pavedData.DwfSewerPumpVariableCapacitySeries.Time.SetValues(times);
                pavedData.MixedSewerPumpVariableCapacitySeries.Components[0].SetValues(mixedValues);
                pavedData.DwfSewerPumpVariableCapacitySeries.Components[0].SetValues(dwfValues);
            }
            catch (Exception e)
            {
                log.WarnFormat("Exception while setting variable pump capacities for {0}. Skipped...: {1}", sobekId, e.Message);
            }
        }

        private static PavedEnums.SpillingDefinition ConvertSobekSpilling(SpillingOption spillingOption, string id)
        {
            switch (spillingOption)
            {
                case SpillingOption.NoDelay:
                    return PavedEnums.SpillingDefinition.NoDelay;
                case SpillingOption.UsingCoefficient:
                    return PavedEnums.SpillingDefinition.UseRunoffCoefficient;
                case SpillingOption.UsingQHRelation:
                    log.ErrorFormat("{0}: Spilling option 'use QH relation' is not supported, switching to no delay", id);
                    return PavedEnums.SpillingDefinition.NoDelay;
                default:
                    log.ErrorFormat("{0}: Spilling option '{1}' is not unknown, switching to no delay", id, (int)spillingOption);
                    return PavedEnums.SpillingDefinition.NoDelay;
            }
        }
    }
}