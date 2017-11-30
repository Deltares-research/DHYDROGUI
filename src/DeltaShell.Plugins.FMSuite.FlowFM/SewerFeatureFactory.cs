using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerFeatureFactory
    {
        private static ILog Log = LogManager.GetLogger(typeof(SewerFeatureFactory));
        // For now, the types are: Node, Connection, Structure, Surface, Runoff, Discharge, Distribution, Meta
        private static Dictionary<SewerFeatureType, Func<GwswElement, IHydroNetwork, INetworkFeature>> CreateSewerFeature = new Dictionary<SewerFeatureType, Func<GwswElement, IHydroNetwork, INetworkFeature>>
        {
            { SewerFeatureType.Node, CreateSewerCompartment },
            { SewerFeatureType.Connection, CreateSewerConnection },
            { SewerFeatureType.Crosssection, CreateSewerProfile },
            { SewerFeatureType.Structure, CreateSewerStructure }
        };

        public static IEnumerable<INetworkFeature> CreateMultipleInstances(List<GwswElement> listOfElements, IHydroNetwork network = null)
        {
            var networkFeatures = new List<INetworkFeature>();
            foreach (var element in listOfElements)
            {
                var createdFeatures = CreateInstance(element, network);
                if( createdFeatures != null)
                    networkFeatures.Add(createdFeatures);
            }
            return networkFeatures;
        }

        public static INetworkFeature CreateInstance(object element, IHydroNetwork network = null)
        {
            SewerFeatureType elementType;
            var gwswElement = element as GwswElement;
            if (gwswElement != null && Enum.TryParse(gwswElement.ElementTypeName, out elementType))
            {
                if( CreateSewerFeature.ContainsKey(elementType))
                    return CreateSewerFeature[elementType](gwswElement, network);
            }

            return null;
        }

        #region Creating sewer profiles
        
        private static CrossSection CreateSewerProfile(GwswElement gwswElement, IHydroNetwork network = null)
        {
            if (gwswElement == null) return null;
            
            var csIdAttribute = GetAttributeFromList(gwswElement, CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionId);
            var csShapeAttribute = GetAttributeFromList(gwswElement, CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionShape);
            if (csIdAttribute == null || csShapeAttribute == null) return null;
            
            var definitionReader = CrossSectionFactory(csShapeAttribute);
            var readCrossSectionDefinition = definitionReader.ReadCrossSectionDefinition(gwswElement);
            var crossSection = new CrossSection(readCrossSectionDefinition)
            {
                Name = csIdAttribute.ValueAsString
            };

            if (network != null)
            {
                network.SewerProfiles.RemoveAllWhere(sp => sp.Definition.Name == crossSection.Name);
                network.SewerProfiles.Add(crossSection);
            }

            return crossSection;
        }
    
        private static SewerCrossSectionDefinitionReader CrossSectionFactory(GwswAttribute crossSectionTypeAttribute)
        {
            var structureType = GetValueFromDescription<CrossSectionMapping.CrossSectionType>(crossSectionTypeAttribute.ValueAsString);
            switch (structureType)
            {
                case CrossSectionMapping.CrossSectionType.Egg:
                    return new CsdEggDefinitionReader();
                case CrossSectionMapping.CrossSectionType.Arch:
                    return new CsdArchDefinitionReader();
                case CrossSectionMapping.CrossSectionType.Cunette:
                    return new CsdCunetteDefinitionReader();
                case CrossSectionMapping.CrossSectionType.Rectangle:
                    return new CsdRectangleDefinitionReader();
                case CrossSectionMapping.CrossSectionType.Circle:
                    return new CsdCircleDefinitionReader();
                case CrossSectionMapping.CrossSectionType.Trapezoid:
                    return new CsdTrapezoidDefinitionReader();
                default:
                    return null;
            }
        }

        #endregion


        #region Creating Sewer Compartments

        private static INetworkFeature CreateSewerCompartment(GwswElement gwswElement, IHydroNetwork network = null)
        {
            if (gwswElement == null) return null;

            var nodeType = GetAttributeFromList(gwswElement, ManholeMapping.PropertyKeys.NodeType);
            var compartmentType = SewerCompartmentGenerator.GetSewerCompartmentGenerator(nodeType);

            return compartmentType?.Generate(gwswElement, network);
        }

        protected static bool IsValidGwswCompartment(GwswElement gwswElement)
        {
            var manholeName = GetAttributeFromList(gwswElement, ManholeMapping.PropertyKeys.ManholeId);
            if (manholeName == null || manholeName.ValueAsString == string.Empty)
            {
                Log.WarnFormat(
                    Resources
                        .SewerFeatureFactory_CreateManholeNode_There_are_lines_in__Knooppunt_csv__that_do_not_contain_a_Manhole_Id__These_lines_are_not_imported_);
                return false;
            }

            var compartmentName = GetAttributeFromList(gwswElement, ManholeMapping.PropertyKeys.UniqueId);
            if (compartmentName == null || compartmentName.ValueAsString == string.Empty)
            {
                Log.WarnFormat(
                    Resources
                        .SewerFeatureFactory_CreateManHoleCompartment_Manhole_with_manhole_id___0___could_not_be_created__because_one_of_its_compartments_misses_its_unique_id_,
                    manholeName.ValueAsString);
                return false;
            }

            return true;
        }

        #endregion

        #region Creating Sewer Connections

        private static ISewerNetworkFeatureGenerator GetSewerConnectionGeneratorFromGwswAttribute(GwswAttribute sewerTypeAttribute)
        {
            var basicGenerator = new SewerConnectionGenerator();
            if (string.IsNullOrEmpty(sewerTypeAttribute?.ValueAsString)) return basicGenerator;

            var connectionType = GetValueFromDescription<SewerConnectionMapping.ConnectionType>(sewerTypeAttribute.ValueAsString);

            if (IsGwswPipe(connectionType)) return new SewerConnectionPipeGenerator();

            return IsGwswPump(connectionType) ? (ISewerNetworkFeatureGenerator) new SewerPumpGenerator() : basicGenerator;
        }


        private static INetworkFeature CreateSewerConnection(GwswElement gwswElement, IHydroNetwork network = null)
        {
            if (gwswElement == null) return null;

            var sewerTypeAttribute = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.PipeType);
            var connectionGenerator = GetSewerConnectionGeneratorFromGwswAttribute(sewerTypeAttribute);

            return connectionGenerator?.Generate(gwswElement, network);
        }

        protected static bool IsValidGwswSewerConnection(GwswElement gwswElement)
        {
            var nodeIdStart = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart);
            var nodeIdEnd = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd);
            if (nodeIdStart == null || nodeIdEnd == null)
            {
                Log.ErrorFormat(Resources
                    .SewerFeatureFactory_SewerConnectionFactory_Cannot_import_sewer_connection_s__without_Source_and_Target_nodes__Please_check_the_file_for_said_empty_fields);
                return false;
            }

            return true;
        }

        private static bool IsGwswPump(SewerConnectionMapping.ConnectionType connectionType)
        {
            return connectionType == SewerConnectionMapping.ConnectionType.Pump;
        }

        private static bool IsGwswPipe(SewerConnectionMapping.ConnectionType connectionType)
        {
            if (connectionType == SewerConnectionMapping.ConnectionType.ClosedConnection ||
                connectionType == SewerConnectionMapping.ConnectionType.InfiltrationPipe ||
                connectionType == SewerConnectionMapping.ConnectionType.Open)
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Creating Structures

        public static ISewerNetworkFeatureGenerator GetSewerStructureGenerator(GwswAttribute structureTypeAttribute)
        {
            if (structureTypeAttribute == null || structureTypeAttribute.ValueAsString == string.Empty) return null;

            var structureType = GetValueFromDescription<SewerStructureMapping.StructureType>(structureTypeAttribute.ValueAsString);
            switch (structureType)
            {
                case SewerStructureMapping.StructureType.Pump:
                    return new SewerPumpGenerator();
                case SewerStructureMapping.StructureType.Outlet:
                    return new SewerCompartmentOutletGenerator();
                case SewerStructureMapping.StructureType.Orifice:
                        return new SewerConnectionOrificeGenerator();
                default:
                    return new SewerConnectionGenerator();
            }
        }

        private static INetworkFeature CreateSewerStructure(GwswElement gwswElement, IHydroNetwork network = null)
        {
            if (gwswElement == null) return null;

            var structureTypeAttribute = GetAttributeFromList(gwswElement, SewerStructureMapping.PropertyKeys.StructureType);
            var structureGenerator = GetSewerStructureGenerator(structureTypeAttribute);

            return structureGenerator?.Generate(gwswElement, network);
        }

        #endregion

        
        #region Helpers

        protected static void AddStructureToBranch(ISewerConnection connection, IStructure structure)
        {
            structure.Branch = connection;
            structure.Network = connection.Network;
            structure.Chainage = 0;

            if (connection.Geometry != null && connection.Geometry.Coordinates.Any())
            {
                structure.Geometry = new Point(connection.Geometry.Coordinates[0]);
            }
            structure.Name = connection.Name;

            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(structure, connection);
        }

        protected static T GetValueFromDescription<T>(string description)
        {
            try
            {
                return (T) EnumDescriptionAttributeTypeConverter.GetEnumValue<T>(description);
            }
            catch (Exception)
            {
                Log.WarnFormat("Type {0} is not recognized, please check the syntax", description);
            }

            return default(T);
        }

        protected static GwswAttribute GetAttributeFromList(GwswElement element, string attributeName)
        {
            var attribute = element.GwswAttributeList.FirstOrDefault(attr => attr.GwswAttributeType.Key.Equals(attributeName));
            if (attribute == null)
            {
                Log.WarnFormat(Resources.SewerFeatureFactory_GetAttributeFromList_Attribute__0__was_not_found_for_element__1_, attributeName, element.ElementTypeName);
                return null;
            }
            return attribute;
        }

        protected static bool TryParseDoubleElseLogError(GwswAttribute attribute, Type valueType, out double value)
        {
            if (!double.TryParse(ReplaceCommaWithPoint(attribute.ValueAsString), NumberStyles.Any,
                CultureInfo.InvariantCulture, out value))
            {
                Log.ErrorFormat(Resources.SewerFeatureFactory_CreatePipe_Not_possible_to_parse_value__0__into__1_, attribute.ValueAsString, valueType.Name);
            }
            return true;
        }

        protected static bool TryGetDoubleValueElseLogException(string columnKey, IReadOnlyDictionary<string, string> elementValues, string id, string manholeId, out double doubleValue)
        {
            string stringValue;
            if (!elementValues.TryGetValue(columnKey, out stringValue) || stringValue == string.Empty)
            {
                doubleValue = 0.0;
                return false;
            }
            if (!double.TryParse(ReplaceCommaWithPoint(stringValue), NumberStyles.Any, CultureInfo.InvariantCulture, out doubleValue))
                LogManholeWarningMessage(id, "double", columnKey, stringValue, manholeId);
            return true;
        }

        protected static void LogManholeWarningMessage(string id, string dataType, string columnKey, string wrongValue, string manholeId)
        {
            Log.WarnFormat(Resources.SewerFeatureFactory_ThrowException_, id, dataType, columnKey, wrongValue, manholeId);
        }

        private static string ReplaceCommaWithPoint(string doubleString)
        {
            return doubleString.Replace(',', '.');
        }

        #endregion
    }
}
