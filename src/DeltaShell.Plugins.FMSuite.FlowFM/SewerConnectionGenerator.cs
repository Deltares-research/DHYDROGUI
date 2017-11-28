using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerConnectionGenerator: SewerFeatureFactory, ISewerNetworkFeatureGenerator
    {
        private static ILog Log = LogManager.GetLogger(typeof(SewerConnectionGenerator));

        public INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network)
        {
            if (gwswElement == null) return null;

            return CreateSewerConnection<SewerConnection>(gwswElement, network);
        }

        protected static T CreateSewerConnection<T>(GwswElement gwswElement, IHydroNetwork network = null, Action<T, GwswElement, IHydroNetwork> connectionAction = null) where T : SewerConnection, new()
        {
            /* First we need to check whether there are target and source nodes, otherwise we will not create this sewer connection.*/
            if (!IsValidSewerConnection(gwswElement)) return null;

            //Now we are free to create the connection.
            var connection = FindOrGetNewConnection<T>(gwswElement, network);

            SetSewerConnectionAttributes(connection, gwswElement, network);
            connectionAction?.Invoke(connection, gwswElement, network);
            SetSewerConnectionDefaultGeometry(connection);

            return connection;
        }

        private static bool IsValidSewerConnection(GwswElement gwswElement)
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

        private static T FindOrGetNewConnection<T>(GwswElement gwswElement, IHydroNetwork network = null) where T : SewerConnection, new()
        {
            if (network == null) return new T();

            var nodeIdString = SewerFeatureFactory.GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.UniqueId);
            var connectionName = string.Empty;
            if (nodeIdString?.ValueAsString != null)
            {
                connectionName = nodeIdString.ValueAsString;
            }

            var foundConnection = network.SewerConnections.OfType<T>().FirstOrDefault(sc => sc.Name.Equals(connectionName));

            return foundConnection ?? new T();
        }

        private static void SetSewerConnectionAttributes(ISewerConnection sewerConnection, GwswElement gwswElement, IHydroNetwork network)
        {
            double newDoubleValue;
            sewerConnection.Network = network;

            var nodeIdString = SewerFeatureFactory.GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.UniqueId);
            if (nodeIdString?.ValueAsString != null)
            {
                sewerConnection.Name = nodeIdString.ValueAsString;
            }

            var nodeIdStart = SewerFeatureFactory.GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart);
            var nodeIdEnd = SewerFeatureFactory.GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd);
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

            var levelStart = SewerFeatureFactory.GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.LevelStart);
            if (levelStart != null)
            {
                var valueType = levelStart.GwswAttributeType.AttributeType;
                if (valueType == sewerConnection.LevelSource.GetType() &&
                    TryParseDoubleElseLogError(levelStart, valueType, out newDoubleValue))
                {
                    sewerConnection.LevelSource = newDoubleValue;
                }
            }
            var levelEnd = SewerFeatureFactory.GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.LevelEnd);
            if (levelEnd != null)
            {
                var valueType = levelEnd.GwswAttributeType.AttributeType;
                if (valueType == sewerConnection.LevelTarget.GetType() &&
                    TryParseDoubleElseLogError(levelEnd, valueType, out newDoubleValue))
                {
                    sewerConnection.LevelTarget = newDoubleValue;
                }
            }

            var length = SewerFeatureFactory.GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.Length);
            if (length != null)
            {
                var valueType = length.GwswAttributeType.AttributeType;
                if (valueType == sewerConnection.Length.GetType() &&
                    TryParseDoubleElseLogError(length, valueType, out newDoubleValue))
                {
                    sewerConnection.Length = newDoubleValue;
                }
            }

            var waterType = SewerFeatureFactory.GetAttributeFromList(gwswElement, SewerConnectionMapping.PropertyKeys.WaterType);
            if (waterType != null && waterType.ValueAsString != string.Empty)
            {
                //Find type;
                sewerConnection.WaterType = GetValueFromDescription<SewerConnectionWaterType>(waterType.ValueAsString);
            }
        }

        private static void SetSewerConnectionDefaultGeometry(ISewerConnection sewerConnection)
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

        private static Manhole GetNewManholeForCompartment(string compartmentName)
        {
            var manholePlaceholder = new Manhole(string.Format("Manhole_For_Compartment_{0}", compartmentName));

            manholePlaceholder.Compartments.Add(new Compartment(compartmentName));

            Log.InfoFormat(Resources.SewerFeatureFactory_GetNewManholeForCompartment_Created_Manhole__0__and_compartment__1__with_default_values_as_they_were_not_found_in_the_network_, manholePlaceholder.Name, compartmentName);

            return manholePlaceholder;
        }
    }
}