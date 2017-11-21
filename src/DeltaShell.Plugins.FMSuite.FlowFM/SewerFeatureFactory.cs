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
            { SewerFeatureType.Connection, SewerConnectionFactory },
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

        private static Type GetGwswElementSewerConnectionType(GwswAttribute sewerTypeAttribute)
        {
            var sewerConnectionType = typeof(SewerConnection);
            if (sewerTypeAttribute == null || sewerTypeAttribute.ValueAsString == string.Empty) return sewerConnectionType;

            var connectionType = GetValueFromDescription<SewerConnectionMapping.ConnectionType>(sewerTypeAttribute.ValueAsString);
            if (connectionType == SewerConnectionMapping.ConnectionType.ClosedConnection ||
                connectionType == SewerConnectionMapping.ConnectionType.InfiltrationPipe ||
                connectionType == SewerConnectionMapping.ConnectionType.Open)
            {
                return typeof(Pipe);
            }

            if (connectionType == SewerConnectionMapping.ConnectionType.Pump)
                return typeof(Pump);

            return sewerConnectionType;
        }

        private static SewerConnection SewerConnectionFactory(object element, HydroNetwork network = null)
        {
            var gwswElement = element as GwswElement;
            if (gwswElement == null) return null;

            var connectionType =
                GetGwswElementSewerConnectionType(GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.PipeType));

            if( connectionType == typeof(Pipe))
            {
                return CreateSewerConnection<Pipe>(gwswElement, network, SetPipeAttributes);
            }

            var sewerConnection = CreateSewerConnection<SewerConnection>(gwswElement, network);
            if (connectionType == typeof(Pump))
            {
                AddPumpAndAttributesToSewerConnection(sewerConnection, gwswElement, network);
            }

            return sewerConnection;
        }

        private static T CreateSewerConnection<T>(GwswElement gwswElement, HydroNetwork network = null, Action<T, GwswElement, HydroNetwork> connectionAction = null) where T : SewerConnection, new()
        {
            var connection = new T();

            SetSewerConnectionAttributes(connection, gwswElement, network);
            connectionAction?.Invoke(connection, gwswElement, network);
            SetSewerConnectionDefaultGeometry(connection);

            return connection;
        }

        private static void SetSewerConnectionAttributes(SewerConnection sewerConnection, GwswElement gwswElement, HydroNetwork network)
        {
            double newDoubleValue;
            sewerConnection.Network = network;

            var nodeIdString = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.UniqueId);
            if (nodeIdString?.ValueAsString != null)
            {
                sewerConnection.Name = nodeIdString.ValueAsString;
            }

            var nodeIdStart = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart);
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
                    sewerConnection.Source = foundNode;
                    sewerConnection.SourceCompartment = foundNode.GetCompartmentByName(nodeIdStart.ValueAsString);
                }
            }

            var nodeIdEnd = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd);
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
                    sewerConnection.Target = foundNode;
                    sewerConnection.TargetCompartment = foundNode.GetCompartmentByName(nodeIdEnd.ValueAsString);
                }
            }
            var levelStart = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.LevelStart);
            if (levelStart != null)
            {
                var valueType = levelStart.GwswAttributeType.AttributeType;
                if (valueType == sewerConnection.LevelSource.GetType() &&
                    TryParseDoubleElseLogError(levelStart, valueType, out newDoubleValue))
                {
                    sewerConnection.LevelSource = newDoubleValue;
                }
            }
            var levelEnd = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.LevelEnd);
            if (levelEnd != null)
            {
                var valueType = levelEnd.GwswAttributeType.AttributeType;
                if (valueType == sewerConnection.LevelTarget.GetType() &&
                    TryParseDoubleElseLogError(levelEnd, valueType, out newDoubleValue))
                {
                    sewerConnection.LevelTarget = newDoubleValue;
                }
            }

            var length = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.Length);
            if (length != null)
            {
                var valueType = length.GwswAttributeType.AttributeType;
                if (valueType == sewerConnection.Length.GetType() &&
                    TryParseDoubleElseLogError(length, valueType, out newDoubleValue))
                {
                    sewerConnection.Length = newDoubleValue;
                }
            }

            var waterType = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.WaterType);
            if (waterType != null && waterType.ValueAsString != string.Empty)
            {
                //Find type;
                sewerConnection.WaterType = GetValueFromDescription<SewerConnectionWaterType>(waterType.ValueAsString);
            }
        }

        private static void SetPipeAttributes(SewerConnection element, GwswElement gwswElement, HydroNetwork network = null)
        {
            var newPipe = element as Pipe;
            if (newPipe == null) return ;

            var pipeIndicator = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.PipeIndicator);
            if (pipeIndicator?.ValueAsString != null)
            {
                newPipe.PipeId = pipeIndicator.ValueAsString;
            }

            var profileDef = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.CrossSectionDef);
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
            var inletLossStart = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.InletLossStart);
            if (inletLossStart != null)
            {
                var valueType = inletLossStart.GwswAttributeType.AttributeType;
                if (valueType == sewerPump.StartSuction.GetType() &&
                    TryParseDoubleElseLogError(inletLossStart, valueType, out newDoubleValue))
                {
                    sewerPump.StartSuction = newDoubleValue;
                }
            }
            var inletLossEnd = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.InletLossEnd);
            if (inletLossEnd != null)
            {
                var valueType = inletLossEnd.GwswAttributeType.AttributeType;
                if (valueType == sewerPump.StopSuction.GetType() &&
                    TryParseDoubleElseLogError(inletLossEnd, valueType, out newDoubleValue))
                {
                    sewerPump.StopSuction = newDoubleValue;
                }
            }

            var outletLossStart = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.OutletLossStart);
            if (outletLossStart != null)
            {
                var valueType = outletLossStart.GwswAttributeType.AttributeType;
                if (valueType == sewerPump.StartDelivery.GetType() &&
                    TryParseDoubleElseLogError(outletLossStart, valueType, out newDoubleValue))
                {
                    sewerPump.StartDelivery = newDoubleValue;
                }
            }

            var outletLossEnd = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.OutletLossEnd);
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

            connection.BranchFeatures.Add(structure);
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

        private static Manhole GetNewManholeForCompartment(string compartmentName)
        {
            var manholePlaceholder = new Manhole(string.Format("Manhole_For_Compartment_{0}", compartmentName));

            manholePlaceholder.Compartments.Add(new Compartment(compartmentName));
            
            Log.InfoFormat(Resources.SewerFeatureFactory_GetNewManholeForCompartment_Created_Manhole__0__and_compartment__1__with_default_values_as_they_were_not_found_in_the_network_, manholePlaceholder.Name, compartmentName);

            return manholePlaceholder;
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

    #region Mappings

    public static class SewerConnectionMapping
    {
        public enum ConnectionType
        {
            [Description("DRL")] Orifice,
            [Description("GSL")] ClosedConnection /*Should be created as a pipe*/,
            [Description("ITR")] InfiltrationPipe /*Should be created as a pipe*/,
            [Description("OPL")] Open /*Should be created as a pipe*/,
            [Description("OVS")] Crest,
            [Description("PMP")] Pump
        }
        public static class PropertyKeys
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

    #endregion
}
