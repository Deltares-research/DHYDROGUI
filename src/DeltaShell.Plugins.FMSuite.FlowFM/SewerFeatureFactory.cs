using System;
using System.Collections.Generic;
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
            
            var csIdAttribute = gwswElement.GetAttributeFromList(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionId);
            var csShapeAttribute = gwswElement.GetAttributeFromList(CrossSectionMapping.CrossSectionPropertyKeys.CrossSectionShape);
            if (!csIdAttribute.IsValidAttribute() || !csShapeAttribute.IsValidAttribute()) return null;
            
            var definitionReader = CrossSectionFactory(csShapeAttribute);
            var readCrossSectionDefinition = definitionReader.ReadCrossSectionDefinition(gwswElement);
            var crossSection = new CrossSection(readCrossSectionDefinition)
            {
                Name = csIdAttribute.GetValidStringValue()
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
        private static ISewerNetworkFeatureGenerator GetSewerCompartmentGenerator(GwswAttribute sewerTypeAttribute)
        {
            var basicGenerator = new SewerCompartmentGenerator();
            if (string.IsNullOrEmpty(sewerTypeAttribute?.ValueAsString)) return basicGenerator;

            return sewerTypeAttribute.IsGwswOutlet() ? new SewerCompartmentOutletGenerator() : basicGenerator;
        }

        private static INetworkFeature CreateSewerCompartment(GwswElement gwswElement, IHydroNetwork network = null)
        {
            if (gwswElement == null) return null;

            var nodeType = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.NodeType);
            var compartmentType = GetSewerCompartmentGenerator(nodeType);

            return compartmentType?.Generate(gwswElement, network);
        }

        #endregion

        #region Creating Sewer Connections

        private static ISewerNetworkFeatureGenerator GetSewerConnectionGeneratorFromGwswAttribute(GwswAttribute sewerTypeAttribute)
        {
            var basicGenerator = new SewerConnectionGenerator();
            if (string.IsNullOrEmpty(sewerTypeAttribute?.ValueAsString)) return basicGenerator;

            if (sewerTypeAttribute.IsGwswPipe()) return new SewerConnectionPipeGenerator();

            return sewerTypeAttribute.IsGwswPump() ? (ISewerNetworkFeatureGenerator) new SewerPumpGenerator() : basicGenerator;
        }


        private static INetworkFeature CreateSewerConnection(GwswElement gwswElement, IHydroNetwork network = null)
        {
            if (gwswElement == null) return null;

            var sewerTypeAttribute = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.PipeType);
            var connectionGenerator = GetSewerConnectionGeneratorFromGwswAttribute(sewerTypeAttribute);

            return connectionGenerator?.Generate(gwswElement, network);
        }

        #endregion

        #region Creating Structures

        public static ISewerNetworkFeatureGenerator GetSewerStructureGenerator(GwswAttribute structureTypeAttribute, IHydroNetwork network)
        {
            if (structureTypeAttribute == null || structureTypeAttribute.ValueAsString == string.Empty) return null;

            var structureType = GetValueFromDescription<SewerStructureMapping.StructureType>(structureTypeAttribute.ValueAsString);
            switch (structureType)
            {
                case SewerStructureMapping.StructureType.Pump:
                    if (network != null) return new SewerPumpGenerator();
                    Log.ErrorFormat(Resources.SewerPumpGenerator_CreatePumpFromGwswStructure_Pump_s__cannot_be_created_without_a_network_previously_defined_);
                    return null;
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

            var structureTypeAttribute = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.StructureType);
            var structureGenerator = GetSewerStructureGenerator(structureTypeAttribute, network);

            return structureGenerator?.Generate(gwswElement, network);
        }

        #endregion

        
        #region Helpers

        public static void AddStructureToBranch(ISewerConnection connection, IStructure structure)
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

        public static T GetValueFromDescription<T>(string description)
        {
            try
            {
                return (T) EnumDescriptionAttributeTypeConverter.GetEnumValue<T>(description);
            }
            catch (Exception)
            {
                Log.WarnFormat(Resources.SewerFeatureFactory_GetValueFromDescription_Type__0__is_not_recognized__please_check_the_syntax, description);
            }

            return default(T);
        }

        #endregion
    }

    public static class GwswElementValidationExtensions
    {
        private static ILog Log = LogManager.GetLogger(typeof(GwswElementValidationExtensions));

        public static bool IsGwswOutlet(this GwswAttribute sewerTypeAttribute)
        {
            var nodeType = SewerFeatureFactory.GetValueFromDescription<ManholeMapping.NodeType>(sewerTypeAttribute.ValueAsString);
            return nodeType == ManholeMapping.NodeType.Outlet;
        }

        public static bool IsGwswPump(this GwswAttribute sewerTypeAttribute)
        {
            var connectionType = SewerFeatureFactory.GetValueFromDescription<SewerConnectionMapping.ConnectionType>(sewerTypeAttribute.ValueAsString);
            return connectionType == SewerConnectionMapping.ConnectionType.Pump;
        }

        public static bool IsGwswPipe(this GwswAttribute sewerTypeAttribute)
        {
            var connectionType = SewerFeatureFactory.GetValueFromDescription<SewerConnectionMapping.ConnectionType>(sewerTypeAttribute.ValueAsString);
            if (connectionType == SewerConnectionMapping.ConnectionType.ClosedConnection ||
                connectionType == SewerConnectionMapping.ConnectionType.InfiltrationPipe ||
                connectionType == SewerConnectionMapping.ConnectionType.Open)
            {
                return true;
            }
            return false;
        }

        public static bool IsValidGwswCompartment(this GwswElement gwswElement)
        {
            var manholeName = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.ManholeId);
            if (manholeName == null || manholeName.ValueAsString == string.Empty)
            {
                Log.WarnFormat(
                    Resources
                        .SewerFeatureFactory_CreateManholeNode_There_are_lines_in__Knooppunt_csv__that_do_not_contain_a_Manhole_Id__These_lines_are_not_imported_);
                return false;
            }

            var compartmentName = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.UniqueId);
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

        public static bool IsValidGwswSewerConnection(this GwswElement gwswElement)
        {
            var nodeIdStart = gwswElement.GetAttributeFromList( SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart);
            var nodeIdEnd = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd);
            if (nodeIdStart == null || nodeIdEnd == null)
            {
                Log.ErrorFormat(Resources
                    .SewerFeatureFactory_SewerConnectionFactory_Cannot_import_sewer_connection_s__without_Source_and_Target_nodes__Please_check_the_file_for_said_empty_fields);
                return false;
            }

            return true;
        }
    }
}
