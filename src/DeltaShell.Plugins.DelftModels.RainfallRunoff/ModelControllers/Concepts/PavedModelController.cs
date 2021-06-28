using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers.Concepts
{
    public class PavedModelController : ConceptModelController<PavedData>
    {
        private bool AddPaved(IRainfallRunoffModel model, IRRModelHybridFileWriter writer, PavedData pavedData, IList<ModelLink> links)
        {
            string pavedId = pavedData.Catchment.Name;

            double[] waterUse24HoursPerCapita = CalculateWaterUseFor24Hours(pavedData);

            var capacityMixedAndOrRainfallInM3 = GetFixedCapacityInMm(pavedData, nameof(pavedData.CapacityMixedAndOrRainfall));
            var capacityDryWeatherFlowInM3 = GetFixedCapacityInMm(pavedData, nameof(pavedData.CapacityDryWeatherFlow));
            var initialStreetStorageMm = GetStorageInMm(pavedData, nameof(pavedData.InitialStreetStorage));
            var maximumStreetStorageMm = GetStorageInMm(pavedData, nameof(pavedData.MaximumStreetStorage));
            var initialSewerMixedAndOrRainfallStorageMm = GetStorageInMm(pavedData, nameof(pavedData.InitialSewerMixedAndOrRainfallStorage));
            var maximumSewerMixedAndOrRainfallStorageMm = GetStorageInMm(pavedData, nameof(pavedData.MaximumSewerMixedAndOrRainfallStorage));
            var initialSewerDryWeatherFlowStorageMm = GetStorageInMm(pavedData, nameof(pavedData.InitialSewerDryWeatherFlowStorage));
            var maximumSewerDryWeatherFlowStorageMm = GetStorageInMm(pavedData, nameof(pavedData.MaximumSewerDryWeatherFlowStorage));
            
            int inhabitants = pavedData.NumberOfInhabitants;

            if (pavedData.DryWeatherFlowOptions == PavedEnums.DryWeatherFlowOptions.ConstantDWF
                || pavedData.DryWeatherFlowOptions == PavedEnums.DryWeatherFlowOptions.VariableDWF)
            {
                inhabitants = 0;
            }

            var wwtpLink = pavedData.Catchment.Links.FirstOrDefault(l => l.Target is WasteWaterTreatmentPlant);
            if (wwtpLink != null)
            {
                RainfallRunoffModelController.AddLink(links, pavedData.Catchment, wwtpLink);
            }
            //this link is either the 2nd outflow in non-mixed systems, OR it is an 'overstort' even in mixed systems
            RainfallRunoffModelController.AddLink(links, pavedData.Catchment);

            var mixedLinkType = DischargeTargetToLinkType(pavedData.MixedAndOrRainfallSewerPumpDischarge);
            var dwfLinkType = DischargeTargetToLinkType(pavedData.DryWeatherFlowSewerPumpDischarge);

            var runoffCoeff = (pavedData.SpillingDefinition == PavedEnums.SpillingDefinition.UseRunoffCoefficient)
                                     ? pavedData.RunoffCoefficient
                                     : 0.0;

            int iref = writer.AddPaved(pavedId,
                                         pavedData.CalculationArea,
                                         pavedData.SurfaceLevel,
                                         initialStreetStorageMm,
                                         maximumStreetStorageMm,
                                         initialSewerMixedAndOrRainfallStorageMm,
                                         maximumSewerMixedAndOrRainfallStorageMm,
                                         initialSewerDryWeatherFlowStorageMm,
                                         maximumSewerDryWeatherFlowStorageMm,
                                         ToEngineSewerType(pavedData.SewerType),
                                         pavedData.IsSewerPumpCapacityFixed,
                                         capacityMixedAndOrRainfallInM3,
                                         capacityDryWeatherFlowInM3,
                                         mixedLinkType,
                                         dwfLinkType,
                                         inhabitants, ToEngineDwfComputationOption(pavedData.DryWeatherFlowOptions),
                                         waterUse24HoursPerCapita,
                                         runoffCoeff,
                                         GetMeteoId(model, pavedData), GetAreaAdjustmentFactor(model, pavedData), pavedData.Catchment?.InteriorPoint?.X ?? 0d, pavedData.Catchment?.InteriorPoint?.Y ?? 0d);

            if (!pavedData.IsSewerPumpCapacityFixed)
            {
                SetVariablePumpCapacities(iref, pavedData, writer);
            }


            return true;
        }

        private static double GetFixedCapacityInMm(PavedData pavedData, string propName)
        {
            var capacityInNativeUnit = (double) TypeUtils.GetPropertyValue(pavedData, propName);

            return pavedData.IsSewerPumpCapacityFixed
                       ? RainfallRunoffUnitConverter.ConvertPumpCapacity(
                           PavedEnums.DefaultPumpCapacityUnit,
                           PavedEnums.SewerPumpCapacityUnit.m3_s,
                           capacityInNativeUnit,
                           pavedData.CalculationArea)
                       : 0.0;
        }

        private static double GetStorageInMm(PavedData pavedData, string propName)
        {
            var storageInNativeUnit = (double) TypeUtils.GetPropertyValue(pavedData, propName);

            return RainfallRunoffUnitConverter.ConvertStorage(pavedData.StorageUnit,
                                                              RainfallRunoffEnums.StorageUnit.mm,
                                                              storageInNativeUnit,
                                                              pavedData.CalculationArea);
        }

        private LinkType DischargeTargetToLinkType(PavedEnums.SewerPumpDischargeTarget dischargeTarget)
        {
            switch(dischargeTarget)
            {
                case PavedEnums.SewerPumpDischargeTarget.BoundaryNode:
                    return LinkType.Boundary;
                case PavedEnums.SewerPumpDischargeTarget.WWTP:
                    return LinkType.WasteWaterTreatmentPlant;
                default:
                    throw new ArgumentOutOfRangeException("dischargeTarget");
            }
        }

        private static void SetVariablePumpCapacities(int iref, PavedData pavedData, IRRModelHybridFileWriter writer)
        {
            List<DateTime> mixedCapacities = pavedData.MixedSewerPumpVariableCapacitySeries.Time.Values.ToList();

            List<DateTime> allTimes = mixedCapacities;
            double[] values1 =
                pavedData.MixedSewerPumpVariableCapacitySeries.Components[0].Values.OfType<double>().ToArray();
            var values2 = new double[values1.Length]; //empty array

            if (pavedData.SewerType != PavedEnums.SewerType.MixedSystem)
            {
                //they must be defined for the same time values for the rekenhart
                List<DateTime> dwfCapacities = pavedData.DwfSewerPumpVariableCapacitySeries.Time.Values.ToList();
                allTimes.AddRange(dwfCapacities);
                allTimes = allTimes.Distinct().OrderBy(dt => dt).ToList();

                values1 = new double[allTimes.Count];
                values2 = new double[allTimes.Count];
                int index = 0;
                foreach (DateTime time in allTimes)
                {
                    values1[index] = pavedData.MixedSewerPumpVariableCapacitySeries.Evaluate<double>(time);
                    values2[index] = pavedData.DwfSewerPumpVariableCapacitySeries.Evaluate<double>(time);
                    index++;
                }
            }

            int[] dates = allTimes.Select(RRModelEngineHelper.DateToInt).ToArray();
            int[] times = allTimes.Select(RRModelEngineHelper.TimeToInt).ToArray();

            writer.SetPavedVariablePumpCapacities(iref, dates, times, values1, values2);
        }

        private static double[] CalculateWaterUseFor24Hours(PavedData pavedData)
        {
            var totalWaterUse = new double[24];

            double totalWaterUsePerDay = RainfallRunoffUnitConverter.ConvertWaterUse(pavedData.WaterUseUnit,
                                                                                     PavedEnums.WaterUseUnit.l_day,
                                                                                     pavedData.WaterUse);

            switch (pavedData.DryWeatherFlowOptions)
            {
                case PavedEnums.DryWeatherFlowOptions.NumberOfInhabitantsTimesConstantDWF:
                case PavedEnums.DryWeatherFlowOptions.ConstantDWF:
                    {
                        for (int i = 0; i < 24; i++)
                        {
                            totalWaterUse[i] = totalWaterUsePerDay/24;
                        }

                        break;
                    }
                case PavedEnums.DryWeatherFlowOptions.NumberOfInhabitantsTimesVariableDWF:
                case PavedEnums.DryWeatherFlowOptions.VariableDWF:
                    {
                        for (int i = 0; i < 24; i++)
                        {
                            double percentage = (double) pavedData.VariableWaterUseFunction[i]/100.0;
                            totalWaterUse[i] = percentage*totalWaterUsePerDay;
                        }
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return totalWaterUse;
        }
        
        private static SewerType ToEngineSewerType(PavedEnums.SewerType sewerType)
        {
            return (SewerType) sewerType;
        }

        private static DwfComputationOption ToEngineDwfComputationOption(
            PavedEnums.DryWeatherFlowOptions dryWeatherFlowOptions)
        {
            return (DwfComputationOption) (dryWeatherFlowOptions + 1);
        }

        public override bool CanHandle(ElementSet elementSet)
        {
            return elementSet == ElementSet.PavedElmSet;
        }

        protected override void OnAddArea(IRainfallRunoffModel model, PavedData data, IList<ModelLink> links)
        {
            AddPaved(model, Writer, data, links);
        }
    }
}