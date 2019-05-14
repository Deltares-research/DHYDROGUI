using System.Collections.Generic;
using DelftTools.Hydro.Structures.KnownStructureProperties;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    public static class HydroModelHelper
    {
        private static readonly Dictionary<string, string> backwardsCompatibilityMapping = new Dictionary<string, string>
        {
            {
                "levelcenter", KnownStructureProperties.CrestLevel
            },
            {
                "sill_level", KnownStructureProperties.CrestLevel
            },
            {
                "crest_level", KnownStructureProperties.CrestLevel
            },
            {
                "gateheight", KnownStructureProperties.GateLowerEdgeLevel
            },
            {
                "lower_edge_level", KnownStructureProperties.GateLowerEdgeLevel
            },
            {
                "door_opening_width", KnownStructureProperties.GateOpeningWidth
            },
            {
                "opening_width", KnownStructureProperties.GateOpeningWidth
            }
        };

        public static string UpdateOldNamesOfStructuresComponentsToNewNamesIfNeeded(string targetName)
        {
            string [] partsTargetName = targetName.Split('.');

            if (partsTargetName.Length > 1 && backwardsCompatibilityMapping.TryGetValue(partsTargetName[partsTargetName.Length - 1], out string newName))
            {
                partsTargetName[partsTargetName.Length - 1] = newName;
                targetName = string.Join(".", partsTargetName);
            }

            return targetName;
        }
    }
}