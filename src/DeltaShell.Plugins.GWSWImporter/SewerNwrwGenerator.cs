using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using System;
using System.Collections.Generic;
using DeltaShell.Plugins.ImportExport.GWSW;

namespace DeltaShell.Plugins.ImportExport.Gwsw
{
    public static class GwswNwrwGenerator 
    {
       public static NwrwSurfaceData CreateNewNwrwSurfaceData(GwswElement gwswElement, IList<string> errorsDuringImport)
        {
            var nwrwSurface = new NwrwSurfaceData();
            try
            {

                nwrwSurface.Name = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.UniqueId)
                    .ValueAsString;
                nwrwSurface.MeteoStationId = gwswElement
                    .GetAttributeFromList(SewerConnectionMapping.PropertyKeys.MeteoStationId).ValueAsString;
                nwrwSurface.RunoffDefinitionFile = gwswElement
                    .GetAttributeFromList(SewerConnectionMapping.PropertyKeys.RunoffDefinitionFile).ValueAsString;

                var surfaceType = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.SurfaceId)
                    .ValueAsString.Trim();
                nwrwSurface.NwrwSurfaceType =
                    (NwrwSurfaceType) typeof(NwrwSurfaceType).GetEnumValueFromDescription(surfaceType);

                double auxDouble;
                var surfaceArea = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Surface);
                if (surfaceArea.TryGetValueAsDouble(out auxDouble))
                    nwrwSurface.SurfaceArea = auxDouble;

