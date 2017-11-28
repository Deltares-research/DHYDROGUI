using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerCompartmentGenerator: SewerFeatureFactory, ISewerNetworkFeatureGenerator
    {
        private static ILog Log = LogManager.GetLogger(typeof(SewerCompartmentGenerator));

        public INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network)
        {
            if (gwswElement == null) return null;
            return CreateCompartment<Compartment>(gwswElement, network);
        }

        public static ISewerNetworkFeatureGenerator GetSewerCompartmentGenerator(GwswAttribute sewerTypeAttribute)
        {
            var basicGenerator = new SewerCompartmentGenerator();
            if (string.IsNullOrEmpty(sewerTypeAttribute?.ValueAsString)) return basicGenerator;

            var nodeType = GetValueFromDescription<ManholeMapping.NodeType>(sewerTypeAttribute.ValueAsString);
            return IsGwswOutlet(nodeType) ? new SewerCompartmentOutletGenerator() : basicGenerator;
        }

        private static bool IsGwswOutlet(ManholeMapping.NodeType nodeType)
        {
            return nodeType == ManholeMapping.NodeType.Outlet;
        }

        public static T FindOrGetNewCompartment<T>(GwswElement gwswElement, IHydroNetwork network = null) where T : Compartment, new()
        {
            //We know they are not null, otherwise we would have already returned in the CompartmentFactory method.
            var compartmentName = GetAttributeFromList(gwswElement, ManholeMapping.PropertyKeys.UniqueId).ValueAsString;
            var parentManhole = GetParentManholeFromGwswElement(gwswElement, network);

            if (network == null || !parentManhole.ContainsCompartment(compartmentName))
                return new T{Name = compartmentName, ParentManhole = parentManhole};

            var compartmentFound = parentManhole.GetCompartmentByName(compartmentName);
            return (T) compartmentFound ?? new T { Name = compartmentName, ParentManhole = parentManhole };
        }

        protected static T CreateCompartment<T>(GwswElement gwswElement, IHydroNetwork network = null) where T : Compartment, new()
        {
            if (!IsValidGwswCompartment(gwswElement)) return null;

            var compartment = FindOrGetNewCompartment<T>(gwswElement, network);
            SetCompartmentAttributes(compartment, gwswElement, network);
            return compartment;
        }

        private static Manhole GetParentManholeFromGwswElement(GwswElement gwswElement, IHydroNetwork network)
        {
            var manholeAttribute = GetAttributeFromList(gwswElement, ManholeMapping.PropertyKeys.ManholeId);
            var compartmentName = GetAttributeFromList(gwswElement, ManholeMapping.PropertyKeys.UniqueId).ValueAsString;
            
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
    }
}