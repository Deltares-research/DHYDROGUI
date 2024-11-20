using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Plugins.ImportExport.GWSW.Properties;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    internal static class GwswElementValidationExtensions
    {
        public static bool IsGwswOutlet(this GwswAttribute sewerTypeAttribute, ILogHandler logHandler)
        {
            var nodeType = sewerTypeAttribute.GetValueFromDescription<ManholeMapping.NodeType>(logHandler);
            return nodeType == ManholeMapping.NodeType.Outlet;
        }

        public static bool IsGwswOrifice(this GwswAttribute sewerTypeAttribute, ILogHandler logHandler)
        {
            var connectionType = sewerTypeAttribute.GetValueFromDescription<SewerConnectionMapping.ConnectionType>(logHandler);
            return connectionType == SewerConnectionMapping.ConnectionType.Orifice;
        }

        public static bool IsGwswPump(this GwswAttribute sewerTypeAttribute, ILogHandler logHandler)
        {
            var connectionType = sewerTypeAttribute.GetValueFromDescription<SewerConnectionMapping.ConnectionType>(logHandler);
            return connectionType == SewerConnectionMapping.ConnectionType.Pump;
        }

        public static bool IsGwswPipe(this GwswAttribute sewerTypeAttribute, ILogHandler logHandler)
        {
            var connectionType = sewerTypeAttribute.GetValueFromDescription<SewerConnectionMapping.ConnectionType>(logHandler);
            if (connectionType == SewerConnectionMapping.ConnectionType.ClosedConnection ||
                connectionType == SewerConnectionMapping.ConnectionType.InfiltrationPipe ||
                connectionType == SewerConnectionMapping.ConnectionType.Open)
            {
                return true;
            }
            return false;
        }

        public static bool IsGwswWeir(this GwswAttribute sewerTypeAttribute, ILogHandler logHandler)
        {
            var connectionType = sewerTypeAttribute.GetValueFromDescription<SewerConnectionMapping.ConnectionType>(logHandler);
            return connectionType == SewerConnectionMapping.ConnectionType.Crest;
        }

        public static bool IsValidGwswCompartment(this GwswElement gwswElement, ILogHandler logHandler)
        {
            if (gwswElement == null) return false;

            var featureType = GetEnumValueFromDescription<SewerFeatureType>(gwswElement.ElementTypeName);
            var isNodeGwswElement = featureType == SewerFeatureType.Node;
            if (isNodeGwswElement) return true;

            bool isOutletGwswElement;
            var structureTypeAttribute = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.StructureType, logHandler);
            if (structureTypeAttribute == null)
            {
                isOutletGwswElement = false;
            }
            else
            {
                var structureType = GetEnumValueFromDescription<SewerStructureMapping.StructureType>(structureTypeAttribute.ValueAsString);
                isOutletGwswElement = featureType == SewerFeatureType.Structure && structureType == SewerStructureMapping.StructureType.Outlet;
            }

            return isOutletGwswElement;
        }

        private static TEnum GetEnumValueFromDescription<TEnum>(string valueAsString)
        {
            return (TEnum) typeof(TEnum).GetEnumValueFromDescription(valueAsString);
        }

        public static bool IsValidGwswSewerConnection(this GwswElement gwswElement, ILogHandler logHandler)
        {
            if (gwswElement == null || gwswElement.ElementTypeName != SewerFeatureType.Connection.ToString()) return false;

            var nodeIdStart = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.SourceCompartmentId, logHandler);
            var nodeIdEnd = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.TargetCompartmentId, logHandler);
            if (nodeIdStart == null || nodeIdEnd == null)
            {
                logHandler?.ReportErrorFormat(Resources
                                    .SewerFeatureFactory_SewerConnectionFactory_Cannot_import_sewer_connection_s__without_Source_and_Target_nodes__Please_check_the_file_for_said_empty_fields);
                return false;
            }

            return true;
        }

        public static bool IsValidGwswSewerProfile(this GwswElement gwswElement, ILogHandler logHandler)
        {
            if (gwswElement == null) return false;

            var profileId = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileId, logHandler);
            if (!profileId.IsValidAttribute(logHandler))
            {
                logHandler?.ReportError(Resources.GwswElementValidationExtensions_IsValidGwswSewerProfile_Cannot_import_sewer_profile_s__without_profile_id__Please_check__Profiel_csv__for_empty_profile_id_s);
                return false;
            }

            var featureType = (SewerFeatureType)typeof(SewerFeatureType).GetEnumValueFromDescription(gwswElement.ElementTypeName);
            return featureType == SewerFeatureType.Crosssection;
        }

        public static bool IsValidGwswStructure(this GwswElement gwswElement, ILogHandler logHandler)
        {
            if (gwswElement == null) return false;

            var profileId = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.UniqueId, logHandler);
            if (!profileId.IsValidAttribute(logHandler))
            {
                logHandler?.ReportError(Resources.GwswElementValidationExtensions_IsValidGwswStructure_Cannot_import_sewer_structure_s__without_a_unique_id__Please_check__Kunstwerk_csv__for_empty_unique_id_s);
                return false;
            }

            var featureType = (SewerFeatureType)typeof(SewerFeatureType).GetEnumValueFromDescription(gwswElement.ElementTypeName);
            return featureType == SewerFeatureType.Structure;
        }
    }
}