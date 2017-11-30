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
    public class SewerCompartmentGenerator: ISewerNetworkFeatureGenerator
    {
        private static ILog Log = LogManager.GetLogger(typeof(SewerCompartmentGenerator));

        public INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network)
        {
            if (gwswElement == null) return null;
            return CreateCompartment<Compartment>(gwswElement, network);
        }

        public static T FindOrGetNewCompartment<T>(GwswElement gwswElement, IHydroNetwork network = null) where T : Compartment, new()
        {
            //We know they are not null, otherwise we would have already returned in the CompartmentFactory method.
            var compartmentName = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.UniqueId).ValueAsString;
            var parentManhole = GetParentManholeFromGwswElement(gwswElement, network);

            if (network == null || !parentManhole.ContainsCompartment(compartmentName))
                return new T{Name = compartmentName, ParentManhole = parentManhole};

            var compartmentFound = parentManhole.GetCompartmentByName(compartmentName);
            return (T) compartmentFound ?? new T { Name = compartmentName, ParentManhole = parentManhole };
        }

        protected static T CreateCompartment<T>(GwswElement gwswElement, IHydroNetwork network = null) where T : Compartment, new()
        {
            if (!gwswElement.IsValidGwswCompartment()) return null;

            var compartment = FindOrGetNewCompartment<T>(gwswElement, network);
            SetCompartmentAttributes(compartment, gwswElement, network);
            return compartment;
        }

        private static Manhole GetParentManholeFromGwswElement(GwswElement gwswElement, IHydroNetwork network)
        {
            var manholeAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.ManholeId);
            var compartmentName = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.UniqueId).ValueAsString;
            
            //if we could not place an ID then we use the compartment name to create a place holder (if needed)
            var manholeName = manholeAttribute != null ? manholeAttribute.ValueAsString : compartmentName;

            Manhole parentManhole = null;
            if (network != null)
            {
                //Find manhole by its name
                parentManhole = network.Manholes.FirstOrDefault(m => m.Name.Equals(manholeName)) as Manhole;
                //If it was not found, try searching by containing this compartment.
                if (parentManhole == null)
                {
                    parentManhole = network.Manholes.FirstOrDefault(m => m.ContainsCompartment(compartmentName)) as Manhole;
                }
            }

            if (parentManhole == null)
            {
                parentManhole = new Manhole(manholeName);
                if (network != null)
                {
                    network.Nodes.Add(parentManhole);
                    Log.InfoFormat(Resources.SewerFeatureFactory_GetNewManholeForCompartment_Created_Manhole__0__and_compartment__1__with_default_values_as_they_were_not_found_in_the_network_, manholeName, compartmentName);
                }
            }
            return parentManhole;
        }

        private static void SetCompartmentAttributes(Compartment compartment, GwswElement gwswElement, IHydroNetwork network)
        {
            compartment.ParentManhole = GetParentManholeFromGwswElement(gwswElement, network);

            // Set the rest of manhole values
            var auxDouble = 0.0;
            var nodeLength = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.NodeLength);
            if( nodeLength.TryGetValueAsDouble(out auxDouble))
                compartment.ManholeLength = auxDouble;

            var nodeWidth = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.NodeWidth);
            if( nodeWidth.TryGetValueAsDouble(out auxDouble))
                compartment.ManholeWidth = auxDouble;

            var floodableArea = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.FloodableArea);
            if(floodableArea.TryGetValueAsDouble(out auxDouble))
                compartment.FloodableArea = auxDouble;

            var bottomLevel = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.BottomLevel);
            if( bottomLevel.TryGetValueAsDouble(out auxDouble))
                compartment.BottomLevel = auxDouble;

            var surfaceLevel = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.SurfaceLevel);
            if (surfaceLevel.TryGetValueAsDouble(out auxDouble))
                compartment.SurfaceLevel = auxDouble;

            double yCoordinate;
            double xCoordinate;
            var xCoord = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.XCoordinate);
            var yCoord = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.YCoordinate);
            if (xCoord.TryGetValueAsDouble(out xCoordinate) && yCoord.TryGetValueAsDouble(out yCoordinate))
            {
                compartment.Geometry = new Point(xCoordinate, yCoordinate);
            }

            // Set shape value of the manhole
            var nodeShape = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.NodeShape);
            if(nodeShape.IsValidAttribute())
            {
                compartment.Shape = SewerFeatureFactory.GetValueFromDescription<CompartmentShape>(nodeShape.GetValidStringValue());
            }
        }
    }
}