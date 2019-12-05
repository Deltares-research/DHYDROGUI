using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.NWRW;
using DeltaShell.Plugins.FMSuite.FlowFM;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public static class GWSWNWRWGenerator 
    {
       public static NWRWData CreateNewNWRWSurfaceData(GwswElement gwswElement)
        {
            var nwrwData = new NWRWData(new Catchment());
            nwrwData.SurfaceLevelDict = new Dictionary<NWRWSurfaceType, double>();
            nwrwData.Name = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.UniqueId).ValueAsString;
            nwrwData.MeteoStationId = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.MeteoStationId).ValueAsString;

            var surfaceType = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.SurfaceId).ValueAsString;
            double auxDouble;
            var surface = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Surface);
            if (surface.TryGetValueAsDouble(out auxDouble))
            {
                var nwrwSurfaceType = (NWRWSurfaceType)typeof(NWRWSurfaceType).GetEnumValueFromDescription(surfaceType);
                nwrwData.SurfaceLevelDict[nwrwSurfaceType] = auxDouble;
            }
            
            return nwrwData;
        }

       public static GwswNWRWGlobalData CreateNewNWRWRunoffData(GwswElement gwswElement)
       {
           var gwswNWRWGlobalData = new GwswNWRWGlobalData();

           double auxDouble;

           var surfaceType = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.SurfaceId).ValueAsString;
           gwswNWRWGlobalData.SurfaceType = (NWRWSurfaceType) typeof(NWRWSurfaceType).GetEnumValueFromDescription(surfaceType);

           var surfaceStorage = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.SurfaceStorage);
           if (surfaceStorage.TryGetValueAsDouble(out auxDouble))
               gwswNWRWGlobalData.SurfaceStorage = auxDouble;

           var infiltrationCapacityMax =
               gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.InfiltrationCapacityMax);
           if (infiltrationCapacityMax.TryGetValueAsDouble(out auxDouble))
               gwswNWRWGlobalData.InfiltrationCapacityMax = auxDouble;

           var infiltrationCapacityMin =
               gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.InfiltrationCapacityMin);
           if (infiltrationCapacityMin.TryGetValueAsDouble(out auxDouble))
               gwswNWRWGlobalData.InfiltrationCapacityMin = auxDouble;

           var infiltrationCapacityReduction =
               gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.InfiltrationCapacityReduction);
           if (infiltrationCapacityReduction.TryGetValueAsDouble(out auxDouble))
               gwswNWRWGlobalData.InfiltrationCapacityReduction = auxDouble;

           var infiltrationCapacityRecovery =
               gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.InfiltrationCapacityRecovery);
           if (infiltrationCapacityRecovery.TryGetValueAsDouble(out auxDouble))
               gwswNWRWGlobalData.InfiltrationCapacityRecovery = auxDouble;

           var runoffDelay = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.RunoffDelay);
           if (runoffDelay.TryGetValueAsDouble(out auxDouble))
               gwswNWRWGlobalData.RunoffDelay = auxDouble;

           return gwswNWRWGlobalData;
       }
    }
}