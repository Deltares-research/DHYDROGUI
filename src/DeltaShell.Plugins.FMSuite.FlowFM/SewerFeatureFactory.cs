using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerFeatureFactory
    {
        private static ILog Log = LogManager.GetLogger(typeof(SewerFeatureFactory));
        // For now, the types are: Node, Connection, Structure, Surface, Runoff, Discharge, Distribution, Meta
        private static Dictionary<SewerFeatureType, Func<object, HydroNetwork, INetworkFeature>> CreateSewerFeature = new Dictionary<SewerFeatureType, Func<object, HydroNetwork, INetworkFeature>>
        {
            { SewerFeatureType.Node, CreateCompartment },
            { SewerFeatureType.Connection, CreateSewerConnection },
        };

        public static IEnumerable<INetworkFeature> CreateMultipleInstances(List<GwswElement> listOfElements, HydroNetwork network = null)
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

        public static INetworkFeature CreateInstance(object element, HydroNetwork network = null)
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

        #region Creating Manholes

        private static Compartment CreateCompartment(object element, HydroNetwork network = null)
        {
            var gwswElement = element as GwswElement;
            if (gwswElement == null) return null;

            var gwswElementKeyValuePairs = gwswElement.GwswAttributeList.ToDictionary(attr => attr.GwswAttributeType.Key, attr => attr.ValueAsString);

            Compartment compartment;
            try
            {
                string manholeId;
                if(!gwswElementKeyValuePairs.TryGetValue(ManholePropertyKeys.ManholeId, out manholeId))
                    throw new Exception(Resources.SewerFeatureFactory_CreateManholeNode_There_are_lines_in__Knooppunt_csv__that_do_not_contain_a_Manhole_Id__These_lines_are_not_imported_);

                string uniqueId;
                if (!gwswElementKeyValuePairs.TryGetValue(ManholePropertyKeys.UniqueId, out uniqueId))
                    throw new Exception(string.Format(Resources.SewerFeatureFactory_CreateManHoleCompartment_Manhole_with_manhole_id___0___could_not_be_created__because_one_of_its_compartments_misses_its_unique_id_,manholeId));

                compartment = new Compartment(uniqueId)
                {
                    ParentManhole = new Manhole(manholeId)
                };

                // Set manhole value
                double doubleValue;
                if (TryGetDoubleValueElseThrowException(ManholePropertyKeys.NodeLength, gwswElementKeyValuePairs,
                    uniqueId, manholeId, out doubleValue)) compartment.ManholeLength = doubleValue;
                if (TryGetDoubleValueElseThrowException(ManholePropertyKeys.NodeWidth, gwswElementKeyValuePairs,
                    uniqueId, manholeId, out doubleValue)) compartment.ManholeWidth = doubleValue;
                if (TryGetDoubleValueElseThrowException(ManholePropertyKeys.FloodableArea, gwswElementKeyValuePairs,
                    uniqueId, manholeId, out doubleValue)) compartment.FloodableArea = doubleValue;
                if (TryGetDoubleValueElseThrowException(ManholePropertyKeys.BottomLevel, gwswElementKeyValuePairs,
                    uniqueId, manholeId, out doubleValue)) compartment.BottomLevel = doubleValue;
                if (TryGetDoubleValueElseThrowException(ManholePropertyKeys.SurfaceLevel, gwswElementKeyValuePairs,
                    uniqueId, manholeId, out doubleValue)) compartment.SurfaceLevel = doubleValue;

                double yCoordinate;
                double xCoordinate;
                if (TryGetDoubleValueElseThrowException(ManholePropertyKeys.XCoordinate, gwswElementKeyValuePairs,
                        uniqueId, manholeId, out xCoordinate)
                    && TryGetDoubleValueElseThrowException(ManholePropertyKeys.YCoordinate, gwswElementKeyValuePairs,
                        uniqueId, manholeId, out yCoordinate))
                    compartment.Geometry = new Point(xCoordinate, yCoordinate);

                // Set shape value of the manhole
                string nodeShape;
                if (gwswElementKeyValuePairs.TryGetValue(ManholePropertyKeys.NodeShape, out nodeShape))
                {
                    try
                    {
                        compartment.Shape =
                            (CompartmentShape) EnumDescriptionAttributeTypeConverter.GetEnumValue<CompartmentShape>(
                                nodeShape);
                    }
                    catch
                    {
                        ThrowException(uniqueId, "string", ManholePropertyKeys.NodeShape, nodeShape, manholeId);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warn(e.Message);
                return null;
            }
            return compartment;
        }
        
        #endregion

        #region Creating Sewer Connections

        private static SewerConnection CreateSewerConnection(object element, HydroNetwork network = null)
        {
            var gwswElement = element as GwswElement;
            if (gwswElement == null) return null;

            SewerConnectionType connectionType = default(SewerConnectionType);
            //Get the correct element based on its type.
            var pipeTypeAttr = GetAttributeFromList(gwswElement, ConnectionPropertyKeys.PipeType);
            if (pipeTypeAttr != null && pipeTypeAttr.ValueAsString != string.Empty)
            {
                //Find type;
                connectionType = GetValueFromDescription<SewerConnectionType>(pipeTypeAttr.ValueAsString);
            }

            var newConnection = GetNewConnectionElement(connectionType);
            newConnection.SewerConnectionType = connectionType;
            newConnection.Network = network;

            //Assigning new values
            SetSewerConnectionAttributes(network, gwswElement, newConnection);

            //Setting Pipe attributes
            if (newConnection is Pipe)
            {
                SetPipeAttributes(newConnection, gwswElement, network);
            }
            
            /*  Setting up the geometry
             *  Needs to be done before adding structures because they will use the connection's geometry.
             */
            SetSewerConnectionDefaultGeometry(newConnection);

            #region Adding structures to the SewerConnection

            if (newConnection.SewerConnectionType == SewerConnectionType.Pump)
            {
                AddPumpAndAttributesToSewerConnection(newConnection, gwswElement, network);

            }

            #endregion

            return newConnection;
        }

        private static void SetSewerConnectionDefaultGeometry(SewerConnection sewerConnection)
        {
            var manholeSource = sewerConnection.Source;
            var manholeTarget = sewerConnection.Target;

            if (manholeSource == null || manholeTarget == null) return;
            var coordX = 0.0;

            if (sewerConnection.Geometry == null || !sewerConnection.Geometry.Coordinates.Any())
            {
                //Check source Geometry
                var compartmentSource = sewerConnection.SourceCompartment;
                var compartmentTarget = sewerConnection.TargetCompartment;
                if (!manholeSource.Geometry.Equals(compartmentSource.Geometry))
                {
                    if (compartmentSource.Geometry == null)
                    {
                        if (compartmentTarget.Geometry != null)
                        {
                            coordX = compartmentTarget.Geometry.Coordinate.X - sewerConnection.Length;
                        }

                        var sourceGeometry = new Point(coordX, sewerConnection.LevelSource);
                        compartmentSource.Geometry = sourceGeometry;
                    }
                    manholeSource.Geometry = compartmentSource.Geometry;
                }

                if (!manholeTarget.Geometry.Equals(compartmentTarget.Geometry))
                {
                    if (compartmentTarget.Geometry == null)
                    {
                        var targetGeometry = new Point(sewerConnection.Length + coordX, sewerConnection.LevelSource);
                        compartmentTarget.Geometry = targetGeometry;
                    }
                    manholeTarget.Geometry = compartmentTarget.Geometry;
                }
            }

            sewerConnection.Geometry = new LineString(
                new[]
                {
                    manholeSource.Geometry.Coordinate,
                    manholeTarget.Geometry.Coordinate
                });
        }

        private static void AddPumpAndAttributesToSewerConnection(SewerConnection connection, GwswElement gwswElement, HydroNetwork network = null)
        {
            //Add pump to structure
            var sewerPump = new Pump();

            //Add pump to network
            AddStructureToBranch(connection, sewerPump);

            //Add attributes.
            double newDoubleValue;
            var inletLossStart = GetAttributeFromList(gwswElement, ConnectionPropertyKeys.InletLossStart);
            if (inletLossStart != null)
            {
                var valueType = inletLossStart.GwswAttributeType.AttributeType;
                if (valueType == sewerPump.StartSuction.GetType() &&
                    TryParseDoubleElseLogError(inletLossStart, valueType, out newDoubleValue))
                {
                    sewerPump.StartSuction = newDoubleValue;
                }
            }
            var inletLossEnd = GetAttributeFromList(gwswElement, ConnectionPropertyKeys.InletLossEnd);
            if (inletLossEnd != null)
            {
                var valueType = inletLossEnd.GwswAttributeType.AttributeType;
                if (valueType == sewerPump.StopSuction.GetType() &&
                    TryParseDoubleElseLogError(inletLossEnd, valueType, out newDoubleValue))
                {
                    sewerPump.StopSuction = newDoubleValue;
                }
            }

            var outletLossStart = GetAttributeFromList(gwswElement, ConnectionPropertyKeys.OutletLossStart);
            if (outletLossStart != null)
            {
                var valueType = outletLossStart.GwswAttributeType.AttributeType;
                if (valueType == sewerPump.StartDelivery.GetType() &&
                    TryParseDoubleElseLogError(outletLossStart, valueType, out newDoubleValue))
                {
                    sewerPump.StartDelivery = newDoubleValue;
                }
            }

            var outletLossEnd = GetAttributeFromList(gwswElement, ConnectionPropertyKeys.OutletLossEnd);
            if (outletLossEnd != null)
            {
                var valueType = outletLossEnd.GwswAttributeType.AttributeType;
                if (valueType == sewerPump.StopDelivery.GetType() &&
                    TryParseDoubleElseLogError(outletLossEnd, valueType, out newDoubleValue))
                {
                    sewerPump.StopDelivery = newDoubleValue;
                }
            }
        }

        private static void AddStructureToBranch(SewerConnection connection, BranchStructure structure)
        {
            structure.Branch = connection;
            structure.Network = connection.Network;
            structure.Chainage = 0;

            if (connection.Geometry != null && connection.Geometry.Coordinates.Any())
            {
                structure.Geometry = new Point(connection.Geometry.Coordinates[0]);
            }
            structure.Name = HydroNetworkHelper.GetUniqueFeatureName(structure.Network as HydroNetwork, structure);
            //structure.Name = string.Concat(structurePrefix, connection.Name);
            connection.BranchFeatures.Add(structure);
        }

        private static void SetPipeAttributes(SewerConnection element, GwswElement gwswElement, HydroNetwork network = null)
        {
            var newPipe = element as Pipe;
            if (newPipe == null) return ;

            var pipeIndicator = GetAttributeFromList(gwswElement, ConnectionPropertyKeys.PipeIndicator);
            if (pipeIndicator?.ValueAsString != null)
            {
                newPipe.PipeId = pipeIndicator.ValueAsString;
            }

            var profileDef = GetAttributeFromList(gwswElement, ConnectionPropertyKeys.CrossSectionDef);
            if (profileDef != null)
            {
                //Find crossSectionDef;
                //Profiles are needed first.
                if (profileDef.ValueAsString != string.Empty && network != null)
                {
                    var foundCs = network.SewerProfiles.FirstOrDefault(n => n.Name.Equals(profileDef.ValueAsString));
                    if (foundCs == null)
                    {
                        foundCs = CrossSection.CreateDefault();
                        foundCs.Name = profileDef.ValueAsString;
                        network.SewerProfiles.Add(foundCs);
                    }
                    newPipe.CrossSectionShape = (CrossSection)foundCs;
                }
            }
        }

        private static void SetSewerConnectionAttributes(HydroNetwork network, GwswElement gwswElement,
            SewerConnection newConnection)
        {
            double newDoubleValue;
            var nodeIdString = GetAttributeFromList(gwswElement, ConnectionPropertyKeys.UniqueId);
            if (nodeIdString?.ValueAsString != null)
            {
                newConnection.Name = nodeIdString.ValueAsString;
            }

            var nodeIdStart = GetAttributeFromList(gwswElement, ConnectionPropertyKeys.NodeUniqueIdStart);
            if (nodeIdStart != null)
            {
                //Find node;
                if (nodeIdStart.ValueAsString != string.Empty && network != null)
                {
                    var foundNode = network.Manholes.FirstOrDefault( m => m.GetCompartmentByName(nodeIdStart.ValueAsString) != null);
                    if (foundNode == null)
                    {
                        //create node
                        foundNode = GetNewManholeForCompartment(nodeIdStart.ValueAsString);
                        network.Nodes.Add(foundNode);
                    }
                    newConnection.Source = foundNode;
                    newConnection.SourceCompartment = foundNode.GetCompartmentByName(nodeIdStart.ValueAsString);
                }
            }

            var nodeIdEnd = GetAttributeFromList(gwswElement, ConnectionPropertyKeys.NodeUniqueIdEnd);
            if (nodeIdEnd != null)
            {
                //Find node;                
                if (nodeIdEnd.ValueAsString != string.Empty && network != null)
                {
                    var foundNode = network.Manholes.FirstOrDefault(m => m.GetCompartmentByName(nodeIdEnd.ValueAsString) != null);
                    if (foundNode == null)
                    {
                        //create node
                        foundNode = GetNewManholeForCompartment(nodeIdEnd.ValueAsString);
                        network.Nodes.Add(foundNode);
                    }
                    newConnection.Target = foundNode;
                    newConnection.TargetCompartment = foundNode.GetCompartmentByName(nodeIdEnd.ValueAsString);
                }
            }
            var levelStart = GetAttributeFromList(gwswElement, ConnectionPropertyKeys.LevelStart);
            if (levelStart != null)
            {
                var valueType = levelStart.GwswAttributeType.AttributeType;
                if (valueType == newConnection.LevelSource.GetType() &&
                    TryParseDoubleElseLogError(levelStart, valueType, out newDoubleValue))
                {
                    newConnection.LevelSource = newDoubleValue;
                }
            }
            var levelEnd = GetAttributeFromList(gwswElement, ConnectionPropertyKeys.LevelEnd);
            if (levelEnd != null)
            {
                var valueType = levelEnd.GwswAttributeType.AttributeType;
                if (valueType == newConnection.LevelTarget.GetType() &&
                    TryParseDoubleElseLogError(levelEnd, valueType, out newDoubleValue))
                {
                    newConnection.LevelTarget = newDoubleValue;
                }
            }

            var length = GetAttributeFromList(gwswElement, ConnectionPropertyKeys.Length);
            if (length != null)
            {
                var valueType = length.GwswAttributeType.AttributeType;
                if (valueType == newConnection.Length.GetType() &&
                    TryParseDoubleElseLogError(length, valueType, out newDoubleValue))
                {
                    newConnection.Length = newDoubleValue;
                }
            }

            var waterType = GetAttributeFromList(gwswElement, ConnectionPropertyKeys.WaterType);
            if (waterType != null && waterType.ValueAsString != string.Empty)
            {
                //Find type;
                newConnection.WaterType = GetValueFromDescription<SewerConnectionWaterType>(waterType.ValueAsString);
            }
        }

        private static Manhole GetNewManholeForCompartment(string compartmentName)
        {
            var manholePlaceholder = new Manhole(string.Format("Manhole_For_Compartment_{0}", compartmentName));

            manholePlaceholder.Compartments.Add(new Compartment(compartmentName));
            
            Log.InfoFormat(Resources.SewerFeatureFactory_GetNewManholeForCompartment_Created_Manhole__0__and_compartment__1__with_default_values_as_they_were_not_found_in_the_network_, manholePlaceholder.Name, compartmentName);

            return manholePlaceholder;
        }

        private static SewerConnection GetNewConnectionElement(SewerConnectionType type)
        {
            if (type == SewerConnectionType.ClosedConnection ||
                type == SewerConnectionType.InfiltrationPipe ||
                type == SewerConnectionType.Open)
            {
                return new Pipe();
            }
            return new SewerConnection();
        }

        #endregion

        #region Helpers

        public static T GetValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }
            
            Log.WarnFormat("Type {0} is not recognized, please check the syntax", description);
            return default(T);
        }

        private static GwswAttribute GetAttributeFromList(GwswElement element, string attributeName)
        {
            var attribute = element.GwswAttributeList.FirstOrDefault(attr => attr.GwswAttributeType.Key.Equals(attributeName));
            if (attribute == null)
            {
                Log.WarnFormat(Resources.SewerFeatureFactory_GetAttributeFromList_Attribute__0__was_not_found_for_element__1_, attributeName, element.ElementTypeName);
                return null;
            }
            return attribute;
        }

        private static bool TryParseDoubleElseLogError(GwswAttribute attribute, Type valueType, out double value)
        {
            if (!double.TryParse(ReplaceCommaWithPoint(attribute.ValueAsString), NumberStyles.Any,
                CultureInfo.InvariantCulture, out value))
            {
                Log.ErrorFormat(Resources.SewerFeatureFactory_CreatePipe_Not_possible_to_parse_value__0__into__1_, attribute.ValueAsString, valueType.Name);
            }
            return true;
        }

        private static bool TryGetDoubleValueElseThrowException(string columnKey, IReadOnlyDictionary<string, string> elementValues, string id, string manholeId, out double doubleValue)
        {
            string stringValue;
            if (!elementValues.TryGetValue(columnKey, out stringValue) || stringValue == string.Empty)
            {
                doubleValue = 0.0;
                return false;
            }
            if (!double.TryParse(ReplaceCommaWithPoint(stringValue), NumberStyles.Any, CultureInfo.InvariantCulture, out doubleValue))
                ThrowException(id, "double", columnKey, stringValue, manholeId);
            return true;
        }

        private static void ThrowException(string id, string dataType, string columnKey, string wrongValue, string manholeId)
        {
            throw new Exception(string.Format(Resources.SewerFeatureFactory_ThrowException_, id, dataType, columnKey, wrongValue, manholeId));
        }

        private static string ReplaceCommaWithPoint(string doubleString)
        {
            return doubleString.Replace(',', '.');
        }

        #endregion
    }

    public static class ConnectionPropertyKeys
    {
        public const string UniqueId = "UNIQUE_ID";
        public const string NodeUniqueIdStart = "NODE_UNIQUE_ID_START";
        public const string NodeUniqueIdEnd = "NODE_UNIQUE_ID_END";
        public const string PipeType = "PIPE_TYPE";
        public const string LevelStart = "LEVEL_START";
        public const string LevelEnd = "LEVEL_END";
        public const string Length = "LENGTH";
        public const string CrossSectionDef = "CROSS_SECTION_DEF";
        public const string PipeIndicator = "PIPE_INDICATOR";
        public const string WaterType = "WATER_TYPE";
        public const string InletLossStart = "INLETLOSS_START";
        public const string OutletLossStart = "OUTLETLOSS_START";
        public const string InletLossEnd = "INLETLOSS_END";
        public const string OutletLossEnd = "OUTLETLOSS_END";
        public const string FlowDirection = "FLOW_DIRECTION";
        public const string InfiltrationDef = "INFILTRATION_DEF";
        public const string Status = "STATUS";
        public const string ALevelStart = "A_LEVEL_START";
        public const string ALevelEnd = "A_LEVEL_END";
        public const string InitialWaterLevel = "INITIAL_WATER_LEVEL";
        public const string Remarks = "REMARKS";
    }

    public static class ManholePropertyKeys
    {
        public const string ManholeId = "MANHOLE_ID";
        public const string UniqueId = "UNIQUE_ID";
        public const string NodeLength = "NODE_LENGTH";
        public const string NodeWidth = "NODE_WIDTH";
        public const string NodeShape = "NODE_SHAPE";
        public const string FloodableArea = "FLOODABLE_AREA";
        public const string BottomLevel = "BOTTOM_LEVEL";
        public const string SurfaceLevel = "SURFACE_LEVEL";
        public const string XCoordinate = "X_COORDINATE";
        public const string YCoordinate = "Y_COORDINATE";
    }
}
