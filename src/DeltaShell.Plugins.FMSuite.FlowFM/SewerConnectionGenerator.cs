using System.Collections.Generic;
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
        private static readonly ILog Log = LogManager.GetLogger(typeof(SewerConnectionGenerator));

        public virtual INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network, object importHelper = null)
        {
            if (!gwswElement.IsValidGwswSewerConnection()) return null;
            return CreateSewerConnection<SewerConnection>(gwswElement, network, importHelper);
        }

        protected  T CreateSewerConnection<T>(GwswElement gwswElement, IHydroNetwork network = null, object importHelper = null) where T : SewerConnection, new()
        {
            if (gwswElement == null) return null;

            //Now we are free to create the connection.
            var connection = FindOrGetNewConnection<T>(gwswElement, network);
            SetSewerConnectionAttributes(connection, gwswElement, network, importHelper);
            SetSewerConnectionDefaultGeometry(connection);

            return connection;
        }

        private static T FindOrGetNewConnection<T>(GwswElement gwswElement, IHydroNetwork network = null) where T : SewerConnection, new()
        {
            var nodeIdString = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.UniqueId);
            var connectionName = string.Empty;
            if (nodeIdString.IsValidAttribute())
            {
                connectionName = nodeIdString.ValueAsString;
            }

            if (network == null) return new T { Name = connectionName};
            var foundConnection = network.SewerConnections.OfType<T>().FirstOrDefault(sc => sc.Name.Equals(connectionName));

            return foundConnection ?? new T { Name = connectionName };
        }

        protected virtual void SetSewerConnectionAttributes(ISewerConnection sewerConnection, GwswElement gwswElement, IHydroNetwork network, object helper)
        {
            sewerConnection.Network = network;
            var manholeCompartmentDict = helper as Dictionary<string, IManhole>;

            var nodeIdString = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.UniqueId);
            if (nodeIdString.IsValidAttribute())
            {
                sewerConnection.Name = nodeIdString.ValueAsString;
            }
            
            if (gwswElement.ElementTypeName != SewerFeatureType.Connection.ToString()) return;
            var nodeIdStart = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.NodeUniqueIdStart);
            var nodeIdEnd = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.NodeUniqueIdEnd);
            if (nodeIdStart.IsValidAttribute() && network != null)
            {
                IManhole foundNode = null; 
                if (manholeCompartmentDict!= null)
                {
                    IManhole auxFoundNode;
                    if (manholeCompartmentDict.TryGetValue(nodeIdStart.ValueAsString, out auxFoundNode))
                    {
                        foundNode = auxFoundNode;
                    }
                }
                else
                {
                    foreach (var m in network.Manholes)
                    {
                        if (m.GetCompartmentByName(nodeIdStart.ValueAsString) == null) continue;
                        foundNode = m;
                        break;
                    }
                }
                
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
                IManhole foundNode = null;

                if (manholeCompartmentDict != null)
                {
                    IManhole auxFoundNode;
                    if (manholeCompartmentDict.TryGetValue(nodeIdEnd.ValueAsString, out auxFoundNode))
                    {
                        foundNode = auxFoundNode;
                    }
                }
                else
                {
                    foundNode = network.Manholes.FirstOrDefault(m => m.GetCompartmentByName(nodeIdEnd.ValueAsString) != null);
                }
                if (foundNode == null)
                {
                    //create node
                    foundNode = GetNewManholeForSewerCompartment(nodeIdEnd.ValueAsString);
                    network.Nodes.Add(foundNode);
                }
                sewerConnection.Target = foundNode;
                sewerConnection.TargetCompartment = foundNode.GetCompartmentByName(nodeIdEnd.ValueAsString); 
            }

            double auxDouble;

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
                //Find type
                sewerConnection.WaterType = waterType.GetValueFromDescription<SewerConnectionWaterType>();
            }
        }

        private static void SetSewerConnectionDefaultGeometry(ISewerConnection sewerConnection)
        {
            if (sewerConnection.Geometry?.Coordinate != null) return;

            var manholeSource = sewerConnection.Source;
            var manholeTarget = sewerConnection.Target;

            if (manholeSource == null || manholeTarget == null) return;
            var defaultPoint = new Point(0,0);

            if (manholeSource.Geometry?.Coordinate == null)
                manholeSource.Geometry = defaultPoint;
            if (manholeTarget.Geometry?.Coordinate == null)
                manholeTarget.Geometry = defaultPoint;

            sewerConnection.Geometry = new LineString(
                new[]
                {
                    manholeSource.Geometry.Coordinate,
                    manholeTarget.Geometry.Coordinate
                });
        }

        private static Manhole GetNewManholeForSewerCompartment(string compartmentName)
        {
            var manholePlaceholder = new Manhole(string.Format("Manhole_For_Compartment_{0}", compartmentName));

            manholePlaceholder.Compartments.Add(new Compartment(compartmentName));
            Log.InfoFormat(Resources.SewerFeatureFactory_GetNewManholeForCompartment_Created_Manhole__0__and_compartment__1__with_default_values_as_they_were_not_found_in_the_network_, manholePlaceholder.Name, compartmentName);

            return manholePlaceholder;
        }
    }
}