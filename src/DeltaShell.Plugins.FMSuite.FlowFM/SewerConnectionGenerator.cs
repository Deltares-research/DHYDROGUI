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
    public class SewerConnectionGenerator: ISewerNetworkFeatureGenerator
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
            if (!gwswElement.IsValidGwswSewerConnection()) return null;

            //Now we are free to create the connection.
            var connection = FindOrGetNewConnection<T>(gwswElement, network);

            SetSewerConnectionAttributes(connection, gwswElement, network);
            connectionAction?.Invoke(connection, gwswElement, network);
            SetSewerConnectionDefaultGeometry(connection);

            return connection;
        }

        protected static T FindOrGetNewConnection<T>(GwswElement gwswElement, IHydroNetwork network = null) where T : SewerConnection, new()
        {
            var nodeIdString = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.UniqueId);
            var connectionName = string.Empty;
            if (nodeIdString.IsValidAttribute())
            {
                connectionName = nodeIdString.ValueAsString;
            }

            if (network == null) return new T(){ Name = connectionName};
            var foundConnection = network.SewerConnections.OfType<T>().FirstOrDefault(sc => sc.Name.Equals(connectionName));

            return foundConnection ?? new T() { Name = connectionName };
        }

        private static void SetSewerConnectionAttributes(ISewerConnection sewerConnection, GwswElement gwswElement, IHydroNetwork network)
        {
            sewerConnection.Network = network;

            var nodeIdString = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.UniqueId);
            if (nodeIdString?.ValueAsString != null)
            {
                sewerConnection.Name = nodeIdString.ValueAsString;
            }

            var nodeIdStart = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart);
            var nodeIdEnd = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd);
            if (nodeIdStart.IsValidAttribute() && network != null)
            {
                var foundNode = network.Manholes.FirstOrDefault(m => m.GetCompartmentByName(nodeIdStart.ValueAsString) != null);
                if (foundNode == null)
                {
                    //create node
                    foundNode = GetNewManholeForSewerCompartment(nodeIdStart.ValueAsString);
                    network.Nodes.Add(foundNode);
                }
                sewerConnection.Source = foundNode;
                sewerConnection.SourceCompartment = foundNode.GetCompartmentByName(nodeIdStart.ValueAsString);
            }

            if (nodeIdEnd.IsValidAttribute() && network != null)
            {
                var foundNode = network.Manholes.FirstOrDefault(m => m.GetCompartmentByName(nodeIdEnd.ValueAsString) != null);
                if (foundNode == null)
                {
                    //create node
                    foundNode = GetNewManholeForSewerCompartment(nodeIdEnd.ValueAsString);
                    network.Nodes.Add(foundNode);
                }
                sewerConnection.Target = foundNode;
                sewerConnection.TargetCompartment = foundNode.GetCompartmentByName(nodeIdEnd.ValueAsString);
            }

            var auxDouble = 0.0;

            var levelStart = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.LevelStart);
            if( levelStart.TryGetValueAsDouble(out auxDouble))
                sewerConnection.LevelSource = auxDouble;

            var levelEnd = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.LevelEnd);
            if( levelEnd.TryGetValueAsDouble(out auxDouble))
                sewerConnection.LevelTarget = auxDouble;

            var length = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Length);
            if( length.TryGetValueAsDouble(out auxDouble))
                sewerConnection.Length = auxDouble;

            var waterType = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.WaterType);
            if (waterType.IsValidAttribute())
            {
                //Find type;
                sewerConnection.WaterType = SewerFeatureFactory.GetValueFromDescription<SewerConnectionWaterType>(waterType.ValueAsString);
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

        public static Manhole GetNewManholeForSewerCompartment(string compartmentName)
        {
            var manholePlaceholder = new Manhole(string.Format("Manhole_For_Compartment_{0}", compartmentName));

            manholePlaceholder.Compartments.Add(new Compartment(compartmentName));
            Log.InfoFormat(Resources.SewerFeatureFactory_GetNewManholeForCompartment_Created_Manhole__0__and_compartment__1__with_default_values_as_they_were_not_found_in_the_network_, manholePlaceholder.Name, compartmentName);

            return manholePlaceholder;
        }
    }
}