                nwrwSurface.Remark = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Remarks)
                    .ValueAsString;
            }
            catch (Exception e)
            {
                errorsDuringImport.Add($"Could not import Nwrw surface data: {e.Message}");
            }

            return nwrwSurface;
        }

       public static NwrwDischargeData CreateNewNwrwDischargeData(GwswElement gwswElement, IList<string> errorsDuringImport)
       {
           var nwrwDischargeData = new NwrwDischargeData();
           try
           {
               nwrwDischargeData.Name = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.UniqueId).ValueAsString;

               var dischargeType = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.DischargeType).ValueAsString;
               nwrwDischargeData.DischargeType = (DischargeType)typeof(DischargeType).GetEnumValueFromDescription(dischargeType);

               nwrwDischargeData.DryWeatherFlowId = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.DischargeId).ValueAsString;

               double auxDouble;
               int auxInt;

                var pollutinUnits = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.PollutingUnits);
               if (pollutinUnits.TryGetValueAsInt(out auxInt))
                   nwrwDischargeData.NumberOfPeople = auxInt;

               var surface = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Surface);
               if (surface.TryGetValueAsDouble(out auxDouble))
                   nwrwDischargeData.LateralSurface = auxDouble;

               nwrwDischargeData.Remark = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Remarks).ValueAsString;
           }
           catch (Exception e)
           {
               errorsDuringImport.Add($"Could not import Nwrw discharge data: {e.Message}");
           }
           
           return nwrwDischargeData;
       }

       public static NwrwDefinition CreateNewNwrwRunoffDefinition(GwswElement gwswElement, IList<string> errorsDuringImport)
       {
           var nwrwDefinition = new NwrwDefinition();

           try
           {
               double auxDouble;

               var surfaceType = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.SurfaceId)
                   .ValueAsString.Trim();
               nwrwDefinition.Name = surfaceType;
               nwrwDefinition.SurfaceType =
                   (NwrwSurfaceType) typeof(NwrwSurfaceType).GetEnumValueFromDescription(surfaceType);

               var surfaceStorage =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.SurfaceStorage);
               if (surfaceStorage.TryGetValueAsDouble(out auxDouble))
                   nwrwDefinition.SurfaceStorage = auxDouble;

               var infiltrationCapacityMax =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.InfiltrationCapacityMax);
               if (infiltrationCapacityMax.TryGetValueAsDouble(out auxDouble))
                   nwrwDefinition.InfiltrationCapacityMax = auxDouble;

               var infiltrationCapacityMin =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.InfiltrationCapacityMin);
               if (infiltrationCapacityMin.TryGetValueAsDouble(out auxDouble))
                   nwrwDefinition.InfiltrationCapacityMin = auxDouble;

               var infiltrationCapacityReduction =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.InfiltrationCapacityReduction);
               if (infiltrationCapacityReduction.TryGetValueAsDouble(out auxDouble))
                   nwrwDefinition.InfiltrationCapacityReduction = auxDouble;

               var infiltrationCapacityRecovery =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.InfiltrationCapacityRecovery);
               if (infiltrationCapacityRecovery.TryGetValueAsDouble(out auxDouble))
                   nwrwDefinition.InfiltrationCapacityRecovery = auxDouble;

               var runoffDelay = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.RunoffDelay);
               if (runoffDelay.TryGetValueAsDouble(out auxDouble))
                   nwrwDefinition.RunoffDelay = auxDouble;

               var runoffLength = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.RunoffLength);
               if (runoffLength.TryGetValueAsDouble(out auxDouble))
                   nwrwDefinition.RunoffLength = auxDouble;

               var runoffSlope = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.RunoffSlope);
               if (runoffSlope.TryGetValueAsDouble(out auxDouble))
                   nwrwDefinition.RunoffSlope = auxDouble;

               var terrainRoughness = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.TerrainRoughness);
               if (terrainRoughness.TryGetValueAsDouble(out auxDouble))
                   nwrwDefinition.TerrainRoughness = auxDouble;

                nwrwDefinition.Remark = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Remarks)
                   .ValueAsString;
           }
           catch (Exception e)
           {
               errorsDuringImport.Add($"Could not import Nwrw runoff data: {e.Message}");
           }

           return nwrwDefinition;
       }

       public static NwrwDryWeatherFlowDefinition CreateNewNwrwDryWeatherFlowDefinition(GwswElement gwswElement,
           IList<string> errorsDuringImport)
       {
           var nwrwDryWeatherFlowDefinition = new NwrwDryWeatherFlowDefinition();

           try
           {
               double auxDouble;
               int auxInt;

               nwrwDryWeatherFlowDefinition.DryWeatherFlowId = gwswElement
                   .GetAttributeFromList(SewerConnectionMapping.PropertyKeys.DistributionId).ValueAsString;
               nwrwDryWeatherFlowDefinition.Name = nwrwDryWeatherFlowDefinition.DryWeatherFlowId;


               var distributionType = gwswElement
                   .GetAttributeFromList(SewerConnectionMapping.PropertyKeys.DistributionType).ValueAsString;
               nwrwDryWeatherFlowDefinition.DistributionType =
                   (DwfDistributionType) typeof(DwfDistributionType).GetEnumValueFromDescription(distributionType);

               var dayNumber = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.DayNumber);
               if (dayNumber.TryGetValueAsInt(out auxInt))
                   nwrwDryWeatherFlowDefinition.DayNumber = auxInt;

               #region Daily Volume

               var dailyVolume = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.DailyVolume);
               if (dailyVolume.TryGetValueAsDouble(out auxDouble))
               {
                   nwrwDryWeatherFlowDefinition.DailyVolumeVariable = auxDouble;
                   nwrwDryWeatherFlowDefinition.DailyVolumeConstant = auxDouble;
               }
                   
               

               var hourlyPercentage00 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage00);
               if (hourlyPercentage00.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[0] = auxDouble;

               var hourlyPercentage01 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage01);
               if (hourlyPercentage01.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[1] = auxDouble;

               var hourlyPercentage02 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage02);
               if (hourlyPercentage02.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[2] = auxDouble;

               var hourlyPercentage03 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage03);
               if (hourlyPercentage03.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[3] = auxDouble;

               var hourlyPercentage04 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage04);
               if (hourlyPercentage04.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[4] = auxDouble;

               var hourlyPercentage05 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage05);
               if (hourlyPercentage05.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[5] = auxDouble;

               var hourlyPercentage06 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage06);
               if (hourlyPercentage06.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[6] = auxDouble;

               var hourlyPercentage07 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage07);
               if (hourlyPercentage07.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[7] = auxDouble;

               var hourlyPercentage08 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage08);
               if (hourlyPercentage08.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[8] = auxDouble;

               var hourlyPercentage09 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage09);
               if (hourlyPercentage09.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[9] = auxDouble;

               var hourlyPercentage10 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage10);
               if (hourlyPercentage10.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[10] = auxDouble;

               var hourlyPercentage11 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage11);
               if (hourlyPercentage11.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[11] = auxDouble;

               var hourlyPercentage12 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage12);
               if (hourlyPercentage12.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[12] = auxDouble;

               var hourlyPercentage13 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage13);
               if (hourlyPercentage13.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[13] = auxDouble;

               var hourlyPercentage14 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage14);
               if (hourlyPercentage14.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[14] = auxDouble;

               var hourlyPercentage15 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage15);
               if (hourlyPercentage15.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[15] = auxDouble;

               var hourlyPercentage16 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage16);
               if (hourlyPercentage16.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[16] = auxDouble;

               var hourlyPercentage17 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage17);
               if (hourlyPercentage17.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[17] = auxDouble;

               var hourlyPercentage18 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage18);
               if (hourlyPercentage18.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[18] = auxDouble;

               var hourlyPercentage19 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage19);
               if (hourlyPercentage19.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[19] = auxDouble;

               var hourlyPercentage20 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage20);
               if (hourlyPercentage20.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[20] = auxDouble;

               var hourlyPercentage21 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage21);
               if (hourlyPercentage21.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[21] = auxDouble;

               var hourlyPercentage22 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage22);
               if (hourlyPercentage22.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[22] = auxDouble;

               var hourlyPercentage23 =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.HourlyPercentage23);
               if (hourlyPercentage23.TryGetValueAsDouble(out auxDouble))
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[23] = auxDouble;

               #endregion

               nwrwDryWeatherFlowDefinition.Remark = gwswElement
                   .GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Remarks).ValueAsString;
           }
           catch (Exception e)
           {
               errorsDuringImport.Add($"Could not import Nwrw dry weather flow data: {e.Message}");
           }

           return nwrwDryWeatherFlowDefinition;
       }

    }
}