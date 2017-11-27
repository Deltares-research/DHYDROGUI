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
        private static Dictionary<SewerFeatureType, Func<GwswElement, HydroNetwork, INetworkFeature>> CreateSewerFeature = new Dictionary<SewerFeatureType, Func<GwswElement, HydroNetwork, INetworkFeature>>
        {
            { SewerFeatureType.Node, CompartmentFactory },
            { SewerFeatureType.Connection, SewerConnectionFactory },
            { SewerFeatureType.Crosssection, CreateSewerProfile },
            { SewerFeatureType.Structure, StructureFactory }
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

        #region Creating sewer profiles
        
        private static CrossSection CreateSewerProfile(GwswElement gwswElement, HydroNetwork network = null)
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

        private static Compartment CompartmentFactory(GwswElement gwswElement, HydroNetwork network = null)
        {
            if (gwswElement == null) return null;

            var manholeName = GetAttributeFromList(gwswElement, ManholeMapping.PropertyKeys.ManholeId);
            if (manholeName == null || manholeName.ValueAsString == string.Empty)
            {
                Log.WarnFormat(
                    Resources.SewerFeatureFactory_CreateManholeNode_There_are_lines_in__Knooppunt_csv__that_do_not_contain_a_Manhole_Id__These_lines_are_not_imported_);
                return null;
            }

            var compartmentName = GetAttributeFromList(gwswElement, ManholeMapping.PropertyKeys.UniqueId);
            if (compartmentName == null || compartmentName.ValueAsString == string.Empty)
            {

                Log.WarnFormat(
                    Resources.SewerFeatureFactory_CreateManHoleCompartment_Manhole_with_manhole_id___0___could_not_be_created__because_one_of_its_compartments_misses_its_unique_id_, manholeName.ValueAsString);
                return null;
            }

            var compartmentType =
                GetGwswElementCompartmentType(GetAttributeFromList(gwswElement, ManholeMapping.PropertyKeys.NodeType));

            if (compartmentType == typeof(OutletCompartment))
            {
                return CreateCompartment<OutletCompartment>(gwswElement, network);
            }

            return CreateCompartment<Compartment>(gwswElement, network);
        }

        private static Type GetGwswElementCompartmentType(GwswAttribute sewerTypeAttribute)
        {
            var sewerConnectionType = typeof(Compartment);
            if (sewerTypeAttribute == null || sewerTypeAttribute.ValueAsString == string.Empty) return sewerConnectionType;

            var nodeType = GetValueFromDescription<ManholeMapping.NodeType>(sewerTypeAttribute.ValueAsString);
            if (nodeType == ManholeMapping.NodeType.Outlet)
            {
                return typeof(OutletCompartment);
            }

            return sewerConnectionType;
        }

        private static T CreateCompartment<T>(GwswElement gwswElement, HydroNetwork network = null) where T : Compartment, new()
        {
            var compartment = FindOrGetNewCompartment<T>(gwswElement, network);
            SetCompartmentAttributes(compartment, gwswElement, network);
            return compartment;
        }

        private static Manhole GetParentManholeFromGwswElement(GwswElement gwswElement, HydroNetwork network)
        {
            var manholeAttribute = GetAttributeFromList(gwswElement, ManholeMapping.PropertyKeys.ManholeId);
            var compartmentName = GetAttributeFromList(gwswElement, ManholeMapping.PropertyKeys.UniqueId).ValueAsString;
            
            //if we could not place an ID then we use the compartment name to create a place holder (if needed)
            var manholeId = manholeAttribute != null ? manholeAttribute.ValueAsString : compartmentName;

            Manhole parentManhole = null;
            if (network != null)
            {
                //Find manhole by its name
                parentManhole = network.Manholes.FirstOrDefault(m => m.Name.Equals(manholeId)) as Manhole;
                //If it was not found, try searching by containing this compartment.
                if (parentManhole == null)
                {
                    parentManhole = network.Manholes.FirstOrDefault(m => m.ContainsCompartment(compartmentName)) as Manhole;
                }
            }

            if (parentManhole == null)
            {
                parentManhole = new Manhole(manholeId);
            }
            return parentManhole;
        }

        private static T FindOrGetNewCompartment<T>(GwswElement gwswElement, HydroNetwork network = null) where T : Compartment, new()
        {
            //We know they are not null, otherwise we would have already returned in the CompartmentFactory method.
            var compartmentName = GetAttributeFromList(gwswElement, ManholeMapping.PropertyKeys.UniqueId).ValueAsString;
            var parentManhole = GetParentManholeFromGwswElement(gwswElement, network);

            if (network == null || !parentManhole.ContainsCompartment(compartmentName))
                return new T{Name = compartmentName, ParentManhole = parentManhole};

            var compartmentFound = parentManhole.GetCompartmentByName(compartmentName);
            return (T) (compartmentFound ?? new T { Name = compartmentName, ParentManhole = parentManhole });
        }

        private static void SetCompartmentAttributes(Compartment compartment, GwswElement gwswElement, HydroNetwork network)
        {
            var gwswElementKeyValuePairs = gwswElement.GwswAttributeList.ToDictionary(attr => attr.GwswAttributeType.Key, attr => attr.ValueAsString);

            var uniqueId = compartment.Name;
            compartment.ParentManhole = GetParentManholeFromGwswElement(gwswElement, network);
            var manholeId = compartment.ParentManhole.Name;

            // Set the rest of manhole values
            double doubleValue;
            if (TryGetDoubleValueElseLogException(ManholeMapping.PropertyKeys.NodeLength, gwswElementKeyValuePairs,
                uniqueId, manholeId, out doubleValue)) compartment.ManholeLength = doubleValue;
            if (TryGetDoubleValueElseLogException(ManholeMapping.PropertyKeys.NodeWidth, gwswElementKeyValuePairs,
                uniqueId, manholeId, out doubleValue)) compartment.ManholeWidth = doubleValue;
            if (TryGetDoubleValueElseLogException(ManholeMapping.PropertyKeys.FloodableArea, gwswElementKeyValuePairs,
                uniqueId, manholeId, out doubleValue)) compartment.FloodableArea = doubleValue;
            if (TryGetDoubleValueElseLogException(ManholeMapping.PropertyKeys.BottomLevel, gwswElementKeyValuePairs,
                uniqueId, manholeId, out doubleValue)) compartment.BottomLevel = doubleValue;
            if (TryGetDoubleValueElseLogException(ManholeMapping.PropertyKeys.SurfaceLevel, gwswElementKeyValuePairs,
                uniqueId, manholeId, out doubleValue)) compartment.SurfaceLevel = doubleValue;

            double yCoordinate;
            double xCoordinate;
            if (TryGetDoubleValueElseLogException(ManholeMapping.PropertyKeys.XCoordinate, gwswElementKeyValuePairs,
                    uniqueId, manholeId, out xCoordinate)
                && TryGetDoubleValueElseLogException(ManholeMapping.PropertyKeys.YCoordinate, gwswElementKeyValuePairs,
                    uniqueId, manholeId, out yCoordinate))
                compartment.Geometry = new Point(xCoordinate, yCoordinate);

            // Set shape value of the manhole
            string nodeShape;
            if (gwswElementKeyValuePairs.TryGetValue(ManholeMapping.PropertyKeys.NodeShape, out nodeShape))
            {
                try
                {
                    compartment.Shape =
                        (CompartmentShape)EnumDescriptionAttributeTypeConverter.GetEnumValue<CompartmentShape>(
                            nodeShape);
                }
                catch
                {
                    LogManholeWarningMessage(uniqueId, "string", ManholeMapping.PropertyKeys.NodeShape, nodeShape, manholeId);
                }
            }
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

        private static SewerConnection SewerConnectionFactory(GwswElement gwswElement, HydroNetwork network = null)
        {
            if (gwswElement == null) return null;

            var connectionType =
                GetGwswElementSewerConnectionType(GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.PipeType));

            /* First we need to check whether there are target and source nodes, otherwise we will not create this sewer connection.*/
            var nodeIdStart = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart);
            var nodeIdEnd = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd);
            if (nodeIdStart == null || nodeIdEnd == null)
            {
                Log.ErrorFormat(Resources.SewerFeatureFactory_SewerConnectionFactory_Cannot_import_sewer_connection_s__without_Source_and_Target_nodes__Please_check_the_file_for_said_empty_fields);
                return null;
            }

            if ( connectionType == typeof(Pipe))
            {
                return CreateSewerConnection<Pipe>(gwswElement, network, SetPipeAttributes);
            }

            var sewerConnection = CreateSewerConnection<SewerConnection>(gwswElement, network);

            //Add structures if needed
            if (connectionType == typeof(Pump))
            {
                AddPumpAndAttributesToSewerConnection(sewerConnection, gwswElement);
            }

            return sewerConnection;
        }

        private static T CreateSewerConnection<T>(GwswElement gwswElement, HydroNetwork network = null, Action<T, GwswElement, HydroNetwork> connectionAction = null) where T : SewerConnection, new()
        {
            var connection = FindOrGetNewConnection<T>(gwswElement, network);

            SetSewerConnectionAttributes(connection, gwswElement, network);
            connectionAction?.Invoke(connection, gwswElement, network);
            SetSewerConnectionDefaultGeometry(connection);

            return connection;
        }

        private static T FindOrGetNewConnection<T>(GwswElement gwswElement, HydroNetwork network = null) where T : SewerConnection, new()
        {
            if( network == null) return new T();

            var nodeIdString = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.UniqueId);
            var connectionName = string.Empty;
            if (nodeIdString?.ValueAsString != null)
            {
                connectionName = nodeIdString.ValueAsString;
            }

            var foundConnection = network.SewerConnections.OfType<T>().FirstOrDefault(sc => sc.Name.Equals(connectionName));

            return foundConnection ?? new T();
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
            var nodeIdEnd = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd);
            if (nodeIdStart.ValueAsString != string.Empty && network != null)
            {
                var foundNode = network.Manholes.FirstOrDefault(m => m.GetCompartmentByName(nodeIdStart.ValueAsString) != null);
                if (foundNode == null)
                {
                    //create node
                    foundNode = GetNewManholeForCompartment(nodeIdStart.ValueAsString);
                    network.Nodes.Add(foundNode);
                }
                sewerConnection.Source = foundNode;
                sewerConnection.SourceCompartment = foundNode.GetCompartmentByName(nodeIdStart.ValueAsString);
            }
 
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

        private static Pump FindOrCreatePump(SewerConnection connection)
        {
            var structureFound = connection.BranchFeatures.OfType<Pump>().FirstOrDefault( bf => bf.Name.Equals(connection.Name));
            return structureFound != null ? structureFound : new Pump(connection.Name);
        }
        private static void AddPumpAndAttributesToSewerConnection(SewerConnection connection, GwswElement gwswElement)
        {
            //Add pump to structure
            var sewerPump = FindOrCreatePump(connection);

            //Add Attributes
            var flowDirection = GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.FlowDirection);
            if (flowDirection != null && flowDirection.ValueAsString != string.Empty)
            {
                var directionValue = GetValueFromDescription<SewerConnectionMapping.FlowDirection>(flowDirection.ValueAsString);
                if (directionValue == SewerConnectionMapping.FlowDirection.FromStartToEnd)
                {
                    sewerPump.DirectionIsPositive = true;
                }
                if (directionValue == SewerConnectionMapping.FlowDirection.FromEndToStart)
                {
                    sewerPump.DirectionIsPositive = false;
                }
            }

            //Add pump to network if it´s not present already
            if(!connection.BranchFeatures.Contains(sewerPump))
                AddStructureToBranch(connection, sewerPump);
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
            structure.Name = connection.Name;

            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(structure, connection);
        }

        #endregion

        #region Creating Structures

        private static Type GetGwswElementStructureType(GwswAttribute structureTypeAttribute)
        {
            if (structureTypeAttribute == null || structureTypeAttribute.ValueAsString == string.Empty) return null;

            var structureType = GetValueFromDescription<StructureMapping.StructureType>(structureTypeAttribute.ValueAsString);
            if (structureType == StructureMapping.StructureType.Pump)
            {
                return typeof(Pump);
            }
            if (structureType == StructureMapping.StructureType.Outlet)
            {
                return typeof(OutletCompartment);
            }

            return null;
        }

        private static INetworkFeature StructureFactory(object element, HydroNetwork network = null)
        {
            var gwswElement = element as GwswElement;
            if (gwswElement == null) return null;

            var structureType =
                GetGwswElementStructureType(GetAttributeFromList(gwswElement, StructureMapping.PropertyKeys.StructureType));

            var structureName = GetAttributeFromList(gwswElement, StructureMapping.PropertyKeys.UniqueId);
            if (structureType == null || structureName == null || structureName.ValueAsString == string.Empty) return null;
            
            /* First we need to check whether there are target and source nodes, otherwise we will not create this sewer connection.*/
            if (structureType == typeof(Pump))
            {
                if (network != null)
                {
                    var pumpFound = network.BranchFeatures.OfType<Pump>()
                        .FirstOrDefault(p => p.Name.Equals(structureName.ValueAsString));
                    if (pumpFound == null)
                    {
                        pumpFound = new Pump(structureName.ValueAsString);
                        //Create a sewer connection placeholder and add it to the network so that the structure is later added as well.
                        var auxSewerConnection = new SewerConnection(structureName.ValueAsString){ Network = network };
                        network.Branches.Add(auxSewerConnection);
                        AddStructureToBranch(auxSewerConnection, pumpFound);
                    }
                    ExtendPumpAttributes(pumpFound, gwswElement);
                    
                    return pumpFound;
                }
            }
            if (structureType == typeof(OutletCompartment))
            {
                var outletStructure = FindOrGetNewCompartment<OutletCompartment>(gwswElement, network);
                ExtendOutletAttributes(outletStructure, gwswElement);

                //If the network does not contain the manhole then it means it is a placeholder, but we still need to add it.
                if (network != null && !network.Manholes.Contains(outletStructure.ParentManhole))
                {
                    network.Nodes.Add(outletStructure.ParentManhole);
                }

                return outletStructure;
            }
            return null;
        }

        private static void ExtendPumpAttributes(Pump pump, GwswElement gwswElement)
        {
            //Add Attributes
            var newDoubleValue = 0.0;
            var pumpCapacity = GetAttributeFromList(gwswElement, StructureMapping.PropertyKeys.PumpCapacity);
            if (pumpCapacity != null && pumpCapacity.ValueAsString != string.Empty)
            {
                var valueType = pumpCapacity.GwswAttributeType.AttributeType;
                if (valueType == pump.Capacity.GetType() &&
                    TryParseDoubleElseLogError(pumpCapacity, valueType, out newDoubleValue))
                {
                    pump.Capacity = newDoubleValue;
                }
            }
            var startLevelDown = GetAttributeFromList(gwswElement, StructureMapping.PropertyKeys.StartLevelDownstreams);
            if (startLevelDown != null && startLevelDown.ValueAsString != string.Empty)
            {
                var valueType = startLevelDown.GwswAttributeType.AttributeType;
                if (valueType == pump.StartSuction.GetType() &&
                    TryParseDoubleElseLogError(startLevelDown, valueType, out newDoubleValue))
                {
                    pump.StartSuction = newDoubleValue;
                }
            }
            var stopLevelDown = GetAttributeFromList(gwswElement, StructureMapping.PropertyKeys.StopLevelDownstreams);
            if (stopLevelDown != null && stopLevelDown.ValueAsString != string.Empty)
            {
                var valueType = stopLevelDown.GwswAttributeType.AttributeType;
                if (valueType == pump.StopSuction.GetType() &&
                    TryParseDoubleElseLogError(stopLevelDown, valueType, out newDoubleValue))
                {
                    pump.StopSuction = newDoubleValue;
                }
            }

            var startLevelUp = GetAttributeFromList(gwswElement, StructureMapping.PropertyKeys.StartLevelUpstreams);
            if (startLevelUp != null && startLevelUp.ValueAsString != string.Empty)
            {
                var valueType = startLevelUp.GwswAttributeType.AttributeType;
                if (valueType == pump.StartDelivery.GetType() &&
                    TryParseDoubleElseLogError(startLevelUp, valueType, out newDoubleValue))
                {
                    pump.StartDelivery = newDoubleValue;
                }
            }
            var stopLevelUp = GetAttributeFromList(gwswElement, StructureMapping.PropertyKeys.StopLevelUpstreams);
            if (stopLevelUp != null && stopLevelUp.ValueAsString != string.Empty)
            {
                var valueType = stopLevelUp.GwswAttributeType.AttributeType;
                if (valueType == pump.StopDelivery.GetType() &&
                    TryParseDoubleElseLogError(stopLevelUp, valueType, out newDoubleValue))
                {
                    pump.StopDelivery = newDoubleValue;
                }
            }
        }

        private static void ExtendOutletAttributes(Compartment compartment, GwswElement gwswElement)
        {
            var newOutlet = compartment as OutletCompartment;
            if (newOutlet == null) return;

            var newDoubleValue = 0.0;
            var surfaceWaterLevel = GetAttributeFromList(gwswElement, StructureMapping.PropertyKeys.SurfaceWaterLevel);
            if (surfaceWaterLevel != null && surfaceWaterLevel.ValueAsString != string.Empty)
            {
                var valueType = surfaceWaterLevel.GwswAttributeType.AttributeType;
                if (valueType == newOutlet.SurfaceWaterLevel.GetType() &&
                    TryParseDoubleElseLogError(surfaceWaterLevel, valueType, out newDoubleValue))
                {
                    newOutlet.SurfaceWaterLevel = newDoubleValue;
                }
            }
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

        private static bool TryGetDoubleValueElseLogException(string columnKey, IReadOnlyDictionary<string, string> elementValues, string id, string manholeId, out double doubleValue)
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

        private static void LogManholeWarningMessage(string id, string dataType, string columnKey, string wrongValue, string manholeId)
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
