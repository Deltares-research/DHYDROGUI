using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public static class SewerFeatureFactory
    {
        private static ILog Log = LogManager.GetLogger(typeof(SewerFeatureFactory));

        #region Creators
        
        /// <summary>
        /// Generate multiple Network features from a list of GwswElements. Additionally use an existent HydroNetwork to find elements
        /// already present on it or add auxiliar ones such as structures or profiles.
        /// </summary>
        /// <param name="listOfElements">List of GwswElements.</param>
        /// <param name="network">HydroNetwork</param>
        /// <returns>List of Network Features representing the elements stored in the <param name="listOfElements"/></returns>
        public static IEnumerable<INetworkFeature> CreateMultipleInstances(IList<GwswElement> listOfElements, IHydroNetwork network)
        {
            var auxNetwork = network ?? new HydroNetwork();
            
            //check if extra elements are needed.
            var auxList = CreateAuxiliarGwswElements(listOfElements);
            //By using the auxiliar network the objects will be added to it and later used by the other list.
            if (auxList.Any())
            {
                var validAuxFeatures = CreateInstances(auxList, auxNetwork).ToList();
                AddAuxFeaturesToNetwork(validAuxFeatures, auxNetwork);
            }

            var networkFeatures = CreateInstances(listOfElements, network).Where(c => c != null);
            return networkFeatures;
        }

        /// <summary>
        /// Generates a single Network Feature out of a GwswElement.
        /// </summary>
        /// <param name="gwswElement">Collection of attributes and values extracted from a Csv Element.</param>
        /// <param name="network">HydroNetwork where we can find existing Network Features or add new ones.</param>
        /// <returns>Single Network Feature representing the <param name="gwswElement"/> given as a parameter.</returns>
        public static INetworkFeature CreateInstance(GwswElement gwswElement, IHydroNetwork network = null, object importHelper = null)
        {
            var generator = GetSewerNetworkFeatureGenerator(gwswElement, network);
            return generator?.Generate(gwswElement, network, importHelper);
        }

        private static IEnumerable<INetworkFeature> CreateInstances(IList<GwswElement> listOfElements, IHydroNetwork network = null)
        {
            var createdNetworkFeatures = new List<INetworkFeature>();
            // try parse all element type -> make list of items of SewerFeatureTypes
            var elementTypesList = new List<KeyValuePair<SewerFeatureType, GwswElement>>();

            foreach (var gwswElement in listOfElements)
            {
                SewerFeatureType elementType;
                if(!Enum.TryParse(gwswElement?.ElementTypeName, out elementType)) continue;

                elementTypesList.Add(new KeyValuePair<SewerFeatureType, GwswElement>(elementType, gwswElement));
            }

            // node types
            var nodeTypes = elementTypesList.Where(k => k.Key == SewerFeatureType.Node).Select(k => k.Value).ToList();
            if (nodeTypes.Any())
            {
                createdNetworkFeatures.AddRange(nodeTypes.Select(n => CreateInstance(n, network)).Where(c => c != null));
            }
            
            // Cross section types
            var crossSectionTypes = elementTypesList.Where(k => k.Key == SewerFeatureType.Crosssection).Select(k => k.Value).ToList();
            if (crossSectionTypes.Any())
            {
                createdNetworkFeatures.AddRange(crossSectionTypes.Select(element => CreateInstance(element, network)).Where(c => c != null));
            }

            // Connection types
            var connectionTypes = elementTypesList.Where(k => k.Key == SewerFeatureType.Connection).Select(k => k.Value).ToList();
            if (connectionTypes.Any())
            {
                var connectionImportHelper = CreateConnectionImportHelper(network);
                var connectionNetworkFeatures = connectionTypes.Select(element => CreateInstance(element, network, connectionImportHelper)).Where(c => c != null);
                createdNetworkFeatures.AddRange(connectionNetworkFeatures);
            }
            

            // Structure types 
            var structureTypes = elementTypesList.Where(k => k.Key == SewerFeatureType.Structure).Select(k => k.Value).ToList();
            if (structureTypes.Any())
            {
                var structureFeatures = structureTypes.Select(element => CreateInstance(element, network)).Where(c => c != null);
                createdNetworkFeatures.AddRange(structureFeatures);
            }

            return createdNetworkFeatures;
        }

        #endregion

        private static ISewerNetworkFeatureGenerator GetSewerNetworkFeatureGenerator(GwswElement gwswElement, IHydroNetwork network = null)
        {
            SewerFeatureType elementType;
            if (!Enum.TryParse(gwswElement?.ElementTypeName, out elementType)) return null;

            ISewerNetworkFeatureGenerator generator;
            switch (elementType)
            {
                case SewerFeatureType.Node:
                    generator = gwswElement.GetSewerCompartmentGenerator();
                    break;
                case SewerFeatureType.Crosssection:
                    generator = gwswElement.GetCrossSectionGenerator();
                    break;
                case SewerFeatureType.Connection:
                    generator = gwswElement.GetSewerConnectionGenerator();
                    break;
                case SewerFeatureType.Structure:
                    generator = gwswElement.GetSewerStructureGenerator(network);
                    break;
                default:
                    generator = null;
                    break;
            }
            return generator;
        }

        private static ISewerNetworkFeatureGenerator GetCrossSectionGenerator(this GwswElement gwswElement)
        {
            if (!gwswElement.IsValidGwswSewerProfile()) return null;

            var profileShapeAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileShape);
            var structureType = profileShapeAttribute.GetValueFromDescription<SewerProfileMapping.SewerProfileType>();
            switch (structureType)
            {
                case SewerProfileMapping.SewerProfileType.Egg:
                    return new EggCrossSectionDefinitionGenerator();
                case SewerProfileMapping.SewerProfileType.Arch:
                    return new ArchCrossSectionDefinitionGenerator();
                case SewerProfileMapping.SewerProfileType.Cunette:
                    return new CunetteCrossSectionDefinitionGenerator();
                case SewerProfileMapping.SewerProfileType.Rectangle:
                    return new RectangleCrossSectionDefinitionGenerator();
                case SewerProfileMapping.SewerProfileType.Circle:
                    return new CircleCrossSectionDefinitionGenerator();
                case SewerProfileMapping.SewerProfileType.Trapezoid:
                    return new TrapezoidCrossSectionDefinitionGenerator();
                default:
                    return new DefaultCrossSectionDefinitionGenerator();
            }
        }

        private static ISewerNetworkFeatureGenerator GetSewerStructureGenerator(this GwswElement gwswElement, IHydroNetwork network)
        {
            var structureTypeAttribute = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.StructureType);
            if (!structureTypeAttribute.IsValidAttribute()) return null;

            var structureType = structureTypeAttribute.GetValueFromDescription<SewerStructureMapping.StructureType>();
            switch (structureType)
            {
                case SewerStructureMapping.StructureType.Pump:
                    if (network != null) return new SewerPumpGenerator();
                    Log.ErrorFormat(Resources.SewerPumpGenerator_CreatePumpFromGwswStructure_Pump_s__cannot_be_created_without_a_network_previously_defined_);
                    return null;
                case SewerStructureMapping.StructureType.Crest:
                    return new SewerWeirGenerator();
                case SewerStructureMapping.StructureType.Outlet:
                    return new SewerCompartmentOutletGenerator();
                case SewerStructureMapping.StructureType.Orifice:
                    return new SewerConnectionOrificeGenerator();
                default:
                    return new SewerConnectionGenerator();
            }
        }

        private static ISewerNetworkFeatureGenerator GetSewerCompartmentGenerator(this GwswElement gwswElement)
        {
            var basicGenerator = new SewerCompartmentGenerator();

            var nodeTypeAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.NodeType);
            if (!nodeTypeAttribute.IsValidAttribute()) return basicGenerator;
            if (nodeTypeAttribute.IsAuxGwswManhole()) return new SewerManholeGenerator();

            return nodeTypeAttribute.IsGwswOutlet() ? new SewerCompartmentOutletGenerator() : basicGenerator;
        }

        private static ISewerNetworkFeatureGenerator GetSewerConnectionGenerator(this GwswElement gwswElement)
        {
            var sewerTypeAttribute = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.PipeType);
            var basicGenerator = new SewerConnectionGenerator();
            if (!sewerTypeAttribute.IsValidAttribute()) return basicGenerator;

            if (sewerTypeAttribute.IsGwswPipe()) return new SewerConnectionPipeGenerator();
            if (sewerTypeAttribute.IsGwswOrifice()) return new SewerConnectionOrificeGenerator();
            if (sewerTypeAttribute.IsGwswPump()) return new SewerPumpGenerator();
            if (sewerTypeAttribute.IsGwswWeir()) return new SewerWeirGenerator();

            return basicGenerator;
        }

        #region Auxiliar Gwsw Elements

        private static List<GwswElement> CreateAuxiliarGwswElements(IList<GwswElement> listOfElements)
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

        private static void AddAuxFeaturesToNetwork(IEnumerable<INetworkFeature> features, IHydroNetwork network)
        {
            features.ForEach(f =>
            {
                var item = f as IManhole;
                if( item != null) network.Nodes.Add(item); // TODO Sil: Added an item
            });
        }

        private static GwswAttribute GetAuxGwswAttribute(string value, Type attrType, string keyValue)
        {
            return new GwswAttribute
            {
                ValueAsString = value,
                GwswAttributeType = new GwswAttributeType()
                {
                    AttributeType = attrType,
                    Key = keyValue
                }
            };
        }

        #endregion

        #region Helpers

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

        private static Dictionary<string, IManhole> CreateConnectionImportHelper(IHydroNetwork network)
        {
            var connectionImportHelper = new Dictionary<string, IManhole>();
            if (network?.Manholes != null)
            {
                // get a list of compartment names 
                foreach (var manhole in network.Manholes)
                {
                    var compartments = manhole.Compartments;
                    foreach (var compartment in compartments)
                    {
                        IManhole auxManhole = null;
                        if (connectionImportHelper.TryGetValue(compartment.Name, out auxManhole)) continue;

                        connectionImportHelper.Add(compartment.Name, manhole);
                    }
                }
            }
            return connectionImportHelper;
        }

        #endregion
    }

    internal static class GwswElementValidationExtensions
    {
        private static ILog Log = LogManager.GetLogger(typeof(GwswElementValidationExtensions));

        public static bool IsAuxGwswManhole(this GwswAttribute sewerTypeAttribute)
        {
            var nodeType = sewerTypeAttribute.GetValueFromDescription<ManholeMapping.NodeType>();
            return nodeType == ManholeMapping.NodeType.Manhole;
        }

        public static bool IsGwswOutlet(this GwswAttribute sewerTypeAttribute)
        {
            var nodeType = sewerTypeAttribute.GetValueFromDescription<ManholeMapping.NodeType>();
            return nodeType == ManholeMapping.NodeType.Outlet;
        }

        public static bool IsGwswOrifice(this GwswAttribute sewerTypeAttribute)
        {
            var connectionType = sewerTypeAttribute.GetValueFromDescription<SewerConnectionMapping.ConnectionType>();
            return connectionType == SewerConnectionMapping.ConnectionType.Orifice;
        }

        public static bool IsGwswPump(this GwswAttribute sewerTypeAttribute)
        {
            var connectionType = sewerTypeAttribute.GetValueFromDescription<SewerConnectionMapping.ConnectionType>();
            return connectionType == SewerConnectionMapping.ConnectionType.Pump;
        }

        public static bool IsGwswPipe(this GwswAttribute sewerTypeAttribute)
        {
            var connectionType = sewerTypeAttribute.GetValueFromDescription<SewerConnectionMapping.ConnectionType>();
            if (connectionType == SewerConnectionMapping.ConnectionType.ClosedConnection ||
                connectionType == SewerConnectionMapping.ConnectionType.InfiltrationPipe ||
                connectionType == SewerConnectionMapping.ConnectionType.Open)
            {
                return true;
            }
            return false;
        }

        public static bool IsGwswWeir(this GwswAttribute sewerTypeAttribute)
        {
            var connectionType = sewerTypeAttribute.GetValueFromDescription<SewerConnectionMapping.ConnectionType>();
            return connectionType == SewerConnectionMapping.ConnectionType.Crest;
        }

        public static bool IsValidGwswManhole(this GwswElement gwswElement)
        {
            if (gwswElement == null) return false;
            var manholeName = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.ManholeId);
            
            //No need for log message because as per now, GwswManholes are our own creation (check CreateAuxiliarGwswElements)
            if (!manholeName.IsValidAttribute()) return false;

            var typeAttr = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.NodeType);
            var manholeType = typeAttr.GetValueFromDescription<ManholeMapping.NodeType>();
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
            if (gwswElement == null || gwswElement.ElementTypeName != SewerFeatureType.Connection.ToString()) return false;

            var nodeIdStart = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart);
            var nodeIdEnd = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd);
            if (nodeIdStart == null || nodeIdEnd == null)
            {
                Log.ErrorFormat(Resources
                    .SewerFeatureFactory_SewerConnectionFactory_Cannot_import_sewer_connection_s__without_Source_and_Target_nodes__Please_check_the_file_for_said_empty_fields);
                return false;
            }

            return true;
        }

        public static bool IsValidGwswSewerProfile(this GwswElement gwswElement)
        {
            if (gwswElement == null) return false;

            var profileId = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileId);
            if (!profileId.IsValidAttribute())
            {
                Log.Error(Resources.GwswElementValidationExtensions_IsValidGwswSewerProfile_Cannot_import_sewer_profile_s__without_profile_id__Please_check__Profiel_csv__for_empty_profile_id_s);
                return false;
            }

            var featureType = (SewerFeatureType)EnumDescriptionAttributeTypeConverter.GetEnumValue<SewerFeatureType>(gwswElement.ElementTypeName);
            return featureType == SewerFeatureType.Crosssection;
        }

        public static bool IsValidGwswStructure(this GwswElement gwswElement)
        {
            if (gwswElement == null) return false;

            var profileId = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.UniqueId);
            if (!profileId.IsValidAttribute())
            {
                Log.Error(Resources.GwswElementValidationExtensions_IsValidGwswStructure_Cannot_import_sewer_structure_s__without_a_unique_id__Please_check__Kunstwerk_csv__for_empty_unique_id_s);
                return false;
            }

            var featureType = (SewerFeatureType)EnumDescriptionAttributeTypeConverter.GetEnumValue<SewerFeatureType>(gwswElement.ElementTypeName);
            return featureType == SewerFeatureType.Structure;
        }
    }
}
