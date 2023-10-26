using System;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class GwswNwrwSurfaceDataGenerator : ASewerGenerator, IGwswFeatureGenerator<INwrwFeature>
    {
        public GwswNwrwSurfaceDataGenerator(ILogHandler logHandler)
            : base(logHandler)
        {
        }
        public INwrwFeature Generate(GwswElement gwswElement)
        {
            return gwswElement == null ? null : GwswNwrwGenerator.CreateNewNwrwSurfaceData(gwswElement, logHandler);
        }

        
    }
    public class GwswNwrwDischargeDataGenerator : ASewerGenerator, IGwswFeatureGenerator<INwrwFeature>
    {
        public GwswNwrwDischargeDataGenerator(ILogHandler logHandler)
            : base(logHandler)
        {
        }

        public INwrwFeature Generate(GwswElement gwswElement)
        {
            return gwswElement == null ? null : GwswNwrwGenerator.CreateNewNwrwDischargeData(gwswElement, logHandler);
        }
    }
    public class GwswNwrwRunoffDefinitionGenerator : ASewerGenerator, IGwswFeatureGenerator<INwrwFeature>
    { 
        public GwswNwrwRunoffDefinitionGenerator(ILogHandler logHandler)
            : base(logHandler)
        {
        }

        public INwrwFeature Generate(GwswElement gwswElement)
        {
            return gwswElement == null ? null : GwswNwrwGenerator.CreateNewNwrwRunoffDefinition(gwswElement, logHandler);
        }
    }
    public class GwswNwrwDryWeatherFlowDefinitionGenerator : ASewerGenerator, IGwswFeatureGenerator<INwrwFeature>
    {
        public GwswNwrwDryWeatherFlowDefinitionGenerator(ILogHandler logHandler)
            : base(logHandler)
        {
        }

        public INwrwFeature Generate(GwswElement gwswElement)
        {
            return gwswElement == null ? null : GwswNwrwGenerator.CreateNewNwrwDryWeatherFlowDefinition(gwswElement, logHandler);
        }
    }
    public static class GwswNwrwGenerator 
    {
       public static NwrwSurfaceData CreateNewNwrwSurfaceData(GwswElement gwswElement, ILogHandler logHandler)
        {
            var nwrwSurface = new NwrwSurfaceData();
            try
            {

                nwrwSurface.Name = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.UniqueId, logHandler)
                    .ValueAsString;
                nwrwSurface.MeteoStationId = gwswElement
                    .GetAttributeFromList(SewerConnectionMapping.PropertyKeys.MeteoStationId, logHandler).ValueAsString;
                nwrwSurface.RunoffDefinitionFile = gwswElement
                    .GetAttributeFromList(SewerConnectionMapping.PropertyKeys.RunoffDefinitionFile, logHandler).ValueAsString;

                var surfaceType = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.SurfaceId, logHandler)
                    .ValueAsString.Trim();
                nwrwSurface.NwrwSurfaceType =
                    (NwrwSurfaceType) typeof(NwrwSurfaceType).GetEnumValueFromDescription(surfaceType);

                double auxDouble;
                var surfaceArea = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Surface, logHandler);
                if (surfaceArea.TryGetValueAsDouble(logHandler, out auxDouble))
                    nwrwSurface.SurfaceArea = auxDouble;

                nwrwSurface.Remark = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Remarks, logHandler)
                    .ValueAsString;
            }
            catch (Exception e)
            {
                logHandler?.ReportError(e.Message);
                return null;
            }

            return nwrwSurface;
        }

       public static NwrwDischargeData CreateNewNwrwDischargeData(GwswElement gwswElement, ILogHandler logHandler)
       {
           var nwrwDischargeData = new NwrwDischargeData();
           try
           {
               nwrwDischargeData.Name = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.UniqueId, logHandler).ValueAsString;

               var dischargeType = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.DischargeType, logHandler).ValueAsString;
               nwrwDischargeData.DischargeType = (DischargeType)typeof(DischargeType).GetEnumValueFromDescription(dischargeType);

               nwrwDischargeData.DryWeatherFlowId = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.DischargeId, logHandler).ValueAsString;

               double auxDouble;
               int auxInt;

               var pollutinUnits = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.PollutingUnits, logHandler);
               if (pollutinUnits.TryGetValueAsInt(logHandler, out auxInt))
                   nwrwDischargeData.NumberOfPeople = auxInt;

               var surface = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Surface, logHandler);
               if (surface.TryGetValueAsDouble(logHandler, out auxDouble))
                   nwrwDischargeData.LateralSurface = auxDouble;

               nwrwDischargeData.Remark = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Remarks, logHandler).ValueAsString;
           }
           catch (Exception e)
           {
               logHandler?.ReportError($"Could not import Nwrw discharge data: {e.Message}");
               return null;
           }
           return nwrwDischargeData;
       }

       public static NwrwDefinition CreateNewNwrwRunoffDefinition(GwswElement gwswElement, ILogHandler logHandler)
       {
           var nwrwDefinition = new NwrwDefinition();

           try
           {
               double auxDouble;

               var surfaceType = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.SurfaceId, logHandler)
                   .ValueAsString.Trim();
               nwrwDefinition.Name = surfaceType;
               nwrwDefinition.SurfaceType =
                   (NwrwSurfaceType) typeof(NwrwSurfaceType).GetEnumValueFromDescription(surfaceType);

               var surfaceStorage =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.SurfaceStorage, logHandler);
               if (surfaceStorage.TryGetValueAsDouble(logHandler, out auxDouble))
                   nwrwDefinition.SurfaceStorage = auxDouble;

               var infiltrationCapacityMax =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.InfiltrationCapacityMax, logHandler);
               if (infiltrationCapacityMax.TryGetValueAsDouble(logHandler, out auxDouble))
                   nwrwDefinition.InfiltrationCapacityMax = auxDouble;

               var infiltrationCapacityMin =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.InfiltrationCapacityMin, logHandler);
               if (infiltrationCapacityMin.TryGetValueAsDouble(logHandler, out auxDouble))
                   nwrwDefinition.InfiltrationCapacityMin = auxDouble;

               var infiltrationCapacityReduction =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.InfiltrationCapacityReduction, logHandler);
               if (infiltrationCapacityReduction.TryGetValueAsDouble(logHandler, out auxDouble))
                   nwrwDefinition.InfiltrationCapacityReduction = auxDouble;

               var infiltrationCapacityRecovery =
                   gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.InfiltrationCapacityRecovery, logHandler);
               if (infiltrationCapacityRecovery.TryGetValueAsDouble(logHandler, out auxDouble))
                   nwrwDefinition.InfiltrationCapacityRecovery = auxDouble;

               var runoffDelay = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.RunoffDelay, logHandler);
               if (runoffDelay.TryGetValueAsDouble(logHandler, out auxDouble))
                   nwrwDefinition.RunoffDelay = auxDouble;

               var runoffLength = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.RunoffLength, logHandler);
               if (runoffLength.TryGetValueAsDouble(logHandler, out auxDouble))
                   nwrwDefinition.RunoffLength = auxDouble;

               var runoffSlope = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.RunoffSlope, logHandler);
               if (runoffSlope.TryGetValueAsDouble(logHandler, out auxDouble))
                   nwrwDefinition.RunoffSlope = auxDouble;

               var terrainRoughness = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.TerrainRoughness, logHandler);
               if (terrainRoughness.TryGetValueAsDouble(logHandler, out auxDouble))
                   nwrwDefinition.TerrainRoughness = auxDouble;

               nwrwDefinition.Remark = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Remarks, logHandler)
                   .ValueAsString;
           }
           catch (Exception e)
           {
               logHandler?.ReportError(e.Message);
               return null;
           }
           return nwrwDefinition;
       }

       public static NwrwDryWeatherFlowDefinition CreateNewNwrwDryWeatherFlowDefinition(GwswElement gwswElement, ILogHandler logHandler)
       {
           var nwrwDryWeatherFlowDefinition = new NwrwDryWeatherFlowDefinition();

           try
           {
               double auxDouble;
               int auxInt;

               nwrwDryWeatherFlowDefinition.Name = gwswElement
                   .GetAttributeFromList(SewerConnectionMapping.PropertyKeys.DistributionId, logHandler).ValueAsString;

               var distributionType = gwswElement
                   .GetAttributeFromList(SewerConnectionMapping.PropertyKeys.DistributionType, logHandler).ValueAsString;
               nwrwDryWeatherFlowDefinition.DistributionType =
                   DryweatherFlowDistributionTypeConverter.ConvertStringToDryweatherFlowDistributionType(distributionType);
               
               var dayNumber = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.DayNumber, logHandler);
               if (dayNumber.TryGetValueAsInt(logHandler, out auxInt))
                   nwrwDryWeatherFlowDefinition.DayNumber = auxInt;

               #region Daily Volume

               var dailyVolume = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.DailyVolume, logHandler);
               if (dailyVolume.TryGetValueAsDouble(logHandler, out auxDouble))
               {
                   nwrwDryWeatherFlowDefinition.DailyVolumeVariable = auxDouble;
                   nwrwDryWeatherFlowDefinition.DailyVolumeConstant = auxDouble;
               }

               for (int i = 0; i < SewerConnectionMapping.PropertyKeys.HourlyPercentage.Length; i++)
               {
                   nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume[i] = GetHourlyPercentageDailyVolume(gwswElement, SewerConnectionMapping.PropertyKeys.HourlyPercentage[i], nwrwDryWeatherFlowDefinition.Name, logHandler);
               }
               #endregion

               nwrwDryWeatherFlowDefinition.Remark = gwswElement
                   .GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Remarks, logHandler).ValueAsString;
           }
           catch (Exception e)
           {
               logHandler?.ReportError(e.Message);
               return null;
           }
           return nwrwDryWeatherFlowDefinition;
       }

       private static double GetHourlyPercentageDailyVolume(GwswElement gwswElement, string percentageDayVolumeAtHourKey, string nwrwDryWeatherFlowDefinitionName, ILogHandler logHandler)
       {
           var hourlyPercentage =
               gwswElement.GetAttributeFromList(percentageDayVolumeAtHourKey, logHandler);
           if (!hourlyPercentage.TryGetValueAsDouble(logHandler, out double auxDouble))
           {
               logHandler?.ReportWarning($"Could not retrieve percentage day volume for DryWeatherFlowDefinition {nwrwDryWeatherFlowDefinitionName} at hour {percentageDayVolumeAtHourKey}, using default 0.0.");
           }
           return auxDouble;
       }
    }
}