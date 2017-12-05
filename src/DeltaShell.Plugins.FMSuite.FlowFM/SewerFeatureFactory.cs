using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
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
            var auxNetwork = network ?? new HydroNetwork();
            
            //check if extra elements are needed.
            var auxList = CreateAuxiliarGwswElements(listOfElements);
            //By using the auxiliar network the objects will be added to it and later used by the other list.
            if (auxList.Any())
            {
                var validAuxFeatures = auxList.Select(element => CreateInstance(element, auxNetwork)).Where(createdFeatures => createdFeatures != null).ToList();
                AddAuxFeaturesToNetwork(validAuxFeatures, auxNetwork);
            }

            return listOfElements.Select(element => CreateInstance(element, auxNetwork)).Where(createdFeatures => createdFeatures != null).ToList();
        }

        private static void AddAuxFeaturesToNetwork(IEnumerable<INetworkFeature> features, IHydroNetwork network)
        {
            features.ForEach(f =>
            {
                var item = f as IManhole;
                if( item != null) network.Nodes.Add(item);
            });
        }

        private static List<GwswElement> CreateAuxiliarGwswElements(List<GwswElement> listOfElements)
        {
            var elementsGrouped = listOfElements.GroupBy(el => el.ElementTypeName).ToList();
            SewerFeatureType elementType;
            var nodeGroup = elementsGrouped.FirstOrDefault(group => Enum.TryParse(@group.Key, out elementType) &&
                                                                    elementType == SewerFeatureType.Node);

            var auxList = new List<GwswElement>();
            if (nodeGroup == null) return auxList;
            var manholeGroups = nodeGroup
                .GroupBy(ng => ng
                    .GetAttributeFromList(ManholeMapping.PropertyKeys.ManholeId).GetValidStringValue());

            foreach (var manhole in manholeGroups)
            {
                var nameAttr = manhole.Select(mv => mv.GetAttributeFromList(ManholeMapping.PropertyKeys.ManholeId)).First();
                if (!nameAttr.IsValidAttribute()) continue;

                var xCoords = manhole.Select(mv => mv.GetAttributeFromList(ManholeMapping.PropertyKeys.XCoordinate))
                    .Where(v => v.IsValidAttribute()).ToList();
                var yCoords = manhole.Select(mv => mv.GetAttributeFromList(ManholeMapping.PropertyKeys.YCoordinate))
                    .Where(v => v.IsValidAttribute()).ToList();
                if (!xCoords.Any() || !yCoords.Any()) continue;


                var typeNode = GetAuxGwswAttribute(
                    EnumDescriptionAttributeTypeConverter.GetEnumDescription(ManholeMapping.NodeType.Manhole),
                    typeof(string),
                    ManholeMapping.PropertyKeys.NodeType);

                var xCoordAttr = GetAuxGwswAttribute(
                    GetAverageCoordinate(xCoords).ToString(CultureInfo.InvariantCulture),
                    typeof(double),
                    ManholeMapping.PropertyKeys.XCoordinate);

                var yCoordAttr = GetAuxGwswAttribute(
                    GetAverageCoordinate(yCoords).ToString(CultureInfo.InvariantCulture),
                    typeof(double), 
                    ManholeMapping.PropertyKeys.YCoordinate);

                var auxElement = new GwswElement()
                {
                    ElementTypeName = nodeGroup.Key,
                    GwswAttributeList = new List<GwswAttribute>()
                    {
                        nameAttr,
                        typeNode,
                        xCoordAttr,
                        yCoordAttr
                    }
                };

                auxList.Add(auxElement);
            }

            return auxList;
        }

        private static GwswAttribute GetAuxGwswAttribute(string value, Type attrType, string keyValue)
        {
            return new GwswAttribute()
            {
                ValueAsString = value,
                GwswAttributeType = new GwswAttributeType()
                {
                    AttributeType = attrType,
                    Key = keyValue
                }
            };
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
            GwswAttribute csIdAttribute;
            if (gwswElement == null || !gwswElement.IsValidGwswSewerProfile(out csIdAttribute)) return null;
            var profileShapeId = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileShape);

            // If the cross section definition reader, return a cross section with default shape
            var definitionReader = CrossSectionFactory(profileShapeId);
            if (definitionReader == null)
            {
                Log.WarnFormat(Resources.SewerFeatureFactory_CreateSewerProfile_Shape_was_not_defined_for_sewer_profile___0___in__Profiel_csv___A_default_round_profile_with_diameter_of_400_mm_is_used_for_this_profile_, csIdAttribute.ValueAsString);
                var defaultShape = new CrossSectionStandardShapeRound { Diameter = 0.4 };
                return new CrossSection(new CrossSectionDefinitionStandard(defaultShape)
                {
                    Name = csIdAttribute.GetValidStringValue()
                });
            }

            var readCrossSectionDefinition = definitionReader.ReadSewerProfileDefinition(gwswElement);
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
    
        private static ISewerProfileDefinitionReader CrossSectionFactory(GwswAttribute crossSectionTypeAttribute)
        {
            var structureType = GetValueFromDescription<SewerProfileMapping.SewerProfileType>(crossSectionTypeAttribute);
            switch (structureType)
            {
                case SewerProfileMapping.SewerProfileType.Egg:
                    return new CsdEggDefinitionReader();
                case SewerProfileMapping.SewerProfileType.Arch:
                    return new CsdArchDefinitionReader();
                case SewerProfileMapping.SewerProfileType.Cunette:
                    return new CsdCunetteDefinitionReader();
                case SewerProfileMapping.SewerProfileType.Rectangle:
                    return new CsdRectangleDefinitionReader();
                case SewerProfileMapping.SewerProfileType.Circle:
                    return new CsdCircleDefinitionReader();
                case SewerProfileMapping.SewerProfileType.Trapezoid:
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
            if (!sewerTypeAttribute.IsValidAttribute()) return basicGenerator;
            if (sewerTypeAttribute.IsAuxGwswManhole()) return new SewerManholeGenerator();

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
            
            var structureType = GetValueFromDescription<SewerStructureMapping.StructureType>(structureTypeAttribute);
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

        private static double GetAverageCoordinate(IEnumerable<GwswAttribute> xCoords)
        {
            var cSum = 0.0;
            var validCoords = 0;
            foreach (var xCoord in xCoords)
            {
                var auxDouble = 0.0;
                if (xCoord.TryGetValueAsDouble(out auxDouble)) ++validCoords;
                cSum += auxDouble;
            }
            var xAvgCoord = cSum / validCoords;
            return xAvgCoord;
        }

        public static T GetValueFromDescription<T>(GwswAttribute gwswAttribute)
        {
            var description = gwswAttribute.GetValidStringValue();
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

        public static bool IsAuxGwswManhole(this GwswAttribute sewerTypeAttribute)
        {
            var nodeType = SewerFeatureFactory.GetValueFromDescription<ManholeMapping.NodeType>(sewerTypeAttribute);
            return nodeType == ManholeMapping.NodeType.Manhole;
        }

        public static bool IsGwswOutlet(this GwswAttribute sewerTypeAttribute)
        {
            var nodeType = SewerFeatureFactory.GetValueFromDescription<ManholeMapping.NodeType>(sewerTypeAttribute);
            return nodeType == ManholeMapping.NodeType.Outlet;
        }

        public static bool IsGwswPump(this GwswAttribute sewerTypeAttribute)
        {
            var connectionType = SewerFeatureFactory.GetValueFromDescription<SewerConnectionMapping.ConnectionType>(sewerTypeAttribute);
            return connectionType == SewerConnectionMapping.ConnectionType.Pump;
        }

        public static bool IsGwswPipe(this GwswAttribute sewerTypeAttribute)
        {
            var connectionType = SewerFeatureFactory.GetValueFromDescription<SewerConnectionMapping.ConnectionType>(sewerTypeAttribute);
            if (connectionType == SewerConnectionMapping.ConnectionType.ClosedConnection ||
                connectionType == SewerConnectionMapping.ConnectionType.InfiltrationPipe ||
                connectionType == SewerConnectionMapping.ConnectionType.Open)
            {
                return true;
            }
            return false;
        }

        public static bool IsValidGwswManhole(this GwswElement gwswElement)
        {
            if (gwswElement == null) return false;
            var manholeName = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.ManholeId);
            
            //No need for log message because as per now, GwswManholes are our own creation (check CreateAuxiliarGwswElements)
            if (!manholeName.IsValidAttribute()) return false;

            var typeAttr = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.NodeType);
            var manholeType = SewerFeatureFactory.GetValueFromDescription<ManholeMapping.NodeType>(typeAttr);
            return manholeType == ManholeMapping.NodeType.Manhole;
        }

        public static bool IsValidGwswCompartment(this GwswElement gwswElement)
            {
            if (gwswElement == null) return false;

            var featureType = (SewerFeatureType) EnumDescriptionAttributeTypeConverter.GetEnumValue<SewerFeatureType>(gwswElement.ElementTypeName);
            return featureType == SewerFeatureType.Node;
        }

        public static bool IsValidGwswSewerConnection(this GwswElement gwswElement)
        {
            if (gwswElement == null) return false;
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

        public static bool IsValidGwswSewerProfile(this GwswElement gwswElement, out GwswAttribute profileId)
        {
            profileId = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileId);
            if (!profileId.IsValidAttribute())
            {
                Log.Error(Resources.GwswElementValidationExtensions_IsValidGwswSewerProfile_Cannot_import_sewer_profile_s__without_profile_id__Please_check__Profiel_csv__for_empty_profile_id_s);
                return false;
            }
            return true;
        }
    }
}
