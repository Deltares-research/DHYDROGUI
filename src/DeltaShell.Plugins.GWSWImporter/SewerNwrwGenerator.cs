using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.FMSuite.FlowFM;

namespace DeltaShell.Plugins.ImportExport.Gwsw
{
    public static class GwswNwrwGenerator 
    {
       public static NwrwData CreateNewNwrwSurfaceData(GwswElement gwswElement)
        {
            //var catchment = new Catchment() {CatchmentType = CatchmentType.NWRW, IsGeometryDerivedFromAreaSize = true};
            var catchment = Catchment.CreateDefault();
            catchment.CatchmentType = CatchmentType.NWRW;
            var nwrwData = new NwrwData(catchment);
            nwrwData.SurfaceLevelDict = new Dictionary<NwrwSurfaceType, double>();
            nwrwData.Name = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.UniqueId).ValueAsString;
            nwrwData.MeteoStationId = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.MeteoStationId).ValueAsString;

            var surfaceType = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.SurfaceId).ValueAsString.Trim();
            double auxDouble;
            var surface = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Surface);
            if (surface.TryGetValueAsDouble(out auxDouble))
            {
                Type type = typeof(NwrwSurfaceType);
                object enumValueFromDescription = type.GetEnumValueFromDescription(surfaceType);
                NwrwSurfaceType nwrwSurfaceType = (NwrwSurfaceType)enumValueFromDescription;
                nwrwData.SurfaceLevelDict[nwrwSurfaceType] = auxDouble;
            }
            
            return nwrwData;
        }

       public static NwrwGlobalData CreateNewNwrwRunoffData(GwswElement gwswElement)
       {
           var gwswNwrwGlobalData = new NwrwGlobalData();

           double auxDouble;

           var surfaceType = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.SurfaceId).ValueAsString;
           gwswNwrwGlobalData.SurfaceType = (NwrwSurfaceType) typeof(NwrwSurfaceType).GetEnumValueFromDescription(surfaceType);

           var surfaceStorage = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.SurfaceStorage);
           if (surfaceStorage.TryGetValueAsDouble(out auxDouble))
               gwswNwrwGlobalData.SurfaceStorage = auxDouble;

           var infiltrationCapacityMax =
               gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.InfiltrationCapacityMax);
           if (infiltrationCapacityMax.TryGetValueAsDouble(out auxDouble))
               gwswNwrwGlobalData.InfiltrationCapacityMax = auxDouble;

           var infiltrationCapacityMin =
               gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.InfiltrationCapacityMin);
           if (infiltrationCapacityMin.TryGetValueAsDouble(out auxDouble))
               gwswNwrwGlobalData.InfiltrationCapacityMin = auxDouble;

           var infiltrationCapacityReduction =
               gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.InfiltrationCapacityReduction);
           if (infiltrationCapacityReduction.TryGetValueAsDouble(out auxDouble))
               gwswNwrwGlobalData.InfiltrationCapacityReduction = auxDouble;

           var infiltrationCapacityRecovery =
               gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.InfiltrationCapacityRecovery);
           if (infiltrationCapacityRecovery.TryGetValueAsDouble(out auxDouble))
               gwswNwrwGlobalData.InfiltrationCapacityRecovery = auxDouble;

           var runoffDelay = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.RunoffDelay);
           if (runoffDelay.TryGetValueAsDouble(out auxDouble))
               gwswNwrwGlobalData.RunoffDelay = auxDouble;

           return gwswNwrwGlobalData;
       }

       public static NwrwDryWeatherFlowDefinition CreateNewNwrwDistributionData(GwswElement gwswElement)
       {
           var gwswNwrwDryWeatherFlowDefinition = new NwrwDryWeatherFlowDefinition();

           double auxDouble;
           int auxInt;

           gwswNwrwDryWeatherFlowDefinition.Name = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.DistributionId).ValueAsString;
           
           var distributionType = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.DistributionType).ValueAsString;
           gwswNwrwDryWeatherFlowDefinition.DistributionType = (DwfDistributionType)typeof(DwfDistributionType).GetEnumValueFromDescription(distributionType);

           var dayNumber = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.DayNumber);
           if (dayNumber.TryGetValueAsInt(out auxInt))
               gwswNwrwDryWeatherFlowDefinition.DayNumber = auxInt;

           #region Daily Volume
           var dailyVolume = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.DailyVolume);
           if (dailyVolume.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.DailyVolume = auxDouble;

           var hourlyPercentage00 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage00);
           if (hourlyPercentage00.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[0] = auxDouble;

           var hourlyPercentage01 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage01);
           if (hourlyPercentage01.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[1] = auxDouble;

           var hourlyPercentage02 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage02);
           if (hourlyPercentage02.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[2] = auxDouble;

           var hourlyPercentage03 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage03);
           if (hourlyPercentage03.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[3] = auxDouble;

           var hourlyPercentage04 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage04);
           if (hourlyPercentage04.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[4] = auxDouble;

           var hourlyPercentage05 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage05);
           if (hourlyPercentage05.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[5] = auxDouble;

           var hourlyPercentage06 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage06);
           if (hourlyPercentage06.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[6] = auxDouble;

           var hourlyPercentage07 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage07);
           if (hourlyPercentage07.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[7] = auxDouble;

           var hourlyPercentage08 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage08);
           if (hourlyPercentage08.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[8] = auxDouble;

           var hourlyPercentage09 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage09);
           if (hourlyPercentage09.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[9] = auxDouble;

           var hourlyPercentage10 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage10);
           if (hourlyPercentage10.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[10] = auxDouble;

           var hourlyPercentage11 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage11);
           if (hourlyPercentage11.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[11] = auxDouble;

           var hourlyPercentage12 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage12);
           if (hourlyPercentage12.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[12] = auxDouble;

           var hourlyPercentage13 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage13);
           if (hourlyPercentage13.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[13] = auxDouble;

           var hourlyPercentage14 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage14);
           if (hourlyPercentage14.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[14] = auxDouble;

           var hourlyPercentage15 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage15);
           if (hourlyPercentage15.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[15] = auxDouble;

           var hourlyPercentage16 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage16);
           if (hourlyPercentage16.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[16] = auxDouble;

           var hourlyPercentage17 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage17);
           if (hourlyPercentage17.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[17] = auxDouble;

           var hourlyPercentage18 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage18);
           if (hourlyPercentage18.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[18] = auxDouble;

           var hourlyPercentage19 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage19);
           if (hourlyPercentage19.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[19] = auxDouble;

           var hourlyPercentage20 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage20);
           if (hourlyPercentage20.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[20] = auxDouble;

           var hourlyPercentage21 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage21);
           if (hourlyPercentage21.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[21] = auxDouble;

           var hourlyPercentage22 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage22);
           if (hourlyPercentage22.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[22] = auxDouble;

           var hourlyPercentage23 = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage23);
           if (hourlyPercentage23.TryGetValueAsDouble(out auxDouble))
               gwswNwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[23] = auxDouble;
           #endregion

            return gwswNwrwDryWeatherFlowDefinition;
       }

       public static NwrwDischargeData CreateNewNwrwDischargeData(GwswElement gwswElement)
       {
           var nwrwDischargeData = new NwrwDischargeData();
           nwrwDischargeData.Name = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.UniqueId).ValueAsString;

           var dischargeType = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.DischargeType).ValueAsString;
           nwrwDischargeData.DischargeType = (DischargeType) typeof(DischargeType).GetEnumValueFromDescription(dischargeType);

           nwrwDischargeData.DischargeId = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.DischargeId).ValueAsString;

           double auxDouble;

           var pollutinUnits = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.PollutingUnits);
           if (pollutinUnits.TryGetValueAsDouble(out auxDouble))
               nwrwDischargeData.PollutingUnits= auxDouble;

           var surface = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Surface);
           if (surface.TryGetValueAsDouble(out auxDouble))
               nwrwDischargeData.Surface = auxDouble;

           return nwrwDischargeData;
       }
    }
}