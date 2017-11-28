using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils;
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
            
            var csId = GetAttributeFromList(gwswElement, CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionId);
            var csShape = GetAttributeFromList(gwswElement, CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionShape);
            if (csId == null || csShape == null) return null;
            
            var definitionReader = CrossSectionFactory(csShape.ValueAsString);
            var readCrossSectionDefinition = definitionReader.ReadCrossSectionDefinition(gwswElement);
            var crossSection = new CrossSection(readCrossSectionDefinition)
            {
                Name = csId.ValueAsString
            };
                        
            return crossSection;
        }
    
        private static SewerCrossSectionDefinitionReader CrossSectionFactory(string csShape)
        {
            switch (csShape)
            {
                case "EIV":
                    return new CsdEggDefinitionReader();
                case "HEU":
                    return new CsdHeulDefinitionReader();
                case "MVR":
                    return new CsdMuilDefinitionReader();
                case "RHK":
                    return new CsdRectangleDefinitionReader();
                case "RND":
                    return new CsdCircleDefinitionReader();
                case "TPZ":
                    return new CsdTrapezoidDefinitionReader();
                default:
                    return null;
            }
        }

        #endregion

        #region Creating Manholes

        private static ISewerNetworkFeatureGenerator GetSewerCompartmentGenerator(GwswAttribute sewerTypeAttribute)
        {
            var basicGenerator = new SewerCompartmentGenerator();
            if (string.IsNullOrEmpty(sewerTypeAttribute?.ValueAsString)) return basicGenerator;

            var nodeType = GetValueFromDescription<ManholeMapping.NodeType>(sewerTypeAttribute.ValueAsString);
            return IsGwswOutlet(nodeType) ? new SewerCompartmentOutletGenerator() : basicGenerator;
        }

        private static INetworkFeature CreateSewerCompartment(GwswElement gwswElement, IHydroNetwork network = null)
        {
            if (gwswElement == null) return null;

            var nodeType = GetAttributeFromList(gwswElement, ManholeMapping.PropertyKeys.NodeType);
            var compartmentType = GetSewerCompartmentGenerator(nodeType);
            return compartmentType?.Generate(gwswElement, network);
        }

        private static bool IsGwswOutlet(ManholeMapping.NodeType nodeType)
        {
            return nodeType == ManholeMapping.NodeType.Outlet;
        }

        #endregion

        #region Creating Sewer Connections

        private static ISewerNetworkFeatureGenerator GetSewerConnectionGenerator(GwswAttribute sewerTypeAttribute)
        {
            var basicGenerator = new SewerConnectionGenerator();
            if (string.IsNullOrEmpty(sewerTypeAttribute?.ValueAsString)) return basicGenerator;

            var connectionType = GetValueFromDescription<SewerConnectionMapping.ConnectionType>(sewerTypeAttribute.ValueAsString);

            if (IsGwswPipe(connectionType)) return new SewerConnectionPipeGenerator();

            return IsGwswPump(connectionType) ? new SewerConnectionPumpGenerator() : basicGenerator;
        }

        private static INetworkFeature CreateSewerConnection(GwswElement gwswElement, IHydroNetwork network = null)
        {
            if (gwswElement == null) return null;

            var sewerTypeAttribute = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.PipeType);
            var connectionGenerator = GetSewerConnectionGenerator(sewerTypeAttribute);
            return connectionGenerator?.Generate(gwswElement, network);
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

            var structureType = GetValueFromDescription<StructureMapping.StructureType>(structureTypeAttribute.ValueAsString);
            if (structureType == StructureMapping.StructureType.Pump)
            {
                return new SewerPumpGenerator();
            }
            if (structureType == StructureMapping.StructureType.Outlet)
            {
                return new SewerOutletGenerator();
            }

            return null;
        }

        private static INetworkFeature CreateSewerStructure(GwswElement gwswElement, IHydroNetwork network = null)
        {
            if (gwswElement == null) return null;

            var structureTypeAttribute = GetAttributeFromList(gwswElement, StructureMapping.PropertyKeys.StructureType);
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
