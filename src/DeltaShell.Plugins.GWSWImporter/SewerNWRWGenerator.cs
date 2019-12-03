using DelftTools.Hydro.SewerFeatures;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.NWRW;
using DeltaShell.Plugins.FMSuite.FlowFM;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public static class GWSWNWRWGenerator 
    {
       public static INwrwFeature CreateNewNWRWData(GwswElement gwswElement)
        {
            var uniqueIdKey = gwswElement.IsValidGwswSewerConnection()
                ? SewerConnectionMapping.PropertyKeys.UniqueId
                : SewerStructureMapping.PropertyKeys.UniqueId;
            var nwrwData = new GwswNwRWData();

            double auxDouble;

            var surfaceStorage = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.SurfaceStorage);
            if (surfaceStorage.TryGetValueAsDouble(out auxDouble))
                nwrwData.SurfaceStorage = auxDouble;

            var infiltrationCapacityMax = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.InfiltrationCapacityMax);
            if (infiltrationCapacityMax.TryGetValueAsDouble(out auxDouble))
                nwrwData.InfiltrationCapacityMax = auxDouble;

            var infiltrationCapacityMin = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.InfiltrationCapacityMin);
            if (infiltrationCapacityMin.TryGetValueAsDouble(out auxDouble))
                nwrwData.InfiltrationCapacityMin = auxDouble;

            var infiltrationCapacityReduction = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.InfiltrationCapacityReduction);
            if (infiltrationCapacityReduction.TryGetValueAsDouble(out auxDouble))
                nwrwData.InfiltrationCapacityReduction = auxDouble;

            var infiltrationCapacityRecovery = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.InfiltrationCapacityRecovery);
            if (infiltrationCapacityRecovery.TryGetValueAsDouble(out auxDouble))
                nwrwData.InfiltrationCapacityRecovery = auxDouble;

            var runoffDelay = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.RunoffDelay);
            if (runoffDelay.TryGetValueAsDouble(out auxDouble))
                nwrwData.RunoffDelay = auxDouble;

            //var runoffLength = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.RunoffLength);
            //if (runoffLength.TryGetValueAsDouble(out auxDouble))
            //    nwrwData.RunoffLength = auxDouble;

            //var runoffSlope = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.RunoffSlope);
            //if (runoffSlope.TryGetValueAsDouble(out auxDouble))
            //    nwrwData.RunoffSlope = auxDouble;

            //var terrainRoughness = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.TerrainRoughness);
            //if (terrainRoughness.TryGetValueAsDouble(out auxDouble))
            //    nwrwData.TerrainRoughness = auxDouble;

            return nwrwData;
        }
    }
}